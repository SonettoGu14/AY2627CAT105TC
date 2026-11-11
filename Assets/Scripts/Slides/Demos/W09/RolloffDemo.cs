using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  ROLLOFF: the same clip, the same point, the same volume - only the curve differs. The two
//  sources cannot be audible at once or you would just hear one of them, so the demo measures
//  them one at a time: SPACE plays ~0.7 s of logarithmic, then ~0.7 s of linear, then goes quiet
//  again. Silence between measurements is deliberate - a permanently alternating A/B of a
//  looping clip under a lecture is not something anyone wants to listen to.
// ==============================================================================================

public class RolloffDemo : AudioDemoBase
{
    public Transform logFill;
    public Transform linearFill;
    public SpriteRenderer logFillSprite;
    public SpriteRenderer linearFillSprite;
    public float meterWidth = 4.6f;
    public float moveSpeed = 3f;
    public float measureTime = 0.7f;

    private int phase;                 // 0 = measuring logarithmic, 1 = measuring linear
    private bool measuring;
    private float measureLeft;
    private float clock;
    private float phaseMax;
    private float logLevel;
    private float linearLevel;
    private float measuredDistance = -1f;

    public override string Title { get { return "Rolloff: two curves at the same distance"; } }
    public override string Keys { get { return "Left / Right move the ears    SPACE measure    R reset"; } }

    public override void OnActivate()
    {
        if (listener != null) listener.transform.localPosition = new Vector3(-6f, 0.2f, 0f);
        logLevel = 0f;
        linearLevel = 0f;
        measuredDistance = -1f;
        if (source != null) { source.Stop(); source.mute = true; source.Play(); }
        if (secondSource != null) { secondSource.Stop(); secondSource.mute = true; secondSource.Play(); }
        BeginMeasure();                 // one measurement on arrival, so the bars are not empty
    }

    public override void OnDeactivate()
    {
        if (source != null) { source.Stop(); source.mute = false; }
        if (secondSource != null) { secondSource.Stop(); secondSource.mute = false; }
        measuring = false;
    }

    private void Update()
    {
        if (listener == null || source == null || secondSource == null) return;

        Vector3 p = listener.transform.localPosition;
        float dt = Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) p.x -= moveSpeed * dt;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) p.x += moveSpeed * dt;
        if (Input.GetKeyDown(KeyCode.R)) p.x = 0f;
        p.x = Mathf.Clamp(p.x, -7f, 7f);
        p.y = 0.2f;
        listener.transform.localPosition = p;

        if (Input.GetKeyDown(KeyCode.Space)) BeginMeasure();

        if (measuring)
        {
            // Keep the loudest sample of the settled part of the window: that is a real measurement
            // of ONE curve at the current distance.
            Ears(out float left, out float right);
            float level = Mathf.Max(left, right);
            clock += dt;
            if (clock > 0.12f) phaseMax = Mathf.Max(phaseMax, level);
            measureLeft -= dt;

            if (measureLeft <= 0f)
            {
                if (phase == 0)
                {
                    logLevel = phaseMax;
                    phase = 1;
                    clock = 0f;
                    phaseMax = 0f;
                    measureLeft = measureTime;
                    ApplySolo();
                }
                else
                {
                    linearLevel = phaseMax;
                    measuring = false;
                    MuteBoth();
                }
            }
        }

        UpdateBars();
    }

    private void BeginMeasure()
    {
        if (measuring) return;
        measuredDistance = Vector3.Distance(source.transform.position, listener.transform.position);
        phase = 0;
        clock = 0f;
        phaseMax = 0f;
        measureLeft = measureTime;
        measuring = true;
        if (source != null) { source.Stop(); source.Play(); }
        if (secondSource != null) { secondSource.Stop(); secondSource.Play(); }
        ApplySolo();
    }

    private void ApplySolo()
    {
        if (source != null) source.mute = phase != 0;
        if (secondSource != null) secondSource.mute = phase != 1;
    }

    private void MuteBoth()
    {
        if (source != null) source.mute = true;
        if (secondSource != null) secondSource.mute = true;
    }

    private void UpdateBars()
    {
        AudioDemoUi.Fill(logFill, AudioDemoUi.Meter(logLevel), meterWidth);
        AudioDemoUi.Fill(linearFill, AudioDemoUi.Meter(linearLevel), meterWidth);

        bool measuringLog = measuring && phase == 0;
        bool measuringLin = measuring && phase == 1;
        if (logFillSprite != null)
            logFillSprite.color = measuringLog ? UiPalette.Accent : AudioDemoUi.Alpha(UiPalette.Muted, 0.5f);
        if (linearFillSprite != null)
            linearFillSprite.color = measuringLin ? UiPalette.Pink : AudioDemoUi.Alpha(UiPalette.Muted, 0.5f);
    }

    public override void Readout(List<string> lines)
    {
        if (source == null || listener == null) return;

        float distance = Vector3.Distance(source.transform.position, listener.transform.position);
        lines.Add("distance  " + F(distance, 2) + " m");
        if (measuring)
            lines.Add((phase == 0 ? "hearing LOGARITHMIC" : "hearing LINEAR") + " now   "
                      + F(Mathf.Max(0f, measureLeft), 1) + " s left");
        else
            lines.Add("LOG  " + Db(logLevel) + "    LIN  " + Db(linearLevel)
                      + (measuredDistance >= 0f ? "   at " + F(measuredDistance, 1) + " m" : ""));
        lines.Add(measuring ? "the other source is muted while it measures"
                            : "press SPACE to measure both curves at this distance");
        lines.Add(Verdict());
        lines.Add("same clip, same point, same volume - only the curve differs");
    }

    private string Verdict()
    {
        if (logLevel <= 0.0001f && linearLevel <= 0.0001f) return "press SPACE to take a measurement";
        if (linearLevel > logLevel * 1.05f) return "LINEAR is louder at this distance";
        if (logLevel > linearLevel * 1.05f) return "LOGARITHMIC is louder at this distance";
        return "about the same at this distance";
    }
}
