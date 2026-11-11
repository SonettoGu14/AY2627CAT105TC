using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 10 - two failures you can recognise on sight, each shown as a pair that differs in one
/// value only. Wet wood: the right sphere's smoothness is 0.9 instead of 0.2. Painted metal: the
/// right sphere is metallic, but its albedo is a paint colour rather than the metal's reflection
/// tint.
/// </summary>
public class MaterialComparisonDemo : DemoBase
{
    public enum Mode { WetWood, PaintedMetal }

    public Mode mode = Mode.WetWood;
    public Transform[] spinners;

    private bool paused;
    private float angle;

    public override string Title
    {
        get
        {
            return mode == Mode.WetWood
                ? "Smoothness too high makes wood look wet"
                : "Albedo is the reflection tint on a metal";
        }
    }

    public override string Keys { get { return "Space  pause the turntable"; } }

    public override void OnActivate()
    {
        paused = false; angle = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (paused) return;
        angle += 12f * Time.deltaTime;
        if (spinners != null)
            foreach (Transform t in spinners) if (t != null) t.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    public override void Readout(List<string> lines)
    {
        if (mode == Mode.WetWood)
        {
            lines.Add("left   wood   metallic 0   smoothness 0.2");
            lines.Add("right  wood   metallic 0   smoothness 0.9");
            lines.Add("");
            lines.Add("Only one slider moved. The right surface now reads");
            lines.Add("wet, lacquered or plastic, because a broad matte");
            lines.Add("highlight became a small sharp one.");
            lines.Add("");
            lines.Add("Wood belongs around 0.2-0.3.");
        }
        else
        {
            lines.Add("left   metal  metallic 1   albedo gold");
            lines.Add("right  metal  metallic 1   albedo #8B5A2B");
            lines.Add("");
            lines.Add("Both are fully metallic. The right one uses a paint");
            lines.Add("colour as its albedo, so its reflection is tinted brown");
            lines.Add("and the metal looks dull and off-colour.");
            lines.Add("");
            lines.Add("For metals: albedo is the reflection tint (gold, copper, silver).");
        }
    }
}
