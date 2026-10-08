# Slides — the presentation system

Turns the course's PowerPoint lectures into **Unity-native slide decks**, so one scene can show
the slides and — with a single keypress — hide them and reveal the live lab game behind them,
without switching applications.

## Files

| Path | What it is |
|---|---|
| `Core/SlideData.cs` | `[Serializable]` shapes mirroring the deck JSON (JsonUtility-safe) |
| `Core/SlideCursor.cs` | pure slide/step state machine — no `UnityEngine` types, unit-tested |
| `Core/Slides.Core.asmdef` | the engine-free assembly a test asmdef is allowed to reference |
| `SlideDeckPlayer.cs` | parses the deck, owns the cursor, raises `Changed` |
| `SlideView.cs` | renders one slide into five TMP fields (null-guards all of them) |
| `SlidePresenter.cs` | shows/hides the deck; owns `Time.timeScale`; picks the slide's demo |
| `Slides.Runtime.asmdef` | the runtime assembly, so the PlayMode tests can see these scripts |
| `SlideInput.cs` | the key map; inert while the deck is hidden |
| `SlideNavigator.cs` | prev/next buttons, page label, jump-to-slide grid |
| `Assets/Editor/Slides/TmpBootstrap.cs` | imports TMP Essential Resources once (async-safe) |
| `Assets/Editor/Slides/SlideDeckBuilder.cs` | deck JSON → scene |
| `Assets/Tests/EditMode/` | `SlideCursorTests` (10) + `DeckLoadTests` (4) |
| `Assets/Tests/PlayMode/` | `SlidePresenterPlayTests` (3) — the presenter ↔ demo contract |
| `Assets/Slides/<key>.json` | a generated deck |
| `Assets/Scenes/W04Slides.unity` | a generated scene |

## Keys

| Key | Deck visible | Deck hidden |
|---|---|---|
| `Tab` | hide the slides → the game | show the slides again |
| `→` / `←` | next / previous slide | — |
| `Space`, or clicking empty space | next bullet; after the last one, next slide | jump (the game) |
| `Backspace` | previous bullet | — |
| `G` | open / close the jump-to-slide grid | — |
| `Esc` | close the jump grid | — |

While the deck is up the game is **frozen** (`Time.timeScale = 0`), so `Space` cannot both advance
a bullet and jump the character. The top-right button on the Chrome Canvas does exactly what `Tab`
does, and stays clickable while the deck is hidden.

`JumpTo` reveals the **whole** target slide — deliberate, for 跳页/复习. Forward teaching uses
`Space` / `→` with `NextSlide` + `StepForward`.

## Regenerating

```bash
# 1. deck .pptx -> Assets/Slides/<key>.json
uv run --with python-pptx python tools/pptx_to_deck.py "<deck.pptx>"

# 2. JSON -> scene:  menu  CAT105TC ▸ Slides ▸ Build W04Slides
```

**Rule: edit the `.pptx` (or the generated JSON), never the scene by hand.** The builder rebuilds
the scene from scratch every time, so hand edits are lost.

## Adding another week

1. Run the converter for that `.pptx` (only W04 has been converted so far).
2. Add a menu item beside `BuildW04Slides`:
   `[MenuItem("CAT105TC/Slides/Build W05Slides")]` → `BuildDeckScene(json, scene, header, true)`.
3. Run it — it rebuilds the scene and adds it to Build Settings.

Two caveats:
* **CJK.** `LiberationSans SDF` is the only TMP font and has no CJK glyphs, so any non-ASCII label
  renders as `□□□`. The W7 and W11 decks contain Chinese; before building those, add a dynamic CJK
  font asset. (The converter already reports per-deck CJK counts.)
* **The grid caps at 64 slides.** W10's deck is 43, so everything current fits; a longer deck would
  need the cap raised or a scroll view.

## What the renderer does not draw yet

`SlideView` renders **title, subtitle and the bullet / blank / code blocks** only. It parses but does
not draw:

| Field | Status |
|---|---|
| `slide.image` | parsed, **not rendered** |
| `slide.table` | drawn only when the slide has no blocks — a slide with both loses the table |
| `slide.layout` (`"twoColumn"`) | parsed, **ignored**; every slide renders single-column |

`SlideDeckBuilder` logs a warning naming the counts whenever a deck carries any of these, so nothing
is dropped silently. W04 has none of them; the whole course has 2 images and 3 tables.

## Tests

| Where | What it covers |
|---|---|
| `Assets/Tests/EditMode/` | `SlideCursorTests` (10) — the state machine; `DeckLoadTests` (4) — the real deck's deserialisation contract |
| `Assets/Tests/PlayMode/` | `SlidePresenterPlayTests` (3) — hiding the deck activates that slide's demo and unfreezes, returning keeps the slide + step, a one-demo slide survives `Next()` |

The runtime lives in its own `Slides.Runtime` assembly and the pure logic in a separate engine-free
`Slides.Core` one, precisely so those test assemblies can reference them (a test asmdef cannot see
the predefined `Assembly-CSharp`).

**Still only verified by hand:** `SlideView`'s formatting, `SlideNavigator`'s grid, `SlideInput`'s
key map, and the demos themselves. `PlayerController2D` is in `Assembly-CSharp`, so the PlayMode test
pins the *mechanism* the "Space latches a jump" fix relies on — the demo roots are inactive while the
deck is up, so no `Update()` can run on them — rather than the controller itself.

## Adding a demo (for a later week)

`Assets/Editor/Slides/DemoKit.cs` holds the furniture every demo is built from: `Visual`, `Character`,
`Floor`, `Line`, `Bar`, `OwnCamera`, `CameraKit`, plus the shared palette. A new demo is usually:
a `DemoBase` subclass in `Demos/` (one concept, with `Title` / `Keys` / `Readout`), a `Spec` in
`SlideDemoBuilder.BuildSpecs()`, and an entry in `<deck>.demos.json`.

