# Slides-in-Unity — design

**Date:** 2026-10-08
**Status:** design approved in chat; spec awaiting review
**Scope of this document:** the full slides system + the W04Slides scene

---

## 1. Goal

Turn the course's PowerPoint lectures into **Unity-native slide decks**, so one Unity
scene can show the slides and — with a single keypress or button — hide them and reveal
the **live game behind them**, with no application switching.

## 2. Non-goals

* Not a WYSIWYG slide editor inside Unity. Content is authored in PowerPoint, or
  hand-edited in the generated JSON.
* Not a pixel-faithful clone of the PowerPoint theme. A clean, purpose-built teaching
  template is used. **Content fidelity matters; visual fidelity of the original theme
  does not.**
* No slide transitions, per-bullet PowerPoint animations, or speaker notes — the source
  decks contain none.

## 3. Facts about the source (measured)

| | value |
|---|---|
| Decks | 13 (`…/CAT105TC/Slides/*.pptx`), 335 slides total |
| Aspect | 13.33 × 7.5 in = **16:9**, exactly 1920×1080 |
| Pictures in the whole course | **2** |
| Tables in the whole course | 3 |
| Slide animations | **0** |
| W4 deck specifically | 28 slides, **0 pictures, 0 tables, 0 animations** |

The work is therefore: **parse text, lay it out well, and get the teaching interaction right.**

## 4. Architecture

### 4.1 Content pipeline — source → generator → artefact

```
Slides/W4 - L4 - Animations and Camera.pptx      source of truth (authored in PowerPoint)
        │   tools/pptx_to_deck.py                generator  (uv + python-pptx)
        ▼
Assets/Slides/W04_L4.json                        artefact   (plain text, hand-editable)
        │   Unity menu: CAT105TC ▸ Slides ▸ Build scene
        ▼
Assets/Scenes/W04Slides.unity                    artefact
```

**Rule:** edit the `.pptx` or the `.json`; never hand-edit the scene — regenerate it.
Re-running the generator and the builder is always safe (both are idempotent).

### 4.2 JSON schema

```jsonc
{
  "key": "W04_L4",
  "title": "Animations and 2D Art",
  "subtitle": "Lecture 4",
  "source": "Slides/W4 - L4 - Animations and Camera.pptx",
  "generated": "2026-10-08T12:00:00",
  "slides": [
    {
      "index": 1,
      "layout": "title",              // "title" | "content" | "twoColumn"
      "title": "Animations and 2D Art",
      "subtitle": "Lecture 4",
      "blocks": [
        { "level": 0, "text": "Physics 2D Controller", "kind": "bullet" },
        { "level": 1, "text": "rb.velocity = ...",     "kind": "code"   }
      ],
      "table": null,                  // or { "rows": [["A","B"],["1","2"]] }
      "image": null                   // or { "path": "W04_L4/img_01.png", "w": 800, "h": 450 }
    }
  ]
}
```

* `kind` ∈ `bullet` | `code` | `plain`. The converter auto-tags a paragraph as `code`
  when it looks like a standalone code line (ends in `;`, or matches `Identifier.Identifier(`);
  everything else is `bullet`. Editable by hand afterwards.
* `blocks` order **is** the reveal order (逐条显示).
* C# side: `[Serializable]` classes matching this shape, parsed with `JsonUtility`.

### 4.3 Unity runtime — `Assets/Scripts/Slides/`

| Component | Responsibility |
|---|---|
| `SlideDeckData` / `SlideData` / `SlideBlock` | `[Serializable]` data, matches the JSON |
| `SlideDeckPlayer` | owns the deck + `slideIndex` + `revealedBlocks`; `Next/Prev/StepForward/StepBack/JumpTo`; raises a refresh |
| `SlideView` | binds one slide to the UI (title, body, page number) |
| `SlideNavigator` | ← → buttons, `G` jump panel (list of slide titles, click to jump) |
| `SlidePresenter` | shows/hides the deck, owns `Time.timeScale`, updates the corner button label |
| `SlideInput` | maps keys → player actions, active only while the deck is visible |

**Scene structure**

```
Main Camera (CameraFollow2D)                ┐ the W4 platformer, built by
Environment / Gameplay / Annotations        ┘ W04LabBuilder.BuildGameplay()
Slides Canvas            (1920×1080)        ← the deck
  HeaderBar · Title · Subtitle · Body · PageIndicator · Prev/Next · JumpPanel
Chrome Canvas            (always on top)
  ToggleButton (top-right) · key hints
```

**Input map**

| Key | Deck visible | Deck hidden |
|---|---|---|
| `→` | next slide | — |
| `←` | previous slide | — |
| `Backspace` | previous bullet | — |
| `Space` / left-click | next bullet; when the last bullet is shown → next slide | jump (the game) |
| `G` | toggle jump panel | — |
| `Tab` | hide deck → game | show deck |
| `R` | — | restart the level |

**Toggle behaviour** — the important part:

* Deck visible → `Time.timeScale = 0` (game frozen), `Slides Canvas` active,
  `Chrome Canvas` active.
* Deck hidden → `Time.timeScale = 1` (game playable), `Slides Canvas` inactive,
  `Chrome Canvas` stays active so the button is always clickable.
* Returning to the deck restores the **same slide and same revealed-bullet count**, and
  the game is exactly where it was (frozen, not reset).

This is also what removes the `Space` conflict: while the deck is up, the game is frozen
and its input is inert.

### 4.4 Editor tooling — `Assets/Editor/Slides/`

* `SlideDeckBuilder` — reads `Assets/Slides/<key>.json`, generates
  `Assets/Scenes/<SceneName>.unity`. Menu: `CAT105TC ▸ Slides ▸ Build W04Slides`.
  Idempotent (rebuilds the scene from scratch).
* `TmpBootstrap` — imports TMP Essential Resources if they are missing (idempotent),
  so the first build cannot fail on a fresh clone.
* Reuses `LabKit` helpers (camera, UI panel/text factories) where they fit; the slide
  template itself lives here.

### 4.5 Refactor required

`W04LabBuilder.Build()` currently builds the platformer **and** the HUD. Extract:

```csharp
public static void BuildGameplay(Transform parent, out PlayerController2D player, out Camera cam)
```

`W04Lab` keeps its `LabHud`; `W04Slides` calls `BuildGameplay` and adds the deck instead.
`W04Lab` must be byte-for-byte equivalent in behaviour after the refactor.

### 4.6 Text technology — **TextMeshPro** (per user)

* TMP Essential Resources are imported into `Assets/TextMesh Pro/` if missing.
* Font: **LiberationSans SDF** (TMP default) — enough for every deck in scope now.
  Measured: only **W7 (slide 20)** and **W11 (slides 11–12)** contain CJK; when those decks
  are built, add a **dynamic CJK font asset** (TMP dynamic atlas from a system CJK font).
  The converter already flags any deck containing non-Latin text.
* Body text uses TMP `enableAutoSizing` (min/max in points) so **no slide can overflow**
  1920×1080 — long slides shrink instead of clipping.
* Code blocks (`kind:"code"`) use a monospace TMP font asset if one is available,
  otherwise the same font on a darkened panel.

## 5. Failure handling

| Situation | Behaviour |
|---|---|
| `Assets/Slides/<key>.json` missing | menu logs an error naming the exact python command to run |
| TMP essentials missing | imported automatically before building |
| Text too long even at min autosize | logged with the slide index; the slide still renders (TMP clips) |
| Unknown `kind` | rendered as `bullet` |
| Slide has no `title` | falls back to the deck title |
| `table`/`image` present | rendered; both are optional and unused by the W4 deck |

## 6. Verification plan

**Converter**
* For every deck: asserted `len(json.slides) == len(prs.slides)`.
* Print every slide title; eyeball against the source.
* Report per-deck counts of pictures / tables / CJK so nothing is silently dropped.

**Unity**
* Build W04Slides, open it, `read_console` → 0 errors.
* In play mode, assert: `SlideDeckPlayer.SlideCount == 28`; `Next()` advances the index;
  `StepForward()` reveals blocks one at a time and rolls over to the next slide at the end;
  `Tab` flips `Time.timeScale` 0↔1 **and** the `Slides Canvas` active state; the player can
  move while the deck is hidden; `JumpTo(n)` lands on slide n.
* Screenshot slides 1, 2 and a mid-deck slide for legibility at 1920×1080.
* Rebuild `W4Lab` and confirm it is unchanged (0 errors, HUD present).

## 7. Build order

1. `tools/pptx_to_deck.py`; run it for **all 13 decks** → `Assets/Slides/*.json`.
2. Unity data classes + `SlideDeckPlayer` + `SlideView` + the slide template.
3. `SlideDeckBuilder` menu + generate `W04Slides`.
4. Split `W04LabBuilder.BuildGameplay`; embed the platformer in `W04Slides`.
5. Toggle + pause behaviour + `Chrome Canvas` button.
6. Jump panel (`G`).
7. Verification, screenshots, rebuild `W4Lab`.

## 8. Decisions taken

| Decision | Choice | Why |
|---|---|---|
| Content source | PPTX → JSON → scene | decks are finished; keeps PowerPoint as the authoring tool |
| Deck data format | JSON, not ScriptableObject | plain text, diffable, script-generatable |
| Text tech | **TextMeshPro** | legibility + autosize (user's call) |
| Game integration | same scene, gameplay built by the shared builder | no scene loading, state survives the toggle |
| Scene name | `W04Slides` | as requested |
| Keep `W4Lab`? | yes, both scenes coexist | `W4Lab` is still the plain lab |

## 9. Open questions

None blocking. Two notes for later:
* Which key opens the jump panel is `G`; trivial to change.
* CJK font support is needed for **W7** (slide 20) and **W11** (slides 11–12), not for W4 —
  deferred until those decks are built.

## 10. Environment notes (measured before writing this spec)

* TMP Essential Resources are **not yet imported** into this project
  (`Assets/TextMesh Pro/` does not exist) → `TmpBootstrap` must import them on first build.
  The TMP package itself is present (`com.unity.textmeshpro@3.0.7`).
* This project is **not a git repository**, so this spec is written to disk but not committed.
