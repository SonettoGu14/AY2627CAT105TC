using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 13 - the lab preview, live. The tiny studio the class will build by hand: a metal cube, a
/// wood sphere, a glass capsule, and one Directional, one Point and one Spot light. Q/W/E toggle the
/// lights so the class can see what each one contributes; A/D orbits the camera.
/// </summary>
public class StudioDemo : DemoBase
{
    public Camera cam;
    public Vector3 orbitTarget = Vector3.zero;
    public Transform[] spinners;
    public Light key;
    public Light point;
    public Light spot;

    private readonly bool[] on = { true, true, true };
    private Vector3 defaultPos;
    private float orbit;
    private float spinAngle;

    public override string Title { get { return "The tiny studio: three materials, three lights"; } }

    public override string Keys { get { return "Q / W / E  lights     A / D  orbit     R  reset"; } }

    public override void OnActivate()
    {
        on[0] = on[1] = on[2] = true;
        orbit = 0f; spinAngle = 0f;
        if (cam != null) defaultPos = cam.transform.localPosition;
        ApplyLights();
        ApplyCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) { on[0] = !on[0]; ApplyLights(); }
        if (Input.GetKeyDown(KeyCode.W)) { on[1] = !on[1]; ApplyLights(); }
        if (Input.GetKeyDown(KeyCode.E)) { on[2] = !on[2]; ApplyLights(); }

        bool moved = false;
        if (Input.GetKey(KeyCode.A)) { orbit = Mathf.Max(-30f, orbit - 30f * Time.deltaTime); moved = true; }
        if (Input.GetKey(KeyCode.D)) { orbit = Mathf.Min(30f, orbit + 30f * Time.deltaTime); moved = true; }
        if (Input.GetKeyDown(KeyCode.R)) { orbit = 0f; moved = true; }
        if (moved) ApplyCamera();

        spinAngle += 12f * Time.deltaTime;
        if (spinners != null)
            foreach (Transform t in spinners) if (t != null) t.localRotation = Quaternion.Euler(0f, spinAngle, 0f);
    }

    private void ApplyLights()
    {
        if (key != null) key.enabled = on[0];
        if (point != null) point.enabled = on[1];
        if (spot != null) spot.enabled = on[2];
    }

    private void ApplyCamera()
    {
        if (cam == null) return;
        Vector3 offset = defaultPos - orbitTarget;
        offset = Quaternion.Euler(0f, orbit, 0f) * offset;
        cam.transform.localPosition = orbitTarget + offset;
        cam.transform.LookAt(cam.transform.parent.TransformPoint(orbitTarget), Vector3.up);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("lights   Q directional " + (on[0] ? "ON " : "off") +
                  "   W point " + (on[1] ? "ON " : "off") +
                  "   E spot " + (on[2] ? "ON" : "off"));
        lines.Add("");
        lines.Add("metal   cube     metallic 1.0  smooth 0.90  #888888");
        lines.Add("wood    sphere   metallic 0.0  smooth 0.20  #8B5A2B");
        lines.Add("glass   capsule  metallic 0.0  smooth 1.00  alpha 0.30");
        lines.Add("");
        lines.Add("This is the lab: Lab8_MaterialsLights.");
        lines.Add("The metal takes the light's colour; the wood keeps a");
        lines.Add("broad highlight; the glass shows what is behind it.");
    }
}
