using UnityEngine;

/// <summary>
/// The colour vocabulary the UI demos share. It lives in the runtime assembly because the demos
/// themselves set colours (image tint, bar colour, readout text) — the editor-side UiKit, which
/// only builds the objects, uses these same values so kit and demos never drift apart.
/// </summary>
public static class UiPalette
{
    public static readonly Color Paper  = new Color32(0xF2, 0xF6, 0xFC, 0xFF);
    public static readonly Color Ink    = new Color32(0x0A, 0x0F, 0x18, 0xFF);
    public static readonly Color Panel  = new Color32(0x16, 0x1E, 0x2E, 0xF2);
    public static readonly Color Card   = new Color32(0x22, 0x2C, 0x40, 0xFF);
    public static readonly Color Muted  = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
    public static readonly Color Accent = new Color32(0x6F, 0xA8, 0xFF, 0xFF);
    public static readonly Color Green  = new Color32(0x7C, 0xE3, 0x8B, 0xFF);
    public static readonly Color Gold   = new Color32(0xFF, 0xB4, 0x54, 0xFF);
    public static readonly Color Pink   = new Color32(0xF2, 0x6B, 0x8A, 0xFF);
}
