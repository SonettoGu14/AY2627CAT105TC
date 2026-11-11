using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Bug 3 from the slide: the mixer slider moves and the sound does not, because the source's
/// Output is None - it never enters the mixer. O routes it to SFX so the slider suddenly works.</summary>
public class BugOutputNoneDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public Transform sliderFill;
    public Transform sliderKnob;
    public Transform levelMeter;
    public TextMesh sliderText;
    public TextMesh outputText;
    public TextMesh levelText;

    private const float SliderWidth = 4.8f;
    private const float MinDb = -40f;
    private const float MaxDb = 6f;

    private float volumeDb;
    private bool routed;
    private float level;
    private float nextPing;

    public override string Title { get { return "Bug 3: Output Is None"; } }
    public override string Keys { get { return "LEFT/RIGHT SFX volume   O route to SFX   SPACE play   R reset"; } }

    public override void OnActivate()
    {
        volumeDb = 0f;
        routed = false;
        if (source != null)
        {
            source.outputAudioMixerGroup = null;   // Output None: outside the mixer
            source.Stop();
            nextPing = 0f;
        }
        PracticeMixer.Set(mixerGroup, PracticeMixer.SfxParam, 0f);
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null)
        {
            source.Stop();
            source.outputAudioMixerGroup = null;
        }
        PracticeMixer.Set(mixerGroup, PracticeMixer.SfxParam, 0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Nudge(-3f);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Nudge(3f);
        if (Input.GetKeyDown(KeyCode.R)) volumeDb = 0f;
        if (Input.GetKeyDown(KeyCode.O))
        {
            routed = !routed;
            if (source != null) source.outputAudioMixerGroup = routed ? mixerGroup : null;
            nextPing = 0f;                       // hear the routing take effect straight away
        }
        if (Input.GetKeyDown(KeyCode.Space)) nextPing = 0f;
        Ping(source, 2.2f, ref nextPing, 0.8f);

        PracticeMixer.Set(mixerGroup, PracticeMixer.SfxParam, volumeDb);

        float left, right;
        Ears(out left, out right);
        level = Mathf.Max(left, right);

        PracticeBars.Fill(levelMeter, level, SliderWidth);
        PracticeBars.Slider(sliderFill, sliderKnob, Mathf.InverseLerp(MinDb, MaxDb, volumeDb), SliderWidth);
        ShowText();
    }

    private void Nudge(float delta)
    {
        volumeDb = Mathf.Clamp(volumeDb + delta, MinDb, MaxDb);
    }

    private void ShowText()
    {
        if (sliderText != null) sliderText.text = "SFX   " + volumeDb.ToString("0.0") + " dB";
        if (outputText != null)
        {
            outputText.text = routed ? "OUTPUT: SFX GROUP" : "OUTPUT: NONE";
            outputText.color = routed ? DemoColors.Green : DemoColors.Pink;
        }
        if (levelText != null) levelText.text = "ears " + Level(level, 10) + " " + Db(level);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("SFX volume   " + volumeDb.ToString("0.0") + " dB   (LEFT/RIGHT, clamped -40..+6)");
        lines.Add("source output " + (routed ? "-> SFX group" : "None - the source skips the mixer"));
        lines.Add("measured     " + Level(level) + " " + Db(level));
        lines.Add(routed
            ? "now the slider works: the source is inside the mixer"
            : "the slider does nothing: the source's Output is None");
        lines.Add("O routes the source into SFX and back; SPACE plays the coin now");
    }
}
