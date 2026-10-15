using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// W5's lab STARTER - the scene a lab session begins from.
///
/// The lecture's worked example, at a size a student can reproduce in one session: the W4 character
/// (body + Animator) on a floor with two platforms, four coins, one hazard, and a top-right HUD made
/// of nothing but a Canvas, three Image icons (each the character's own sprite) and one Text. No HUD
/// panel and no key hints - only the components the slides are about. W05Playground is the same
/// week's full version.
///
/// So the week reads: W5Lab (start here) -> add UI -> W05Playground (what it looks like done).
/// </summary>
public static class W05LabBuilder
{
    [MenuItem("CAT105TC/W05 UI/Build W5 Lab (Starter)")]
    public static void BuildMenu()
    {
        Build();
    }

    public static void Build()
    {
        LabKit.SetupSharedAssets();
        CharacterRig.Build();                 // the character's clips and controllers

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        PlayerController2D player;
        BuildGameplay(null, out player, out Camera _);
        HudLives lives = BuildHudUi(null, player);

        // The hazard lives in the world and calls into the UI; wire the two together by name so the
        // gameplay builder stays reusable without a UI.
        GameObject hazard = GameObject.Find("Hazard");
        if (hazard != null)
        {
            Hazard2D hz = hazard.GetComponent<Hazard2D>();
            if (hz != null) hz.lives = lives;
        }

        EditorSceneManager.SaveScene(scene, LabKit.W05LabPath);
        LabKit.AddSceneToBuildSettings(LabKit.W05LabPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[W05LabBuilder] W05Lab (starter) built.");
    }

    /// <summary>
    /// A floor and two platforms, the character, and a follow camera - nothing else. Kept public so
    /// a later week can start from the same body.
    /// </summary>
    public static void BuildGameplay(Transform parent, out PlayerController2D player, out Camera cam)
    {
        GameObject env = new GameObject("Environment");
        if (parent != null) env.transform.SetParent(parent, false);
        LabKit.MakeLight(parent);

        LabKit.Platform(env.transform, "Ground", 0f, -1f, 20f, 1f, LabKit.GroundColor, "Ground");
        LabKit.Platform(env.transform, "Platform_A", -4f, 1.4f, 3.5f, 0.5f, LabKit.PlatformColor, "Ground");
        LabKit.Platform(env.transform, "Platform_B", 4f, 2.8f, 3.5f, 0.5f, LabKit.PlatformColor, "Ground");

        GameObject playerGO = W03PlaygroundBuilder.CreatePlayer(parent, new Vector3(-7f, -0.5f, 0f),
                                                                new Vector3(-7f, -0.5f, 0f), out player);

        // the W4 animator, so the character looks alive while the students work on the UI
        GameObject visual = playerGO.transform.Find("Visual").gameObject;
        Animator animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CharacterRig.PlayerControllerPath);
        animator.applyRootMotion = false;
        player.animator = animator;

        // ---- the two things the HUD will report on ----

        // coins: W4's Collectible2D, which raises Picked so a HUD can count them
        Vector3[] coinPositions =
        {
            new Vector3(-4f, 2.6f, 0f),
            new Vector3(0f, 0.2f, 0f),
            new Vector3(4f, 4.0f, 0f),
            new Vector3(7.5f, 0.2f, 0f)
        };
        foreach (Vector3 pos in coinPositions)
        {
            GameObject coin = LabKit.Child(env.transform, "Coin", pos);
            coin.tag = "Coin";
            coin.layer = LabKit.Layer("Trigger");
            CircleCollider2D col = coin.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.24f;
            GameObject vis = LabKit.Child(coin.transform, "Visual", Vector3.zero);
            vis.layer = coin.layer;
            vis.transform.localScale = Vector3.one * 0.5f;
            LabKit.AddSprite(vis, LabKit.Circle, LabKit.Gold, 5);
            Collectible2D pickup = coin.AddComponent<Collectible2D>();
            pickup.burstSprite = LabKit.Circle;
            pickup.burstColor = LabKit.Gold;
        }

        // one hazard: touching it costs a life
        GameObject hazard = LabKit.Child(env.transform, "Hazard", new Vector3(9f, -1f, 0f));
        hazard.layer = LabKit.Layer("Trigger");
        BoxCollider2D hz = hazard.AddComponent<BoxCollider2D>();
        hz.isTrigger = true;
        hz.size = new Vector2(1.6f, 2f);
        GameObject hv = LabKit.Child(hazard.transform, "Visual", Vector3.zero);
        hv.layer = hazard.layer;
        hv.transform.localScale = new Vector3(1.6f, 2f, 1f);
        LabKit.AddSprite(hv, LabKit.White, new Color(0.9f, 0.25f, 0.35f, 0.5f), 1);
        hazard.AddComponent<Hazard2D>();

        cam = W03PlaygroundBuilder.MakeFollowCamera(parent, new Vector3(-7f, 1f, -10f), 5f, player.transform);
    }

    /// <summary>
    /// The week's material in its smallest form: a Canvas, three Image icons (each the character's own
    /// sprite) for lives, and one Text for the coins. Layout rule: everything anchors to the TOP-RIGHT,
    /// where nothing else lives.
    /// </summary>
    private static HudLives BuildHudUi(Transform parent, PlayerController2D player)
    {
        GameObject root = new GameObject("HUD");
        if (parent != null) root.transform.SetParent(parent, false);

        Canvas canvas = UiKit.MakeCanvas(root.transform, "HUD Canvas", 0);
        Transform ct = canvas.transform;

        // lives: three icons, the rightmost one disappearing first as they are lost
        Sprite face = LabKit.CharacterSprite("idle");
        const int LifeCount = 3;
        Image[] icons = new Image[LifeCount];
        for (int i = 0; i < LifeCount; i++)
        {
            RectTransform r = UiKit.MakeRect(ct, "Life" + (i + 1), UiKit.TopRight,
                                             new Vector2(-40f - (LifeCount - 1 - i) * 68f, -28f),
                                             new Vector2(56f, 72f));
            Image img = UiKit.MakeImage(r, Color.white, false);
            img.sprite = face;             // the character's own sprite, as an icon
            img.preserveAspect = true;
            icons[i] = img;
        }

        // coins: one Text, kept up to date by HudCoinCounter
        TMP_Text coins = UiKit.MakeText(ct, "Coins", UiKit.TopRight,
                                        new Vector2(-40f - LifeCount * 68f - 24f, -40f),
                                        new Vector2(360f, 48f), 34f, UiPalette.Paper,
                                        TextAlignmentOptions.TopRight);
        coins.text = "Coins  0";

        HudLives lives = root.AddComponent<HudLives>();
        lives.icons = icons;
        lives.player = player;
        lives.respawnPoint = new Vector2(-7f, -0.5f);

        HudCoinCounter counter = root.AddComponent<HudCoinCounter>();
        counter.label = coins;
        return lives;
    }
}
