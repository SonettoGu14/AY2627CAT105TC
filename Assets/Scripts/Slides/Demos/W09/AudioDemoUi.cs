using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Week 9 concept demos (slides 3-6): why sound matters, the audio map, Play vs PlayOneShot,
/// pitch, spatial blend, min/max distance and rolloff.
///
/// W09ConceptDemos (editor) furnishes the objects and wires the public fields; these classes only
/// decide what a key does and what the readout says. One demo = one lesson, keyboard only, and
/// everything happens in Update() because the demo root is inactive while the deck is up.
/// </summary>
internal static class AudioDemoUi
{
    // The kit's Fill/SliderValue are editor-side, the demos need the same maths every frame.
    // The bar sprites are centre-pivoted, so a scaled fill has to be re-centred on its own left
    // edge: setting only localScale grows the bar both ways and it spills out of its track.
    public static void Fill(Transform fill, float level, float width)
    {
        if (fill == null) return;
        float w = Mathf.Max(0.02f, Mathf.Clamp01(level) * width);
        fill.localScale = new Vector3(w, 0.32f, 1f);
        fill.localPosition = new Vector3(-width * 0.5f + w * 0.5f, 0f, 0f);
    }

    public static void SliderValue(Transform fill, Transform knob, float value, float width)
    {
        float v = Mathf.Clamp01(value);
        if (fill != null) Fill(fill, v, width);
        if (knob != null) knob.localPosition = new Vector3(-width * 0.5f + v * width, 0f, 0f);
    }

    /// <summary>RMS to a bar height the way AudioDemoBase.Level draws it: sqrt makes quiet speak.</summary>
    public static float Meter(float linear)
    {
        return Mathf.Clamp01(Mathf.Sqrt(Mathf.Clamp01(linear)));
    }

    public static Color Alpha(Color c, float a)
    {
        c.a = a;
        return c;
    }
}
