using UnityEngine;
using UnityEngine.Audio;

/// <summary>Colours the practice demos need at runtime (the editor palette lives in DemoKit, which
/// is editor-only, so the three the demos tint text with are repeated here).</summary>
internal static class DemoColors
{
    public static readonly Color Green = new Color32(0x7C, 0xE3, 0x8B, 0xFF);
    public static readonly Color Pink = new Color32(0xF2, 0x6B, 0x8A, 0xFF);
    public static readonly Color Dim = new Color32(0x8F, 0xA3, 0xBF, 0xFF);
}

/// <summary>Filling the readout meters at runtime. DemoKit's meters are built with a centre-pivot
/// sprite, so the fill has to grow from the left end rather than outwards from the middle.</summary>
internal static class PracticeBars
{
    public static void Fill(Transform fill, float level, float width)
    {
        if (fill == null) return;
        float w = Mathf.Max(0.02f, Mathf.Clamp01(level) * width);
        fill.localPosition = new Vector3(-width * 0.5f + w * 0.5f, 0f, 0f);
        fill.localScale = new Vector3(w, 0.32f, 1f);
    }

    public static void Slider(Transform fill, Transform knob, float value, float width)
    {
        float v = Mathf.Clamp01(value);
        if (fill != null)
        {
            float w = Mathf.Max(0.02f, v * width);
            fill.localPosition = new Vector3(-width * 0.5f + w * 0.5f, 0f, 0f);
            fill.localScale = new Vector3(w, 0.32f, 1f);
        }
        if (knob != null) knob.localPosition = new Vector3(-width * 0.5f + v * width, 0f, 0f);
    }
}

/// <summary>The mixer's exposed parameters, and the "the mixer asset may not exist yet" guard: with
/// a null group the demos still run and the readout says the mixer is missing.</summary>
internal static class PracticeMixer
{
    public const string MasterParam = "MasterVolume";
    public const string BgmParam = "BGMVolume";
    public const string SfxParam = "SFXVolume";

    public static void Set(AudioMixerGroup group, string param, float db)
    {
        if (group == null) return;
        group.audioMixer.SetFloat(param, db);
    }

    public static float Get(AudioMixerGroup group, string param)
    {
        float value = 0f;
        if (group == null) return value;
        group.audioMixer.GetFloat(param, out value);
        return value;
    }
}
