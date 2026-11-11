using System.Collections.Generic;
using UnityEngine;

/// <summary>Bug 4 from the slide: the "footstep" fires every frame with Play(), so it is a machine
/// gun instead of footsteps. T swaps in the real manager: 0.4 s throttle, PlayOneShot, random pitch.</summary>
public class BugMachineGunDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public Transform rateMeter;
    public TextMesh modeText;
    public TextMesh rateText;
    public TextMesh verdictText;

    private const float Interval = 0.4f;

    private bool fixedMode;
    private float nextStep;
    private float windowStart;
    private int fired;
    private int rate;
    private int steps;
    private float level;

    public override string Title { get { return "Bug 4: Machine-Gun Footsteps"; } }
    public override string Keys { get { return "T toggle the fix (0.4 s + PlayOneShot)   R reset counters"; } }

    public override void OnActivate()
    {
        fixedMode = false;
        nextStep = 0f;
        windowStart = Time.time;
        fired = 0;
        rate = 0;
        steps = 0;
        if (source != null) source.Stop();
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            fixedMode = !fixedMode;
            if (source != null) source.Stop();
            nextStep = 0f;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            steps = 0;
            fired = 0;
            rate = 0;
            windowStart = Time.time;
        }

        if (source != null && source.clip != null)
        {
            if (!fixedMode)
            {
                // The bug: restart the clip every frame.
                source.pitch = 1f;
                source.Play();
                steps++;
                fired++;
            }
            else if (Time.time >= nextStep)
            {
                nextStep = Time.time + Interval;
                source.pitch = Random.Range(0.9f, 1.1f);
                source.PlayOneShot(source.clip, 1f);
                steps++;
                fired++;
            }
        }

        if (Time.time - windowStart >= 1f)
        {
            rate = fired;
            fired = 0;
            windowStart = Time.time;
        }

        float left, right;
        Ears(out left, out right);
        level = Mathf.Max(left, right);

        PracticeBars.Fill(rateMeter, rate / 60f, 9f);
        ShowText();
    }

    private void ShowText()
    {
        if (modeText != null)
        {
            modeText.text = fixedMode ? "FIXED: 0.4 s throttle + PlayOneShot" : "BROKEN: Play() every frame";
            modeText.color = fixedMode ? DemoColors.Green : DemoColors.Pink;
        }
        if (rateText != null) rateText.text = "fire rate   " + rate + " steps/s   (" + steps + " total)";
        if (verdictText != null)
            verdictText.text = fixedMode ? "one step every 0.4 s: it walks" : "60 restarts a second: a machine gun";
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("mode         " + (fixedMode ? "FIXED: throttle + PlayOneShot" : "BROKEN: Play() every frame"));
        lines.Add("fire rate    " + rate + " steps last second   (fixed: 2-3)");
        lines.Add("ears         " + Level(level) + " " + Db(level));
        lines.Add(fixedMode
            ? "a longer gap and a random pitch make it read as walking"
            : "no throttle and Play() restarting the clip: a machine gun");
        lines.Add("T switches the fixed version in and out");
    }
}
