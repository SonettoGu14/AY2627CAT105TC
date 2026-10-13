using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// W5 slides 19-22, one Maths concept per demo:
///   Type      - why it is Mathf and not Math (float vs double, and the drift that causes)
///   Lerp      - Mathf.Lerp(a, b, t) interpolates a value
///   Clamp     - Mathf.Clamp keeps a value inside a range (and shows Min / Max beside it)
///   LerpBlend - Lerp blends two numbers, e.g. the 400-800 SAT example
///
/// All four are the same shape: a track with a marker on it, and the numbers in the readout. Nothing
/// moves on its own, so the lecturer can hold a value and talk over it.
/// </summary>
public class MathfDemo : DemoBase
{
    public enum Mode { Type, Lerp, Clamp, LerpBlend }

    public Mode mode = Mode.Lerp;

    public Transform track;            // the visual range (local centre = 0)
    public Transform marker;           // the value marker, moved along X
    public Transform clampedMarker;    // Clamp only: where the clamped value lands
    public SpriteRenderer markerSprite;
    public SpriteRenderer clampedSprite;
    public float halfWidth = 3f;       // world units from the track centre to either end

    private float t = 0.5f;            // Lerp / LerpBlend parameter
    private float raw = 5f;            // Clamp: the raw value the demo pushes around
    private float sum;                 // Type: accumulated 0.1f additions
    private int adds;                  // Type: how many additions

    private const float ClampMin = 2f;
    private const float ClampMax = 8f;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Type:      return "Mathf: Unity's float maths, not double";
                case Mode.Lerp:      return "Mathf.Lerp(a, b, t) interpolates between two values";
                case Mode.Clamp:     return "Mathf.Clamp keeps a value inside a range";
                default:             return "Lerp also blends two numbers";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Type:      return "A  add 0.1f        S  reset";
                case Mode.Lerp:      return "A / D  change t by 0.1";
                case Mode.Clamp:     return "A / D  push the raw value out of range";
                default:             return "A / D  change t by 0.1";
            }
        }
    }

    public override void OnActivate() { Reset(); }

    private void Reset()
    {
        t = 0.5f;
        raw = 5f;
        sum = 0f;
        adds = 0;
        Place();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) Step(-1);
        if (Input.GetKeyDown(KeyCode.D)) Step(+1);
        if (mode == Mode.Type && Input.GetKeyDown(KeyCode.S)) { sum = 0f; adds = 0; }

        Place();
        if (mode == Mode.Type) Drift();
    }

    private void Step(int dir)
    {
        switch (mode)
        {
            case Mode.Type:
                if (dir > 0) { sum += 0.1f; adds++; }
                break;
            case Mode.Clamp:
                raw = Mathf.Clamp(raw + dir * 0.5f, ClampMin - 2f, ClampMax + 2f);
                break;
            default:
                t = Mathf.Clamp01(Mathf.Round((t + dir * 0.1f) * 10f) / 10f);
                break;
        }
    }

    /// <summary>Type only: keep a second, independently accumulated value so the drift is visible.</summary>
    private float driftExact;
    private void Drift() { driftExact = adds * 0.1f; }

    private void Place()
    {
        if (marker != null)
        {
            switch (mode)
            {
                case Mode.Clamp:
                    marker.localPosition = new Vector3(Mathf.Lerp(-halfWidth, halfWidth, Mathf.InverseLerp(ClampMin - 2f, ClampMax + 2f, raw)), 0f, 0f);
                    break;
                case Mode.Type:
                    marker.localPosition = new Vector3(Mathf.Clamp(sum, -halfWidth, halfWidth), 0f, 0f);
                    break;
                default:
                    marker.localPosition = new Vector3(Mathf.Lerp(-halfWidth, halfWidth, t), 0f, 0f);
                    break;
            }
        }
        if (clampedMarker != null && mode == Mode.Clamp)
            clampedMarker.localPosition = new Vector3(Mathf.Lerp(-halfWidth, halfWidth, Mathf.InverseLerp(ClampMin - 2f, ClampMax + 2f, Mathf.Clamp(raw, ClampMin, ClampMax))), 0f, 0f);

        if (markerSprite != null)
            markerSprite.color = mode == Mode.Clamp ? new Color32(0x6F, 0xA8, 0xFF, 0xFF) : new Color32(0x7C, 0xE3, 0x8B, 0xFF);
        if (clampedSprite != null)
            clampedSprite.color = new Color32(0x7C, 0xE3, 0x8B, 0xFF);
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Type:
                lines.Add("0.1f + 0.2f == 0.3f    -> " + B(0.1f + 0.2f == 0.3f));
                lines.Add("Mathf.Approximately  -> " + B(Mathf.Approximately(0.1f + 0.2f, 0.3f)));
                lines.Add("");
                lines.Add("sum   = " + sum.ToString("0.0000000") + "   (" + adds + " additions of 0.1f)");
                lines.Add("exact = " + driftExact.ToString("0.0000000"));
                lines.Add("sum == exact          -> " + B(sum == driftExact));
                break;

            case Mode.Lerp:
                lines.Add("Mathf.Lerp(a, b, t)");
                lines.Add("");
                lines.Add("a = 0.0     b = 6.0     t = " + F(t, 1));
                lines.Add("result = " + F(Mathf.Lerp(0f, 6f, t), 2));
                break;

            case Mode.Clamp:
                lines.Add("raw = " + F(raw, 1) + "     min = " + F(ClampMin, 0) + "     max = " + F(ClampMax, 0));
                lines.Add("");
                lines.Add("Mathf.Clamp(raw, 2f, 8f) = " + F(Mathf.Clamp(raw, ClampMin, ClampMax), 1));
                lines.Add("Mathf.Min(4f, 7f) = " + F(Mathf.Min(4f, 7f), 1));
                lines.Add("Mathf.Max(4f, 7f) = " + F(Mathf.Max(4f, 7f), 1));
                break;

            default:
                lines.Add("Mathf.Lerp(400f, 800f, t)");
                lines.Add("");
                lines.Add("t = " + F(t, 1) + "   70% correct would be t = 0.7");
                lines.Add("result = " + F(Mathf.Lerp(400f, 800f, t), 0));
                break;
        }
    }
}
