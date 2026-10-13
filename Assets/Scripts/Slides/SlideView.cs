using System.Text; using TMPro; using UnityEngine;
public class SlideView : MonoBehaviour
{
    public SlideDeckPlayer player;
    public TMP_Text headerText, titleText, subtitleText, bodyText, pageText;
    public string header = "";

    void OnEnable()  { if (player != null) player.Changed += Refresh; Refresh(); }
    void OnDisable() { if (player != null) player.Changed -= Refresh; }

    public void Refresh()
    {
        if (player == null || player.Deck == null || player.Cursor == null) return;
        if (player.Deck.slides == null || player.Deck.slides.Length == 0) return;   // empty deck
        int i = player.SlideIndex;
        var s = player.Deck.slides[i];
        if (headerText  != null) headerText.text  = header;
        if (titleText   != null) titleText.text   = s.title;
        if (subtitleText!= null) subtitleText.text= s.subtitle;
        if (pageText    != null) pageText.text    = (i + 1) + " / " + player.Cursor.SlideCount;
        if (bodyText    != null) bodyText.text    = BuildBody(s);
    }

    /// <summary>The whole slide at once - every block, in order. No incremental reveal.</summary>
    public static string BuildBody(SlideData s)
    {
        if (s.blocks == null || s.blocks.Length == 0)
            return s.table != null ? TableToText(s.table) : "";
        var sb = new StringBuilder();
        for (int k = 0; k < s.blocks.Length; k++)
        {
            var b = s.blocks[k];
            if (b.kind == "blank") { sb.Append('\n'); continue; }
            // Only glyphs the shipped font actually contains: LiberationSans SDF has U+2022 but
            // NOT U+25E6 or U+25AA, which used to render as nothing plus a per-codepoint TMP error.
            string indent = b.level <= 0 ? "\u2022 " : b.level == 1 ? "     - " : "          - ";
            // Code lines are tinted, not highlighted. TMP's <mark> tag draws a bar that covers the
            // glyphs with this font's material - verified three ways (6-digit colour, 8-digit
            // colour, and <color> wrapped outside the <mark>), all unreadable - so a coloured,
            // indented line is what actually shows up.
            if (b.kind == "code") sb.Append("<color=#8FD9A8>      ").Append(b.text).Append("</color>");
            else sb.Append(indent).Append(b.text);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    static string TableToText(SlideTable t)
    {
        if (t == null || t.cells == null || t.columns <= 0) return "";
        var sb = new StringBuilder();
        for (int r = 0; r * t.columns < t.cells.Length; r++)
        {
            for (int c = 0; c < t.columns; c++)
            { int k = r * t.columns + c; sb.Append(k < t.cells.Length ? t.cells[k] : ""); if (c < t.columns - 1) sb.Append("   |   "); }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}
