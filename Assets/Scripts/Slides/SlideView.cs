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
        int i = player.SlideIndex;
        var s = player.Deck.slides[i];
        if (headerText  != null) headerText.text  = header;
        if (titleText   != null) titleText.text   = s.title;
        if (subtitleText!= null) subtitleText.text= s.subtitle;
        if (pageText    != null) pageText.text    = (i + 1) + " / " + player.Cursor.SlideCount;
        if (bodyText    != null) bodyText.text    = BuildBody(s, player.RevealedBlocks);
    }

    static string BuildBody(SlideData s, int revealed)
    {
        if (s.blocks == null || s.blocks.Length == 0)
            return s.table != null ? TableToText(s.table) : "";
        var sb = new StringBuilder();
        int n = Mathf.Clamp(revealed, 0, s.blocks.Length);
        for (int k = 0; k < n; k++)
        {
            var b = s.blocks[k];
            if (b.kind == "blank") { sb.Append('\n'); continue; }
            string indent = b.level <= 0 ? "\u2022 " : b.level == 1 ? "     \u25E6 " : "          \u25AA ";
            if (b.kind == "code") sb.Append("<mark=#1E2430><color=#8FD9A8>  ").Append(b.text).Append("  </color></mark>");
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
