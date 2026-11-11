using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 9 - the reference shot. A metal cube, a wood sphere and a glass capsule under one key
/// light: three sets of PBR values, the same light, three different surfaces. L turns the key light
/// off so the class can see how much of the look came from the light and how much from the material.
/// </summary>
public class PbrThreeDemo : DemoBase
{
    public Transform[] spinners;
    public Light keyLight;

    private bool paused, lightOn = true;
    private float angle;

    public override string Title { get { return "One light, three surfaces"; } }

    public override string Keys { get { return "L  key light on / off     Space  pause the turntable"; } }

    public override void OnActivate()
    {
        paused = false; lightOn = true; angle = 0f;
        if (keyLight != null) keyLight.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (Input.GetKeyDown(KeyCode.L))
        {
            lightOn = !lightOn;
            if (keyLight != null) keyLight.enabled = lightOn;
        }
        if (!paused)
        {
            angle += 14f * Time.deltaTime;
            if (spinners != null)
                foreach (Transform t in spinners) if (t != null) t.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("metal   cube     metallic 1.0  smooth 0.90  #888888");
        lines.Add("wood    sphere   metallic 0.0  smooth 0.20  #8B5A2B");
        lines.Add("glass   capsule  metallic 0.0  smooth 1.00  alpha 0.30");
        lines.Add("");
        lines.Add("One Directional light, " + (lightOn ? "on" : "off") + ".");
        lines.Add("Sharp highlight / broad highlight / see-through:");
        lines.Add("the same light produces all three.");
    }
}
