using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Generates a slide-deck scene from a deck JSON. Week-independent: one [MenuItem] per deck.
public static class SlideDeckBuilder
{
    static readonly Color BgColor     = new Color32(0x0E, 0x14, 0x20, 0xFF);
    static readonly Color BarColor    = new Color32(0x10, 0x18, 0x28, 0xFF);
    static readonly Color TitleColor  = new Color32(0xF2, 0xF6, 0xFC, 0xFF);
    static readonly Color BodyColor   = new Color32(0xE6, 0xEC, 0xF5, 0xFF);
    static readonly Color MutedColor  = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
    static readonly Color AccentColor = new Color32(0x6F, 0xA8, 0xFF, 0xFF);

    static readonly Vector2 TopLeft = new Vector2(0f, 1f);

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

        if (withGameplay)
        {
            // The platformer brings its own light and follow camera. The deck canvas is
            // Screen Space Overlay, so it needs no camera of its own - and exactly one
            // camera must exist in the scene.
            PlayerController2D labPlayer;
            Camera labCamera;
            W04LabBuilder.BuildGameplay(null, out labPlayer, out labCamera);
        }
        else
        {
            LabKit.MakeLight(null);
            LabKit.MakeCamera(null, new Vector3(0f, 0f, -10f), 5f);
        }

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

        EditorSceneManager.SaveScene(scene, scenePath);
        LabKit.AddSceneToBuildSettings(scenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("[SlideDeckBuilder] built " + scenePath + " from " + deckJsonPath);
    }

    // ------------------------------------------------------------------ helpers

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
