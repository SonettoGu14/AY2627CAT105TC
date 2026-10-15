using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the 44 slide demos and the DemoStage that owns them. Kept apart from SlideDeckBuilder so
/// each file keeps one job: this one knows about demo content, that one knows about the deck.
///
/// Every demo sits on its own stage (a 6-wide grid, 16 units apart) and is inactive until DemoStage
/// activates it, so only the demo for the current slide is ever live.
/// </summary>
public static class SlideDemoBuilder
{
    private const int Columns = 6;
    private const float CellX = 16f;
    private const float CellY = 12f;

    private static readonly Color Orange = new Color32(0xFF, 0xB4, 0x54, 0xFF);
    private static readonly Color Blue = new Color32(0x6F, 0xA8, 0xFF, 0xFF);
    private static readonly Color Green = new Color32(0x7C, 0xE3, 0x8B, 0xFF);
    private static readonly Color Pink = new Color32(0xF2, 0x6B, 0x8A, 0xFF);
    private static readonly Color Slate = new Color32(0x2A, 0x36, 0x4E, 0xFF);
    private static readonly Color Dim = new Color32(0x8F, 0xA3, 0xBF, 0xFF);

    private class Spec
    {
        public string key;
        public float orthoSize = 5f;
        public System.Action<Transform> build;
        public Camera produced;          // set by demos that own a camera
    }

    public static DemoStage Build(Transform parent, bool withPlatformer)
    {
        // The demo clips/controllers and the shared sprites/layers must exist even when the W04 playground
        // has never been built. Both calls are idempotent.
        LabKit.SetupSharedAssets();
        CharacterRig.EnsureDemoAssets();
        GameObject root = LabKit.Child(parent, "Demos", Vector3.zero);

        // one camera for every demo that does not bring its own
        GameObject camGo = LabKit.Child(parent, "Demo Camera", new Vector3(0f, 0f, -10f));
        Camera stageCam = camGo.AddComponent<Camera>();
        stageCam.orthographic = true;
        stageCam.orthographicSize = 5f;
        stageCam.clearFlags = CameraClearFlags.SolidColor;
        stageCam.backgroundColor = new Color32(0x0E, 0x14, 0x20, 0xFF);
        camGo.AddComponent<AudioListener>();      // the only one in the scene
        camGo.SetActive(false);

        GameObject stageGo = LabKit.Child(parent, "Demo Stage", Vector3.zero);
        DemoStage stage = stageGo.AddComponent<DemoStage>();
        stage.stageCamera = stageCam;

        List<Spec> specs = BuildSpecs();
        if (withPlatformer) specs.Add(PlatformerSpec());
        List<DemoStage.Slot> slots = new List<DemoStage.Slot>();

        for (int i = 0; i < specs.Count; i++)
        {
            Spec spec = specs[i];
            Vector3 anchor = Anchor(i);
            GameObject go = LabKit.Child(root.transform, spec.key, anchor);
            spec.build(go.transform);
            Camera own = spec.produced != null ? spec.produced : go.GetComponentInChildren<Camera>(true);
            go.SetActive(false);
            slots.Add(new DemoStage.Slot
            {
                key = spec.key,
                root = go,
                anchor = new Vector2(anchor.x, anchor.y),
                orthoSize = spec.orthoSize,
                camera = own
            });
        }

        stage.slots = slots.ToArray();
        return stage;
    }

    private static Vector3 Anchor(int index)
    {
        int col = index % Columns;
        int row = index / Columns;
        return new Vector3(col * CellX, -row * CellY, 0f);
    }

    // ==================================================================================
    //  The demo list
    // ==================================================================================

    private static List<Spec> BuildSpecs()
    {
        List<Spec> s = new List<Spec>();

        // ---------------- slide 2: the 2D controller review ----------------
        s.Add(S(new Spec { key = "ctrl_move", build = t => BuildController(t, ControllerDemo.Mode.Move, false) }));
        s.Add(S(new Spec { key = "ctrl_jump", build = t => BuildController(t, ControllerDemo.Mode.Jump, false) }));
        s.Add(S(new Spec { key = "ctrl_ground", build = t => BuildController(t, ControllerDemo.Mode.Ground, true) }));

        // ---------------- slides 3-4: raycast ----------------
        s.Add(S(new Spec { key = "rc_basic", build = t => BuildRaycast(t, RaycastDemo.Mode.Basic, true) }));
        s.Add(S(new Spec { key = "rc_nocollider", build = t => BuildRaycast(t, RaycastDemo.Mode.NoCollider, true) }));
        s.Add(S(new Spec { key = "rc_origin", build = t => BuildRaycast(t, RaycastDemo.Mode.Origin, true) }));
        s.Add(S(new Spec { key = "rc_direction", build = t => BuildRaycast(t, RaycastDemo.Mode.Direction, true) }));
        s.Add(S(new Spec { key = "rc_distance", build = t => BuildRaycast(t, RaycastDemo.Mode.Distance, true) }));
        s.Add(S(new Spec { key = "rc_layermask", build = t => BuildRaycast(t, RaycastDemo.Mode.LayerMask, false) }));

        // ---------------- slide 6: field modifiers ----------------
        s.Add(S(new Spec { key = "fld_serialize", build = t => BuildField(t, FieldModifierDemo.Mode.Serialize) }));
        s.Add(S(new Spec { key = "fld_hide", build = t => BuildField(t, FieldModifierDemo.Mode.HideInspector) }));
        s.Add(S(new Spec { key = "fld_range", build = t => BuildField(t, FieldModifierDemo.Mode.Range) }));

        // ---------------- slides 7-10: 2D rendering ----------------
        s.Add(S(new Spec { key = "sr_color", build = t => BuildSpriteRenderer(t, SpriteRendererDemo.Mode.Color) }));
        s.Add(S(new Spec { key = "sr_flip", build = t => BuildSpriteRenderer(t, SpriteRendererDemo.Mode.Flip) }));
        s.Add(S(new Spec { key = "sr_sorting", build = t => BuildSpriteRenderer(t, SpriteRendererDemo.Mode.Sorting) }));
        s.Add(S(new Spec { key = "sp_mask", build = BuildSpriteMask }));
        s.Add(S(new Spec { key = "sp_slice", build = t => BuildSpriteFrames(t, SpriteDemo.Mode.Slice) }));
        s.Add(S(new Spec { key = "sp_swap", build = t => BuildSpriteFrames(t, SpriteDemo.Mode.Swap) }));

        // ---------------- slides 11-14: animation ----------------
        s.Add(S(new Spec { key = "an_clip", build = t => BuildAnimation(t, AnimationDemo.Mode.Clip, "DemoStates", "A", "B") }));
        s.Add(S(new Spec { key = "an_animator", build = t => BuildAnimation(t, AnimationDemo.Mode.AnimatorComponent, "DemoStates", "A", "B") }));
        s.Add(S(new Spec { key = "an_keyframes", build = t => BuildAnimation(t, AnimationDemo.Mode.Keyframes, "DemoKeyframes", "Keyframes", "") }));
        s.Add(S(new Spec { key = "an_channels", build = t => BuildAnimation(t, AnimationDemo.Mode.Channels, "DemoChannels", "Channels", "") }));
        s.Add(S(new Spec { key = "an_oneatatime", build = t => BuildAnimation(t, AnimationDemo.Mode.OneAtATime, "DemoStates", "A", "B") }));
        s.Add(S(new Spec { key = "an_priority", build = t => BuildAnimation(t, AnimationDemo.Mode.Priority, "DemoPriority", "Priority", "") }));

        // ---------------- slides 15-21: the Animator ----------------
        s.Add(S(new Spec { key = "am_basics", build = t => BuildAnimator(t, AnimatorDemo.Mode.Basics) }));
        s.Add(S(new Spec { key = "am_states", build = t => BuildAnimator(t, AnimatorDemo.Mode.States) }));
        s.Add(S(new Spec { key = "am_default", build = t => BuildAnimator(t, AnimatorDemo.Mode.Default) }));
        s.Add(S(new Spec { key = "am_transitions", build = t => BuildAnimator(t, AnimatorDemo.Mode.Transitions) }));
        s.Add(S(new Spec { key = "am_exittime", build = t => BuildAnimator(t, AnimatorDemo.Mode.ExitTime) }));
        s.Add(S(new Spec { key = "am_duration", build = t => BuildAnimator(t, AnimatorDemo.Mode.Duration) }));
        s.Add(S(new Spec { key = "am_vars", build = t => BuildAnimator(t, AnimatorDemo.Mode.Vars) }));
        s.Add(S(new Spec { key = "am_booltrigger", build = t => BuildAnimator(t, AnimatorDemo.Mode.BoolTrigger) }));
        s.Add(S(new Spec { key = "am_scripting", build = t => BuildAnimator(t, AnimatorDemo.Mode.Scripting) }));
        s.Add(S(new Spec { key = "am_play", build = t => BuildAnimator(t, AnimatorDemo.Mode.Play) }));

        // ---------------- slides 23-27: camera ----------------
        s.Add(S(new Spec { key = "cam_projection", build = t => BuildCamera(t, CameraDemo.Mode.Projection) }));
        s.Add(S(new Spec { key = "cam_fov", build = t => BuildCamera(t, CameraDemo.Mode.Fov) }));
        s.Add(S(new Spec { key = "cam_ortho", build = t => BuildCamera(t, CameraDemo.Mode.OrthoSize) }));
        s.Add(S(new Spec { key = "cam_clearflags", build = t => BuildCamera(t, CameraDemo.Mode.ClearFlags) }));
        s.Add(S(new Spec { key = "cam_background", build = t => BuildCamera(t, CameraDemo.Mode.Background) }));
        s.Add(S(new Spec { key = "cam_culling", build = t => BuildCamera(t, CameraDemo.Mode.CullingMask) }));
        s.Add(S(new Spec { key = "cam_depth", build = t => BuildCamera(t, CameraDemo.Mode.Depth) }));
        s.Add(S(new Spec { key = "cam_follow", build = t => BuildCamera(t, CameraDemo.Mode.Follow) }));
        s.Add(S(new Spec { key = "cam_follow_smooth", build = t => BuildCamera(t, CameraDemo.Mode.FollowSmooth) }));

        // ---------------- W5: the UI lecture ----------------
        // Slide 3 is the camera recap: it wires the W4 camera demos above, so it adds no spec.

        // slides 4-5 / 7-8: prefabs, Instantiate, Destroy
        s.Add(S(new Spec { key = "pf_instances",   build = t => BuildSpawn(t, SpawnDemo.Mode.Instances) }));
        s.Add(S(new Spec { key = "go_instantiate", build = t => BuildSpawn(t, SpawnDemo.Mode.Instantiate) }));
        s.Add(S(new Spec { key = "go_destroy",     build = t => BuildSpawn(t, SpawnDemo.Mode.Destroy) }));

        // slides 9-18: what UI is, the Canvas, and the first components
        s.Add(S(new Spec { key = "ui_vs_scene",        build = t => BuildUi(t, UiDemo.Mode.VsScene) }));
        s.Add(S(new Spec { key = "ui_canvas_required", build = t => BuildUi(t, UiDemo.Mode.CanvasRequired) }));
        s.Add(S(new Spec { key = "ui_canvas_mode",     build = t => BuildUi(t, UiDemo.Mode.CanvasMode) }));
        s.Add(S(new Spec { key = "ui_scaler",          build = t => BuildUi(t, UiDemo.Mode.Scaler) }));
        s.Add(S(new Spec { key = "ui_raycaster",       build = t => BuildUi(t, UiDemo.Mode.Raycaster) }));
        s.Add(S(new Spec { key = "ui_image",           build = t => BuildUi(t, UiDemo.Mode.Image) }));
        s.Add(S(new Spec { key = "ui_text",            build = t => BuildUi(t, UiDemo.Mode.Text) }));
        s.Add(S(new Spec { key = "ui_button",          build = t => BuildUi(t, UiDemo.Mode.Button) }));
        s.Add(S(new Spec { key = "ui_code",            build = t => BuildUi(t, UiDemo.Mode.Code) }));

        // slides 19-22: Mathf, Lerp, and a Lerp-driven UI element
        s.Add(S(new Spec { key = "math_type",        build = t => BuildMathf(t, MathfDemo.Mode.Type) }));
        s.Add(S(new Spec { key = "math_lerp",        build = t => BuildMathf(t, MathfDemo.Mode.Lerp) }));
        s.Add(S(new Spec { key = "math_clamp",       build = t => BuildMathf(t, MathfDemo.Mode.Clamp) }));
        s.Add(S(new Spec { key = "ui_lerp_move",     build = t => BuildUi(t, UiDemo.Mode.LerpMove) }));
        s.Add(S(new Spec { key = "math_lerp_blend",  build = t => BuildMathf(t, MathfDemo.Mode.LerpBlend) }));

        // slides 25-31: the rest of the components and UI animation
        s.Add(S(new Spec { key = "ui_slider",  build = t => BuildUi(t, UiDemo.Mode.Slider) }));
        s.Add(S(new Spec { key = "ui_mask",    build = t => BuildUi(t, UiDemo.Mode.Mask) }));
        s.Add(S(new Spec { key = "ui_rect",    build = t => BuildUi(t, UiDemo.Mode.Rect) }));
        s.Add(S(new Spec { key = "ui_pivot",   build = t => BuildUi(t, UiDemo.Mode.Pivot) }));
        s.Add(S(new Spec { key = "ui_fade",    build = t => BuildUi(t, UiDemo.Mode.Fade) }));
        s.Add(S(new Spec { key = "ui_group",   build = t => BuildUi(t, UiDemo.Mode.Group) }));

        // slides 32-33: the two combined demos
        s.Add(S(new Spec { key = "dialogue",   build = t => BuildWidget(t, WidgetDemo.Mode.Dialogue) }));
        s.Add(S(new Spec { key = "health_bar", build = t => BuildWidget(t, WidgetDemo.Mode.HealthBar) }));

        return s;
    }

    private static Spec S(Spec spec) { return spec; }

    /// <summary>Slide 22's demo: the W4 platformer, built under its own stage root so DemoStage can
    /// switch it on and off like any other demo. Its follow camera is created at the scene root by
    /// BuildGameplay, so it is captured and handed to the slot explicitly.</summary>
    private static Spec PlatformerSpec()
    {
        Spec p = new Spec { key = "platformer", orthoSize = 6f };
        p.build = t =>
        {
            PlayerController2D labPlayer;
            Camera labCam;
            W04PlaygroundBuilder.BuildGameplay(t, out labPlayer, out labCam);
            p.produced = labCam;
            t.gameObject.AddComponent<PlatformerDemo>();   // so the readout panel has something to show
        };
        return p;
    }

    // ==================================================================================
    //  W5 - UI demos
    //
    //  Layout rule: the readout panel owns the bottom-left (880x380 at 28,28 in the 1920x1080
    //  reference space), so every UI demo anchors its content to the TOP or to the RIGHT.
    // ==================================================================================

    /// <summary>The standard content frame for a UI demo: the top half, clear of the readout.</summary>
    private static RectTransform UiFrame(Canvas canvas, string name, Vector2 size)
    {
        return UiKit.MakePanel(canvas.transform, name, UiKit.TopCenter, new Vector2(0f, -70f), size);
    }

    private static void BuildUi(Transform t, UiDemo.Mode mode)
    {
        UiDemo demo = t.gameObject.AddComponent<UiDemo>();
        demo.mode = mode;
        Canvas canvas = UiKit.MakeCanvas(t, "Canvas");
        demo.canvas = canvas;
        demo.sprites = new[] { LabKit.White, LabKit.Circle, LabKit.CharacterSprite("idle") };
        Transform ct = canvas.transform;

        switch (mode)
        {
            case UiDemo.Mode.VsScene:
            {
                GameObject hero = LabKit.Child(t, "SceneObject", new Vector3(-3.2f, 0f, 0f));
                demo.worldSprite = LabKit.AddSprite(hero, LabKit.CharacterSprite("idle"), Color.white, 5);
                RectTransform p = UiKit.MakePanel(ct, "Panel", new Vector2(1f, 0.5f), new Vector2(-80f, 0f), new Vector2(540f, 340f));
                UiKit.MakeText(p, "Label", UiKit.Center, Vector2.zero, new Vector2(500f, 200f), 44f, UiKit.Paper, TextAlignmentOptions.Center)
                      .text = "UI\n(drawn by a Canvas)";
                demo.panel = p;
                break;
            }

            case UiDemo.Mode.CanvasRequired:
            {
                RectTransform frame = UiFrame(canvas, "Frame", new Vector2(1040f, 560f));
                UiKit.MakeText(frame, "Note", UiKit.TopLeft, new Vector2(30f, -24f), new Vector2(980f, 50f), 30f, UiKit.Muted)
                      .text = "One of these two Images is NOT under the Canvas.";
                RectTransform ok = UiKit.MakeRect(ct, "Attached", UiKit.TopCenter, new Vector2(-230f, -270f), new Vector2(320f, 220f));
                UiKit.MakeImage(ok, UiKit.Accent);
                UiKit.MakeText(ok, "Label", UiKit.Center, Vector2.zero, new Vector2(300f, 60f), 30f, UiKit.Ink, TextAlignmentOptions.Center).text = "under Canvas";
                demo.detached = UiKit.MakeRect(t, "Detached", UiKit.TopCenter, new Vector2(230f, -270f), new Vector2(320f, 220f));
                UiKit.MakeImage(demo.detached, UiKit.Gold);
                RectTransform ghost = UiKit.MakeRect(ct, "Ghost", UiKit.TopCenter, new Vector2(230f, -270f), new Vector2(320f, 220f));
                UiKit.MakeText(ghost, "Label", UiKit.Center, Vector2.zero, new Vector2(300f, 180f), 26f, UiKit.Muted, TextAlignmentOptions.Center)
                      .text = "not under a Canvas\n-> nothing is drawn\n\npress C";
                break;
            }

            case UiDemo.Mode.CanvasMode:
            {
                GameObject hero = LabKit.Child(t, "SceneObject", new Vector3(0f, 0f, 0f));
                demo.worldSprite = LabKit.AddSprite(hero, LabKit.CharacterSprite("idle"), Color.white, 5);
                demo.cam = DemoKit.OwnCamera(t, "Camera", new Vector3(0f, 0f, -10f), 3.2f, true);
                RectTransform p = UiKit.MakePanel(ct, "Panel", UiKit.TopRight, new Vector2(-60f, -60f), new Vector2(520f, 300f));
                UiKit.MakeText(p, "Label", UiKit.Center, Vector2.zero, new Vector2(480f, 200f), 40f, UiKit.Paper, TextAlignmentOptions.Center)
                      .text = "Canvas panel";
                demo.panel = p;
                break;
            }

            case UiDemo.Mode.Scaler:
            {
                RectTransform p = UiKit.MakePanel(ct, "Panel", UiKit.TopLeft, new Vector2(80f, -80f), new Vector2(420f, 260f));
                UiKit.MakeText(p, "Label", UiKit.Center, Vector2.zero, new Vector2(380f, 200f), 30f, UiKit.Paper, TextAlignmentOptions.Center)
                      .text = "420 x 260\nreference pixels";
                demo.panel = p;
                break;
            }

            case UiDemo.Mode.Raycaster:
            {
                demo.button = UiKit.MakeButton(ct, "Button", UiKit.TopCenter, new Vector2(0f, -220f), new Vector2(360f, 130f), "Click me", UiKit.Accent);
                demo.probe = demo.button.gameObject.AddComponent<UiPointerProbe>();
                break;
            }

            case UiDemo.Mode.Image:
            {
                RectTransform holder = UiKit.MakeRect(ct, "Image", UiKit.TopCenter, new Vector2(0f, -240f), new Vector2(400f, 400f));
                demo.image = UiKit.MakeImage(holder, Color.white, true);
                demo.probe = holder.gameObject.AddComponent<UiPointerProbe>();
                break;
            }

            case UiDemo.Mode.Text:
            {
                demo.text = UiKit.MakeText(ct, "Text", UiKit.TopLeft, new Vector2(90f, -110f), new Vector2(1400f, 320f), 36f, UiKit.Paper);
                demo.text.text = "The Text component\nshows text in the UI.";
                break;
            }

            case UiDemo.Mode.Button:
            {
                demo.button = UiKit.MakeButton(ct, "Button", UiKit.TopCenter, new Vector2(0f, -240f), new Vector2(420f, 150f), "Press me", UiKit.Accent);
                demo.probe = demo.button.gameObject.AddComponent<UiPointerProbe>();
                break;
            }

            case UiDemo.Mode.Code:
            {
                RectTransform img = UiKit.MakeRect(ct, "Image", UiKit.TopCenter, new Vector2(-320f, -250f), new Vector2(360f, 360f));
                demo.image = UiKit.MakeImage(img, Color.white, false);
                demo.text = UiKit.MakeText(ct, "Text", UiKit.TopCenter, new Vector2(320f, -180f), new Vector2(560f, 100f), 40f, UiKit.Paper, TextAlignmentOptions.Center);
                demo.text.text = "Picture 0";
                demo.button = UiKit.MakeButton(ct, "Button", UiKit.TopCenter, new Vector2(320f, -300f), new Vector2(360f, 120f), "Change both", UiKit.Green);
                break;
            }

            case UiDemo.Mode.Slider:
            {
                demo.slider = UiKit.MakeSlider(ct, "Slider", UiKit.TopCenter, new Vector2(0f, -230f), new Vector2(560f, 56f), UiKit.Green);
                demo.slider.value = 0.67f;
                RectTransform meter = UiKit.MakeRect(ct, "Meter", UiKit.TopCenter, new Vector2(-210f, -360f), new Vector2(420f, 46f));
                demo.meter = UiKit.MakeImage(meter, UiKit.Green, false);
                break;
            }

            case UiDemo.Mode.Mask:
            {
                UiKit.MakeText(ct, "A", UiKit.TopCenter, new Vector2(-280f, -110f), new Vector2(200f, 44f), 26f, UiKit.Muted, TextAlignmentOptions.Center).text = "Mask";
                UiKit.MakeText(ct, "B", UiKit.TopCenter, new Vector2(280f, -110f), new Vector2(200f, 44f), 26f, UiKit.Muted, TextAlignmentOptions.Center).text = "RectMask2D";
                RectTransform frameA = UiKit.MakeMask(ct, "FrameA", UiKit.TopCenter, new Vector2(-280f, -300f), new Vector2(300f, 300f), false);
                frameA.GetComponent<Image>().sprite = LabKit.Circle;
                frameA.GetComponent<Image>().color = UiKit.Card;
                RectTransform frameB = UiKit.MakeMask(ct, "FrameB", UiKit.TopCenter, new Vector2(280f, -300f), new Vector2(300f, 300f), true);
                frameB.GetComponent<Image>().color = UiKit.Card;
                demo.clipA = UiKit.MakeRect(frameA, "ChildA", UiKit.Center, new Vector2(0f, 0f), new Vector2(420f, 220f));
                UiKit.MakeImage(demo.clipA, UiKit.Gold, false);
                demo.clipB = UiKit.MakeRect(frameB, "ChildB", UiKit.Center, new Vector2(0f, 0f), new Vector2(420f, 220f));
                UiKit.MakeImage(demo.clipB, UiKit.Gold, false);
                break;
            }

            case UiDemo.Mode.Rect:
            {
                RectTransform box = UiKit.MakeRect(ct, "Box", UiKit.TopCenter, new Vector2(0f, -260f), new Vector2(260f, 180f));
                UiKit.MakeImage(box, UiKit.Accent, false);
                demo.mover = box;
                break;
            }

            case UiDemo.Mode.Pivot:
            {
                RectTransform box = UiKit.MakeRect(ct, "Box", UiKit.TopCenter, new Vector2(0f, -260f), new Vector2(300f, 200f));
                UiKit.MakeImage(box, UiKit.Accent, false);
                // a dot at the rect's own origin: it IS the pivot, so the rotation centre is visible
                RectTransform dot = UiKit.MakeRect(box, "PivotDot", UiKit.TopLeft, Vector2.zero, new Vector2(26f, 26f));
                UiKit.MakeImage(dot, UiKit.Gold, false);
                demo.mover = box;
                break;
            }

            case UiDemo.Mode.LerpMove:
            {
                UiKit.MakeText(ct, "Mover", UiKit.TopCenter, new Vector2(-340f, -130f), new Vector2(320f, 44f), 26f, UiKit.Muted, TextAlignmentOptions.Center).text = "Vector3.Lerp target";
                RectTransform box = UiKit.MakeRect(ct, "Box", UiKit.TopCenter, new Vector2(-340f, -260f), new Vector2(300f, 200f));
                UiKit.MakeImage(box, UiKit.Green, false);
                demo.mover = box;
                break;
            }

            case UiDemo.Mode.Fade:
            {
                demo.group = UiKit.MakeGroup(ct, "Panel", UiKit.TopCenter, new Vector2(0f, -70f), new Vector2(1000f, 540f));
                RectTransform bg = UiKit.MakeStretch(demo.group.transform, "Bg");
                UiKit.MakeImage(bg, UiKit.Panel, false);
                UiKit.MakeText(demo.group.transform, "Label", UiKit.TopCenter, new Vector2(0f, -140f), new Vector2(880f, 200f), 40f, UiKit.Paper, TextAlignmentOptions.Center)
                      .text = "A dialogue box\nis one Canvas Group fading in.";
                UiKit.MakeButton(demo.group.transform, "Ok", UiKit.TopCenter, new Vector2(0f, -400f), new Vector2(280f, 110f), "OK", UiKit.Accent);
                break;
            }

            default:   // Group
            {
                demo.group = UiKit.MakeGroup(ct, "Panel", UiKit.TopCenter, new Vector2(0f, -70f), new Vector2(1000f, 540f));
                RectTransform bg = UiKit.MakeStretch(demo.group.transform, "Bg");
                UiKit.MakeImage(bg, UiKit.Panel, false);
                demo.button = UiKit.MakeButton(demo.group.transform, "Save", UiKit.TopCenter, new Vector2(-190f, -300f), new Vector2(320f, 130f), "Save", UiKit.Accent);
                UiKit.MakeButton(demo.group.transform, "Quit", UiKit.TopCenter, new Vector2(190f, -300f), new Vector2(320f, 130f), "Quit", UiKit.Card);
                demo.probe = demo.button.gameObject.AddComponent<UiPointerProbe>();
                UiKit.MakeText(demo.group.transform, "Note", UiKit.TopCenter, new Vector2(0f, -140f), new Vector2(880f, 60f), 28f, UiKit.Muted, TextAlignmentOptions.Center)
                      .text = "Z alpha     X interactable     C blocks raycasts";
                break;
            }
        }
    }

    private static void BuildMathf(Transform t, MathfDemo.Mode mode)
    {
        MathfDemo demo = t.gameObject.AddComponent<MathfDemo>();
        demo.mode = mode;
        GameObject track = Visual(t, "Track", LabKit.White, new Color32(0x1E, 0x24, 0x30, 0xFF), new Vector2(0f, 0f), new Vector2(6f, 0.12f), 1);
        demo.track = track.transform;
        GameObject marker = Visual(t, "Marker", LabKit.Circle, Green, new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), 3);
        demo.marker = marker.transform;
        demo.markerSprite = marker.GetComponent<SpriteRenderer>();

        if (mode == MathfDemo.Mode.Clamp)
        {
            GameObject c = Visual(t, "Clamped", LabKit.Circle, Blue, new Vector2(0f, -0.7f), new Vector2(0.5f, 0.5f), 3);
            demo.clampedMarker = c.transform;
            demo.clampedSprite = c.GetComponent<SpriteRenderer>();
        }
        if (mode == MathfDemo.Mode.Type)
            Visual(t, "Note", LabKit.White, Slate, new Vector2(0f, -1.2f), new Vector2(6f, 0.1f), 0);
    }

    private static void BuildSpawn(Transform t, SpawnDemo.Mode mode)
    {
        SpawnDemo demo = t.gameObject.AddComponent<SpawnDemo>();
        demo.mode = mode;

        GameObject template = LabKit.Child(t, "Template", new Vector3(0f, -6f, 0f));
        template.SetActive(false);                       // an inactive child: the "prefab"
        if (mode == SpawnDemo.Mode.Instances)
        {
            template.transform.localScale = Vector3.one;
            LabKit.AddSprite(template, LabKit.CharacterSprite("idle"), Color.white, 4);
            GameObject src = Visual(t, "Source", LabKit.CharacterSprite("idle"), Dim, new Vector2(-4.4f, 1.6f), Vector2.one, 4);
            demo.sourceMark = src.transform;
            demo.sourceSprite = src.GetComponent<SpriteRenderer>();
        }
        else
        {
            template.transform.localScale = Vector3.one * 0.5f;
            LabKit.AddSprite(template, LabKit.Circle, Orange, 4);
        }
        demo.template = template;
    }

    private static void BuildWidget(Transform t, WidgetDemo.Mode mode)
    {
        WidgetDemo demo = t.gameObject.AddComponent<WidgetDemo>();
        demo.mode = mode;

        GameObject hero = LabKit.Child(t, "Hero", new Vector3(-4.6f, -1.2f, 0f));
        demo.hero = LabKit.AddSprite(hero, LabKit.CharacterSprite("idle"), Color.white, 5);
        GameObject zone = LabKit.Child(t, "Zone", new Vector3(demo.zoneX, -1.2f, 0f));
        zone.transform.localScale = new Vector3(0.9f, 2.4f, 1f);
        demo.zone = LabKit.AddSprite(zone, LabKit.White, new Color(0.55f, 0.6f, 0.7f, 0.35f), 1);

        Canvas canvas = UiKit.MakeCanvas(t, "Canvas");
        if (mode == WidgetDemo.Mode.Dialogue)
        {
            demo.group = UiKit.MakeGroup(canvas.transform, "Dialogue", UiKit.TopCenter, new Vector2(0f, -70f), new Vector2(1000f, 360f));
            RectTransform bg = UiKit.MakeStretch(demo.group.transform, "Bg");
            UiKit.MakeImage(bg, UiKit.Panel, false);
            demo.panelText = UiKit.MakeText(demo.group.transform, "Line", UiKit.TopLeft, new Vector2(40f, -40f), new Vector2(920f, 200f), 34f, UiKit.Paper);
            UiKit.MakeText(demo.group.transform, "Who", UiKit.TopLeft, new Vector2(40f, -270f), new Vector2(920f, 50f), 26f, UiKit.Accent).text = "Guide";
        }
        else
        {
            RectTransform holder = UiKit.MakePanel(canvas.transform, "Health", UiKit.TopRight, new Vector2(-80f, -90f), new Vector2(560f, 200f));
            demo.bar = UiKit.MakeSlider(holder, "Bar", UiKit.TopCenter, new Vector2(0f, -90f),
                                       new Vector2(460f, 50f), UiKit.Green,
                                       withHandle: false, interactable: false);
            demo.bar.value = 1f;
            demo.barFill = demo.bar.fillRect.GetComponent<Image>();
            demo.barLabel = UiKit.MakeText(holder, "Label", UiKit.TopCenter, new Vector2(0f, -24f), new Vector2(480f, 50f), 30f, UiKit.Paper, TextAlignmentOptions.Center);
            demo.barLabel.text = "Health";
        }
        if (mode == WidgetDemo.Mode.HealthBar) demo.zone = null;   // the hazard is described, not walked into
    }

    // ==================================================================================
    //  Demo construction
    // ==================================================================================

    private static GameObject Visual(Transform parent, string name, Sprite sprite, Color color, Vector2 pos, Vector2 scale, int order)
    {
        return DemoKit.Visual(parent, name, sprite, color, pos, scale, order);
    }

    // ---------------- controller ----------------
    private static void BuildController(Transform t, ControllerDemo.Mode mode, bool withLedge)
    {
        if (withLedge)
        {
            LabKit.Platform(t, "FloorLeft", -3f, -1.5f, 5f, 1f, Slate, "Ground");
            LabKit.Platform(t, "FloorRight", 3.5f, -1.5f, 5f, 1f, Slate, "Ground");
        }
        else
        {
            LabKit.Platform(t, "Floor", 0f, -1.5f, 16f, 1f, Slate, "Ground");
        }

        GameObject player = LabKit.Child(t, "Player", new Vector3(-4f, -0.5f, 0f));
        player.layer = LabKit.Layer("Player");
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Vertical;
        col.size = new Vector2(0.55f, 0.95f);
        col.offset = new Vector2(0f, 0.48f);
        GameObject vis = Visual(player.transform, "Visual", LabKit.CharacterSprite("idle"), Color.white, Vector2.zero, Vector2.one, 5);

        GameObject check = LabKit.Child(player.transform, "GroundCheck", new Vector3(0f, 0.05f, 0f));
        LineRenderer line = Line(t, "GroundRay", 150);
        line.transform.SetParent(player.transform, false);
        line.transform.localPosition = Vector3.zero;

        ControllerDemo demo = t.gameObject.AddComponent<ControllerDemo>();
        demo.mode = mode;
        demo.body = rb;
        demo.groundCheckFrom = check.transform;
        demo.groundRay = line;
        demo.groundLayer = 1 << LabKit.Layer("Ground");
    }

    // ---------------- raycast ----------------
    private static void BuildRaycast(Transform t, RaycastDemo.Mode mode, bool targetOnGround)
    {
        LabKit.Platform(t, "Floor", 0f, -2.6f, 18f, 0.5f, Slate, "Ground");

        GameObject emitter = LabKit.Child(t, "Emitter", new Vector3(-5f, 0f, 0f));
        LabKit.AddSprite(emitter, LabKit.Circle, mode == RaycastDemo.Mode.NoCollider ? Green : Blue, 6)
            .transform.localScale = Vector3.one * 0.45f;

        GameObject target = LabKit.Platform(t, "Target", 2f, 0f, 1.2f, 3f,
            mode == RaycastDemo.Mode.LayerMask ? Dim : Orange, targetOnGround ? "Ground" : "Ground");
        // for the mask demo the target sits on the Ignore Raycast layer so the mask means something
        if (mode == RaycastDemo.Mode.LayerMask) target.layer = 2;

        LineRenderer line = Line(t, "Ray", 150);

        RaycastDemo demo = t.gameObject.AddComponent<RaycastDemo>();
        demo.mode = mode;
        demo.emitter = emitter.transform;
        demo.target = target.GetComponent<Collider2D>();
        demo.targetVisual = target.GetComponentInChildren<SpriteRenderer>();
        demo.emitterVisual = emitter.GetComponent<SpriteRenderer>();
        demo.line = line;
        demo.distance = 10f;
        demo.targetLayerBit = mode == RaycastDemo.Mode.LayerMask ? 2 : LabKit.Layer("Ground");
        demo.mask = ~0;
        if (mode == RaycastDemo.Mode.LayerMask) demo.mask = (1 << 2) | (1 << LabKit.Layer("Ground"));

        if (mode == RaycastDemo.Mode.NoCollider)
        {
            LabKit.WorldLabel(t, "emitter: no collider", new Vector3(-5f, 1.1f, 0f), Green);
            LabKit.WorldLabel(t, "target: has a collider", new Vector3(2f, 2.2f, 0f), Orange);
        }
        if (mode == RaycastDemo.Mode.LayerMask)
        {
            LabKit.WorldLabel(t, "grey = layer 'Ignore Raycast'", new Vector3(2f, 2.2f, 0f), Dim);
        }
    }

    // ---------------- field modifiers ----------------
    private static void BuildField(Transform t, FieldModifierDemo.Mode mode)
    {
        GameObject track = Visual(t, "Track", LabKit.White, new Color32(0x1E, 0x24, 0x30, 0xFF), new Vector2(0f, 0f), new Vector2(6f, 0.5f), 1);
        GameObject bar = Visual(t, "Bar", LabKit.White, Green, new Vector2(-3f, 0f), new Vector2(0.1f, 1.1f), 2);

        FieldModifierDemo demo = t.gameObject.AddComponent<FieldModifierDemo>();
        demo.mode = mode;
        demo.bar = bar.transform;
        demo.barSprite = bar.GetComponent<SpriteRenderer>();
    }

    // ---------------- sprite renderer ----------------
    private static void BuildSpriteRenderer(Transform t, SpriteRendererDemo.Mode mode)
    {
        SpriteRendererDemo demo = t.gameObject.AddComponent<SpriteRendererDemo>();
        demo.mode = mode;

        if (mode == SpriteRendererDemo.Mode.Sorting)
        {
            GameObject back = Visual(t, "Back", LabKit.CharacterSprite("idle"), Blue, new Vector2(-0.4f, -0.6f), Vector2.one, 0);
            GameObject front = Visual(t, "Front", LabKit.CharacterSprite("idle"), Orange, new Vector2(0.4f, -0.6f), Vector2.one, 1);
            demo.back = back.GetComponent<SpriteRenderer>();
            demo.front = front.GetComponent<SpriteRenderer>();
            LabKit.WorldLabel(t, "left = blue   right = orange", new Vector3(0f, 1.3f, 0f), Dim);
        }
        else
        {
            GameObject go = Visual(t, "Sprite", LabKit.CharacterSprite("idle"), Color.white, new Vector2(0f, -0.8f), Vector2.one * 2f, 2);
            demo.target = go.GetComponent<SpriteRenderer>();
            if (mode == SpriteRendererDemo.Mode.Flip)
                LabKit.WorldLabel(t, "watch the body, not the position", new Vector3(0f, 1.6f, 0f), Dim);
        }
    }

    // ---------------- sprite mask ----------------
    private static void BuildSpriteMask(Transform t)
    {
        GameObject image = Visual(t, "Image", LabKit.White, Orange, new Vector2(0f, 0f), new Vector2(6f, 3f), 1);
        GameObject maskGo = LabKit.Child(t, "Mask", new Vector3(-2f, 0f, 0f));
        maskGo.AddComponent<SpriteMask>();
        GameObject maskVis = Visual(maskGo.transform, "Visual", LabKit.Circle, new Color(0.4f, 0.8f, 1f, 0.25f), Vector2.zero, Vector2.one * 2.5f, 3);

        SpriteDemo demo = t.gameObject.AddComponent<SpriteDemo>();
        demo.mode = SpriteDemo.Mode.Mask;
        demo.mask = maskGo.GetComponent<SpriteMask>();
        demo.masked = image.GetComponent<SpriteRenderer>();
        demo.maskVisual = maskVis.GetComponent<SpriteRenderer>();
    }

    // ---------------- slices / sprite swap ----------------
    private static void BuildSpriteFrames(Transform t, SpriteDemo.Mode mode)
    {
        GameObject go = Visual(t, "Sprite", LabKit.CharacterSprite("idle"), Color.white, new Vector2(0f, -0.8f), Vector2.one * 2f, 2);

        SpriteDemo demo = t.gameObject.AddComponent<SpriteDemo>();
        demo.mode = mode;
        demo.target = go.GetComponent<SpriteRenderer>();

        if (mode == SpriteDemo.Mode.Slice)
        {
            demo.frames = SlicedFrames();
            LabKit.WorldLabel(t, SlicedFrames().Length + " sprites from ONE texture", new Vector3(0f, 1.8f, 0f), Dim);
        }
        else
        {
            demo.frames = new[]
            {
                LabKit.CharacterSprite("idle"), LabKit.CharacterSprite("walk0"), LabKit.CharacterSprite("walk3"),
                LabKit.CharacterSprite("run1"), LabKit.CharacterSprite("jump"), LabKit.CharacterSprite("fall")
            };
        }
    }

    private static Sprite[] SlicedFrames()
    {
        const string sheet = "Assets/Art/KenneyToonCharacters/Male person/Tilesheet/character_malePerson_sheet.png";
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(sheet);
        List<Sprite> list = new List<Sprite>();
        foreach (Object o in all)
        {
            Sprite sp = o as Sprite;
            if (sp != null) list.Add(sp);
        }
        // sheet order: top row first, left to right
        list.Sort((x, y) =>
        {
            int row = y.rect.y.CompareTo(x.rect.y);
            return row != 0 ? row : x.rect.x.CompareTo(y.rect.x);
        });
        return list.ToArray();
    }

    // ---------------- animation ----------------
    private static void BuildAnimation(Transform t, AnimationDemo.Mode mode, string controllerName, string stateA, string stateB)
    {
        GameObject go = Visual(t, "Animated", LabKit.CharacterSprite("idle"), Color.white, new Vector2(0f, -0.8f), Vector2.one * 2f, 3);
        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath(controllerName));

        if (mode == AnimationDemo.Mode.Keyframes || mode == AnimationDemo.Mode.Channels || mode == AnimationDemo.Mode.Priority)
        {
            // clip paths are relative to the object the Animator sits on
            go.transform.localPosition = new Vector3(0f, -0.8f, 0f);
        }

        AnimationDemo demo = t.gameObject.AddComponent<AnimationDemo>();
        demo.mode = mode;
        demo.animator = animator;
        demo.target = go.transform;
        demo.sprite = go.GetComponent<SpriteRenderer>();
        demo.stateA = stateA;
        demo.stateB = stateB;
        demo.keyTimes = new[] { 0f, 0.5f, 1f };
        demo.keyLabels = new[] { "start", "peak", "back" };
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath(controllerName));
        demo.clipLength = clip != null ? clip.length : 1f;
    }

    // ---------------- animator ----------------
    private static void BuildAnimator(Transform t, AnimatorDemo.Mode mode)
    {
        bool useExitController = mode == AnimatorDemo.Mode.ExitTime || mode == AnimatorDemo.Mode.Duration;
        string controllerName = useExitController ? "DemoExit" : "DemoStates";

        GameObject go = Visual(t, "Animated", LabKit.CharacterSprite("idle"), Color.white, new Vector2(0f, -0.8f), Vector2.one * 2f, 3);
        // a second copy so Play() has somewhere to cut between two States in the Play demo
        GameObject alt = Visual(t, "AnimatedB", LabKit.CharacterSprite("run0"), new Color(1f, 1f, 1f, 0f), new Vector2(0f, -0.8f), Vector2.one * 2f, 2);

        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath(controllerName));

        AnimatorDemo demo = t.gameObject.AddComponent<AnimatorDemo>();
        demo.mode = mode;
        demo.animator = animator;
        demo.defaultState = useExitController ? "Wait" : "A";
        demo.stateNames = useExitController ? new[] { "Wait", "Done" } : new[] { "A", "B", "C" };
        demo.transitionExitTime = 0.9f;
        demo.transitionDuration = 0.6f;
        if (alt != null) alt.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);   // hidden helper
    }

    // ---------------- camera ----------------
    private static void BuildCamera(Transform t, CameraDemo.Mode mode)
    {
        bool needsBack = mode == CameraDemo.Mode.ClearFlags || mode == CameraDemo.Mode.Depth;
        bool needsSmooth = mode == CameraDemo.Mode.FollowSmooth;
        bool needsTarget = mode == CameraDemo.Mode.Follow || mode == CameraDemo.Mode.FollowSmooth;

        CameraKit(t, out SpriteRenderer groundSprite, out SpriteRenderer playerSprite);

        Camera cam = OwnCamera(t, "Camera", new Vector3(0f, 0f, -10f), 3.2f, mode);
        Camera back = null;
        if (needsBack)
        {
            back = OwnCamera(t, "CameraBack", new Vector3(0f, 0f, -8f), 3.2f, mode);
            back.depth = 0f;
            back.clearFlags = CameraClearFlags.SolidColor;
            back.backgroundColor = new Color32(0x20, 0x2A, 0x3C, 0xFF);
            cam.depth = 1f;
            if (mode == CameraDemo.Mode.Depth)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color32(0x7A, 0x50, 0x28, 0xFF);   // warm = camera A
                back.backgroundColor = new Color32(0x1E, 0x38, 0x66, 0xFF); // cool = camera B
            }
        }

        Transform mover = null;
        Camera smooth = null;
        Transform smoothPivot = null;
        if (needsTarget)
        {
            GameObject m = Visual(t, "Mover", LabKit.CharacterSprite("idle"), Green, new Vector2(-3f, -1f), Vector2.one, 5);
            mover = m.transform;
            groundSprite.color = groundSprite.color;
        }
        if (needsSmooth)
        {
            // split screen: instant on the left, smoothed on the right
            cam.rect = new Rect(0f, 0f, 0.5f, 1f);
            smooth = OwnCamera(t, "CameraSmooth", new Vector3(0f, 0f, -10f), 3.2f, mode);
            smooth.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            GameObject pivot = LabKit.Child(t, "SmoothPivot", new Vector3(0f, 0f, 0f));
            smoothPivot = pivot.transform;
        }

        CameraDemo demo = t.gameObject.AddComponent<CameraDemo>();
        demo.mode = mode;
        demo.cam = cam;
        demo.backCam = back;
        demo.mover = mover;
        demo.target = mover;
        demo.smoothCam = smooth;
        demo.smoothPivot = smoothPivot;
    }

    private static Camera OwnCamera(Transform parent, string name, Vector3 pos, float size, CameraDemo.Mode mode)
    {
        bool orthographic = !(mode == CameraDemo.Mode.Projection || mode == CameraDemo.Mode.Fov);
        return DemoKit.OwnCamera(parent, name, pos, size, orthographic);
    }

    private static void CameraKit(Transform t, out SpriteRenderer groundSprite, out SpriteRenderer playerSprite)
    {
        DemoKit.CameraKit(t, out groundSprite, out playerSprite);
    }

    // ---------------- shared bits ----------------
    private static LineRenderer Line(Transform parent, string name, int order)
    {
        return DemoKit.Line(parent, name, order);
    }

    private static string ControllerPath(string name)
    {
        string path = "Assets/Animations/W04_AnimationCamera/" + name + ".controller";
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path) == null)
            Debug.LogWarning("[SlideDemoBuilder] missing animator controller " + path);
        return path;
    }

    private static string ClipPath(string name)
    {
        switch (name)
        {
            case "DemoKeyframes": return "Assets/Animations/W04_AnimationCamera/Demo_Keyframes.anim";
            case "DemoChannels": return "Assets/Animations/W04_AnimationCamera/Demo_Channels.anim";
            case "DemoPriority": return "Assets/Animations/W04_AnimationCamera/Demo_Priority.anim";
            default: return "Assets/Animations/W04_AnimationCamera/Demo_A.anim";
        }
    }
}
