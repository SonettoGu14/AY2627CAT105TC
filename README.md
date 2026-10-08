# CAT105TC — Unity course demos

Teaching material for **CAT105TC**, built as runnable Unity scenes rather than slide decks:
every lecture topic is something you can open and press **Play** on.

Unity **2022.3.62f3c1** · Universal Render Pipeline · 2D

## What is here

The project grows one week at a time, and a week can produce **two scenes with different jobs**:

| Scene | What it is | Audience |
|---|---|---|
| `Assets/Scenes/WnnLab.unity` — **Lab Playground** | the week's topics laid out as labelled stations; what students build in the session | students, hands-on |
| `Assets/Scenes/WnnSlides.unity` — **Slides** | the lecture deck at 1920×1080, with a **live demo of that slide's own content** behind each slide | instructor, presenting |

Both are **generated from code and never hand-edited** — the builders rebuild them from scratch.
Change the builder (or the source data it reads), then re-run the menu item.

The scope rule that shapes everything: **a lab contains only what has been taught by that week.**
`W3Lab`'s player has no `Animator`, because animation is W4 material; the W4 builder calls the W3
builder and adds the animator on top. That is also the story the W4 lecture tells.

### Built so far

| Week | Topic | Lab | Slides |
|---|---|---|---|
| W2 | C# fundamentals — variables and conditionals | `W2Lab` (the early 3D scene) | — |
| W3 | Physics in Unity | `W3Lab` | — |
| W4 | Animations and 2D art | `W4Lab` | `W04Slides` (28 slides, 44 demos) |

## Running it

1. Open the project in Unity **2022.3.62f3c1** (or a later 2022.3 LTS).
2. Open a scene from `Assets/Scenes/` — all four are already in Build Settings — and press **Play**.

### Slides keys

| Key | Deck visible | Deck hidden |
|---|---|---|
| `Tab` (or the top-right button) | hide the slides → the live demo | show the slides again |
| `→` / `←` | next / previous slide | — |
| `Space` | next bullet, then next slide | jump (the demo's game) |
| `Backspace` | previous bullet | — |
| `,` / `.` | — | previous / next demo on this slide |
| `0`–`9` | — | jump straight to that demo |
| `G` | jump-to-slide grid | — |

While the slides are up the demo roots are **deactivated** and `Time.timeScale` is `0`. The
deactivation is what actually stops the game reacting to input — `timeScale = 0` does **not** stop
`Update()`.

## Layout

```
Assets/
  Scenes/            W2Lab · W3Lab · W4Lab · W04Slides        (generated; see Scenes/README.md)
  Scripts/
    Common/          infrastructure every week uses — LabHud
    Slides/          the presentation system + the demo families (see Slides/README.md)
    W02_VariablesConditionals/
    W03_Physics2D/   the 2D controller, follow camera, triggers, raycasts, one-way platforms
    W04_AnimationCamera/  pickups, moving platforms, property animation, W4PlayerController
  Editor/
    Common/          LabKit (scene/HUD/asset helpers), LabMenu, the art importer
    W03_Physics2D/   W03LabBuilder
    W04_AnimationCamera/  W04LabBuilder, CharacterRig
    Slides/          SlideDeckBuilder, SlideDemoBuilder, DemoKit, TmpBootstrap
  Art/               Kenney "Toon Characters" (CC0) + the generated shapes
  Animations/        the AnimationClips and AnimatorControllers
  Slides/            the generated decks (<key>.json) and per-slide demo maps
  Tests/             EditMode + PlayMode
Docs/                the slides design, plan, and demo checklist
tools/               pptx_to_deck.py — the deck converter, with tests
```

`Assets/Scripts/README.md` is the **project conventions** document (the rules, the scene types, the
per-week layout). `Assets/Scenes/README.md` points at the scene-type definitions.

## Regenerating the generated things

```bash
# a deck:  .pptx  ->  Assets/Slides/<key>.json
uv run --with python-pptx python tools/pptx_to_deck.py "<deck.pptx>"

# then, inside Unity:
#   CAT105TC ▸ Slides ▸ Build W04Slides      (deck JSON -> the slides scene)
#   CAT105TC ▸ W03 Physics2D ▸ Build W3 Lab
#   CAT105TC ▸ W04 Animation & Camera ▸ Build W4 Lab (Platformer)
#   CAT105TC ▸ Common ▸ Rebuild shared assets
```

**Never edit a generated scene by hand** — it is rebuilt from scratch and your edits are lost.

## Tests

`Window ▸ General ▸ Test Runner`

| Suite | Covers |
|---|---|
| EditMode (14) | the slides state machine, and the real deck's deserialisation contract |
| PlayMode (3) | the presenter ↔ demo contract: hiding the deck activates that slide's demo and unfreezes, returning keeps the slide and step |

## Docs

| File | What |
|---|---|
| `Assets/Scripts/README.md` | **project conventions** — the rules and the two scene types |
| `Assets/Scripts/Slides/README.md` | the slides system: keys, regeneration, tests, how to add a week |
| `Docs/SlidesSystem-Design.md` | the slides system's design |
| `Docs/SlidesDemos.md` | the 44-demo checklist and the slide → demo mapping |
| `Docs/SlidesSystem-Plan.md` | the implementation plan, with the verification notes |

## Art

The character art is Kenney's **Toon Characters** pack, released **CC0** — see
`Assets/Art/KenneyToonCharacters/License.txt`. It is committed here so the scenes open without an
extra download.

## Local tooling

`Packages/manifest.json` intentionally does **not** list the editor tooling this project was
developed with (a local Unity MCP bridge). Keep such dependencies out of the published manifest —
they are git dependencies, so anyone cloning would otherwise have to fetch them before Unity can
resolve the project.
