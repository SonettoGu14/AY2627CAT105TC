using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 6 - UVs are the 2D coordinates that tell a texture which part of the image lands on which
/// triangle. The quad on the left shows the image at 1:1; the cube on the right uses the same image.
/// A/D change the tiling, W/S the offset, X stretches U only - the classic "the texture looks
/// stretched" failure, caused by the UVs rather than by the material.
/// </summary>
public class UvDemo : DemoBase
{
    public Transform spin;
    public Material targetMaterial;     // this demo's own material asset

    private float tile = 1f;
    private float offset;
    private bool stretched;
    private bool paused;
    private float angle;

    public override string Title { get { return "A UV maps a flat image onto a 3D surface"; } }

    public override string Keys { get { return "A / D  tiling     W / S  offset     X  stretch U     Space  pause"; } }

    public override void OnActivate()
    {
        tile = 1f; offset = 0f; stretched = false; paused = false; angle = -22f;
        Apply();
    }

    private void Update()
    {
        bool changed = false;
        if (Input.GetKey(KeyCode.A)) { tile = Mathf.Max(0.5f, tile - 1.4f * Time.deltaTime); stretched = false; changed = true; }
        if (Input.GetKey(KeyCode.D)) { tile = Mathf.Min(4f, tile + 1.4f * Time.deltaTime); stretched = false; changed = true; }
        if (Input.GetKey(KeyCode.W)) { offset = Mathf.Repeat(offset - 0.18f * Time.deltaTime, 1f); changed = true; }
        if (Input.GetKey(KeyCode.S)) { offset = Mathf.Repeat(offset + 0.18f * Time.deltaTime, 1f); changed = true; }
        if (Input.GetKeyDown(KeyCode.X)) { stretched = !stretched; changed = true; }
        if (changed) Apply();

        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (!paused)
        {
            angle += 16f * Time.deltaTime;
            if (spin != null) spin.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void Apply()
    {
        if (targetMaterial == null) return;
        targetMaterial.SetTextureScale("_BaseMap", stretched ? new Vector2(4f, 1f) : new Vector2(tile, tile));
        targetMaterial.SetTextureOffset("_BaseMap", new Vector2(offset, offset * 0.5f));
    }

    public override void Readout(List<string> lines)
    {
        if (targetMaterial == null) return;
        Vector2 scale = targetMaterial.GetTextureScale("_BaseMap");
        Vector2 off = targetMaterial.GetTextureOffset("_BaseMap");
        lines.Add("texture scale   (" + F(scale.x) + ", " + F(scale.y) + ")");
        lines.Add("texture offset  (" + F(off.x) + ", " + F(off.y) + ")");
        lines.Add("");
        lines.Add("U = horizontal, V = vertical; the range is 0 to 1.");
        lines.Add("Each vertex stores a UV, so the image knows which");
        lines.Add("part belongs to which triangle.");
        if (stretched)
        {
            lines.Add("");
            lines.Add("U is 4x, V is 1x: the image is stretched.");
            lines.Add("The texture did not change - the UVs did.");
        }
    }
}
