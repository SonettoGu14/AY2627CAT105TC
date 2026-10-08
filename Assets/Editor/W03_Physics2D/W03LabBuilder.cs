using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// W3 lecture content - "Physics in Unity".
///
/// Builds Assets/Scenes/W3Lab.unity: a walk-through playground with six labelled review
/// stations (Collision2D + Tag, Trigger2D, Raycast + LayerMask, PlatformEffector2D,
/// Rigidbody2D modes, statics + Physics Material) and a gamified checklist.
///
/// The player here is a plain sprite driven by PlayerController2D - deliberately WITHOUT
/// an Animator, because animation is not taught until W4. The W4 lab calls
/// <see cref="CreatePlayer"/> and then adds its own Animator on top, which is exactly the
/// "W4 reviews W3, then extends it" story the lecture tells.
/// </summary>
public static class W03LabBuilder
{
    [MenuItem("CAT105TC/W03 Physics2D/Build W3 Lab (Physics Playground)")]
    public static void BuildMenu()
    {
        Build();
    }

    // ==================================================================================
    //  W3 scene pieces
    // ==================================================================================

    /// <summary>The shared 2D character body, W3 edition: no Animator.</summary>
    public static GameObject CreatePlayer(Transform parent, Vector3 pos, Vector3 spawn, out PlayerController2D controller)
    {
        GameObject root = LabKit.Child(parent, "Player", pos);
        root.tag = "Player";
        root.layer = LabKit.Layer("Player");

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D col = root.AddComponent<CapsuleCollider2D>();
        col.direction = CapsuleDirection2D.Vertical;
        col.size = new Vector2(0.52f, 0.98f);
        col.offset = new Vector2(0f, 0.49f);
        col.sharedMaterial = LabKit.Slick;

        GameObject vis = LabKit.Child(root.transform, "Visual", Vector3.zero);
        vis.layer = root.layer;
        SpriteRenderer sr = LabKit.AddSprite(vis, LabKit.CharacterSprite("idle"), Color.white, 10);

        controller = root.AddComponent<PlayerController2D>();
        controller.spriteRenderer = sr;
        controller.groundLayer = LabKit.GroundMask;   // no Animator on purpose (W3)
        controller.SetSpawn(spawn);

        root.transform.position = pos;
        return root;
    }

    /// <summary>
    /// Camera + follow. The W3 playground is 54 units wide, so a static camera loses the
    /// player within a second or two - following is needed for the lab to be usable at all.
    /// W4 reuses this same helper (and its lecture explains what is going on inside it).
    /// </summary>
    public static Camera MakeFollowCamera(Transform parent, Vector3 pos, float orthoSize, Transform target)
    {
        Camera cam = LabKit.MakeCamera(parent, pos, orthoSize);
        CameraFollow2D follow = cam.gameObject.AddComponent<CameraFollow2D>();
        follow.target = target;
        follow.captureOffsetAtStart = true;
        return cam;
    }

    public static GameObject MakeOneWayPlatform(Transform parent, string name, Vector3 pos, float w, float h)
    {
        GameObject root = LabKit.Child(parent, name, pos);
        root.layer = LabKit.Layer("OneWay");
        BoxCollider2D col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);
        col.usedByEffector = true;
        root.AddComponent<PlatformEffector2D>();

        GameObject vis = LabKit.Child(root.transform, "Visual", Vector3.zero);
        vis.layer = root.layer;
        vis.transform.localScale = new Vector3(w, h, 1f);
        LabKit.AddSprite(vis, LabKit.White, new Color(0.32f, 0.80f, 0.74f), 1);

        root.AddComponent<OneWayPlatform2D>();
        return root;
    }

    private static LineRenderer MakeLineRenderer(Transform parent, string name)
    {
        GameObject go = LabKit.Child(parent, name, new Vector3(0f, 0.5f, 0f));
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = 0.05f;
        lr.useWorldSpace = true;
        lr.sortingOrder = 150;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }

    private static ConceptZone2D MakeConceptZone(Transform parent, string name, string title, float cx, float cy, float w, float h)
    {
        GameObject go = LabKit.Child(parent, name, new Vector3(cx, cy, 0f));
        go.layer = LabKit.Layer("Trigger");
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(w, h);
        GameObject vis = LabKit.Child(go.transform, "Marker", Vector3.zero);
        vis.layer = go.layer;
        vis.transform.localScale = new Vector3(w, h, 1f);
        ConceptZone2D zone = go.AddComponent<ConceptZone2D>();
        zone.conceptTitle = title;
        zone.markerRenderer = LabKit.AddSprite(vis, LabKit.White, LabKit.ZoneColor, -1);
        return zone;
    }

    private static void MakeRigidbodyModeDemo(Transform parent, string name, Vector3 pos, RigidbodyModeDemo2D.Mode mode, string label)
    {
        GameObject go = LabKit.Child(parent, name, pos);
        go.layer = LabKit.Layer("Ground");
        go.AddComponent<Rigidbody2D>();
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);
        GameObject vis = LabKit.Child(go.transform, "Visual", Vector3.zero);
        vis.transform.localScale = Vector3.one * 0.8f;
        Color c = mode == RigidbodyModeDemo2D.Mode.Dynamic ? new Color(0.55f, 0.85f, 0.6f)
                : mode == RigidbodyModeDemo2D.Mode.Kinematic ? new Color(0.6f, 0.75f, 0.98f)
                : new Color(0.85f, 0.85f, 0.9f);
        LabKit.AddSprite(vis, LabKit.White, c, 3);
        RigidbodyModeDemo2D demo = go.AddComponent<RigidbodyModeDemo2D>();
        demo.mode = mode;
        demo.displayName = label;
    }

    // ==================================================================================
    //  Build
    // ==================================================================================

    public static void Build()
    {
        LabKit.SetupSharedAssets();       // W3 does NOT build the character animation

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject env = new GameObject("Environment");
        GameObject gameplay = new GameObject("Gameplay");
        GameObject annotations = new GameObject("Annotations");
        LabKit.MakeLight(null);

        LabKit.Platform(env.transform, "Ground", 13f, -1f, 54f, 1f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Wall_L", -14.5f, 2f, 1f, 8f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Wall_R", 40.5f, 2f, 1f, 8f, LabKit.GroundColor, "Ground");

        // --- player --------------------------------------------------------------------
        PlayerController2D player;
        GameObject playerGO = CreatePlayer(gameplay.transform, new Vector3(-11f, -0.5f, 0f), new Vector3(-11f, -0.5f, 0f), out player);

        // colour change on collision (the player itself)
        CollisionColorDemo2D colour = playerGO.AddComponent<CollisionColorDemo2D>();
        colour.target = player.spriteRenderer;
        colour.specialTag = "Finish";

        // raycast visualisation (two LineRenderers as children of the player)
        RaycastDemo2D raycast = playerGO.AddComponent<RaycastDemo2D>();
        raycast.facingSprite = player.spriteRenderer;
        raycast.forwardDistance = 3f;
        raycast.downDistance = 0.7f;
        raycast.obstacleMask = LabKit.GroundMask;
        raycast.forwardLine = MakeLineRenderer(playerGO.transform, "ForwardRay");
        raycast.downLine = MakeLineRenderer(playerGO.transform, "DownRay");

        // --- review stations -----------------------------------------------------------
        List<ConceptZone2D> zones = new List<ConceptZone2D>();
        List<string> titles = new List<string>();

        // 1 - Collision2D + Tag  (x ~ -8)
        LabKit.Platform(env.transform, "Pillar_A", -8f, 0.1f, 1f, 1.2f, new Color(0.85f, 0.5f, 0.4f), "Ground");
        LabKit.Platform(env.transform, "Pillar_Special", -6.4f, 0.1f, 1f, 1.2f, new Color(0.95f, 0.82f, 0.3f), "Ground").tag = "Finish";
        zones.Add(MakeConceptZone(gameplay.transform, "CollisionZD", "Collision2D event (+ Tag)", -8f, 0.9f, 4.5f, 3.4f));
        titles.Add("Collision2D event (+ Tag)");
        LabKit.WorldLabel(annotations.transform, "1. Collision2D event\ncolour on hit  /  + Tag match", new Vector3(-8f, 2.6f, 0f), new Color(0.98f, 0.7f, 0.6f));

        // 2 - Trigger2D  (x ~ -2.5)
        GameObject tz = LabKit.Child(gameplay.transform, "TriggerZone", new Vector3(-2.5f, 0.7f, 0f));
        tz.layer = LabKit.Layer("Trigger");
        BoxCollider2D tzCol = tz.AddComponent<BoxCollider2D>();
        tzCol.isTrigger = true;
        tzCol.size = new Vector2(3f, 2.4f);
        GameObject tzVis = LabKit.Child(tz.transform, "Visual", Vector3.zero);
        tzVis.layer = tz.layer;
        tzVis.transform.localScale = new Vector3(3f, 2.4f, 1f);
        TriggerZone2D tzComp = tz.AddComponent<TriggerZone2D>();
        tzComp.visual = LabKit.AddSprite(tzVis, LabKit.White, LabKit.ZoneColor, 1);
        zones.Add(MakeConceptZone(gameplay.transform, "TriggerZD", "Trigger2D event", -2.5f, 0.9f, 3.4f, 3.4f));
        titles.Add("Trigger2D event");
        LabKit.WorldLabel(annotations.transform, "2. Trigger2D event\nnon-solid detection zone", new Vector3(-2.5f, 2.6f, 0f), new Color(0.55f, 0.8f, 1f));

        // 3 - Raycast + LayerMask  (x ~ 4)
        GameObject ghost = LabKit.Platform(env.transform, "GhostWall", 4.6f, 0.1f, 0.6f, 1.6f, new Color(0.5f, 0.5f, 0.5f, 0.45f), "Ground");
        ghost.layer = 2;                                        // layer 2 = "Ignore Raycast"
        ghost.GetComponent<BoxCollider2D>().isTrigger = true;    // walk through it; the forward ray passes through it
        LabKit.Platform(env.transform, "SolidWall", 6.6f, 0.1f, 0.6f, 1.6f, new Color(0.45f, 0.55f, 0.7f), "Ground");
        zones.Add(MakeConceptZone(gameplay.transform, "RaycastZD", "Physics2D.Raycast + LayerMask", 4.5f, 0.9f, 5f, 3.4f));
        titles.Add("Physics2D.Raycast + LayerMask");
        LabKit.WorldLabel(annotations.transform, "3. Physics2D.Raycast + LayerMask\ngrey wall is on 'Ignore Raycast'", new Vector3(4.5f, 3.0f, 0f), new Color(0.6f, 1f, 0.7f));

        // 4 - PlatformEffector2D  (x ~ 12)
        MakeOneWayPlatform(env.transform, "OneWayPlatform", new Vector3(12f, 1.1f, 0f), 3f, 0.4f);
        zones.Add(MakeConceptZone(gameplay.transform, "EffectorZD", "PlatformEffector2D", 12f, 0.9f, 4f, 3.6f));
        titles.Add("PlatformEffector2D");
        LabKit.WorldLabel(annotations.transform, "4. PlatformEffector2D\njump up through it", new Vector3(12f, 2.9f, 0f), new Color(0.5f, 0.95f, 0.88f));

        // 5 - Rigidbody2D modes  (x ~ 19..23)
        MakeRigidbodyModeDemo(gameplay.transform, "Demo_Dynamic", new Vector3(18.5f, 3.2f, 0f), RigidbodyModeDemo2D.Mode.Dynamic, "Dynamic");
        MakeRigidbodyModeDemo(gameplay.transform, "Demo_Kinematic", new Vector3(21f, 3.2f, 0f), RigidbodyModeDemo2D.Mode.Kinematic, "Kinematic");
        MakeRigidbodyModeDemo(gameplay.transform, "Demo_Static", new Vector3(23.5f, 3.2f, 0f), RigidbodyModeDemo2D.Mode.Static, "Static");
        zones.Add(MakeConceptZone(gameplay.transform, "ModesZD", "Rigidbody2D modes", 21f, 0.9f, 7f, 5f));
        titles.Add("Rigidbody2D modes");
        LabKit.WorldLabel(annotations.transform, "5. Rigidbody2D modes\nDynamic / Kinematic / Static", new Vector3(21f, 3.9f, 0f), new Color(0.9f, 0.7f, 1f));

        // 6 - static fields + Physics Material  (x ~ 29..33)
        GameObject ball = LabKit.Child(gameplay.transform, "BouncyBall", new Vector3(29f, 4.5f, 0f));
        ball.layer = LabKit.Layer("Ground");
        Rigidbody2D ballRb = ball.AddComponent<Rigidbody2D>();
        ballRb.gravityScale = 2.5f;
        CircleCollider2D ballCol = ball.AddComponent<CircleCollider2D>();
        ballCol.radius = 0.28f;
        ballCol.sharedMaterial = LabKit.Bouncy;
        GameObject ballVis = LabKit.Child(ball.transform, "Visual", Vector3.zero);
        ballVis.transform.localScale = Vector3.one * 0.6f;
        LabKit.AddSprite(ballVis, LabKit.Circle, new Color(0.98f, 0.45f, 0.55f), 6);
        for (int i = 0; i < 3; i++)
        {
            GameObject pickup = LabKit.Child(gameplay.transform, "StaticPickup_" + i, new Vector3(31f + i * 1.6f, 0.4f, 0f));
            pickup.layer = LabKit.Layer("Trigger");
            CircleCollider2D pcol = pickup.AddComponent<CircleCollider2D>();
            pcol.isTrigger = true;
            pcol.radius = 0.24f;
            GameObject pvis = LabKit.Child(pickup.transform, "Visual", Vector3.zero);
            pvis.layer = pickup.layer;
            pvis.transform.localScale = Vector3.one * 0.5f;
            LabKit.AddSprite(pvis, LabKit.Circle, LabKit.Gold, 5);
            pickup.AddComponent<StaticCounter2D>();
        }
        zones.Add(MakeConceptZone(gameplay.transform, "StaticZD", "static fields + Physic Material", 32f, 0.9f, 6f, 6.5f));
        titles.Add("static fields + Physic Material");
        LabKit.WorldLabel(annotations.transform, "6. static fields + Physic Material\nbouncy ball  /  class-level counter", new Vector3(32f, 4.3f, 0f), new Color(1f, 0.8f, 0.55f));

        // --- kill zone ------------------------------------------------------------------
        MakeKillZone(gameplay.transform, new Vector3(13f, -9f, 0f), 80f, 2f, player);

        // --- camera (must follow - the playground is 54 units wide) --------------------
        Camera cam = MakeFollowCamera(null, new Vector3(-11f, 1f, -10f), 6f, player.transform);

        // --- review manager -------------------------------------------------------------
        GameObject reviewGO = new GameObject("PlaygroundReview");
        PlaygroundReview review = reviewGO.AddComponent<PlaygroundReview>();
        review.zones = zones.ToArray();
        review.titles = titles.ToArray();
        for (int i = 0; i < zones.Count; i++) zones[i].review = review;

        // --- HUD ------------------------------------------------------------------------
        LabHud hud;
        LabKit.BuildHud("CAT105TC  W3 Lab  -  Physics 2D Playground", true, out hud);
        hud.player = player;
        hud.targetCamera = cam;
        hud.annotationRoot = annotations;
        hud.totalCoins = 2;
        hud.help = "A / D  move      Space  jump\nS + Space  drop through one-way platform\nWalk through all six review stations\nR  restart      T  labels      C  camera     H  help";

        EditorSceneManager.SaveScene(scene, LabKit.W3ScenePath);
        LabKit.AddSceneToBuildSettings(LabKit.W3ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[W03LabBuilder] W3Lab built.");
    }

    public static GameObject MakeKillZone(Transform parent, Vector3 pos, float w, float h, PlayerController2D player)
    {
        GameObject go = LabKit.Child(parent, "KillZone", pos);
        go.layer = LabKit.Layer("Hazard");
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(w, h);
        KillZone2D kz = go.AddComponent<KillZone2D>();
        kz.player = player;
        return go;
    }
}
