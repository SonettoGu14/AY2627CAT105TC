using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  SPATIAL BLEND: blend 0 is 2D (the same in both ears, wherever it is); blend 1 is 3D (distance
//  and direction decide the level). A/Left and D/Right sweep it.
// ==============================================================================================

public class SpatialBlendDemo : AudioDemoBase
{
    public Transform earLeftFill;
    public Transform earRightFill;
    public Transform blendFill;
    public Transform blendKnob;
    public float earWidth = 6f;
    public float blendWidth = 5f;
    public float sweepSpeed = 0.55f;

    private float blend;

    public override string Title { get { return "Spatial blend: 2D everywhere vs 3D positioned"; } }
    public override string Keys { get { return "Left / Right sweep    R reset to 2D"; } }

    public override void OnActivate()
    {
        blend = 0f;
        Apply();
        if (source != null) source.Play();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        blend = 0f;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float next = blend;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) next -= sweepSpeed * dt;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) next += sweepSpeed * dt;
        if (Input.GetKeyDown(KeyCode.R)) next = 0f;

        if (!Mathf.Approximately(next, blend))
        {
            blend = Mathf.Clamp01(next);
            Apply();
        }

        float left = 0f, right = 0f;
        if (listener != null) Ears(out left, out right);
        AudioDemoUi.Fill(earLeftFill, AudioDemoUi.Meter(left), earWidth);
        AudioDemoUi.Fill(earRightFill, AudioDemoUi.Meter(right), earWidth);
    }

    private void Apply()
    {
        if (source != null) source.spatialBlend = blend;
        AudioDemoUi.SliderValue(blendFill, blendKnob, blend, blendWidth);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("spatialBlend  " + F(blend, 2) + (blend < 0.05f ? "  (2D)" : blend > 0.95f ? "  (3D)" : "  (mixing)"));
        float left = 0f, right = 0f;
        if (listener != null) Ears(out left, out right);
        lines.Add("L " + Level(left, 10) + "  R " + Level(right, 10));
        lines.Add("pan  " + PanText(Pan(left, right)));
        lines.Add(blend < 0.5f
            ? "2D: the same level in both ears, wherever it is"
            : "3D: distance makes it quieter, direction pans it");
        lines.Add("the source sits at x = +5, the ears at x = -1.4");
    }
}
