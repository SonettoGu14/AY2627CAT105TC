using System.Collections.Generic;
using UnityEngine;

/// <summary>Bug 5 from the slide: twelve one-shots at full volume at the same instant add up to a
/// gain of 12, so the output clips and crackles. C switches to the standard fix: fewer voices, quieter.</summary>
public class BugCracklingDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public Transform levelMeter;
    public TextMesh modeText;
    public TextMesh voiceText;
    public TextMesh gainText;
    public GameObject clipMark;

    private const int Burst = 12;
    private const int FixedVoices = 4;
    private const float FixedVolume = 0.25f;

    private readonly List<float> voices = new List<float>();

    private bool fixedMode;
    private float summedGain;
    private float level;

    public override string Title { get { return "Bug 5: Crackling"; } }
    public override string Keys { get { return "SPACE fire a burst   C toggle the fix   R clear voices"; } }

    public override void OnActivate()
    {
        fixedMode = false;
        voices.Clear();
        if (source != null) source.Stop();
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        voices.Clear();
    }

    private void Update()
    {
        float clipLength = source != null && source.clip != null ? source.clip.length : 0.25f;

        for (int i = voices.Count - 1; i >= 0; i--)
            if (voices[i] <= Time.time) voices.RemoveAt(i);

        if (Input.GetKeyDown(KeyCode.Space)) Fire(clipLength);
        if (Input.GetKeyDown(KeyCode.C))
        {
            fixedMode = !fixedMode;
            voices.Clear();
        }
        if (Input.GetKeyDown(KeyCode.R)) voices.Clear();

        float voiceVolume = fixedMode ? FixedVolume : 1f;
        summedGain = voices.Count * voiceVolume;

        float left, right;
        Ears(out left, out right);
        level = Mathf.Max(left, right);

        PracticeBars.Fill(levelMeter, level, 9f);
        if (clipMark != null) clipMark.SetActive(summedGain > 1.0001f);
        ShowText();
    }

    private void Fire(float clipLength)
    {
        if (source == null || source.clip == null) return;

        int limit = fixedMode ? FixedVoices : Burst;
        float volume = fixedMode ? FixedVolume : 1f;

        for (int i = 0; i < limit; i++)
        {
            if (fixedMode && voices.Count >= FixedVoices) break;
            source.PlayOneShot(source.clip, volume);
            voices.Add(Time.time + clipLength);
        }
    }

    private void ShowText()
    {
        if (modeText != null)
        {
            modeText.text = fixedMode ? "FIXED: 4 voices x 0.25" : "12 one-shots x 1.0 at once";
            modeText.color = fixedMode ? DemoColors.Green : DemoColors.Pink;
        }
        if (voiceText != null) voiceText.text = "voices " + voices.Count + " of " + (fixedMode ? FixedVoices : Burst);
        if (gainText != null)
        {
            gainText.text = "summed gain " + summedGain.ToString("0.0") + (summedGain > 1.0001f ? "  CLIPPING" : "  clean");
            gainText.color = summedGain > 1.0001f ? DemoColors.Pink : DemoColors.Dim;
        }
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("mode         " + (fixedMode ? "FIXED: 4 voices x 0.25 = gain 1.0" : "12 one-shots x 1.0 = gain 12"));
        lines.Add("voices now   " + voices.Count + " of " + (fixedMode ? FixedVoices : Burst));
        lines.Add("summed gain  " + summedGain.ToString("0.0") + "   " + (summedGain > 1.0001f ? "CLIPPING - it clips" : "clean"));
        lines.Add("ears         " + Level(level) + " " + Db(level));
        lines.Add("SPACE fires 12 at once; C switches to 4 x 0.25");
    }
}
