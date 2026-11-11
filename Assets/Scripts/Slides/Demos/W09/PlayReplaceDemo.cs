using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  PLAY REPLACES: one source, two clips. Play() takes the source over, so the clip that was
//  playing is cut off mid-sound. The bars show which clip owns the source.
// ==============================================================================================

public class PlayReplaceDemo : AudioDemoBase
{
    public AudioClip coinClip;
    public AudioClip laserClip;

    public Transform coinFill;
    public Transform laserFill;
    public SpriteRenderer coinFillSprite;
    public SpriteRenderer laserFillSprite;
    public TextMesh flash;
    public float meterWidth = 5f;

    private float flashTimer;
    private string lastCut = "";

    public override string Title { get { return "Play() replaces: one source, one clip at a time"; } }
    public override string Keys { get { return "C coin    L laser"; } }

    public override void OnActivate()
    {
        if (source != null) { source.Stop(); source.pitch = 1f; }
        flashTimer = 0f;
        lastCut = "";
        if (flash != null) flash.text = "";
        AudioDemoUi.Fill(coinFill, 0f, meterWidth);
        AudioDemoUi.Fill(laserFill, 0f, meterWidth);

        // Show something the moment the demo comes up: the source is idle and the bars are flat,
        // which reads as "broken" rather than "waiting for C or L".
        Play(coinClip);
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) Play(coinClip);
        if (Input.GetKeyDown(KeyCode.L)) Play(laserClip);

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && flash != null) flash.text = "";
        }
        UpdateBars();
    }

    private void Play(AudioClip clip)
    {
        if (source == null || clip == null) return;

        // Play() on the same source takes it over - the previous clip is stopped there and then.
        if (source.isPlaying && source.clip != null && source.clip != clip)
        {
            lastCut = source.clip.name.ToUpperInvariant();
            if (flash != null)
            {
                flash.text = lastCut + " CUT OFF";
                flashTimer = 0.9f;
            }
        }
        source.clip = clip;
        source.Play();
    }

    private void UpdateBars()
    {
        float coinLevel = 0f, laserLevel = 0f;
        bool coinPlaying = source != null && source.isPlaying && source.clip == coinClip;
        bool laserPlaying = source != null && source.isPlaying && source.clip == laserClip;

        if (source != null && source.isPlaying && source.clip != null)
        {
            float length = Mathf.Max(0.01f, source.clip.length);
            float u = Mathf.Clamp01(source.time / length);
            if (source.clip == coinClip) coinLevel = u;
            else if (source.clip == laserClip) laserLevel = u;
        }

        AudioDemoUi.Fill(coinFill, coinLevel, meterWidth);
        AudioDemoUi.Fill(laserFill, laserLevel, meterWidth);
        if (coinFillSprite != null)
            coinFillSprite.color = coinPlaying ? UiPalette.Gold : AudioDemoUi.Alpha(UiPalette.Muted, 0.5f);
        if (laserFillSprite != null)
            laserFillSprite.color = laserPlaying ? UiPalette.Pink : AudioDemoUi.Alpha(UiPalette.Muted, 0.5f);
    }

    public override void Readout(List<string> lines)
    {
        if (source != null && source.isPlaying && source.clip != null)
        {
            lines.Add("playing  " + source.clip.name + "   " + F(source.time, 2) + " / "
                      + F(source.clip.length, 2) + " s");
        }
        else
        {
            lines.Add("playing  nothing   (press C or L)");
        }

        lines.Add("one source, one clip: Play() takes it over");
        lines.Add(flashTimer > 0f
            ? lastCut + " was cut off the moment the other clip started"
            : "press L while the coin plays to hear the cut");
        lines.Add("the bar shows how far the playing clip has got");
    }
}
