using System.Collections.Generic;
using UnityEngine;

/// <summary>Same two coins, same place, same sound: one in the SFX group, one with Output None.
/// Drop the group and only the routed speaker moves - Output None means "not in the mixer".</summary>
public class MixerBypassDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public Transform routedMeter;
    public Transform unroutedMeter;
    public TextMesh routedText;
    public TextMesh unroutedText;
    public TextMesh groupText;

    private bool dropped;
    private float nextRouted;
    private float nextUnrouted;
    private float routedLevel;
    private float unroutedLevel;

    public override string Title { get { return "Mixer Bypass: Output None"; } }
    public override string Keys { get { return "M drop SFX to -30 dB (toggle)   R reset to 0 dB"; } }

    public override void OnActivate()
    {
        dropped = false;
        PracticeMixer.Set(mixerGroup, PracticeMixer.SfxParam, 0f);
        // Coins, not loops: one on each side every couple of seconds. Looping a coin forever is
        // exactly the kind of noise this slide is supposed to teach you to avoid.
        if (source != null) source.Stop();
        if (secondSource != null) secondSource.Stop();
        nextRouted = 0f;
        nextUnrouted = 1.25f;
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        if (secondSource != null) secondSource.Stop();
        PracticeMixer.Set(mixerGroup, PracticeMixer.SfxParam, 0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) { dropped = !dropped; nextRouted = 0f; nextUnrouted = 0.15f; }
        if (Input.GetKeyDown(KeyCode.R)) dropped = false;
        Ping(source, 2.5f, ref nextRouted, 0.6f);
        Ping(secondSource, 2.5f, ref nextUnrouted, 0.6f);
        PracticeMixer.Set(mixerGroup, PracticeMixer.SfxParam, dropped ? -30f : 0f);

        float left, right;
        Ears(out left, out right);
        routedLevel = left;
        unroutedLevel = right;

        PracticeBars.Fill(routedMeter, routedLevel, 4.4f);
        PracticeBars.Fill(unroutedMeter, unroutedLevel, 4.4f);
        ShowText();
    }

    private void ShowText()
    {
        if (groupText != null) groupText.text = "SFX group   " + (dropped ? "-30.0 dB" : "0.0 dB");
        if (routedText != null) routedText.text = "L " + Level(routedLevel, 10) + " " + Db(routedLevel);
        if (unroutedText != null) unroutedText.text = "R " + Level(unroutedLevel, 10) + " " + Db(unroutedLevel);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("SFX group    " + (dropped ? "-30.0 dB" : "0.0 dB") + "   (M toggles, R resets)");
        lines.Add("routed       " + Level(routedLevel) + " " + Db(routedLevel));
        lines.Add("unrouted     " + Level(unroutedLevel) + " " + Db(unroutedLevel));
        lines.Add("the unrouted source ignores the slider: its Output is None");
        lines.Add("Output None = the sound skips the mixer entirely");
    }
}
