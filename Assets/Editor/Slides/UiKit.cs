using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The furniture W5's UI demos are built from — the UI counterpart of DemoKit.
///
/// Layout contract: the demo readout panel sits in the BOTTOM-LEFT of the screen (880x380 at
/// 28,28 in the 1920x1080 reference space), so demo UI lives in the TOP and on the RIGHT. Every
/// helper here anchors to the top by default for that reason.
///
/// Sorting: the deck canvas is order 0 and the Chrome canvas (which carries the readout) is order
/// 10, so demo canvases use -10 and can never draw over either. A demo root is deactivated while
/// the deck is up anyway, but the readout must stay readable whenever a UI demo is live.
/// </summary>
public static class UiKit
{
    public const int DemoSOrder = -10;
    public static readonly Vector2 RefResolution = new Vector2(1920f, 1080f);

    // ---- palette: defined in the runtime assembly (UiPalette) so the demos and the kit share it ----
    public static Color Paper   { get { return UiPalette.Paper; } }
    public static Color Ink     { get { return UiPalette.Ink; } }
    public static Color Panel   { get { return UiPalette.Panel; } }
    public static Color Card    { get { return UiPalette.Card; } }
    public static Color Muted   { get { return UiPalette.Muted; } }
    public static Color Accent  { get { return UiPalette.Accent; } }
    public static Color Green   { get { return UiPalette.Green; } }
    public static Color Gold    { get { return UiPalette.Gold; } }
    public static Color Pink    { get { return UiPalette.Pink; } }

    // ---- anchors ----
    public static readonly Vector2 TopLeft     = new Vector2(0f, 1f);
    public static readonly Vector2 TopCenter   = new Vector2(0.5f, 1f);
    public static readonly Vector2 TopRight    = new Vector2(1f, 1f);
    public static readonly Vector2 Center      = new Vector2(0.5f, 0.5f);
    public static readonly Vector2 BottomRight = new Vector2(1f, 0f);

    /// <summary>A screen-space canvas for one demo. Sorts below the deck and the readout.</summary>
    public static Canvas MakeCanvas(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas),
                                       typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(parent, false);
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = DemoSOrder;
        CanvasScaler sc = go.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = RefResolution;
        sc.matchWidthOrHeight = 0.5f;
        return c;
    }

    public static RectTransform MakeRect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    /// <summary>A rect pinned to one anchor with an explicit size. Default: top-centre (away from the readout).</summary>
    public static RectTransform MakeRect(Transform parent, string name, Vector2 anchor,
                                         Vector2 anchoredPos, Vector2 size,
                                         Vector2? pivot = null)
    {
        RectTransform rt = MakeRect(parent, name);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot ?? anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return rt;
    }

    /// <summary>A rect stretched across its parent with an inset (for backgrounds and fills).</summary>
    public static RectTransform MakeStretch(Transform parent, string name, float inset = 0f)
    {
        RectTransform rt = MakeRect(parent, name);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    public static Image MakeImage(RectTransform rt, Color color, bool raycast = true)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = LabKit.White;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    public static Image MakeImage(Transform parent, string name, Vector2 anchor, Vector2 pos,
                                  Vector2 size, Color color, bool raycast = true)
    {
        return MakeImage(MakeRect(parent, name, anchor, pos, size), color, raycast);
    }

    /// <summary>A panel: a sized background plus a child area to put content in.</summary>
    public static RectTransform MakePanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        RectTransform rt = MakeRect(parent, name, anchor, pos, size);
        MakeImage(rt, Panel, true);
        return rt;
    }

    public static TMP_Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pos,
                                    Vector2 size, float fontSize, Color color,
                                    TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        RectTransform rt = MakeRect(parent, name, anchor, pos, size);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;      // the deck's font, by the same GUID
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.richText = true;
        t.raycastTarget = false;
        t.text = string.Empty;
        return t;
    }

    /// <summary>A button: a card-coloured image with a centred label, and the default colour states.</summary>
    public static Button MakeButton(Transform parent, string name, Vector2 anchor, Vector2 pos,
                                    Vector2 size, string label, Color color)
    {
        RectTransform rt = MakeRect(parent, name, anchor, pos, size);
        Image img = MakeImage(rt, color, true);
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        cb.pressedColor = new Color(0.65f, 0.7f, 0.8f, 1f);
        cb.disabledColor = new Color(0.45f, 0.47f, 0.52f, 0.6f);
        cb.fadeDuration = 0.08f;
        b.colors = cb;
        TMP_Text t = MakeText(rt, "Label", new Vector2(0.5f, 0.5f), Vector2.zero, size,
                              28f, Paper, TextAlignmentOptions.Center);
        t.text = label;
        return b;
    }

    /// <summary>
    /// A horizontal slider. uGUI drives the Fill's and the Handle's anchors itself, so the Fill is
    /// just a stretched child of the root and the Handle is a sized child.
    /// </summary>
    public static Slider MakeSlider(Transform parent, string name, Vector2 anchor, Vector2 pos,
                                    Vector2 size, Color fillColor)
    {
        RectTransform rt = MakeRect(parent, name, anchor, pos, size);
        Image bg = MakeImage(rt, new Color32(0x1E, 0x24, 0x30, 0xFF), true);

        RectTransform fill = MakeStretch(rt, "Fill");
        MakeImage(fill, fillColor, false);

        RectTransform handle = MakeRect(rt, "Handle", new Vector2(0f, 0.5f), Vector2.zero, new Vector2(28f, size.y + 12f));
        Image handleImg = MakeImage(handle, Paper, true);

        Slider s = rt.gameObject.AddComponent<Slider>();
        s.targetGraphic = bg;
        s.fillRect = fill;
        s.handleRect = handle;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.wholeNumbers = false;
        return s;
    }

    /// <summary>An image whose children are clipped to its rectangle (the RectMask2D lesson).</summary>
    public static RectTransform MakeMask(Transform parent, string name, Vector2 anchor, Vector2 pos,
                                         Vector2 size, bool useRectMask = true)
    {
        RectTransform rt = MakeRect(parent, name, anchor, pos, size);
        MakeImage(rt, Card, true);
        if (useRectMask) rt.gameObject.AddComponent<RectMask2D>();
        else
        {
            Mask m = rt.gameObject.AddComponent<Mask>();
            m.showMaskGraphic = true;
        }
        return rt;
    }

    public static CanvasGroup MakeGroup(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        RectTransform rt = MakeRect(parent, name, anchor, pos, size);
        return rt.gameObject.AddComponent<CanvasGroup>();
    }
}
