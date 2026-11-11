using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 7 - several objects can share one material asset. Three cubes point at the same material;
/// C changes that one asset and all three change together. O gives the middle cube its own material,
/// so it stops following the shared colour - the "per-renderer override" the slide warns you to use
/// sparingly.
/// </summary>
public class SharedMaterialDemo : DemoBase
{
    public Renderer[] targets;
    public Material shared;
    public Material overrideMaterial;
    public Transform[] spinners;

    private static readonly Color[] Colors =
    {
        new Color(0.30f, 0.48f, 0.95f, 1f),
        new Color(0.95f, 0.55f, 0.20f, 1f),
        new Color(0.35f, 0.82f, 0.50f, 1f),
        new Color(0.85f, 0.35f, 0.55f, 1f),
    };

    private int colorIndex;
    private bool overridden;
    private float angle;

    public override string Title { get { return "Several objects can share one material"; } }

    public override string Keys { get { return "C  change the shared colour     O  override the middle one"; } }

    public override void OnActivate()
    {
        colorIndex = 0; overridden = false; angle = 0f;
        if (targets != null)
            foreach (Renderer r in targets) if (r != null) r.sharedMaterial = shared;
        ApplyColor();
    }

    public override void OnDeactivate()
    {
        if (targets != null)
            foreach (Renderer r in targets) if (r != null) r.sharedMaterial = shared;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) { colorIndex = (colorIndex + 1) % Colors.Length; ApplyColor(); }
        if (Input.GetKeyDown(KeyCode.O))
        {
            overridden = !overridden;
            if (targets != null && targets.Length > 1)
                targets[1].sharedMaterial = overridden ? overrideMaterial : shared;
        }

        angle += 22f * Time.deltaTime;
        if (spinners != null)
            foreach (Transform t in spinners) if (t != null) t.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void ApplyColor()
    {
        if (shared != null) shared.SetColor("_BaseColor", Colors[colorIndex].linear);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("shared material: " + (shared != null ? shared.name : "(none)") + "   (one asset)");
        lines.Add("renderers using it: " + (overridden ? 2 : 3) + " of 3");
        lines.Add("albedo: " + Col(Colors[colorIndex]));
        lines.Add("");
        lines.Add(overridden
            ? "the middle cube has its own Material instance"
            : "all three cubes change together - one edit, three objects");
        lines.Add("");
        lines.Add("Start with shared materials. An override is useful, but it is");
        lines.Add("a second asset to keep track of.");
    }
}
