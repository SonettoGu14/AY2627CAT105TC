using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 8 and 10 - a normal map adds surface detail without adding geometry. The sphere keeps the
/// same silhouette and the same triangle count; only the direction the surface reports to the light
/// changes. Turning it off is what the surface looks like when the texture was imported as Default
/// instead of Normal map.
/// </summary>
public class NormalMapDemo : DemoBase
{
    public Transform spin;
    public Renderer target;
    public Material material;        // this demo's own material
    public Texture normalMap;

    private bool on = true;
    private float angle;

    public override string Title { get { return "A normal map adds detail without geometry"; } }

    public override string Keys { get { return "N  normal map on / off"; } }

    public override void OnActivate()
    {
        on = true; angle = -15f;
        Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) { on = !on; Apply(); }
        angle += 16f * Time.deltaTime;
        if (spin != null) spin.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void Apply()
    {
        if (material == null) return;
        if (on && normalMap != null)
        {
            material.SetTexture("_BumpMap", normalMap);
            material.SetFloat("_BumpScale", 1f);
            material.EnableKeyword("_NORMALMAP");
        }
        else
        {
            material.SetTexture("_BumpMap", null);
            material.DisableKeyword("_NORMALMAP");
        }
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("normal map   " + (on ? "ON" : "OFF"));
        lines.Add("");
        lines.Add("The silhouette and the triangle count never change:");
        lines.Add("the bumps are a texture that bends the normal,");
        lines.Add("so the light shades them as if the surface had detail.");
        lines.Add("");
        lines.Add(on
            ? "OFF is what \"imported as Default\" looks like: flat."
            : "The surface is flat again. Set Texture Type = Normal map");
        if (!on) lines.Add("in the importer, then assign it to the Normal Map slot.");
    }
}
