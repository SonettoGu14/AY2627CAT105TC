using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// W5 lecture content - "Camera and UI".
///
/// Builds Assets/Scenes/W05Lab.unity: **the W4 platformer with a UI layer on top**. Note the shape of
/// BuildGameplay: it calls W04LabBuilder.BuildGameplay and then adds a Health component and a hazard,
/// so the lab literally shows the previous weeks' work being combined rather than re-implemented.
/// That is the lecture's own story ("试着将东西结合到一起").
///
/// The UI is the week's material, and every piece of it is a slide: a Slider health bar, a Text,
/// a Canvas Group dialogue box, a Button that opens a bag whose slots are Instantiate'd, a
/// RectMask2D-clipped grid, and a pause panel.
/// </summary>
public static class W05LabBuilder
{
    [MenuItem("CAT105TC/W05 UI/Build W5 Lab (UI Playground)")]
    public static void BuildMenu()
    {
        Build();
    }

    public static void Build()
    {
        LabKit.SetupSharedAssets();
        CharacterRig.Build();             // the player's animator, the same one W4 uses

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        PlayerController2D player;
        Camera cam;
        BuildGameplay(null, out player, out cam);

        BuildUiLayer(null, player);

        LabHud hud;
        LabKit.BuildHud("CAT105TC  W05 Lab  -  UI", false, out hud);
        hud.player = player;
        hud.targetCamera = cam;
        hud.totalCoins = W04LabBuilder.CoinCount;
        hud.help = "A / D  move      Space  jump\nB  bag      P  pause      R  restart\nT  labels      C  camera      H  hide help";

        EditorSceneManager.SaveScene(scene, LabKit.W05ScenePath);
        LabKit.AddSceneToBuildSettings(LabKit.W05ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[W05LabBuilder] W05Lab built.");
    }

    /// <summary>
    /// The world: the W4 platformer, plus W5's Health and a hazard. No UI - SlideDeckBuilder drops
    /// this behind the W5 deck and supplies its own chrome, exactly as it does for W4.
    /// </summary>
    public static void BuildGameplay(Transform parent, out PlayerController2D player, out Camera cam)
    {
        PlayerController2D w4Player;
        Camera w4Cam;
        W04LabBuilder.BuildGameplay(parent, out w4Player, out w4Cam);
        player = w4Player;
        cam = w4Cam;

        // W5: the thing the health bar shows
        Health health = player.gameObject.AddComponent<Health>();
        health.max = 100;

        Transform env = Find(parent, "Environment");
        Transform annotations = Find(parent, "Annotations");

        // a hazard: "add triggers for objects that can harm the player"
        GameObject hazard = LabKit.Child(env, "Hazard", new Vector3(5f, -1.0f, 0f));
        hazard.layer = LabKit.Layer("Trigger");
        BoxCollider2D hz = hazard.AddComponent<BoxCollider2D>();
        hz.isTrigger = true;
        hz.size = new Vector2(2.4f, 1.6f);
        hz.offset = new Vector2(0f, 0.8f);
        GameObject hv = LabKit.Child(hazard.transform, "Visual", Vector3.zero);
        hv.layer = hazard.layer;
        hv.transform.localScale = new Vector3(2.4f, 1.6f, 1f);
        LabKit.AddSprite(hv, LabKit.White, new Color(0.9f, 0.25f, 0.35f, 0.35f), 1);
        DamageZone2D damage = hazard.AddComponent<DamageZone2D>();
        damage.health = health;
        damage.damage = 20;

        if (annotations != null)
        {
            LabKit.WorldLabel(annotations, "Hazard  (Is Trigger)\ncalls Health.Take()", new Vector3(5f, 1.4f, 0f), new Color(0.98f, 0.5f, 0.55f));
            LabKit.WorldLabel(annotations, "Health is a component on the Player", new Vector3(-13f, 2.0f, 0f), LabKit.Accent);
        }
    }

    // ==================================================================================
    //  The UI layer
    // ==================================================================================

    private static void BuildUiLayer(Transform parent, PlayerController2D player)
    {
        GameObject root = new GameObject("UI");
        if (parent != null) root.transform.SetParent(parent, false);

        // order 0: the lab has no deck to sit under (the slides scene uses -10 for its demos)
        Canvas canvas = UiKit.MakeCanvas(root.transform, "UI Canvas", 0);
        Transform ct = canvas.transform;

        Health health = player != null ? player.GetComponent<Health>() : null;

        BuildHealthBar(ct, health);
        BuildInventory(ct);
        BuildDialogue(ct, root.transform);
        BuildPause(ct, player);
    }

    private static void BuildHealthBar(Transform ct, Health health)
    {
        RectTransform holder = UiKit.MakePanel(ct, "Health", UiKit.TopRight, new Vector2(-40f, -40f), new Vector2(560f, 130f));
        UiKit.MakeText(holder, "Caption", UiKit.TopLeft, new Vector2(24f, -16f), new Vector2(300f, 40f), 26f, UiPalette.Muted).text = "Health";
        Slider bar = UiKit.MakeSlider(holder, "Bar", UiKit.TopCenter, new Vector2(0f, -80f), new Vector2(500f, 44f), UiPalette.Green);
        TMP_Text num = UiKit.MakeText(holder, "Number", UiKit.TopRight, new Vector2(-24f, -14f), new Vector2(220f, 44f), 28f, UiPalette.Paper, TextAlignmentOptions.TopRight);

        HealthBarUI ui = holder.gameObject.AddComponent<HealthBarUI>();
        ui.health = health;
        ui.bar = bar;
        ui.fill = bar.fillRect.GetComponent<Image>();
        ui.label = num;
    }

    private static void BuildInventory(Transform ct)
    {
        // the bag panel: a RectMask2D-clipped grid of slots, plus a button that opens it
        RectTransform panel = UiKit.MakeRect(ct, "Bag", UiKit.Center, Vector2.zero, new Vector2(760f, 520f));
        UiKit.MakeImage(panel, UiPalette.Panel, true);
        CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();

        UiKit.MakeText(panel, "Title", UiKit.TopLeft, new Vector2(28f, -20f), new Vector2(500f, 50f), 34f, UiPalette.Paper).text = "Bag";
        TMP_Text coins = UiKit.MakeText(panel, "Coins", UiKit.TopRight, new Vector2(-28f, -22f), new Vector2(300f, 46f), 28f, UiPalette.Gold, TextAlignmentOptions.TopRight);

        RectTransform frame = UiKit.MakeMask(panel, "Grid", UiKit.TopCenter, new Vector2(0f, -150f), new Vector2(700f, 300f), true);
        frame.GetComponent<Image>().color = new Color32(0x0E, 0x14, 0x20, 0xFF);
        // the template: an inactive child of the panel, so it is never drawn itself
        RectTransform template = UiKit.MakeRect(panel, "SlotTemplate", UiKit.TopLeft, new Vector2(-900f, -900f), new Vector2(150f, 130f));
        UiKit.MakeImage(template, UiPalette.Card, false);
        template.gameObject.SetActive(false);

        Button open = UiKit.MakeButton(ct, "BagButton", UiKit.BottomRight, new Vector2(-40f, 40f), new Vector2(220f, 90f), "Bag  (B)", UiPalette.Accent);

        InventoryUI ui = panel.gameObject.AddComponent<InventoryUI>();
        ui.toggleButton = open;
        ui.panel = group;
        ui.grid = frame;
        ui.slotTemplate = template.gameObject;
        ui.coinsLabel = coins;
        ui.slotCount = 8;
    }

    private static void BuildDialogue(Transform ct, Transform world)
    {
        // the box itself
        RectTransform box = UiKit.MakeRect(ct, "Dialogue", new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(1200f, 260f));
        UiKit.MakeImage(box, UiPalette.Panel, true);
        CanvasGroup group = box.gameObject.AddComponent<CanvasGroup>();
        TMP_Text line = UiKit.MakeText(box, "Line", UiKit.TopLeft, new Vector2(36f, -36f), new Vector2(1120f, 150f), 34f, UiPalette.Paper);
        UiKit.MakeText(box, "Who", UiKit.TopLeft, new Vector2(36f, -196f), new Vector2(600f, 46f), 26f, UiPalette.Accent).text = "Guide";

        // and the world zone that triggers it - the slide's "walk to a position" step
        GameObject zone = LabKit.Child(world, "DialogueZone", new Vector3(-6f, 0.4f, 0f));
        zone.layer = LabKit.Layer("Trigger");
        BoxCollider2D col = zone.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.2f, 3.0f);
        GameObject zv = LabKit.Child(zone.transform, "Visual", Vector3.zero);
        zv.layer = zone.layer;
        zv.transform.localScale = new Vector3(2.2f, 3f, 1f);
        LabKit.AddSprite(zv, LabKit.White, new Color(0.5f, 0.8f, 1f, 0.25f), 1);

        DialogueZone2D zoneScript = zone.AddComponent<DialogueZone2D>();
        zoneScript.panel = group;
        zoneScript.line = line;
        zoneScript.text = "Walk into the box and I fade in.\nThat is one Canvas Group, one number.";
    }

    private static void BuildPause(Transform ct, PlayerController2D player)
    {
        RectTransform panel = UiKit.MakeRect(ct, "Pause", UiKit.Center, Vector2.zero, new Vector2(1920f, 1080f));
        UiKit.MakeImage(panel, new Color(0.02f, 0.03f, 0.05f, 0.82f), true);
        CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
        UiKit.MakeText(panel, "Title", UiKit.Center, new Vector2(0f, 80f), new Vector2(900f, 120f), 64f, UiPalette.Paper, TextAlignmentOptions.Center).text = "Paused";
        UiKit.MakeText(panel, "Hint", UiKit.Center, new Vector2(0f, -20f), new Vector2(900f, 80f), 30f, UiPalette.Muted, TextAlignmentOptions.Center).text = "P to resume";
        Button resume = UiKit.MakeButton(panel, "Resume", UiKit.Center, new Vector2(0f, -140f), new Vector2(320f, 96f), "Resume", UiPalette.Accent);

        PausePanelUI ui = panel.gameObject.AddComponent<PausePanelUI>();
        ui.panel = group;
        ui.button = resume;
        ui.player = player;
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Find a named child of the gameplay W04LabBuilder just built, or the scene root.</summary>
    private static Transform Find(Transform parent, string name)
    {
        Transform scope = parent != null ? parent : null;
        if (scope != null)
        {
            Transform t = scope.Find(name);
            if (t != null) return t;
        }
        GameObject go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }
}
