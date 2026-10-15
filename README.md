# CAT105TC — Unity course demos

Teaching material for **CAT105TC**, built as runnable Unity scenes rather than slide decks:
every lecture topic is something you can open and press **Play** on.

Unity **2022.3.62f3c1** · Universal Render Pipeline · 2D

## What is here

The project grows one week at a time, and a week can produce **three scenes with different jobs**:

| Scene | What it is | Audience |
|---|---|---|
| `Assets/Scenes/WnnLab.unity` — **Lab** | the lecture's worked example at a size a student can reproduce during the session | students, following along |
| `Assets/Scenes/WnnPlayground.unity` — **Playground** | the combined result: the week's topics laid out as labelled stations and runnable | students, hands-on |
| `Assets/Scenes/WnnSlides.unity` — **Slides** | the lecture deck at 1920×1080, with a **live demo of that slide's own content** behind each slide | instructor, presenting |

All of them are **generated from code and never hand-edited** — the builders rebuild them from scratch.
Change the builder (or the source data it reads), then re-run the menu item.

The scope rule that shapes everything: **a lab contains only what has been taught by that week.**
`W03Playground`'s player has no `Animator`, because animation is W4 material; the W4 builder calls the W3
builder and adds the animator on top. That is also the story the W4 lecture tells.

### Built so far

| Week | Topic | Lab | Slides |
|---|---|---|---|
| W2 | C# fundamentals — variables and conditionals | `W02Playground` (the early 3D scene) | — |
| W3 | Physics in Unity | `W03Playground` | — |
| W4 | Animations and 2D art | `W04Playground` | `W04Slides` (28 slides, 44 demos) |
| W5 | Camera and UI | `W05Lab` (the worked example) · `W05Playground` (the full one) | `W05Slides` (34 slides, 27 demos) |

## Running it

1. Open the project in Unity **2022.3.62f3c1** (or a later 2022.3 LTS).
2. Open a scene from `Assets/Scenes/` — all of them are already in Build Settings — and press **Play**.

### Slides keys

| Key | Deck visible | Deck hidden |
|---|---|---|
| `Tab` (or the top-right button) | hide the slides → the live demo | show the slides again |
| `→` / `←` | next / previous slide | — |
| `Space` | next slide | jump (the demo's game) |
| `Backspace` | previous slide | — |
| `,` / `.` | — | previous / next demo on this slide |
| `0`–`9` | — | jump straight to that demo |
| `G` | jump-to-slide grid | — |

While the slides are up the demo roots are **deactivated** and `Time.timeScale` is `0`. The
deactivation is what actually stops the game reacting to input — `timeScale = 0` does **not** stop
`Update()`.

## Layout

```
Assets/
  Scenes/            W02Playground · W03Playground · W04Playground · W04Slides
                     W05Playground · W05Slides · W05Lab (the W5 worked example)
                     (generated; see Scenes/README.md)
  Scripts/
    Common/          infrastructure every week uses — LabHud
    Slides/          the presentation system + the demo families (see Slides/README.md)
    W02_VariablesConditionals/
    W03_Physics2D/   the 2D controller, follow camera, triggers, raycasts, one-way platforms
    W04_AnimationCamera/  pickups, moving platforms, property animation, W4PlayerController
    W05_UI/          Health, the health bar, the dialogue zone, the bag, the pause panel
  Editor/
    Common/          LabKit (scene/HUD/asset helpers), LabMenu, the art importer
    W03_Physics2D/   W03PlaygroundBuilder
    W04_AnimationCamera/  W04PlaygroundBuilder, CharacterRig
    W05_UI/          W05PlaygroundBuilder, W05LabBuilder (the starter)
    Slides/          SlideDeckBuilder, SlideDemoBuilder, DemoKit, TmpBootstrap
  Art/               Kenney "Toon Characters" (CC0) + the generated shapes
  Animations/        the AnimationClips and AnimatorControllers
  Slides/            the generated decks (<key>.json) and per-slide demo maps
  Tests/             EditMode + PlayMode
```

`Assets/Scripts/README.md` is the **project conventions** document (the rules, the scene types, the
per-week layout). `Assets/Scenes/README.md` points at the scene-type definitions.

## Regenerating the generated things

```bash
# inside Unity:
#   CAT105TC ▸ Slides ▸ Build W04Slides      (deck JSON -> the slides scene)
#   CAT105TC ▸ Slides ▸ Build W05Slides
#   CAT105TC ▸ W03 Physics2D ▸ Build W03 Playground
#   CAT105TC ▸ W04 Animation & Camera ▸ Build W04 Playground (Platformer)
#   CAT105TC ▸ W05 UI ▸ Build W05 Playground (UI)      (the finished UI scene)
#   CAT105TC ▸ W05 UI ▸ Build W5 Lab                  (the demo the students reproduce)
#   CAT105TC ▸ W05 UI ▸ Build W5 Lab
#   CAT105TC ▸ Common ▸ Rebuild shared assets
```

**Never edit a generated scene by hand** — it is rebuilt from scratch and your edits are lost.

## Tests

`Window ▸ General ▸ Test Runner`

| Suite | Covers |
|---|---|
| EditMode (18) | the cursor, both decks' deserialisation contract, and the demo maps |
| PlayMode (4) | the presenter ↔ demo contract, and every demo key in the W5 map activating in the built scene |

## Docs

| File | What |
|---|---|
| `Assets/Scripts/README.md` | **project conventions** — the rules and the two scene types |
| `Assets/Scripts/Slides/README.md` | the slides system: keys, regeneration, tests, how to add a week |

## Art

The character art is Kenney's **Toon Characters** pack, released **CC0** — see
`Assets/Art/KenneyToonCharacters/License.txt`. It is committed here so the scenes open without an
extra download.
