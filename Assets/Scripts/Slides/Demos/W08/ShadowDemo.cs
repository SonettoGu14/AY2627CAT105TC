using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 12 - a realtime shadow you can switch between soft, hard and off. The object keeps turning,
/// so the shadow has to be re-rendered every frame: that is exactly what Realtime means, and why a
/// static scene should be baked instead.
/// </summary>
public class ShadowDemo : DemoBase
{
    public Transform spinner;
    public Light lamp;

    private int mode;          // 0 soft, 1 hard, 2 none
    private float angle;

    private static readonly LightShadows[] Modes = { LightShadows.Soft, LightShadows.Hard, LightShadows.None };

    public override string Title { get { return "Realtime shadows: soft, hard, or off"; } }

    public override string Keys { get { return "S  cycle Soft -> Hard -> Off"; } }

    public override void OnActivate()
    {
        mode = 0; angle = 0f;
        Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) { mode = (mode + 1) % Modes.Length; Apply(); }
        angle += 20f * Time.deltaTime;
        if (spinner != null) spinner.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void Apply()
    {
        if (lamp != null) lamp.shadows = Modes[mode];
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("light mode   Realtime");
        lines.Add("shadows      " + Modes[mode]);
        lines.Add("strength     " + (lamp != null ? F(lamp.shadowStrength, 2) : "-"));
        lines.Add("");
        lines.Add("Realtime: the shadow map is drawn every frame, so the");
        lines.Add("turning object and the moving light both work. It costs GPU.");
        lines.Add("Baked: pre-computed into a lightmap, static objects only,");
        lines.Add("free at runtime. Mixed: baked room + realtime casters.");
        lines.Add("");
        lines.Add("This object turns, so it cannot be baked.");
    }
}
