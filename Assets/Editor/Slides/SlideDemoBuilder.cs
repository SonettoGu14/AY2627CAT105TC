using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

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
        // The demo clips/controllers and the shared sprites/layers must exist even when the W4 lab
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
            W04LabBuilder.BuildGameplay(t, out labPlayer, out labCam);
            p.produced = labCam;
            t.gameObject.AddComponent<PlatformerDemo>();   // so the readout panel has something to show
        };
        return p;
    }

    // ==================================================================================
    //  Demo construction
    // ==================================================================================

    private static GameObject Visual(Transform parent, string name, Sprite sprite, Color color, Vector2 pos, Vector2 scale, int order)
    {
        GameObject go = LabKit.Child(parent, name, pos);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        LabKit.AddSprite(go, sprite, color, order);
        return go;
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
        GameObject go = LabKit.Child(parent, name, pos);
        Camera c = go.AddComponent<Camera>();
        c.orthographic = !(mode == CameraDemo.Mode.Projection || mode == CameraDemo.Mode.Fov);
        c.orthographicSize = size;
        c.fieldOfView = 60f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color32(0x0E, 0x14, 0x20, 0xFF);
        c.nearClipPlane = -50f;
        c.farClipPlane = 100f;
        return c;
    }

    private static void CameraKit(Transform t, out SpriteRenderer groundSprite, out SpriteRenderer playerSprite)
    {
        GameObject ground = LabKit.Platform(t, "KitGround", -2.5f, -2f, 4f, 0.6f, Orange, "Ground");
        groundSprite = ground.GetComponentInChildren<SpriteRenderer>();
        GameObject player = Visual(t, "KitPlayer", LabKit.CharacterSprite("idle"), Color.white, new Vector2(1.5f, -1.2f), Vector2.one, 2);
        player.layer = LabKit.Layer("Player");
        playerSprite = player.GetComponent<SpriteRenderer>();
    }

    // ---------------- shared bits ----------------
    private static LineRenderer Line(Transform parent, string name, int order)
    {
        GameObject go = LabKit.Child(parent, name, Vector3.zero);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = 0.08f;
        lr.useWorldSpace = true;
        lr.sortingOrder = order;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
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
