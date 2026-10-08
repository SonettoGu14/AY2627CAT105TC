using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// W4 lecture content - "Animations and 2D Art" (sprites, Animation, Animator, Camera).
///
/// Builds Assets/Scenes/W4Lab.unity: a small 2D platformer that reviews the W3 controller
/// and adds the W4 material on top - sprite flip-book animation, an Animator state machine
/// driven from code, a property animation, and an orthographic follow camera.
///
/// Note the shape of Build(): it calls W03LabBuilder.CreatePlayer / MakeOneWayPlatform -
/// W4 visibly builds ON the W3 work rather than re-implementing it.
/// </summary>
public static class W04LabBuilder
{
    // Coin layout. Kept at class scope so Build() can report the HUD total without
    // widening BuildGameplay's signature.
    private static readonly Vector3[] CoinPositions =
    {
        new Vector3(-8f, 0.2f, 0f),
        new Vector3(-6f, 1.5f, 0f),
        new Vector3(-1.5f, 0.3f, 0f),
        new Vector3(3f, 2.2f, 0f),
        new Vector3(7.5f, 3.6f, 0f),
        new Vector3(13.5f, 0.3f, 0f)
    };

    // Annotation root from the most recent BuildGameplay call, so Build() can wire it
    // into the HUD (the fixed signature cannot return it).
    private static GameObject _annotations;

    [MenuItem("CAT105TC/W04 Animation & Camera/Build W4 Lab (Platformer)")]
    public static void BuildMenu()
    {
        Build();
    }

    // ==================================================================================
    //  W4 scene pieces
    // ==================================================================================

    private static GameObject MakeCoin(Transform parent, Vector3 pos)
    {
        GameObject go = LabKit.Child(parent, "Coin", pos);
        go.tag = "Coin";
        go.layer = LabKit.Layer("Trigger");

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.24f;

        GameObject vis = LabKit.Child(go.transform, "Visual", Vector3.zero);
        vis.layer = go.layer;
        vis.transform.localScale = Vector3.one * 0.5f;
        LabKit.AddSprite(vis, LabKit.Circle, LabKit.Gold, 5);

        Collectible2D c = go.AddComponent<Collectible2D>();
        c.burstSprite = LabKit.Circle;
        c.burstColor = LabKit.Gold;
        return go;
    }

    private static GameObject MakeGoal(Transform parent, Vector3 pos)
    {
        GameObject go = LabKit.Child(parent, "Goal", pos);
        go.tag = "Goal";
        go.layer = LabKit.Layer("Trigger");

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.4f, 2.2f);
        col.offset = new Vector2(0f, 1.1f);

        GameObject pole = LabKit.Child(go.transform, "Pole", new Vector3(0f, 1.1f, 0f));
        pole.transform.localScale = new Vector3(0.12f, 2.2f, 1f);
        LabKit.AddSprite(pole, LabKit.White, new Color(0.88f, 0.90f, 0.95f), 4);

        GameObject flag = LabKit.Child(go.transform, "Flag", new Vector3(0.5f, 1.75f, 0f));
        flag.transform.localScale = new Vector3(0.9f, 0.55f, 1f);
        LabKit.AddSprite(flag, LabKit.White, new Color(0.95f, 0.35f, 0.42f), 4);

        go.AddComponent<GoalZone2D>();
        return go;
    }

    private static void MakeMovingPlatform(Transform parent, Vector3 pos)
    {
        GameObject go = LabKit.Child(parent, "MovingPlatform", pos);
        go.layer = LabKit.Layer("Ground");
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3f, 0.5f);
        GameObject vis = LabKit.Child(go.transform, "Visual", Vector3.zero);
        vis.layer = go.layer;
        vis.transform.localScale = new Vector3(3f, 0.5f, 1f);
        LabKit.AddSprite(vis, LabKit.White, new Color(0.56f, 0.46f, 0.86f), 1);
        MovingPlatform2D mp = go.AddComponent<MovingPlatform2D>();
        mp.travel = new Vector2(8f, 0f);
        mp.speed = 1f;
    }

    private static void MakePropertyDemo(Transform parent, Vector3 pos)
    {
        GameObject root = LabKit.Child(parent, "PropertyDemo", pos);
        GameObject vis = LabKit.Child(root.transform, "Visual", Vector3.zero);
        LabKit.AddSprite(vis, LabKit.White, Color.white, 2);
        Animator anim = root.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CharacterRig.PropertyControllerPath);
        anim.applyRootMotion = false;
        PropertyAnimationDemo2D demo = root.AddComponent<PropertyAnimationDemo2D>();
        demo.animator = anim;
    }

    // ==================================================================================
    //  Build
    // ==================================================================================

    public static void Build()
    {
        LabKit.SetupSharedAssets();
        CharacterRig.Build();             // W4 owns the character animation

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        PlayerController2D player;
        Camera cam;
        BuildGameplay(null, out player, out cam);

        // --- HUD -----------------------------------------------------------------------
        LabHud hud;
        LabKit.BuildHud("CAT105TC  W4 Lab  -  Animations & Camera", false, out hud);
        hud.player = player;
        hud.targetCamera = cam;
        hud.annotationRoot = _annotations;
        hud.totalCoins = CoinPositions.Length;
        hud.help = "A / D  move      Space  jump\nS + Space  drop through the one-way platform\nP  replay property animation\nR  restart      T  labels      C  camera     H  hide help";

        EditorSceneManager.SaveScene(scene, LabKit.W4ScenePath);
        LabKit.AddSceneToBuildSettings(LabKit.W4ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[W04LabBuilder] W4Lab built.");
    }

    /// <summary>
    /// Builds the W4 platformer - environment, player, coins, goal, follow camera and the
    /// world-space teaching labels - under <paramref name="parent"/> (null = scene root).
    /// No HUD: SlideDeckBuilder drops this behind the slide deck and supplies its own UI,
    /// while Build() adds the lab HUD on top.
    /// </summary>
    public static void BuildGameplay(Transform parent, out PlayerController2D player, out Camera cam)
    {
        GameObject env = new GameObject("Environment");
        GameObject gameplay = new GameObject("Gameplay");
        GameObject annotations = new GameObject("Annotations");
        if (parent != null)
        {
            env.transform.SetParent(parent, false);
            gameplay.transform.SetParent(parent, false);
            annotations.transform.SetParent(parent, false);
        }
        _annotations = annotations;
        LabKit.MakeLight(null);

        // --- ground / platforms --------------------------------------------------------
        LabKit.Platform(env.transform, "Ground_Start", -10f, -1f, 12f, 1f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Platform_1", -1.5f, -1f, 3f, 1f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Ground_Mid", 13.5f, -1f, 7f, 1f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Float_A", 3f, 1.2f, 3.5f, 0.5f, LabKit.PlatformColor, "Ground");
        LabKit.Platform(env.transform, "Float_B", 7.5f, 2.6f, 3.5f, 0.5f, LabKit.PlatformColor, "Ground");
        LabKit.Platform(env.transform, "Float_C", 19.5f, 1.2f, 3f, 0.5f, LabKit.PlatformColor, "Ground");
        LabKit.Platform(env.transform, "Wall_L", -16.5f, 2f, 1f, 8f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Wall_R", 22f, 2f, 1f, 8f, LabKit.GroundColor, "Ground");

        // W3 platform, reused by W4
        W03LabBuilder.MakeOneWayPlatform(env.transform, "OneWayPlatform", new Vector3(-6f, 0.6f, 0f), 3f, 0.4f);

        MakeMovingPlatform(env.transform, new Vector3(1f, -0.75f, 0f));
        MakePropertyDemo(env.transform, new Vector3(-9f, 3.1f, 0f));

        // --- player: the W3 body, plus the W4 Animator --------------------------------
        GameObject playerGO = W03LabBuilder.CreatePlayer(gameplay.transform,
            new Vector3(-13f, -0.5f, 0f), new Vector3(-13f, -0.5f, 0f), out player);

        GameObject playerVisual = playerGO.transform.Find("Visual").gameObject;
        Animator animator = playerVisual.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CharacterRig.PlayerControllerPath);
        animator.applyRootMotion = false;
        player.animator = animator;

        // --- coins ---------------------------------------------------------------------
        for (int i = 0; i < CoinPositions.Length; i++) MakeCoin(gameplay.transform, CoinPositions[i]);

        MakeGoal(gameplay.transform, new Vector3(16f, 0f, 0f));
        W03LabBuilder.MakeKillZone(gameplay.transform, new Vector3(2f, -9f, 0f), 80f, 2f, player);

        // --- camera (W4: orthographic + follow) ---------------------------------------
        cam = W03LabBuilder.MakeFollowCamera(null, new Vector3(-13f, 1f, -10f), 6f, player.transform);

        // --- teaching labels -----------------------------------------------------------
        LabKit.WorldLabel(annotations.transform, "Player\n(Rigidbody2D + Animator)", new Vector3(-13f, 1.3f, 0f), LabKit.Accent);
        LabKit.WorldLabel(annotations.transform, "One-way platform\n(PlatformEffector2D)", new Vector3(-6f, 2.1f, 0f), new Color(0.5f, 0.95f, 0.88f));
        LabKit.WorldLabel(annotations.transform, "Ground  (static Collider2D)", new Vector3(-10f, 0.7f, 0f), new Color(0.75f, 0.8f, 0.9f));
        LabKit.WorldLabel(annotations.transform, "Coin  (Is Trigger)", new Vector3(3f, 2.9f, 0f), LabKit.Gold);
        LabKit.WorldLabel(annotations.transform, "Moving platform\n(Kinematic Rigidbody2D)", new Vector3(5f, 0.2f, 0f), new Color(0.7f, 0.62f, 0.98f));
        LabKit.WorldLabel(annotations.transform, "Property animation\n(press P)", new Vector3(-9f, 4.2f, 0f), new Color(0.95f, 0.55f, 0.55f));
        LabKit.WorldLabel(annotations.transform, "Goal  (Is Trigger)", new Vector3(16f, 2.7f, 0f), new Color(0.98f, 0.5f, 0.55f));
        LabKit.WorldLabel(annotations.transform, "Kill zone  (Is Trigger)\nrespawns the player", new Vector3(2f, -7f, 0f), new Color(0.98f, 0.6f, 0.4f));
    }
}
