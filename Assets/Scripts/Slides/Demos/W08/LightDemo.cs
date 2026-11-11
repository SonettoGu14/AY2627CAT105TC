using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 11 - the three light types, one per demo, on the same little set. Directional has no
/// position, only rotation, and lights the whole scene with parallel rays. Point has a position and
/// falls off with distance. Spot has a position, a direction, an angle and a range.
/// </summary>
public class LightDemo : DemoBase
{
    public enum Mode { Directional, Point, Spot }

    public Mode mode = Mode.Directional;
    public Light lamp;

    private float yaw = -30f;
    private float pitch = 50f;
    private float x = -2.5f;
    private float range = 9f;
    private float spotAngle = 35f;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Directional: return "Directional: parallel rays, no position";
                case Mode.Point: return "Point: a bulb, falling off with distance";
                default: return "Spot: a cone, falling off with distance and angle";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Directional: return "A / D  turn yaw     W / S  pitch";
                case Mode.Point: return "A / D  move it     W / S  range";
                default: return "A / D  spot angle     W / S  range";
            }
        }
    }

    public override void OnActivate()
    {
        yaw = -30f; pitch = 50f; x = -2.5f; range = 9f; spotAngle = 35f;
        Apply();
    }

    private void Update()
    {
        if (lamp == null) return;
        bool changed = false;
        switch (mode)
        {
            case Mode.Directional:
                if (Input.GetKey(KeyCode.A)) { yaw -= 35f * Time.deltaTime; changed = true; }
                if (Input.GetKey(KeyCode.D)) { yaw += 35f * Time.deltaTime; changed = true; }
                if (Input.GetKey(KeyCode.W)) { pitch = Mathf.Min(85f, pitch + 35f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.S)) { pitch = Mathf.Max(10f, pitch - 35f * Time.deltaTime); changed = true; }
                if (changed) lamp.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
                break;

            case Mode.Point:
                if (Input.GetKey(KeyCode.A)) { x = Mathf.Max(-4.5f, x - 3f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.D)) { x = Mathf.Min(4.5f, x + 3f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.W)) { range = Mathf.Min(16f, range + 6f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.S)) { range = Mathf.Max(3f, range - 6f * Time.deltaTime); changed = true; }
                if (changed)
                {
                    lamp.transform.localPosition = new Vector3(x, 2.6f, -1.5f);
                    lamp.range = range;
                }
                break;

            default:
                if (Input.GetKey(KeyCode.A)) { spotAngle = Mathf.Max(12f, spotAngle - 35f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.D)) { spotAngle = Mathf.Min(80f, spotAngle + 35f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.W)) { range = Mathf.Min(16f, range + 6f * Time.deltaTime); changed = true; }
                if (Input.GetKey(KeyCode.S)) { range = Mathf.Max(4f, range - 6f * Time.deltaTime); changed = true; }
                if (changed)
                {
                    lamp.spotAngle = spotAngle;
                    lamp.range = range;
                }
                break;
        }
    }

    private void Apply()
    {
        if (lamp == null) return;
        switch (mode)
        {
            case Mode.Directional:
                lamp.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
                break;
            case Mode.Point:
                lamp.transform.localPosition = new Vector3(x, 2.6f, -1.5f);
                lamp.range = range;
                break;
            default:
                lamp.spotAngle = spotAngle;
                lamp.range = range;
                break;
        }
    }

    public override void Readout(List<string> lines)
    {
        if (lamp == null) return;
        lines.Add("type       " + lamp.type);
        lines.Add("colour     " + Col(lamp.color));
        lines.Add("intensity  " + F(lamp.intensity, 1));
        switch (mode)
        {
            case Mode.Directional:
                lines.Add("rotation   (" + F(pitch, 0) + ", " + F(yaw, 0) + ", 0)");
                lines.Add("shadows    " + lamp.shadows);
                lines.Add("");
                lines.Add("No position: moving it changes nothing.");
                lines.Add("The rays are parallel, so every object is lit from");
                lines.Add("the same direction and the shadows move together.");
                break;

            case Mode.Point:
                Vector3 p = lamp.transform.localPosition;
                lines.Add("position   (" + F(p.x, 1) + ", " + F(p.y, 1) + ", " + F(p.z, 1) + ")");
                lines.Add("range      " + F(lamp.range, 1));
                lines.Add("");
                lines.Add("A bulb: bright near the light, dark at the range edge.");
                lines.Add("Shadows are off here - the slide recommends leaving them");
                lines.Add("off on point lights unless the scene needs them.");
                break;

            default:
                Vector3 sp = lamp.transform.localPosition;
                lines.Add("position   (" + F(sp.x, 1) + ", " + F(sp.y, 1) + ", " + F(sp.z, 1) + ")");
                lines.Add("angle      " + F(lamp.spotAngle, 0) + " deg   (the cone's width)");
                lines.Add("range      " + F(lamp.range, 1));
                lines.Add("");
                lines.Add("A flashlight: the cone has an edge, and the light");
                lines.Add("also falls off with distance inside it.");
                break;
        }
    }
}
