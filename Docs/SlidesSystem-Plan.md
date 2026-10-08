# Slides-in-Unity — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Every CAT105TC lecture `.pptx` becomes a Unity-native slide deck in a scene that can hide the slides with one key and reveal the live lab game behind them.

**Architecture:** A Python generator turns each `.pptx` into a JsonUtility-friendly `Assets/Slides/<key>.json`. A thin Unity runtime (`SlideCursor` = pure logic, `SlideDeckPlayer` = MonoBehaviour, `SlideView` = TMP rendering, `SlidePresenter` = show/hide + `Time.timeScale`) plays it. An editor builder generates the scene, reusing the week's existing lab-builder for the game behind.

**Tech Stack:** Unity 2022.3.62f3c1, TextMeshPro 3.0.7, uGUI, python-pptx 1.0.2 via `uv`, Unity MCP for verification.

## Global Constraints

- Spec: `Docs/SlidesSystem-Design.md` (authoritative for behaviour).
- Target resolution: **1920 × 1080**; `CanvasScaler` = ScaleWithScreenSize, reference `(1920,1080)`, match `0.5`.
- Text: **TextMeshPro** everywhere in the slides system. Body uses `enableAutoSizing` (min 22, max 40).
- Deck JSON is the artefact; never hand-edit a generated scene.
- Unity layer/tag names already in use: `Ground 6, Player 7, OneWay 8, Hazard 9, Trigger 10`; tags `Coin, Goal, Hazard, Player, Finish`. Do not change them.
- Script folders follow the project convention (`Assets/Scripts/<area>/`). Slides is a new area; **do not** put it under a week folder.
- **Scope: W04 only.** Build the system generically, but generate **only the W4 deck** and **only the `W04Slides` scene**. Do not convert the other 12 decks.
- Git: the project is now a repository, branch `feature/slides-system`. **Commit after every task** (`feat(slides): …` / `test(slides): …`).
- Verification uses the Unity MCP tools (`execute_code`, `manage_scene`, `read_console`, `manage_editor`, `execute_menu_item`) through `execute`, and shell for the Python side.
- **Unity MCP gotchas that will bite you:** `execute_code` refuses file-deleting code unless you pass `safety_checks:false`; play mode is throttled while the Unity Editor is unfocused, so call `Application.runInBackground = true` from `execute_code` right after entering play; `execute_code` bodies have **no `using` directives** (fully-qualify every type) and compile as C# 6.

---

## File Structure

**Created**

| Path | Responsibility |
|---|---|
| `tools/pptx_to_deck.py` | pptx → deck JSON (pure function + CLI) |
| `tools/tests/test_pptx_to_deck.py` | converter tests |
| `Assets/Slides/<key>.json` × 13 | generated decks |
| `Assets/Scripts/Slides/SlideData.cs` | `[Serializable]` JSON shapes |
| `Assets/Scripts/Slides/SlideCursor.cs` | pure slide/step state machine |
| `Assets/Scripts/Slides/SlideDeckPlayer.cs` | MonoBehaviour wrapper + change events |
| `Assets/Scripts/Slides/SlideView.cs` | renders one slide to TMP |
| `Assets/Scripts/Slides/SlidePresenter.cs` | show/hide deck, owns `Time.timeScale` |
| `Assets/Scripts/Slides/SlideInput.cs` | key → action map, deck-visible only |
| `Assets/Scripts/Slides/SlideNavigator.cs` | prev/next buttons, page label, jump grid |
| `Assets/Scripts/Slides/README.md` | how to add a deck |
| `Assets/Editor/Slides/TmpBootstrap.cs` | import TMP essentials if missing |
| `Assets/Editor/Slides/SlideDeckBuilder.cs` | JSON → scene, menu items |
| `Assets/Tests/EditMode/Slides.EditMode.asmdef` | test assembly |
| `Assets/Tests/EditMode/SlideCursorTests.cs` | cursor unit tests |
| `Assets/Scenes/W04Slides.unity` | generated scene |

**Modified**

| Path | Change |
|---|---|
| `Assets/Editor/W04_AnimationCamera/W04LabBuilder.cs` | extract `BuildGameplay()`, `Build()` calls it + adds the HUD |
| `Assets/Scripts/README.md` | document the new `Slides/` areas |

---

## Task 1: Deck converter (Python) + tests

**Files:** create `tools/pptx_to_deck.py`, `tools/tests/test_pptx_to_deck.py`

**Interfaces — produces:** `convert(pptx_path: Path) -> dict` returning exactly:

```jsonc
{ "key": "W04_L4", "title": "Animations and 2D Art", "subtitle": "Lecture 4",
  "source": "Slides/…pptx", "generated": "2026-10-08T12:00:00",
  "slides": [ { "index":1, "layout":"title|content|twoColumn",
                "title":"…", "subtitle":"…",
                "blocks":[ {"level":0,"text":"…","kind":"bullet|code|blank"} ],
                "table": null, "image": null } ] }
```

`table` when present: `{"columns": 2, "cells": ["r0c0","r0c1","r1c0","r1c1"]}` (row-major — **JsonUtility cannot deserialise jagged arrays**). `image`: `{"path":"W04_L4/img_01.png","w":800,"h":450}`.

- [ ] **Step 1: Write the failing test**

```python
# tools/tests/test_pptx_to_deck.py
import sys, json
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from pptx_to_deck import convert, deck_key

DECK = Path("/Users/gyk/Documents/Work/AY26-27/CAT105TC/Slides/W4 - L4 - Animations and Camera.pptx")

def test_key_slug():
    assert deck_key(DECK) == "W04_L4"

def test_slide_count_matches_source():
    from pptx import Presentation
    deck = convert(DECK)
    assert len(deck["slides"]) == len(Presentation(DECK).slides)

def test_shape_is_jsonutility_safe():
    deck = convert(DECK)
    assert deck["slides"][0]["layout"] == "title"
    for s in deck["slides"]:
        assert isinstance(s["blocks"], list)
        for b in s["blocks"]:
            assert b["kind"] in ("bullet", "code", "blank")
            assert isinstance(b["level"], int)
        # no dict/None surprises
        json.dumps(deck)

def test_no_empty_trailing_blocks():
    for s in convert(DECK)["slides"]:
        if s["blocks"]:
            assert s["blocks"][-1]["kind"] != "blank"
```

- [ ] **Step 2: Run it, expect failure**

Run: `cd /Users/gyk/UnityProjects/CAT105TCDemo && uv run --with python-pptx --with pytest pytest tools/tests -q`
Expected: `ModuleNotFoundError: pptx_to_deck` (or import error).

- [ ] **Step 3: Implement the converter**

```python
# tools/pptx_to_deck.py
"""Convert a CAT105TC lecture .pptx into a Unity-consumable deck JSON.

Source of truth stays the .pptx. Output is written to <unity>/Assets/Slides/<key>.json.
JsonUtility constraints: no jagged arrays, no dictionaries, missing field == default.
"""
from __future__ import annotations
import argparse, json, re, sys
from datetime import datetime
from pathlib import Path
from pptx import Presentation
from pptx.enum.shapes import MSO_SHAPE_TYPE

CJK = re.compile(r"[\u4e00-\u9fff\u3040-\u30ff\uac00-\ud7af]")
CODE_HINT = re.compile(r"^\s*[A-Za-z_][\w\.]*\s*\(.*\)\s*;?\s*$|;\s*$")

def deck_key(p: Path) -> str:
    """'W4 - L4 - Animations and Camera.pptx' -> 'W04_L4'"""
    m = re.match(r"W(\d+)\s*-\s*([A-Za-z]\d+)", p.stem)
    if not m:
        raise ValueError(f"cannot derive a week key from {p.name!r}")
    return f"W{int(m.group(1)):02d}_{m.group(2)}"

def deck_title(p: Path) -> str:
    parts = p.stem.split(" - ")
    return parts[-1].strip() if len(parts) >= 3 else p.stem

def _blocks_from_frame(tf) -> list[dict]:
    out = []
    for para in tf.paragraphs:
        text = "".join(r.text for r in para.runs).strip()
        if not text:
            out.append({"level": 0, "text": "", "kind": "blank"})
            continue
        kind = "code" if CODE_HINT.match(text) and len(text) < 80 else "bullet"
        out.append({"level": min(int(para.level or 0), 4), "text": text, "kind": kind})
    # drop leading/trailing blanks so pages don't open with an empty line
    while out and out[0]["kind"] == "blank": out.pop(0)
    while out and out[-1]["kind"] == "blank": out.pop()
    return out

def _table(shape) -> dict | None:
    if not shape.has_table:
        return None
    rows = [[c.text.strip() for c in r.cells] for r in shape.table.rows]
    if not rows:
        return None
    cols = max(len(r) for r in rows)
    cells = [r[i] if i < len(r) else "" for r in rows for i in range(cols)]
    return {"columns": cols, "cells": cells}

def _picture(shape, key: str, out_dir: Path) -> dict | None:
    if shape.shape_type != MSO_SHAPE_TYPE.PICTURE:
        return None
    out_dir.mkdir(parents=True, exist_ok=True)
    name = f"img_{shape.shape_id:02d}.png"
    (out_dir / name).write_bytes(shape.image.blob)
    return {"path": f"{key}/{name}",
            "w": int(shape.width  / 9525),      # EMU -> px @96dpi
            "h": int(shape.height / 9525)}

def convert(pptx_path: Path, image_root: Path | None = None) -> dict:
    prs = Presentation(str(pptx_path))
    key = deck_key(pptx_path)
    slides = []
    for i, s in enumerate(prs.slides, 1):
        title = subtitle = ""
        blocks, table, image, has_two = [], None, None, False
        for sh in s.shapes:
            if sh.has_text_frame and sh == s.shapes.title:
                title = sh.text_frame.text.strip()
            elif sh.has_text_frame:
                blocks += _blocks_from_frame(sh.text_frame)
                if len(blocks) and sh.width and sh.width < prs.slide_width // 2:
                    has_two = True
            if sh.has_table: table = _table(sh)
            if sh.has_image if hasattr(sh, "has_image") else False:
                pass
            if image_root is not None:
                pic = _picture(sh, key, image_root / key)
                if pic: image = pic
        layout = ("title" if not blocks and title else "twoColumn" if has_two else "content")
        if i == 1 or len(s.shapes) <= 2 and not blocks:
            layout = "title"
        slides.append({"index": i, "layout": layout, "title": title,
                       "subtitle": subtitle, "blocks": blocks,
                       "table": table, "image": image})
    return {"key": key, "title": deck_title(pptx_path), "subtitle": "",
            "source": pptx_path.name, "generated": datetime.now().isoformat(timespec="seconds"),
            "slides": slides}

def main(argv=None) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("deck", nargs="*", help=".pptx files (default: all under --slides-dir)")
    ap.add_argument("--slides-dir", default="/Users/gyk/Documents/Work/AY26-27/CAT105TC/Slides")
    ap.add_argument("--out-dir", default="Assets/Slides")
    args = ap.parse_args(argv)
    decks = [Path(p) for p in args.deck] or sorted(Path(args.slides_dir).glob("*.pptx"))
    out_dir = Path(args.out_dir); out_dir.mkdir(parents=True, exist_ok=True)
    for d in decks:
        data = convert(d, image_root=out_dir)
        (out_dir / f"{data['key']}.json").write_text(json.dumps(data, indent=2), encoding="utf8")
        cjk = sum(1 for s in data["slides"] if CJK.search(json.dumps(s, ensure_ascii=False)))
        print(f"{data['key']:8} slides={len(data['slides']):3} cjk={cjk:2}  <- {d.name}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
```

- [ ] **Step 4: Run the tests, expect pass**

Run: `uv run --with python-pptx --with pytest pytest tools/tests -q`
Expected: `4 passed`.

- [ ] **Step 5: Checkpoint** — note the test output in the task log.

---

## Task 2: Generate the W04 deck

**Files:** create `Assets/Slides/W04_L4.json`

- [ ] **Step 1: Generate only the W4 deck**

Run: `uv run --with python-pptx python tools/pptx_to_deck.py "/Users/gyk/Documents/Work/AY26-27/CAT105TC/Slides/W4 - L4 - Animations and Camera.pptx"`
Expected: `W04_L4   slides= 28 cjk= 0  <- W4 - L4 - Animations and Camera.pptx`

- [ ] **Step 2: Verify the generated slide count against the source**

Run:
```bash
uv run --with python-pptx python -c "
import sys, json; sys.path.insert(0,'tools')
from pathlib import Path
from pptx import Presentation
from pptx_to_deck import convert
p = Path('/Users/gyk/Documents/Work/AY26-27/CAT105TC/Slides/W4 - L4 - Animations and Camera.pptx')
src = len(Presentation(str(p)).slides); out = json.loads(Path('Assets/Slides/W04_L4.json').read_text())['slides']
print('src', src, 'out', len(out), 'OK' if src == len(out) else 'MISMATCH')
"
```
Expected: `src 28 out 28 OK`

- [ ] **Step 3: Spot-check the JSON by eye**

Run: `python3 -m json.tool Assets/Slides/W04_L4.json | head -40`
Expected: `"key": "W04_L4"`, `slides[0].layout == "title"`, non-empty `blocks` on a mid-deck slide.

- [ ] **Step 4: Commit** — `git add Assets/Slides/W04_L4.json tools && git commit -m "feat(slides): pptx->deck converter + W04 deck JSON"` (include Task 1's script if it was committed separately, which is fine).

---

## Task 3: Data classes + pure `SlideCursor` + unit tests

**Files:** create `Assets/Scripts/Slides/SlideData.cs` (stays in `Assembly-CSharp`), `Assets/Scripts/Slides/Core/SlideCursor.cs` + `Assets/Scripts/Slides/Core/Slides.Core.asmdef`, `Assets/Tests/EditMode/Slides.EditMode.asmdef`, `Assets/Tests/EditMode/SlideCursorTests.cs`

**Interfaces — produces:**

```csharp
[Serializable] public class SlideDeckData { public string key, title, subtitle, source, generated; public SlideData[] slides; }
[Serializable] public class SlideData { public int index; public string layout, title, subtitle; public SlideBlock[] blocks; public SlideTable table; public SlideImage image; }
[Serializable] public class SlideBlock { public int level; public string text, kind; }
[Serializable] public class SlideTable { public int columns; public string[] cells; }
[Serializable] public class SlideImage { public string path; public int w, h; }

public class SlideCursor {
    public SlideCursor(int[] blocksPerSlide);
    public int SlideCount { get; }
    public int SlideIndex { get; private set; }
    public int RevealedBlocks { get; private set; }
    public bool StepsExhausted { get; }                 // RevealedBlocks >= BlocksAt(SlideIndex)
    public int  BlocksAt(int slideIndex);
    public void JumpTo(int slideIndex);                 // clamp, reveal ALL blocks of the target
    public void NextSlide();                            // clamp, reveal 0   (teaching rhythm)
    public void PrevSlide();                            // clamp, reveal ALL
    public bool StepForward();                          // +1 block; exhausted -> NextSlide
    public bool StepBack();                             // -1 block; none left -> PrevSlide
}
```
Rule: **forward = step by step, backward/jump = show the whole slide.**

- [ ] **Step 1: Write the failing tests**

```csharp
// Assets/Tests/EditMode/SlideCursorTests.cs
using NUnit.Framework;
public class SlideCursorTests
{
    [Test] public void StartsOnFirstSlideWithNothingRevealed()
    { var c = new SlideCursor(new[]{2,1}); Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(0,c.RevealedBlocks); }

    [Test] public void StepForwardRevealsThenRollsOver()
    {
        var c = new SlideCursor(new[]{2,1});
        Assert.IsTrue(c.StepForward()); Assert.AreEqual(1,c.RevealedBlocks);
        Assert.IsTrue(c.StepForward()); Assert.AreEqual(2,c.RevealedBlocks);
        Assert.IsTrue(c.StepForward()); Assert.AreEqual(1,c.SlideIndex); Assert.AreEqual(0,c.RevealedBlocks);
    }

    [Test] public void StepForwardAtTheVeryEndDoesNothing()
    {
        var c = new SlideCursor(new[]{1,1});
        c.StepForward(); c.StepForward();                 // now last slide, all revealed
        Assert.AreEqual(1,c.SlideIndex); Assert.IsFalse(c.StepForward());
    }

    [Test] public void PrevSlideShowsTheWholeSlide()
    { var c = new SlideCursor(new[]{3,1}); c.NextSlide(); c.PrevSlide(); Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(3,c.RevealedBlocks); }

    [Test] public void StepBackFromAnEmptySlideLandsOnAPreviousSlideFullyRevealed()
    {
        var c = new SlideCursor(new[]{3,2});
        c.NextSlide();                                    // slide 1, nothing revealed
        Assert.IsTrue(c.StepBack());
        Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(3,c.RevealedBlocks);
    }

    [Test] public void JumpToClampsAndRevealsEverything()
    { var c = new SlideCursor(new[]{2,4,1}); c.JumpTo(99); Assert.AreEqual(2,c.SlideIndex); Assert.AreEqual(1,c.RevealedBlocks);
      c.JumpTo(-5); Assert.AreEqual(0,c.SlideIndex); Assert.AreEqual(2,c.RevealedBlocks); }

    [Test] public void ZeroBlockSlideIsImmediatelyExhausted()
    { var c = new SlideCursor(new[]{0,1}); Assert.IsTrue(c.StepsExhausted);
      Assert.IsTrue(c.StepForward()); Assert.AreEqual(1,c.SlideIndex); }
}
```

- [ ] **Step 2: Create the two assemblies so the tests can compile**

`SlideCursor` must live in its **own assembly**: a test asmdef **cannot** reference the predefined `Assembly-CSharp`. `Slides.Core` declares no engine references because `SlideCursor` is pure C# — keep it that way (no `UnityEngine` types in that file).

```jsonc
// Assets/Scripts/Slides/Core/Slides.Core.asmdef
{ "name": "Slides.Core", "references": [], "noEngineReferences": true }

// Assets/Tests/EditMode/Slides.EditMode.asmdef
// Unity 2022 form — do NOT use the old "optionalUnityReferences": ["TestAssemblies"]
{
  "name": "Slides.EditMode",
  "references": ["Slides.Core", "UnityEngine.TestRunner", "UnityEditor.TestRunner"],
  "includePlatforms": ["Editor"],
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"],
  "noEngineReferences": false
}
```

- [ ] **Step 3: Run the tests, expect failure**

Via MCP `execute`: `tools.unityMCP.run_tests({mode:"EditMode", filter:"SlideCursorTests"})` — or `manage_editor` + Test Runner.
Expected: compile error / `SlideCursor` not found.

- [ ] **Step 4: Implement `SlideData.cs` and `SlideCursor.cs`**

```csharp
// Assets/Scripts/Slides/SlideData.cs
using System;
[Serializable] public class SlideDeckData { public string key, title, subtitle, source, generated; public SlideData[] slides; }
[Serializable] public class SlideData { public int index; public string layout, title, subtitle; public SlideBlock[] blocks; public SlideTable table; public SlideImage image; }
[Serializable] public class SlideBlock { public int level; public string text, kind; }
[Serializable] public class SlideTable { public int columns; public string[] cells; }   // row-major: cells[r*columns+c]
[Serializable] public class SlideImage { public string path; public int w, h; }
```

```csharp
// Assets/Scripts/Slides/SlideCursor.cs
/// Pure slide/step state machine - no Unity types, so it is unit-testable.
public class SlideCursor
{
    private readonly int[] blocksPerSlide;
    public SlideCursor(int[] blocksPerSlide) { this.blocksPerSlide = blocksPerSlide ?? new int[0]; }
    public int SlideCount => blocksPerSlide.Length;
    public int SlideIndex { get; private set; }
    public int RevealedBlocks { get; private set; }
    public int BlocksAt(int i) => (i >= 0 && i < SlideCount) ? blocksPerSlide[i] : 0;
    public bool StepsExhausted => RevealedBlocks >= BlocksAt(SlideIndex);

    public void JumpTo(int i) { SlideIndex = Clamp(i); RevealedBlocks = BlocksAt(SlideIndex); }
    public void NextSlide()   { SlideIndex = Clamp(SlideIndex + 1); RevealedBlocks = 0; }
    public void PrevSlide()   { SlideIndex = Clamp(SlideIndex - 1); RevealedBlocks = BlocksAt(SlideIndex); }

    public bool StepForward()
    {
        if (!StepsExhausted) { RevealedBlocks++; return true; }
        if (SlideIndex < SlideCount - 1) { NextSlide(); return true; }
        return false;
    }

    public bool StepBack()
    {
        if (RevealedBlocks > 0) { RevealedBlocks--; return true; }
        if (SlideIndex > 0) { PrevSlide(); return true; }
        return false;
    }

    private int Clamp(int i) => SlideCount == 0 ? 0 : (i < 0 ? 0 : (i > SlideCount - 1 ? SlideCount - 1 : i));
}
```

- [ ] **Step 5: Run the tests, expect pass**

Via MCP: `run_tests({mode:"EditMode", filter:"SlideCursorTests"})` → Expected: `7 passed, 0 failed`.

- [ ] **Step 6: Checkpoint.**

---

## Task 4: `SlideDeckPlayer` + `SlideView` (TMP rendering)

**Files:** create `Assets/Scripts/Slides/SlideDeckPlayer.cs`, `SlideView.cs`, `Assets/Tests/EditMode/DeckLoadTests.cs`; **move** `Assets/Scripts/Slides/SlideData.cs` → `Assets/Scripts/Slides/Core/SlideData.cs`

**Interfaces — consumes:** Task 3's types. **Produces:**

```csharp
public class SlideDeckPlayer : MonoBehaviour {
    public TextAsset deckJson;
    public SlideDeckData Deck { get; }
    public SlideCursor Cursor { get; }
    public int SlideIndex { get; }  public int RevealedBlocks { get; }
    public event System.Action Changed;
    public void Reload();
    public void NextSlide(); public void PrevSlide(); public void StepForward(); public void StepBack(); public void JumpTo(int i);
}
public class SlideView : MonoBehaviour {
    public SlideDeckPlayer player;
    public TMPro.TMP_Text headerText, titleText, subtitleText, bodyText, pageText;
    public void Refresh();                 // called on player.Changed
}
```

**Body formatting rule** (one TMP string, rich text):
- `level 0` → `• text`; `level 1` → `   ◦ text`; `level 2+` → `      ▪ text`
- `kind == "code"` → `<mark=#1E2430><color=#8FD9A8>  text  </color></mark>`
- `kind == "blank"` → an empty line

- [ ] **Step 1: Move `SlideData.cs` into `Slides.Core`**

A test assembly cannot see the predefined `Assembly-CSharp`, and the deserialisation contract is the highest-risk part of the whole pipeline — a field-name typo renders blank slides. The data classes use only `System` types, so they belong in the engine-free assembly (which also makes the real deck testable).

```bash
git mv Assets/Scripts/Slides/SlideData.cs      Assets/Scripts/Slides/Core/SlideData.cs
git mv Assets/Scripts/Slides/SlideData.cs.meta Assets/Scripts/Slides/Core/SlideData.cs.meta
```

- [ ] **Step 2: Write the deck-load contract test**

It parses the **real** `Assets/Slides/W04_L4.json` through `JsonUtility` and asserts the C# schema actually matches the artefact. This is a **contract test, not TDD** — if it fails, `SlideData.cs` is wrong, not the test.

```csharp
// Assets/Tests/EditMode/DeckLoadTests.cs
using NUnit.Framework; using UnityEditor; using UnityEngine;
public class DeckLoadTests
{
    static SlideDeckData Load()
    {
        var ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Slides/W04_L4.json");
        Assert.IsNotNull(ta, "Assets/Slides/W04_L4.json is missing");
        var deck = JsonUtility.FromJson<SlideDeckData>(ta.text);
        Assert.IsNotNull(deck, "the deck JSON did not deserialise");
        return deck;
    }

    [Test] public void ParsesTheRealW04Deck()
    {
        var d = Load();
        Assert.AreEqual("W04_L4", d.key);
        Assert.AreEqual("Animations and Camera", d.title);
        Assert.AreEqual("Lecture 4", d.subtitle);
        Assert.AreEqual(28, d.slides.Length);
    }

    [Test] public void SlideIndexesAreOneBasedAndContiguous()
    {
        var d = Load();
        for (int i = 0; i < d.slides.Length; i++) Assert.AreEqual(i + 1, d.slides[i].index);
    }

    [Test] public void TitleSlidesCarryTheSubtitleAndNoBlocks()
    {
        var d = Load();
        foreach (var s in new[] { d.slides[0], d.slides[27] })
        {
            Assert.AreEqual("title", s.layout);
            Assert.AreEqual("Lecture 4", s.subtitle);
            Assert.AreEqual(0, s.blocks.Length);
        }
    }

    [Test] public void CodeBlocksSurviveDeserialisation()
    {
        var d = Load();
        int code = 0;
        foreach (var s in d.slides) foreach (var b in s.blocks) if (b.kind == "code") code++;
        Assert.AreEqual(4, code, "expected the 4 animator.* code blocks");
    }
}
```

- [ ] **Step 3: Implement `SlideDeckPlayer`**

```csharp
using System; using UnityEngine;
public class SlideDeckPlayer : MonoBehaviour
{
    public TextAsset deckJson;
    public SlideDeckData Deck { get; private set; }
    public SlideCursor Cursor { get; private set; }
    public int SlideIndex => Cursor?.SlideIndex ?? 0;
    public int RevealedBlocks => Cursor?.RevealedBlocks ?? 0;
    public event Action Changed;

    void Awake() { Reload(); }
    public void Reload()
    {
        if (deckJson == null) { Debug.LogError("[Slides] no deck JSON assigned"); return; }
        Deck = JsonUtility.FromJson<SlideDeckData>(deckJson.text);
        if (Deck == null || Deck.slides == null) { Debug.LogError("[Slides] deck JSON did not parse"); return; }
        var counts = new int[Deck.slides.Length];
        for (int i = 0; i < counts.Length; i++) counts[i] = Deck.slides[i].blocks?.Length ?? 0;
        Cursor = new SlideCursor(counts);
        Changed?.Invoke();
    }
    public void NextSlide()   { if (Cursor == null) return; Cursor.NextSlide();   Changed?.Invoke(); }
    public void PrevSlide()   { if (Cursor == null) return; Cursor.PrevSlide();   Changed?.Invoke(); }
    public void JumpTo(int i) { if (Cursor == null) return; Cursor.JumpTo(i);     Changed?.Invoke(); }
    public void StepForward(){ if (Cursor == null) return; if (Cursor.StepForward()) Changed?.Invoke(); }
    public void StepBack()   { if (Cursor == null) return; if (Cursor.StepBack())    Changed?.Invoke(); }
}
```

- [ ] **Step 4: Implement `SlideView`**

```csharp
using System.Text; using TMPro; using UnityEngine;
public class SlideView : MonoBehaviour
{
    public SlideDeckPlayer player;
    public TMP_Text headerText, titleText, subtitleText, bodyText, pageText;
    public string header = "";

    void OnEnable()  { if (player != null) player.Changed += Refresh; Refresh(); }
    void OnDisable() { if (player != null) player.Changed -= Refresh; }

    public void Refresh()
    {
        if (player == null || player.Deck == null || player.Cursor == null) return;
        int i = player.SlideIndex;
        var s = player.Deck.slides[i];
        if (headerText  != null) headerText.text  = header;
        if (titleText   != null) titleText.text   = s.title;
        if (subtitleText!= null) subtitleText.text= s.subtitle;
        if (pageText    != null) pageText.text    = (i + 1) + " / " + player.Cursor.SlideCount;
        if (bodyText    != null) bodyText.text    = BuildBody(s, player.RevealedBlocks);
    }

    static string BuildBody(SlideData s, int revealed)
    {
        if (s.blocks == null || s.blocks.Length == 0)
            return s.table != null ? TableToText(s.table) : "";
        var sb = new StringBuilder();
        int n = Mathf.Clamp(revealed, 0, s.blocks.Length);
        for (int k = 0; k < n; k++)
        {
            var b = s.blocks[k];
            if (b.kind == "blank") { sb.Append('\n'); continue; }
            string indent = b.level <= 0 ? "\u2022 " : b.level == 1 ? "     \u25E6 " : "          \u25AA ";
            if (b.kind == "code") sb.Append("<mark=#1E2430><color=#8FD9A8>  ").Append(b.text).Append("  </color></mark>");
            else sb.Append(indent).Append(b.text);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    static string TableToText(SlideTable t)
    {
        if (t == null || t.cells == null || t.columns <= 0) return "";
        var sb = new StringBuilder();
        for (int r = 0; r * t.columns < t.cells.Length; r++)
        {
            for (int c = 0; c < t.columns; c++)
            { int k = r * t.columns + c; sb.Append(k < t.cells.Length ? t.cells[k] : ""); if (c < t.columns - 1) sb.Append("   |   "); }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}
```

- [ ] **Step 5: Verify it compiles and the EditMode tests pass**

Via MCP: `refresh_unity({mode:"force",compile:"request",wait_for_ready:true})`, then `read_console({types:["error"],count:40,format:"plain"})` → 0 errors; then run the EditMode suite (`SlideCursorTests` + `DeckLoadTests`) → **11 passed, 0 failed**.

- [ ] **Step 6: Commit** — `git add -A && git commit -m "feat(slides): deck player + TMP slide view + deck-load contract test"`

---

## Task 5: TMP bootstrap + slide template + `SlideDeckBuilder` → W04Slides (slides only)

**Files:** create `Assets/Editor/Slides/TmpBootstrap.cs`, `Assets/Editor/Slides/SlideDeckBuilder.cs`

**Interfaces — consumes:** `SlideDeckPlayer`, `SlideView`, `SlideDeckData`.
**Produces:** `SlideDeckBuilder.BuildDeckScene(string deckJsonPath, string scenePath, string header, BuildOptions opts)`; menu `CAT105TC ▸ Slides ▸ Build W04Slides`.

**Layout constants (1920×1080, all `anchoredPosition` from the stated pivot):**

| Element | anchor / pivot | pos | size | font |
|---|---|---|---|---|
| background | full stretch | — | 1920×1080 | colour `#0E1420` |
| header bar | (0,1)/(0,1) | (0,0) | 1920×64 | bg `#101828` |
| headerText | (0,1)/(0,1) | (48,−14) | 1400×36 | 26, accent `#6FA8FF`, left |
| titleText | (0,1)/(0,1) | (120,−150) | 1680×130 | 64, white, left, autosize off |
| subtitleText | (0,1)/(0,1) | (120,−290) | 1680×50 | 30, `#8FA3BF`, left |
| bodyText | (0,1)/(0,1) | (120,−360) | 1680×600 | 40 → autosize 22..40, `#E6ECF5`, left, lineSpacing 1.15 |
| pageText | (1,0)/(1,0) | (−170,40) | 140×40 | 28, right |
| prev/next | (1,0)/(1,0) | (−40/…, 32) | 110×56 | buttons |
| jump panel | centre | — | 1400×820 | hidden by default |

- [ ] **Step 1: Implement `TmpBootstrap` (idempotent essentials import)**

```csharp
using System.IO; using UnityEditor; using UnityEngine;
public static class TmpBootstrap
{
    public static void Ensure()
    {
        if (Directory.Exists("Assets/TextMesh Pro")) return;              // already imported
        TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);   // interactive:false
        AssetDatabase.Refresh();
        Debug.Log("[Slides] imported TMP Essential Resources.");
    }
}
```

- [ ] **Step 2: Implement `SlideDeckBuilder`** — a `LabKit`-style static builder that:
  1. calls `TmpBootstrap.Ensure()`;
  2. reads the JSON from `Assets/Slides/`, wraps it in a `TextAsset` (the `.json` is already a TextAsset) and logs an actionable error if missing;
  3. `EditorSceneManager.NewScene(EmptyScene, Single)`, adds a camera (+ `AudioListener`), a light, and the 1920×1080 canvas hierarchy per the table above;
  4. adds `SlideDeckPlayer` (with `deckJson` wired), `SlideView` (all TMP refs wired), `SlideInput`, `SlideNavigator`, `SlidePresenter`;
  5. saves to `scenePath` and adds it to Build Settings.
  Menu:
  ```csharp
  [MenuItem("CAT105TC/Slides/Build W04Slides")] public static void BuildW04Slides()
      => BuildDeckScene("Assets/Slides/W04_L4.json", "Assets/Scenes/W04Slides.unity", "CAT105TC · Week 04 · Animations and 2D Art", default);
  ```
  (Keep `BuildDeckScene` generic — a `[MenuItem]` per deck is added when the other weeks are built.)
  A `Button(label, parent, pos, size, onClick)` and `Panel(parent, name, pos, size, color)` helper live in this file.
  Font asset: `TMP_Settings.defaultFontAsset` (set by the essentials import); if null, log an error and abort.

- [ ] **Step 3: Build and verify the scene exists**

Via MCP: `execute_menu_item("CAT105TC/Slides/Build W04Slides")`, then `read_console({types:["error"]})` → 0 errors; `manage_scene({action:"load", path:"Assets/Scenes/W04Slides.unity"})` → loads.

- [ ] **Step 4: Verify rendering in play mode**

Via MCP: `manage_editor({action:"play"})`; then `manage_camera({action:"screenshot", screenshot_file_name:"slides_01.png"})`; read the PNG and confirm slide 1 renders: header, title, body with the first bullet only (逐条), page "1 / 28".

- [ ] **Step 5: Checkpoint.**

---

## Task 6: Split `W04LabBuilder` and embed the game

**Files:** modify `Assets/Editor/W04_AnimationCamera/W04LabBuilder.cs`

**Interfaces — produces:** `public static void BuildGameplay(Transform parent, out PlayerController2D player, out Camera cam)` — builds `Environment`/`Gameplay`/`Annotations` + camera + follow, **no HUD**. `Build()` becomes `BuildGameplay(...)` + `LabKit.BuildHud(...)` + the extra refs, so `W4Lab` is unchanged in behaviour.

- [ ] **Step 1: Extract `BuildGameplay`** — move everything from the ground platforms through the camera and labels into it; keep `LabKit.SetupSharedAssets()` and `CharacterRig.Build()` at the top of `Build()`, and pass a `Transform` parent for the gameplay root.
- [ ] **Step 2: Re-run `CAT105TC ▸ W04 Animation & Camera ▸ Build W4 Lab`**, then play it and confirm with MCP that the player moves, the HUD shows, and `read_console` reports 0 errors — i.e. the refactor did not change `W4Lab`.
- [ ] **Step 3: Call `BuildGameplay` from `SlideDeckBuilder`** for the W04Slides scene (behind the slides canvas), and re-run `CAT105TC ▸ Slides ▸ Build W04Slides`.
- [ ] **Step 4: Checkpoint.**

---

## Task 7: `SlidePresenter` + `SlideInput` + the always-on Chrome button

**Files:** create `Assets/Scripts/Slides/SlidePresenter.cs`, `SlideInput.cs`; extend `SlideDeckBuilder` with the Chrome canvas.

**Interfaces — produces:**

```csharp
public class SlidePresenter : MonoBehaviour {
    public GameObject slidesRoot;        // the whole deck canvas
    public TMP_Text toggleLabel;
    public bool DeckVisible { get; }
    public void SetDeckVisible(bool visible);
    public void Toggle();
}
```

- [ ] **Step 1: Implement `SlidePresenter`**

```csharp
using TMPro; using UnityEngine;
public class SlidePresenter : MonoBehaviour
{
    public GameObject slidesRoot;       // deck canvas root
    public GameObject gameRoot;         // optional: gameplay root
    public TMP_Text toggleLabel;
    public string hideLabel = "隐藏幻灯片  (Tab)", showLabel = "显示幻灯片  (Tab)";
    public bool DeckVisible { get; private set; } = true;

    void Start() => SetDeckVisible(true);

    public void Toggle() => SetDeckVisible(!DeckVisible);

    public void SetDeckVisible(bool visible)
    {
        DeckVisible = visible;
        if (slidesRoot != null) slidesRoot.SetActive(visible);
        Time.timeScale = visible ? 0f : 1f;          // freeze the game under the deck
        if (toggleLabel != null) toggleLabel.text = visible ? hideLabel : showLabel;
    }
}
```

- [ ] **Step 2: Implement `SlideInput`**

```csharp
using UnityEngine;
/// Maps keys to slide actions while the deck is visible; inert while the game is showing.
public class SlideInput : MonoBehaviour
{
    public SlideDeckPlayer player; public SlidePresenter presenter; public SlideNavigator navigator;
    void Update()
    {
        if (presenter == null || player == null) return;
        if (Input.GetKeyDown(KeyCode.Tab)) { presenter.Toggle(); return; }
        if (!presenter.DeckVisible) return;
        if (navigator != null && Input.GetKeyDown(KeyCode.G)) { navigator.ToggleJumpPanel(); return; }
        if (navigator != null && navigator.JumpPanelOpen)   { if (Input.GetKeyDown(KeyCode.Escape)) navigator.ToggleJumpPanel(); return; }
        if (Input.GetKeyDown(KeyCode.RightArrow))   player.NextSlide();
        if (Input.GetKeyDown(KeyCode.LeftArrow))    player.PrevSlide();
        if (Input.GetKeyDown(KeyCode.Backspace))    player.StepBack();
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) player.StepForward();
    }
}
```

- [ ] **Step 3: Add the Chrome canvas to `SlideDeckBuilder`** — a second canvas with `sortingOrder = 10` holding the top-right toggle button (180×52) whose `onClick` calls `presenter.Toggle()`.
- [ ] **Step 4: Verify the toggle in play mode (this is the user's core ask)**

Via MCP play mode, assert:
```csharp
var p = GameObject.Find("Slides Canvas").GetComponent<SlidePresenter>();  // or wired ref
// 1) deck visible -> frozen
p.SetDeckVisible(true);  check Time.timeScale == 0 && slidesRoot.activeSelf
// 2) hidden -> game playable, gameplay root present
p.SetDeckVisible(false); check Time.timeScale == 1 && !slidesRoot.activeSelf
// 3) back -> same slide, same revealed count
```
Then screenshot with the deck hidden to prove the game is visible, and again with it shown.

- [ ] **Step 5: Checkpoint.**

---

## Task 8: `SlideNavigator` — prev/next buttons + page label + jump grid

**Files:** create `Assets/Scripts/Slides/SlideNavigator.cs`

**Interfaces — consumes:** `SlideDeckPlayer`, `SlidePresenter`. **Produces:** button callbacks + `ToggleJumpPanel()`, `bool JumpPanelOpen`.

- [ ] **Step 1: Implement `SlideNavigator`** — `NextClicked()`/`PrevClicked()` forward to the player; `ToggleJumpPanel()` shows/hides a centred 1400×820 panel; the panel is filled at `Awake` with a **grid of numbered buttons** (8 columns, 36 px cells, up to 64 slides — every current deck fits without scrolling) plus a header line `跳转 · 共 N 页 · 当前 12`; clicking button `k` calls `player.JumpTo(k)` and closes the panel.
- [ ] **Step 2: Verify in play mode** — assert `player.JumpTo(17)` then `SlideIndex == 16` and `RevealedBlocks == BlocksAt(16)`; click-less check via direct calls; screenshot the open jump panel.
- [ ] **Step 3: Checkpoint.**

---

## Task 9: Docs, README, end-to-end verification

**Files:** create `Assets/Scripts/Slides/README.md`; modify `Assets/Scripts/README.md`, `Docs/SlidesSystem-Design.md` (§8 note: jump panel is a numbered grid).

- [ ] **Step 1: Write `Assets/Scripts/Slides/README.md`** — how to add a deck: run `uv run --with python-pptx python tools/pptx_to_deck.py`, then menu `CAT105TC ▸ Slides ▸ Build …`; the key map; the folder convention.
- [ ] **Step 2: Add the `Slides/` areas to `Assets/Scripts/README.md`.**
- [ ] **Step 3: Full end-to-end check** — rebuild `W04Slides` and `W4Lab`, `read_console` → 0 errors; play `W04Slides`: slide 1 → `Space`×4 (逐条) → `→`×3 → `G` → jump to 20 → `Tab` (game) → move the player → `Tab` (deck returns on slide 20).
- [ ] **Step 4: Screenshots** — slide 1, a mid-deck slide with code, the jump panel open, the deck hidden showing the game. Save to `Assets/Screenshots/`.
- [ ] **Step 5: Rebuild + re-verify `W4Lab`** (unchanged) and **`W3Lab`** (untouched).
- [ ] **Step 6: Final report** — slide counts per deck, 0 errors, screenshots attached, known limitations.

---

## Self-review notes

- **Spec coverage:** pipeline §4.1→T1/T2; schema §4.2→T1/T3; runtime §4.3→T4/T7/T8; input map §4.3→T7/T8; toggle behaviour §4.3→T7; editor tooling §4.4→T5; refactor §4.5→T6; TMP §4.6→T5; failure handling §5→T5 (missing JSON, TMP) + T4 (parse error); verification §6→T9.
- **Deviations from the spec, deliberate (both recorded in T9):** (1) the jump panel is a **numbered grid**, not a title list — every deck (≤43 slides) fits without a scroll view, which removes the fiddliest part of uGUI; (2) this project is not a git repo, so "Commit" steps are checkpoints.
- **Type consistency checked:** `SlideCursor` method names are identical in T3's interface block, its tests and its implementation; `SlideDeckPlayer.Cursor/Deck/SlideIndex/RevealedBlocks` are used with the same names in T4/T7/T8; `SlideDeckBuilder.BuildDeckScene`/`BuildGameplay` are consistent between T5/T6.
