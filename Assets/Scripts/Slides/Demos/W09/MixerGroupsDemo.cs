using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>The sound of one mixer slider reaching a whole category: BGM and SFX as two branches of
/// Master. The levels are measured by hard-panning one source per branch, so each meter really is
/// that group and nothing else.</summary>
public class MixerGroupsDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public AudioSource unroutedSource;
    public Transform masterMeter;
    public Transform bgmMeter;
    public Transform sfxMeter;
    public TextMesh masterDbText;
    public TextMesh bgmDbText;
    public TextMesh sfxDbText;
    public Transform[] selectMarks = new Transform[0];

    private static readonly string[] GroupNames = { "BGM", "SFX", "MASTER" };
    private static readonly string[] GroupParams = { PracticeMixer.BgmParam, PracticeMixer.SfxParam, PracticeMixer.MasterParam };

    private readonly float[] volume = new float[3];
    private int selected;
    private float nextCoin;
    private float nextBlip;
    private float masterLevel;
    private float bgmLevel;
    private float sfxLevel;

    public override string Title { get { return "AudioMixer: Groups"; } }
    public override string Keys { get { return "LEFT/RIGHT pick bus   UP/DOWN 3 dB   R reset"; } }

    public override void OnActivate()
    {
        selected = 0;
        for (int i = 0; i < volume.Length; i++)
        {
            volume[i] = 0f;
            Apply(i);
        }
        if (source != null) source.Play();          // bgm_loop, looping, hard left
        // The two short SFX are NOT looped - a coin on repeat for a whole slide is awful. They
        // ping every couple of seconds, which is enough for the meters to move.
        if (secondSource != null) secondSource.Stop();
        if (unroutedSource != null) unroutedSource.Stop();
        nextCoin = 0f;
        nextBlip = 1.2f;
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        if (secondSource != null) secondSource.Stop();
        if (unroutedSource != null) unroutedSource.Stop();
        for (int i = 0; i < volume.Length; i++)
        {
            volume[i] = 0f;
            Apply(i);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Select((selected + GroupNames.Length - 1) % GroupNames.Length);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Select((selected + 1) % GroupNames.Length);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Nudge(3f);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Nudge(-3f);
        if (Input.GetKeyDown(KeyCode.R)) ResetAll();

        Ping(secondSource, 2.5f, ref nextCoin, 0.55f);
        Ping(unroutedSource, 2.5f, ref nextBlip, 0.5f);

        float left, right;
        Ears(out left, out right);
        masterLevel = Mathf.Max(left, right);
        bgmLevel = left;
        sfxLevel = right;

        PracticeBars.Fill(masterMeter, masterLevel, 3.2f);
        PracticeBars.Fill(bgmMeter, bgmLevel, 3.2f);
        PracticeBars.Fill(sfxMeter, sfxLevel, 3.2f);
        ShowText();
    }

    private void Select(int index)
    {
        selected = Mathf.Clamp(index, 0, GroupNames.Length - 1);
        ShowText();
    }

    private void Nudge(float delta)
    {
        volume[selected] = Mathf.Clamp(volume[selected] + delta, -40f, 6f);
        Apply(selected);
    }

    private void ResetAll()
    {
        for (int i = 0; i < volume.Length; i++)
        {
            volume[i] = 0f;
            Apply(i);
        }
    }

    private void Apply(int index)
    {
        PracticeMixer.Set(mixerGroup, GroupParams[index], volume[index]);
    }

    private void ShowText()
    {
        SetText(masterDbText, DbText(volume[2]));
        SetText(bgmDbText, DbText(volume[0]));
        SetText(sfxDbText, DbText(volume[1]));
        for (int i = 0; i < selectMarks.Length && i < GroupNames.Length; i++)
            if (selectMarks[i] != null) selectMarks[i].gameObject.SetActive(i == selected);
    }

    private static void SetText(TextMesh mesh, string text)
    {
        if (mesh != null) mesh.text = text;
    }

    private static string DbText(float db)
    {
        return db.ToString("0.0") + " dB";
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("groups       Master > BGM / SFX  (the tree on the left)");
        if (mixerGroup == null)
        {
            lines.Add("mixer        MISSING - no W09Mixer.mixer, so no SetFloat");
        }
        else
        {
            lines.Add("BGM " + DbText(volume[0]) + "   SFX " + DbText(volume[1]) + "   MASTER " + DbText(volume[2]));
        }
        lines.Add("master meter " + Level(masterLevel) + " " + Db(masterLevel));
        lines.Add("bgm / sfx    " + Level(bgmLevel) + "  " + Level(sfxLevel));
        lines.Add("selected     " + GroupNames[selected] + ": UP/DOWN moves that group 3 dB");
    }
}
