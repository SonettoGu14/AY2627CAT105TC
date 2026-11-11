using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  PITCH: three rapid identical clicks at FIXED pitch sound mechanical; a small random pitch
//  spread makes them human. 1 fixed, 2 random, R reset.
// ==============================================================================================

public class PitchVaryDemo : AudioDemoBase
{
    public Transform[] pitchFills;
    public TextMesh[] pitchLabels;
    public float meterWidth = 1.5f;

    private readonly float[] pitches = new float[3];
    private int mode = 1;
    private int shotIndex;
    private float shotTimer;
    private bool firing;

    public override string Title { get { return "Pitch: identical clicks sound like a machine gun"; } }
    public override string Keys { get { return "F fixed pitch    V random pitch    R reset"; } }

    public override void OnActivate()
    {
        mode = 1;
        if (source != null) { source.Stop(); source.pitch = 1f; }
        ClearShots();

        // Start on the BAD version (one pitch for every shot) so the machine-gun problem is the
        // first thing you hear; V is then the fix rather than an abstract option.
        BeginSequence();
    }

    public override void OnDeactivate()
    {
        if (source != null) { source.Stop(); source.pitch = 1f; }
        firing = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) { mode = 1; BeginSequence(); }
        if (Input.GetKeyDown(KeyCode.V)) { mode = 2; BeginSequence(); }
        if (Input.GetKeyDown(KeyCode.R)) { if (source != null) source.Stop(); ClearShots(); }

        if (firing)
        {
            shotTimer -= Time.deltaTime;
            if (shotTimer <= 0f) FireShot();
        }
        UpdateBars();
    }

    private void BeginSequence()
    {
        ClearShots();
        shotIndex = 0;
        shotTimer = 0f;
        firing = true;
    }

    private void ClearShots()
    {
        firing = false;
        shotIndex = 0;
        for (int i = 0; i < pitches.Length; i++) pitches[i] = 0f;
    }

    private void FireShot()
    {
        if (source == null || source.clip == null) { firing = false; return; }

        float pitch = mode == 1 ? 1f : Random.Range(0.9f, 1.1f);
        source.pitch = pitch;
        source.PlayOneShot(source.clip);
        if (shotIndex < pitches.Length) pitches[shotIndex] = pitch;
        shotIndex++;
        shotTimer = 0.25f;
        if (shotIndex >= pitches.Length)
        {
            firing = false;
            source.pitch = 1f;
        }
    }

    private void UpdateBars()
    {
        for (int i = 0; i < pitches.Length; i++)
        {
            float pitch = pitches[i];
            float level = pitch > 0f ? Mathf.InverseLerp(0.85f, 1.15f, pitch) : 0f;
            if (pitchFills != null && i < pitchFills.Length)
                AudioDemoUi.Fill(pitchFills[i], level, meterWidth);
            if (pitchLabels != null && i < pitchLabels.Length && pitchLabels[i] != null)
                pitchLabels[i].text = pitch > 0f ? pitch.ToString("0.00") : "--";
        }
    }

    public override void Readout(List<string> lines)
    {
        lines.Add(mode == 1 ? "mode   FIXED  pitch 1.00   (F)" : "mode   RANDOM  pitch 0.90..1.10   (V)");
        lines.Add("pitches  " + Texts());
        lines.Add(mode == 1
            ? "three identical pitches = machine gun"
            : "three different pitches = nothing mechanical");
        lines.Add("three clicks, 0.25 s apart, same clip every time");
    }

    private string Texts()
    {
        string s = "";
        for (int i = 0; i < pitches.Length; i++)
        {
            if (i > 0) s += "  ";
            s += pitches[i] > 0f ? pitches[i].ToString("0.00") : "--";
        }
        return s;
    }
}
