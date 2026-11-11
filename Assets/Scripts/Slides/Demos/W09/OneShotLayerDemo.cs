using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  PLAYONESHOT LAYERS: the same source keeps all its voices, so rapid presses stack instead of
//  replacing. Five bars show the voices; R silences them.
// ==============================================================================================

public class OneShotLayerDemo : AudioDemoBase
{
    public AudioClip coinClip;
    public AudioClip laserClip;

    public Transform[] voiceFills;
    public SpriteRenderer[] voiceFillSprites;
    public float meterWidth = 1.35f;

    private struct Voice
    {
        public int kind;      // 0 free, 1 coin, 2 laser
        public float left;    // seconds of clip left
        public float length;  // clip length, for the bar height
    }

    private readonly List<float> starts = new List<float>();
    private Voice[] voices;
    private float flashTimer;

    public override string Title { get { return "PlayOneShot() layers: voices stack instead of replace"; } }
    public override string Keys { get { return "C coin    L laser    R silence"; } }

    public override void OnActivate()
    {
        int slots = voiceFills != null ? voiceFills.Length : 0;
        voices = new Voice[slots];
        starts.Clear();
        flashTimer = 0f;
        if (source != null) { source.Stop(); source.pitch = 1f; }
        for (int i = 0; i < slots; i++)
        {
            AudioDemoUi.Fill(voiceFills[i], 0f, meterWidth);
            if (voiceFillSprites != null && i < voiceFillSprites.Length && voiceFillSprites[i] != null)
                voiceFillSprites[i].color = AudioDemoUi.Alpha(UiPalette.Muted, 0.5f);
        }
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        starts.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) Fire(coinClip, 1);
        if (Input.GetKeyDown(KeyCode.L)) Fire(laserClip, 2);
        if (Input.GetKeyDown(KeyCode.R)) Reset();

        if (flashTimer > 0f) flashTimer -= Time.deltaTime;

        float dt = Time.deltaTime;
        for (int i = 0; i < voices.Length; i++)
        {
            if (voices[i].left <= 0f) continue;
            voices[i].left -= dt;
            if (voices[i].left < 0f) voices[i].left = 0f;
        }

        // "started in the last 2 s" is the honest answer to "how many voices did I make?".
        while (starts.Count > 0 && Time.time - starts[0] > 2f) starts.RemoveAt(0);

        UpdateBars();
    }

    private void Fire(AudioClip clip, int kind)
    {
        if (source == null || clip == null || voices == null || voices.Length == 0) return;

        int slot = -1;
        for (int i = 0; i < voices.Length; i++)
        {
            if (voices[i].left <= 0f) { slot = i; break; }
        }
        if (slot < 0)
        {
            // All five busy: the voice closest to finishing is the one that gives way.
            slot = 0;
            for (int i = 1; i < voices.Length; i++)
                if (voices[i].left < voices[slot].left) slot = i;
        }

        voices[slot].kind = kind;
        voices[slot].left = clip.length;
        voices[slot].length = Mathf.Max(0.01f, clip.length);
        source.PlayOneShot(clip);
        starts.Add(Time.time);
        flashTimer = 0.4f;
    }

    private void Reset()
    {
        if (source != null) source.Stop();
        for (int i = 0; i < voices.Length; i++) voices[i].left = 0f;
        starts.Clear();
    }

    private void UpdateBars()
    {
        for (int i = 0; i < voices.Length; i++)
        {
            float level = voices[i].left > 0f ? voices[i].left / voices[i].length : 0f;
            AudioDemoUi.Fill(voiceFills[i], level, meterWidth);
            if (voiceFillSprites != null && i < voiceFillSprites.Length && voiceFillSprites[i] != null)
            {
                voiceFillSprites[i].color = voices[i].kind == 1
                    ? UiPalette.Gold
                    : voices[i].kind == 2 ? UiPalette.Pink : AudioDemoUi.Alpha(UiPalette.Muted, 0.5f);
            }
        }
    }

    private int Alive()
    {
        int n = 0;
        for (int i = 0; i < voices.Length; i++) if (voices[i].left > 0f) n++;
        return n;
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("voices alive   " + Alive() + " / " + (voices != null ? voices.Length : 0));
        lines.Add("started in the last 2 s   " + starts.Count);
        lines.Add("PlayOneShot adds a voice; nothing is cut off");
        lines.Add("coin voices are gold, laser voices pink");
        lines.Add(flashTimer > 0f ? "all five bars are a stack, not replacements" : "press C or L quickly to build the stack");
    }
}
