using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 8 - the two PBR values people get wrong. Metallic is 0 or 1 in almost every real material;
/// halfway is a look, not a surface. Smoothness controls how large and sharp the highlight is, and
/// high smoothness on a non-metal is what reads as "plastic".
/// </summary>
public class PbrDemo : DemoBase
{
    public enum Mode { Metallic, Smoothness }

    public Mode mode = Mode.Metallic;
    public Transform spin;
    public Renderer target;
    public Material material;        // this demo's own material

    private static readonly float[] MetallicSteps = { 0f, 1f, 0.5f };
    private int step;
    private float smoothness = 0.35f;
    private float angle;

    public override string Title
    {
        get
        {
            return mode == Mode.Metallic
                ? "Metallic: 0 is a dielectric, 1 is a metal"
                : "Smoothness: how sharp the reflection is";
        }
    }

    public override string Keys
    {
        get
        {
            return mode == Mode.Metallic
                ? "M  cycle metallic 0 -> 1 -> 0.5"
                : "A / D  lower / raise smoothness";
        }
    }

    public override void OnActivate()
    {
        step = 0; smoothness = 0.35f; angle = -18f;
        Apply();
    }

    private void Update()
    {
        if (mode == Mode.Metallic)
        {
            if (Input.GetKeyDown(KeyCode.M)) { step = (step + 1) % MetallicSteps.Length; Apply(); }
        }
        else
        {
            bool changed = false;
            if (Input.GetKey(KeyCode.A)) { smoothness = Mathf.Max(0f, smoothness - 0.6f * Time.deltaTime); changed = true; }
            if (Input.GetKey(KeyCode.D)) { smoothness = Mathf.Min(1f, smoothness + 0.6f * Time.deltaTime); changed = true; }
            if (changed) Apply();
        }

        angle += 17f * Time.deltaTime;
        if (spin != null) spin.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void Apply()
    {
        if (material == null) return;
        material.SetFloat("_Metallic", mode == Mode.Metallic ? MetallicSteps[step] : 0f);
        material.SetFloat("_Smoothness", mode == Mode.Metallic ? 0.9f : smoothness);
    }

    public override void Readout(List<string> lines)
    {
        if (material == null) return;
        if (mode == Mode.Metallic)
        {
            float m = MetallicSteps[step];
            lines.Add("metallic   " + F(m, 1));
            lines.Add("albedo     #888888 (the reflection tint)");
            lines.Add("");
            lines.Add("0 = wood, plastic, glass, skin: light reflects off the surface.");
            lines.Add("1 = metal: the reflection takes the albedo's colour.");
            lines.Add("0.5 is the value people pick to \"make it shinier\";");
            lines.Add("it is neither a metal nor a dielectric.");
            if (m == 0f) lines.Add("Now: grey plastic.");
            else if (m == 1f) lines.Add("Now: steel - the highlight is tinted by the albedo.");
            else lines.Add("Now: the in-between look. Compare it with the two ends.");
        }
        else
        {
            lines.Add("smoothness " + F(smoothness, 2));
            lines.Add("");
            lines.Add("0   matte: the highlight is broad and dim.");
            lines.Add("1   mirror: the highlight is small and sharp.");
            lines.Add("");
            lines.Add("On a dielectric (metallic 0), high smoothness reads as");
            lines.Add("plastic or wet. Wood should sit around 0.2.");
        }
    }
}
