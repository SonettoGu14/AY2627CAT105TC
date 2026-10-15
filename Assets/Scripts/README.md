# CAT105TC — project conventions

The project accumulates one week at a time, so **code is split by week**, and only genuinely
week-independent infrastructure is shared.

## Scene types

A week can produce **three scenes**, and they have different jobs. All of them are normal scenes in
`Assets/Scenes/`, all go into Build Settings, and all are **generated from code** — never hand-edited
(the builders rebuild them from scratch).

The words are literal: a **Playground** is the big combined scene, a **Lab** is where a lab session
*starts*. (Before W5 the big scene was called `WnnLab`; it is a Playground.)

### 1. Lab — `Assets/Scenes/WnnLab.unity` (the STARTER)

**What it is.** The scene a lab session **begins from**: the character already works and almost
nothing else. The students are handed it and spend the session adding the week's material on top.
Only W5 has one so far — `W05Lab` gives them the W4 character, a floor, two platforms and a follow
camera, and nothing else: no coins, no goal, and **no UI at all, not even the lab HUD**. The UI layer
is what they build; `W05Playground` is that same scene with it finished. A lab is the **starting point**, a playground is the **result**.

**Built by.** `Assets/Editor/Wnn_Topic/WnnLabBuilder.cs` →
`CAT105TC ▸ Wnn … ▸ Build Wnn Lab (Starter)`.

**Existing.** `W05Lab`.

### 2. Playground — `Assets/Scenes/WnnPlayground.unity`

**What it is.** The week's hands-on scene: what the students build in the lab and what they run
during the session. A small 2D scene with the week's topics laid out as labelled stations, the
shared `LabHud`, and (in review weeks) a checklist.

**Built by.** `Assets/Editor/Wnn_Topic/WnnPlaygroundBuilder.cs` →
`CAT105TC ▸ Wnn … ▸ Build Wnn Playground`.

**Scope — the hard rule.** A lab contains **only what has been taught by that week**. That is why
`W03Playground`'s player has no `Animator` (animation is W4 material), and why `W04Playground`'s builder calls
`W03PlaygroundBuilder.CreatePlayer(...)` and then adds the Animator itself. A later week may reuse an
earlier week's components; an earlier week must never reach forward.

**Existing.** `W02Playground` (hand-made; the 3D leftovers), `W03Playground`, `W04Playground`, `W05Playground`.

### 3. Slides — `Assets/Scenes/WnnSlides.unity`

**What it is.** The lecture delivery scene: the deck at 1920×1080, and behind **each slide** a live
demonstration of *that slide's own content*. It exists so a lecture can be given from inside Unity,
with no window switching, and so every claim on a slide can be shown running.

**Built by.** `Assets/Editor/Slides/SlideDeckBuilder.cs` (+ `SlideDemoBuilder.cs`), from
`Assets/Slides/<deck>.json` and the per-slide demo map `Assets/Slides/<deck>.demos.json` →
`CAT105TC ▸ Slides ▸ Build WnnSlides` (W04Slides, W05Slides).

**How it is used.** `Tab` (or the top-right button) hides the slides and reveals the current slide's
demo(s) together with a readout panel (title / live values / the API call / key hint / "demo 2 / 3").
`,` and `.` move between a slide's demos, a digit jumps straight to one, and `Tab` returns to the
same slide. While the slides are up the demo roots are **deactivated** —
which is also what makes the game's input inert, because `Time.timeScale = 0` does **not** stop
`Update()`.

**Scope.**
* A demo teaches **exactly one thing** — split by knowledge point. Never a composite demo: if a
  slide teaches four things, it gets four demos.
* The Slides scene is **presentation, not a replacement for the lab.** The lab's own scene may
  appear as **one** demo on the "put it together" slide (`platformer` on slide 22 is exactly that).
  That is the only place the lab belongs in a deck.
* Slides with nothing runnable — cover, agenda, an editor-UI walkthrough, "The End" — have **no**
  demo. That is expected, not an omission.
* Content comes from the `.pptx`. To change words, edit the deck or the generated JSON and rebuild;
  never edit the scene.

**Existing.** `W04Slides`, `W05Slides`.

### The relationship

| | Lab (starter) | Playground | Slides |
|---|---|---|---|
| audience | students, at the start | students, hands-on | instructor, presenting |
| content | the character, working | the week's topics combined | one demo per knowledge point |
| scene | the beginning | the result | the deck, with a demo behind each slide |
| scope rule | only what has been taught | only what has been taught | only what that slide teaches |
| shared | `LabKit`, `LabHud`, the art, the character rig | the same, plus every earlier week | the whole `Slides/` runtime + demo families |

```
Assets/Scripts/
  Common/                     infrastructure used by every week - NOT lecture material
    LabHud.cs                 status bar, live input visualiser, camera debug panel, checklist

  Slides/                     the presentation system - shared across all weeks, NOT lecture material
    Core/                       engine-free: SlideData (JSON shapes) + SlideCursor (state machine)
    Slides.Runtime.asmdef       the runtime assembly, so the PlayMode tests can see these scripts
    SlideDeckPlayer.cs          parses a deck, owns the cursor, raises Changed
    SlideView.cs                renders one slide into five TMP fields
    SlidePresenter.cs           shows/hides the deck; owns Time.timeScale; picks the slide's demo
    SlideInput.cs               the key map (Tab / arrows / Space / G, and , . for demos)
    SlideNavigator.cs           prev/next buttons, page label, jump-to-slide grid
    DemoBase.cs                 one demo = one knowledge point + its live readout
    DemoStage.cs                owns every demo's stage, its camera, and which one is live
    DemoReadout.cs              the blackboard panel next to a demo
    SlideDemoMap.cs             which demos belong to which slide (from <deck>.demos.json)
    Demos/                      one class per demo family: SpriteRenderer, Sprite, FieldModifier,
                                Raycast, Controller, Animation, Animator, Camera, Platformer
    README.md                   keys, regeneration, tests, and how to add a week

  W02_VariablesConditionals/  Lecture 2  "C# Fundamentals (1): Variables and Conditionals"
    VariablesDemo.cs            the four core types (string / int / float / bool),
                                the Debug.Log / LogWarning / LogError levels
    HealthPointsDemo.cs         a minimal MonoBehaviour with one field
    PlayerController.cs         early 3D attempt: Input.GetKey + if -> transform.Translate
    PlayerPhysicsController.cs  early 3D attempt: Rigidbody + AddForce + OnCollisionEnter
    FollowPlayer.cs             early 3D attempt: camera follow (offset recorded in Start)
                                -- these three are the 3D leftovers, attached to W02Playground.unity;
                                   the course went 2D from W3 on

  W03_Physics2D/              Lecture 3  "Physics in Unity"
    PlayerController2D.cs       the 2D controller: rb.velocity, AddForce jump, Raycast ground check
    CameraFollow2D.cs           orthographic follow camera. The W3 playground is 54 units
                                wide, so it is needed from W3 on - W4's lecture then opens
                                this file to explain the offset + LateUpdate, and to show
                                the smoothed version.
    CollisionColorDemo2D.cs     OnCollisionEnter2D colour change / specific-object match
    TriggerZone2D.cs            OnTriggerEnter2D / OnTriggerExit2D
    RaycastDemo2D.cs            Physics2D.Raycast + LayerMask, drawn with a LineRenderer
    OneWayPlatform2D.cs         PlatformEffector2D
    RigidbodyModeDemo2D.cs      Dynamic / Kinematic / Static
    StaticCounter2D.cs          static fields
    KillZone2D.cs               trigger + tag + respawn
    ConceptZone2D.cs            checklist trigger
    PlaygroundReview.cs         the checklist driver
    SlideSnippets_W03.cs        the literal slide code, in one place to open live

  W04_AnimationCamera/        Lecture 4  "Animations and 2D Art"
    Collectible2D.cs            trigger pickup + Instantiate & Destroy
    Burst2D.cs                  the one-shot effect spawned on pickup
    GoalZone2D.cs               end-of-level trigger
    MovingPlatform2D.cs         kinematic platform moved by code
    PropertyAnimationDemo2D.cs  animator.Play(); position/scale/colour animation
    W4PlayerController.cs       the student handout: a self-contained standard 2D controller
                                (W3 physics + W4 animation, bilingual comments). Not used by a
                                lab - it is the file students are given, so it references nothing
                                from this project. Drives the Animator params `speed` / `isJumping`.
    SlideSnippets_W04.cs        the literal slide code

  W05_UI/                     Lecture 5  "Camera and UI"
    Health.cs                   hit points; the health bar is a view of it
    HealthBarUI.cs              a Slider + a Text driven by Health, eased
    DamageZone2D.cs             a hazard trigger that calls Health.Take
    DialogueZone2D.cs           walk into the zone -> a Canvas Group fades in
    InventoryUI.cs              a Button opens the bag; slots are Instantiate'd/Destroy'd
    PausePanelUI.cs             a Canvas Group pausing the game (alpha + input at once)
```

```
Assets/Editor/
  Common/
    LabKit.cs                  paths, palette, layers/tags, generated sprites, physics
                               materials, scene helpers, and the shared HUD builder
    LabMenu.cs                 cross-week menu entries
    KenneyToonCharacterImporter.cs   the art import pipeline (AssetPostprocessor)
  W03_Physics2D/
    W03PlaygroundBuilder.cs           builds W03Playground + the W3 player (no Animator) + W3 props
  W04_AnimationCamera/
    W04PlaygroundBuilder.cs           builds W04Playground
  W05_UI/
    W05PlaygroundBuilder.cs           builds W05Playground: calls W04PlaygroundBuilder.BuildGameplay, then adds the UI
    CharacterRig.cs            the character AnimationClips + AnimatorControllers
  Slides/
    TmpBootstrap.cs            imports TMP Essential Resources once (async-safe)
    SlideDeckBuilder.cs        deck JSON -> a 1920x1080 scene (W04Slides, W05Slides)
    UiKit.cs                   the UI furniture the W5 demos and the W5 lab are built from
    SlideDemoBuilder.cs        builds the slide demos onto their own stages
    DemoKit.cs                 the shared demo furniture: visuals, floors, lines, bars, cameras
```

## The rules

1. **One folder per week**, named `Wnn_Topic` (`W05_...`, `W06_...`).
2. A component goes in the week whose lab **first uses it**.
3. Anything reused across weeks that is *not* lecture material goes in `Common`
   (the HUD, the art import pipeline, the scene-building kit).
4. **A week's lab must not contain anything that has not been taught yet.**
   That is why the W3 player has **no Animator** — animation is W4 material. The W4 builder
   calls `W03PlaygroundBuilder.CreatePlayer(...)` and then adds the Animator on top, which is also
   the story the W4 lecture tells ("review the 2D controller, then extend it").
5. Builders live in `Assets/Editor/Wnn_Topic/` with a menu item
   `CAT105TC ▸ Wnn ... ▸ Build`.
6. Dependency direction: `Common` may reference any week; a week may reference **earlier**
   weeks only (W4 → W3 is fine, W3 → W4 is not).
7. `Slides/` is presentation infrastructure, not lecture material. It may reference any week's
   components (a deck scene embeds that week's lab as one of its demos); no week should reference
   it. Its tests live in `Assets/Tests/EditMode/` (the `SlideCursor` state machine + the real deck's
   deserialisation contract) and `Assets/Tests/PlayMode/` (the presenter ↔ demo contract). That is
   why the runtime has its own `Slides.Runtime` assembly and the pure logic a separate engine-free
   `Slides.Core` one — a test asmdef cannot see the predefined `Assembly-CSharp`.
8. A week may produce **three scenes with different jobs** — a **Lab** (the starter a session begins
   from), a **Playground** (the combined result) and a **Slides** scene. See "Scene types" above. A
   lab and a playground are the students' scenes and may use only what has been taught; the Slides
   scene is the instructor's and may reuse any week's components. None is a substitute for another.

## Menu

| Menu | What it does |
|---|---|
| `CAT105TC ▸ Common ▸ Rebuild shared assets` | layers, tags, generated sprites, physics materials |
| `CAT105TC ▸ W03 Physics2D ▸ Build W03 Lab` | rebuilds `Assets/Scenes/W03Playground.unity` |
| `CAT105TC ▸ W04 Animation & Camera ▸ Build W04 Lab` | rebuilds `Assets/Scenes/W04Playground.unity` |
| `CAT105TC ▸ W05 UI ▸ Build W5 Lab` | rebuilds `Assets/Scenes/W05Playground.unity` |
| `CAT105TC ▸ Slides ▸ Build W04Slides` | rebuilds the W4 deck scene (needs `Assets/Slides/W04_L4.json`) |
| `CAT105TC ▸ Slides ▸ Build W05Slides` | rebuilds the W5 deck scene (needs `Assets/Slides/W05_L5.json`) |
| `CAT105TC ▸ Build all current playgrounds` | the two lab builders above |

## Assets

| Path | Belongs to |
|---|---|
| `Assets/Art/KenneyToonCharacters/` | shared art (the character, imported by the Common importer) |
| `Assets/Art/Shapes/`, `Assets/Art/Physics Materials/` | shared, generated by `LabKit` |
| `Assets/Animations/W04_AnimationCamera/` | W4 (the AnimationClips + AnimatorControllers) |
| `Assets/Scenes/W02Playground.unity` | W2 - the legacy 3D player scene (was `PlayerTest.unity`), hand-made, not builder-generated |
| `Assets/Scenes/W03Playground.unity`, `W04Playground.unity` | the 2D lab scenes, regenerated from `Editor/Wnn_.../` |
| `Assets/Slides/<key>.json` | generated decks (source: the course `.pptx` files) |
| `Assets/Scenes/W04Slides.unity` | the W4 deck scene — the slides, with the W4 game behind them |
| `Assets/Scenes/W05Playground.unity`, `W05Slides.unity` | W5 — the W4 platformer plus a UI layer, and its deck |
| `Assets/TextMesh Pro/` | TMP Essential Resources, committed (the deck scene references its font by GUID) |

## Adding the next week

1. `Assets/Scripts/W05_Topic/` — the components W5 teaches.
2. `Assets/Editor/W05_Topic/W05PlaygroundBuilder.cs` — a builder with
   `[MenuItem("CAT105TC/W05 Topic/Build W5 Lab")]`.
3. `Assets/Animations/W05_Topic/` (and `Assets/Scenes/W05Playground.unity`) if it needs its own.
4. Reuse `LabKit` (scene helpers + HUD) and earlier weeks' components where the lecture does.
5. The week's **Slides** scene, if the week is taught from a deck:
   1. generate the deck JSON from the course `.pptx` — the converter is the lecturer's own tooling;
   2. write `Assets/Slides/<deck>.demos.json`: one entry per slide, a comma-separated list of demo
      keys (`""` = that slide shows nothing);
   3. add the demos for that week's knowledge points. Reuse `DemoBase` / `DemoStage` /
      `DemoReadout` / `SlideDemoMap` and the existing demo families; add a new demo family only for a
      concept nothing here demonstrates yet;
   4. add a `[MenuItem]` beside `BuildW05Slides` in `SlideDeckBuilder`, pointing at the new deck's
      scene, and run it.
   The walkthrough is in `Assets/Scripts/Slides/README.md`.
