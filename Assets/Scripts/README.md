# CAT105TC — script layout

The project accumulates one lab per week, so **code is split by week**, and only genuinely
week-independent infrastructure is shared.

```
Assets/Scripts/
  Common/                     infrastructure used by every week - NOT lecture material
    LabHud.cs                 status bar, live input visualiser, camera debug panel, checklist

  Slides/                     the presentation system - shared across all weeks, NOT lecture material
    Core/                       engine-free: SlideData (JSON shapes) + SlideCursor (state machine)
    SlideDeckPlayer.cs          parses a deck, owns the cursor, raises Changed
    SlideView.cs                renders one slide into five TMP fields
    SlidePresenter.cs           shows/hides the deck; owns Time.timeScale
    SlideInput.cs               the key map (Tab / arrows / Space / G)
    SlideNavigator.cs           prev/next buttons, page label, jump-to-slide grid
    README.md                   keys, regeneration, and how to add a week

  W02_VariablesConditionals/  Lecture 2  "C# Fundamentals (1): Variables and Conditionals"
    VariablesDemo.cs            the four core types (string / int / float / bool),
                                the Debug.Log / LogWarning / LogError levels
    HealthPointsDemo.cs         a minimal MonoBehaviour with one field
    PlayerController.cs         early 3D attempt: Input.GetKey + if -> transform.Translate
    PlayerPhysicsController.cs  early 3D attempt: Rigidbody + AddForce + OnCollisionEnter
    FollowPlayer.cs             early 3D attempt: camera follow (offset recorded in Start)
                                -- these three are the 3D leftovers, attached to W2Lab.unity;
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
    SlideSnippets_W04.cs        the literal slide code
```

```
Assets/Editor/
  Common/
    LabKit.cs                  paths, palette, layers/tags, generated sprites, physics
                               materials, scene helpers, and the shared HUD builder
    LabMenu.cs                 cross-week menu entries
    KenneyToonCharacterImporter.cs   the art import pipeline (AssetPostprocessor)
  W03_Physics2D/
    W03LabBuilder.cs           builds W3Lab + the W3 player (no Animator) + W3 props
  W04_AnimationCamera/
    W04LabBuilder.cs           builds W4Lab
    CharacterRig.cs            the character AnimationClips + AnimatorControllers
  Slides/
    TmpBootstrap.cs            imports TMP Essential Resources once (async-safe)
    SlideDeckBuilder.cs        deck JSON -> a 1920x1080 scene (currently W04Slides)
```

## The rules

1. **One folder per week**, named `Wnn_Topic` (`W05_...`, `W06_...`).
2. A component goes in the week whose lab **first uses it**.
3. Anything reused across weeks that is *not* lecture material goes in `Common`
   (the HUD, the art import pipeline, the scene-building kit).
4. **A week's lab must not contain anything that has not been taught yet.**
   That is why the W3 player has **no Animator** — animation is W4 material. The W4 builder
   calls `W03LabBuilder.CreatePlayer(...)` and then adds the Animator on top, which is also
   the story the W4 lecture tells ("review the 2D controller, then extend it").
5. Builders live in `Assets/Editor/Wnn_Topic/` with a menu item
   `CAT105TC ▸ Wnn ... ▸ Build`.
6. Dependency direction: `Common` may reference any week; a week may reference **earlier**
   weeks only (W4 → W3 is fine, W3 → W4 is not).
7. `Slides/` is presentation infrastructure, not lecture material. It may reference any week's
   components (the deck scene embeds the W4 gameplay behind it); no week should reference it.
   Its unit tests live in `Assets/Tests/EditMode/`, whose asmdef can only see `Slides/Core/`.

## Menu

| Menu | What it does |
|---|---|
| `CAT105TC ▸ Common ▸ Rebuild shared assets` | layers, tags, generated sprites, physics materials |
| `CAT105TC ▸ W03 Physics2D ▸ Build W3 Lab` | rebuilds `Assets/Scenes/W3Lab.unity` |
| `CAT105TC ▸ W04 Animation & Camera ▸ Build W4 Lab` | rebuilds `Assets/Scenes/W4Lab.unity` |
| `CAT105TC ▸ Slides ▸ Build W04Slides` | rebuilds the W4 deck scene (needs `Assets/Slides/W04_L4.json`) |
| `CAT105TC ▸ Build all current labs` | the two lab builders above |

## Assets

| Path | Belongs to |
|---|---|
| `Assets/Art/KenneyToonCharacters/` | shared art (the character, imported by the Common importer) |
| `Assets/Art/Shapes/`, `Assets/Art/Physics Materials/` | shared, generated by `LabKit` |
| `Assets/Animations/W04_AnimationCamera/` | W4 (the AnimationClips + AnimatorControllers) |
| `Assets/Scenes/W2Lab.unity` | W2 - the legacy 3D player scene (was `PlayerTest.unity`), hand-made, not builder-generated |
| `Assets/Scenes/W3Lab.unity`, `W4Lab.unity` | the 2D lab scenes, regenerated from `Editor/Wnn_.../` |
| `Assets/Slides/<key>.json` | generated decks (source: the course `.pptx` files) |
| `Assets/Scenes/W04Slides.unity` | the W4 deck scene — the slides, with the W4 game behind them |
| `Assets/TextMesh Pro/` | TMP Essential Resources, committed (the deck scene references its font by GUID) |
| `Captures/` | verification screenshots — not Unity assets, kept as evidence |

## Adding W5

1. `Assets/Scripts/W05_Topic/` — the components W5 teaches.
2. `Assets/Editor/W05_Topic/W05LabBuilder.cs` — a builder with
   `[MenuItem("CAT105TC/W05 Topic/Build W5 Lab")]`.
3. `Assets/Animations/W05_Topic/` (and `Assets/Scenes/W5Lab.unity`) if it needs its own.
4. Reuse `LabKit` (scene helpers + HUD) and earlier weeks' components where the lecture does.
5. If the week teaches from a deck: convert the `.pptx` and add a
   `CAT105TC ▸ Slides ▸ Build WnnSlides` menu item — see `Assets/Scripts/Slides/README.md`.
