using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Shared plumbing for the Week 9 (audio) demos. Nothing here teaches anything: it is the few
/// helpers every audio demo would otherwise have to copy - reading a real level out of the
/// AudioListener so the readout can show what you are hearing, and formatting dB / bar text.
///
/// The rule of the week (see Assets/Scripts/README.md): a demo teaches exactly ONE thing. The
/// builder configures the AudioSources and AudioListeners; the demo only decides what to do with
/// them and what to say about it.
/// </summary>
public abstract class AudioDemoBase : DemoBase
{
    [Header("Wired by W09Kit in the scene builder")]
    public AudioSource source;              // the demo's main source (2D or 3D)
    public AudioSource secondSource;        // only where a demo compares two sources
    public AudioListener listener;          // the demo's own ears (own camera => own listener)
    public AudioMixerGroup mixerGroup;      // the mixer group this demo's sources route to

    // 256 is a power of two, which GetOutputData requires.
    private static readonly float[] Buffer = new float[256];

    /// <summary>RMS of what one source is putting out right now (0..1). Pre-spatialisation, so it
    /// answers "is this source audible at all", not "is it on my left".</summary>
    protected static float SourceLevel(AudioSource s)
    {
        if (s == null || !s.isActiveAndEnabled) return 0f;
        s.GetOutputData(Buffer, 0);
        return Rms(Buffer);
    }

    /// <summary>RMS of the two channels the LISTENER is producing right now. This is after volume,
    /// 3D attenuation and panning, so it is the honest answer to "what do my ears get?"</summary>
    protected static void Ears(out float left, out float right)
    {
        AudioListener.GetOutputData(Buffer, 0);
        left = Rms(Buffer);
        AudioListener.GetOutputData(Buffer, 1);
        right = Rms(Buffer);
    }

    private static float Rms(float[] samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
        return Mathf.Sqrt(sum / samples.Length);
    }

    /// <summary>Linear gain as dB, the way AudioMixer shows it. Silence has no dB value.</summary>
    protected static string Db(float linear)
    {
        if (linear <= 0.0001f) return "-inf dB";
        return (20f * Mathf.Log10(linear)).ToString("0.0") + " dB";
    }

    /// <summary>An ASCII level bar - the readout is text-only, so this is the meter.</summary>
    protected static string Level(float linear, int width = 14)
    {
        int filled = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt(Mathf.Clamp01(linear)) * width), 0, width);
        return new string('#', filled) + new string('.', width - filled);
    }

    /// <summary>Where a source sits between hard left (-1) and hard right (+1) for the listening ears.</summary>
    protected static float Pan(float left, float right)
    {
        float total = left + right;
        return total <= 0.0001f ? 0f : Mathf.Clamp((right - left) / total, -1f, 1f);
    }

    protected static string PanText(float pan)
    {
        if (pan < -0.15f) return "LEFT  " + pan.ToString("0.00");
        if (pan > 0.15f) return "RIGHT " + pan.ToString("+0.00");
        return "CENTRE " + pan.ToString("0.00");
    }

    /// <summary>Fire a source's own clip as a one-shot every `interval` seconds. Looping a short
    /// SFX repeats it forever, which is unbearable across a whole slide; a ping every couple of
    /// seconds still gives the meters something to read. `next` is the caller's own timer field.</summary>
    protected static void Ping(AudioSource s, float interval, ref float next, float volume = 1f)
    {
        if (s == null || s.clip == null) return;
        if (Time.time < next) return;
        next = Time.time + interval;
        s.PlayOneShot(s.clip, volume);
    }
}
