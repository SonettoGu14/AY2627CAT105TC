using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The on-screen slide navigation: the prev/next buttons, the page label and the
/// jump-to-slide grid.
///
/// The panel itself is created (and left hidden) by SlideDeckBuilder; this component
/// fills it at initialisation with a numbered button per slide - 8 columns, up to 64
/// slides, so every current deck fits without a scroll view. Clicking a cell jumps
/// straight to that slide (JumpTo reveals the whole slide by contract) and closes the
/// panel. Its button callbacks are wired persistently by the builder so the links
/// survive a scene reload.
/// </summary>
public class SlideNavigator : MonoBehaviour
{
    public SlideDeckPlayer player;
    public Button nextButton;      // on-click -> NextClicked()
    public Button prevButton;      // on-click -> PrevClicked()
    public TMP_Text pageLabel;     // "12 / 28"
    public GameObject jumpPanel;   // centred 1400x820 panel, starts inactive

    public bool JumpPanelOpen { get; private set; }

    // Grid geometry. 8 columns is the contract; the cell box is sized so all 8
    // columns fit inside the 1400-wide panel with the 36pt numerals legible.
    const int   Columns      = 8;
    const int   MaxSlides    = 64;
    const float CellWidth    = 150f;
    const float CellHeight   = 72f;
    const float CellGap      = 12f;
    const float Margin       = 48f;
    const float HeaderHeight = 86f;

    static readonly Color PanelColor   = new Color32(0x10, 0x18, 0x28, 0xFF);
    static readonly Color CellColor    = new Color32(0x1E, 0x2A, 0x3E, 0xFF);
    static readonly Color TextColor    = new Color32(0xF2, 0xF6, 0xFC, 0xFF);
    static readonly Color CurrentColor = new Color32(0x6F, 0xA8, 0xFF, 0xFF);

    TMP_Text _header;
    RectTransform _gridRoot;
    Image[] _cellImages;
    bool _built;

    void Awake()
    {
        SetPanelOpen(false);
        if (player != null) player.Changed += Refresh;
        BuildGrid();                       // no-op until the player has built its cursor
    }

    void Start()
    {
        if (!_built) BuildGrid();          // component Awake order is not guaranteed
        Refresh();
    }

    void OnDestroy() { if (player != null) player.Changed -= Refresh; }

    // ------------------------------------------------------------------ button callbacks

    public void NextClicked() { if (player != null) player.NextSlide(); }
    public void PrevClicked() { if (player != null) player.PrevSlide(); }

    public void ToggleJumpPanel() { SetPanelOpen(!JumpPanelOpen); }

    public void SetPanelOpen(bool open)
    {
        JumpPanelOpen = open;
        if (jumpPanel != null) jumpPanel.SetActive(open);
        if (open) { RefreshHeader(); UpdateHighlights(); }
    }

    // ------------------------------------------------------------------ refresh

    void Refresh()
    {
        if (pageLabel != null && player != null && player.Cursor != null)
            pageLabel.text = (player.SlideIndex + 1) + " / " + player.Cursor.SlideCount;
        if (JumpPanelOpen) { RefreshHeader(); UpdateHighlights(); }
    }

    void RefreshHeader()
    {
        if (_header == null || player == null || player.Cursor == null) return;
        _header.text = "Jump to slide   (" + (player.SlideIndex + 1) + " / " + player.Cursor.SlideCount + ")";
    }

    // ------------------------------------------------------------------ grid

    void BuildGrid()
    {
        if (_built || jumpPanel == null || player == null || player.Cursor == null) return;
        _built = true;

        RectTransform panel = (RectTransform)jumpPanel.transform;
        panel.GetComponent<Image>().color = PanelColor;

        _header = MakeText(panel, "JumpHeader", 40f, TextAlignmentOptions.Left);
        RectTransform hr = (RectTransform)_header.transform;
        hr.anchorMin = hr.anchorMax = hr.pivot = new Vector2(0f, 1f);
        hr.anchoredPosition = new Vector2(Margin, -20f);
        hr.sizeDelta = new Vector2(panel.sizeDelta.x - 2f * Margin, 56f);

        _gridRoot = MakeRect(panel, "JumpGrid");
        _gridRoot.anchorMin = _gridRoot.anchorMax = _gridRoot.pivot = new Vector2(0f, 1f);
        _gridRoot.anchoredPosition = new Vector2(Margin, -HeaderHeight);
        _gridRoot.sizeDelta = new Vector2(panel.sizeDelta.x - 2f * Margin, panel.sizeDelta.y - HeaderHeight - 24f);

        int count = Mathf.Min(player.Cursor.SlideCount, MaxSlides);
        _cellImages = new Image[count];
        for (int k = 0; k < count; k++) MakeCell(k);
        RefreshHeader();
        UpdateHighlights();
    }

    void UpdateHighlights()
    {
        if (_cellImages == null || player == null) return;
        for (int i = 0; i < _cellImages.Length; i++)
            if (_cellImages[i] != null) _cellImages[i].color = i == player.SlideIndex ? CurrentColor : CellColor;
    }

    void MakeCell(int index)
    {
        int col = index % Columns;
        int row = index / Columns;

        GameObject go = new GameObject("Slide" + (index + 1), typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(_gridRoot, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(col * (CellWidth + CellGap), -row * (CellHeight + CellGap));
        rt.sizeDelta = new Vector2(CellWidth, CellHeight);

        Image img = go.GetComponent<Image>();
        img.color = CellColor;
        img.raycastTarget = true;
        if (_cellImages != null) _cellImages[index] = img;

        Button b = go.GetComponent<Button>();
        b.targetGraphic = img;

        TMP_Text label = MakeText(rt, "Label", 36f, TextAlignmentOptions.Center);
        RectTransform lr = (RectTransform)label.transform;
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        label.text = (index + 1).ToString();

        int target = index;
        // Runtime-only object -> a runtime listener is the only option here.
        b.onClick.AddListener(() => { if (player != null) player.JumpTo(target); SetPanelOpen(false); });
    }

    // ------------------------------------------------------------------ tiny UI factories

    static RectTransform MakeRect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    static TMP_Text MakeText(Transform parent, string name, float size, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        t.font = TMP_Settings.defaultFontAsset;
        t.fontSize = size;
        t.color = TextColor;
        t.alignment = align;
        t.richText = true;
        t.raycastTarget = false;
        t.text = string.Empty;
        return t;
    }
}
