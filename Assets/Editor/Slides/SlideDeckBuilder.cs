using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Generates a slide-deck scene from a deck JSON: the 1920x1080 deck, the navigation, the Chrome
/// canvas with its always-on toggle button, and — behind the deck — the slide demos for that deck
/// (see SlideDemoBuilder).
///
/// Week-independent: one [MenuItem] per deck.
/// </summary>
public static class SlideDeckBuilder
{
    static readonly Color BgColor     = new Color32(0x0E, 0x14, 0x20, 0xFF);
    static readonly Color BarColor    = new Color32(0x10, 0x18, 0x28, 0xFF);
    static readonly Color TitleColor  = new Color32(0xF2, 0xF6, 0xFC, 0xFF);
    static readonly Color BodyColor   = new Color32(0xE6, 0xEC, 0xF5, 0xFF);
    static readonly Color MutedColor  = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
    static readonly Color AccentColor = new Color32(0x6F, 0xA8, 0xFF, 0xFF);

    static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    static readonly Vector2 TopRight = new Vector2(1f, 1f);
    static readonly Vector2 BottomLeft = new Vector2(0f, 0f);

    [System.Serializable]
    private class DemoMapData { public string[] perSlide; }

    [MenuItem("CAT105TC/Slides/Build W04Slides")]
    public static void BuildW04Slides()
    {
        BuildDeckScene("Assets/Slides/W04_L4.json", "Assets/Scenes/W04Slides.unity",
                       "CAT105TC  \u00b7  Week 04  \u00b7  Animations and 2D Art", true);
    }

    public static void BuildDeckScene(string deckJsonPath, string scenePath, string header, bool withGameplay)
    {
        // TMP essentials import asynchronously on a fresh clone; finish the build once ready.
        if (!TmpBootstrap.Ensure(() => BuildDeckScene(deckJsonPath, scenePath, header, withGameplay))) return;

        TextAsset deck = AssetDatabase.LoadAssetAtPath<TextAsset>(deckJsonPath);
        if (deck == null)
        {
            Debug.LogError("[Slides] deck JSON not found: " + deckJsonPath +
                           "  ->  run:  uv run --with python-pptx python tools/pptx_to_deck.py <deck.pptx>");
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        WarnAboutUnrenderedContent(deck);

        // --- the demos that appear behind the deck, one stage each ----------------------
        DemoStage demoStage = SlideDemoBuilder.Build(null, withGameplay);

        // --- the deck canvas -----------------------------------------------------------
        GameObject canvasGo = new GameObject("Slides Canvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform root = canvasGo.transform;

        FullScreen(root, "Background", BgColor);
        UiImage(root, "HeaderBar", TopLeft, new Vector2(0f, 0f), new Vector2(1920f, 64f), BarColor);

        TMP_Text headerText   = TmpText(root, "HeaderText",   TopLeft, new Vector2(48f, -14f),   new Vector2(1400f, 36f),  26f, MutedColor,  TextAlignmentOptions.Left);
        TMP_Text titleText    = TmpText(root, "TitleText",    TopLeft, new Vector2(120f, -150f), new Vector2(1680f, 130f), 64f, TitleColor,  TextAlignmentOptions.TopLeft);
        TMP_Text subtitleText = TmpText(root, "SubtitleText", TopLeft, new Vector2(120f, -290f), new Vector2(1680f, 50f),  30f, AccentColor, TextAlignmentOptions.TopLeft);
        TMP_Text bodyText     = TmpBody(root, "BodyText", TopLeft, new Vector2(120f, -360f), new Vector2(1680f, 600f));

        SlideDeckPlayer player = canvasGo.AddComponent<SlideDeckPlayer>();
        player.deckJson = deck;

        SlideView view = canvasGo.AddComponent<SlideView>();
        view.player = player;
        view.header = header;
        view.headerText = headerText;
        view.titleText = titleText;
        view.subtitleText = subtitleText;
        view.bodyText = bodyText;

        // --- navigation (deck canvas) --------------------------------------------------
        Vector2 bottomRight = new Vector2(1f, 0f);
        TMP_Text pageLabel = TmpText(root, "PageLabel", bottomRight,
            new Vector2(-300f, 40f), new Vector2(140f, 40f), 28f, MutedColor, TextAlignmentOptions.Right);
        Button nextButton = UiButton(root, "NextButton", bottomRight, new Vector2(-40f, 32f), new Vector2(110f, 56f), BarColor);
        TmpText(nextButton.transform, "Label", new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(110f, 56f), 34f, TitleColor, TextAlignmentOptions.Center).text = ">";
        Button prevButton = UiButton(root, "PrevButton", bottomRight, new Vector2(-170f, 32f), new Vector2(110f, 56f), BarColor);
        TmpText(prevButton.transform, "Label", new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(110f, 56f), 34f, TitleColor, TextAlignmentOptions.Center).text = "<";

        Image jumpPanelImage = UiImage(root, "JumpPanel", new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(1400f, 820f), new Color32(0x10, 0x18, 0x28, 0xFF));
        GameObject jumpPanel = jumpPanelImage.gameObject;
        jumpPanel.SetActive(false);

        view.pageText = pageLabel;

        SlideNavigator navigator = canvasGo.AddComponent<SlideNavigator>();
        navigator.player = player;
        navigator.nextButton = nextButton;
        navigator.prevButton = prevButton;
        navigator.pageLabel = pageLabel;
        navigator.jumpPanel = jumpPanel;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextButton.onClick, navigator.NextClicked);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(prevButton.onClick, navigator.PrevClicked);

        // --- which demos belong to which slide -----------------------------------------
        SlideDemoMap demoMap = canvasGo.AddComponent<SlideDemoMap>();
        string mapPath = deckJsonPath.Substring(0, deckJsonPath.Length - ".json".Length) + ".demos.json";
        TextAsset mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(mapPath);
        if (mapAsset != null)
        {
            DemoMapData data = JsonUtility.FromJson<DemoMapData>(mapAsset.text);
            if (data != null && data.perSlide != null) demoMap.perSlide = data.perSlide;
        }
        else
        {
            Debug.LogWarning("[SlideDeckBuilder] no demo map at " + mapPath + " - no demos will show.");
        }

        // --- Chrome canvas (always on top; survives hiding the deck) ---------------------
        GameObject chromeGo = new GameObject("Chrome Canvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas chromeCanvas = chromeGo.GetComponent<Canvas>();
        chromeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        chromeCanvas.sortingOrder = 10;
        CanvasScaler chromeScaler = chromeGo.GetComponent<CanvasScaler>();
        chromeScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        chromeScaler.referenceResolution = new Vector2(1920f, 1080f);
        chromeScaler.matchWidthOrHeight = 0.5f;

        Button toggleButton = UiButton(chromeGo.transform, "ToggleDeckButton", TopRight,
            new Vector2(-40f, -40f), new Vector2(180f, 52f), BarColor);
        TMP_Text toggleLabel = TmpText(toggleButton.transform, "Label", new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(180f, 52f), 26f, TitleColor, TextAlignmentOptions.Center);

        SlidePresenter presenter = chromeGo.AddComponent<SlidePresenter>();
        presenter.slidesRoot = canvasGo;
        presenter.player = player;
        presenter.demoStage = demoStage;
        presenter.demoMap = demoMap;
        presenter.toggleLabel = toggleLabel;
        toggleLabel.text = presenter.hideLabel;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(toggleButton.onClick, presenter.Toggle);

        DemoReadout readout = BuildDemoReadout(chromeGo.transform, demoStage, presenter);

        SlideInput slideInput = chromeGo.AddComponent<SlideInput>();
        slideInput.player = player;
        slideInput.presenter = presenter;
        slideInput.navigator = navigator;
        slideInput.demoStage = demoStage;

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        EditorSceneManager.SaveScene(scene, scenePath);
        LabKit.AddSceneToBuildSettings(scenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[SlideDeckBuilder] built " + scenePath + " from " + deckJsonPath +
                  " (" + (demoStage.slots != null ? demoStage.slots.Length : 0) + " demos)");
    }

    // ------------------------------------------------------------------ demo readout panel

    private static DemoReadout BuildDemoReadout(Transform chrome, DemoStage stage, SlidePresenter presenter)
    {
        RectTransform panelRect = UiPanelRect(chrome, "DemoReadout", BottomLeft, BottomLeft,
            new Vector2(28f, 28f), new Vector2(880f, 380f));
        Image bg = panelRect.gameObject.AddComponent<Image>();
        bg.sprite = LabKit.White;
        bg.color = new Color32(0x0A, 0x0F, 0x18, 0xE6);
        bg.raycastTarget = false;

        TMP_Text title = TmpText(panelRect, "Title", TopLeft, new Vector2(24f, -16f), new Vector2(560f, 48f),
            24f, AccentColor, TextAlignmentOptions.TopLeft);
        title.enableAutoSizing = true;          // long titles shrink instead of running under the counter
        title.fontSizeMin = 15f;
        title.fontSizeMax = 24f;
        TMP_Text index = TmpText(panelRect, "Index", new Vector2(1f, 1f), new Vector2(-24f, -18f), new Vector2(260f, 30f),
            17f, MutedColor, TextAlignmentOptions.TopRight);
        TMP_Text body = TmpText(panelRect, "Body", TopLeft, new Vector2(24f, -74f), new Vector2(832f, 252f),
            21f, BodyColor, TextAlignmentOptions.TopLeft);
        TMP_Text keys = TmpText(panelRect, "Keys", new Vector2(0f, 0f), new Vector2(24f, 14f), new Vector2(832f, 40f),
            20f, new Color32(0x7C, 0xE3, 0x8B, 0xFF), TextAlignmentOptions.BottomLeft);

        // The readout component must live on an ACTIVE object: a component sitting on an inactive
        // GameObject never runs Update, so it could never switch its own panel on.
        DemoReadout readout = chrome.gameObject.AddComponent<DemoReadout>();
        readout.stage = stage;
        readout.presenter = presenter;
        readout.panel = panelRect.gameObject;
        readout.titleText = title;
        readout.indexText = index;
        readout.readoutText = body;
        readout.keysText = keys;
        panelRect.gameObject.SetActive(false);
        return readout;
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>
    /// SlideView draws title/subtitle/blocks only. A deck that carries images or tables would lose
    /// them silently, so say so loudly at build time.
    ///
    /// NB: JsonUtility materialises a JSON `null` object field as a default *instance*, so a plain
    /// `!= null` test reports every slide as having an image and a table. Detect real content.
    /// </summary>
    static void WarnAboutUnrenderedContent(TextAsset deck)
    {
        SlideDeckData parsed;
        try { parsed = JsonUtility.FromJson<SlideDeckData>(deck.text); }
        catch (System.Exception) { return; }
        if (parsed == null || parsed.slides == null) return;

        int images = 0, tables = 0;
        foreach (SlideData s in parsed.slides)
        {
            if (s.image != null && !string.IsNullOrEmpty(s.image.path)) images++;
            if (s.table != null && s.table.cells != null && s.table.cells.Length > 0) tables++;
        }

        if (images + tables > 0)
        {
            Debug.LogWarning("[SlideDeckBuilder] " + deck.name + " contains content SlideView does not draw yet: " +
                             images + " image(s), " + tables + " table(s). See Assets/Scripts/Slides/README.md.");
        }
    }

    static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static RectTransform UiPanelRect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image FullScreen(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static Button UiButton(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
    {
        RectTransform rt = Rect(parent, name, anchor, pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        return button;
    }

    static Image UiImage(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
    {
        RectTransform rt = Rect(parent, name, anchor, pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TMP_Text TmpText(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size,
                            float fontSize, Color color, TextAlignmentOptions align)
    {
        RectTransform rt = Rect(parent, name, anchor, pos, size);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.richText = true;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        t.text = string.Empty;
        return t;
    }

    static TMP_Text TmpBody(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        TMP_Text t = TmpText(parent, name, anchor, pos, size, 40f, BodyColor, TextAlignmentOptions.TopLeft);
        t.enableAutoSizing = true;
        t.fontSizeMin = 22f;
        t.fontSizeMax = 40f;
        t.lineSpacing = 15f;
        t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }
}
