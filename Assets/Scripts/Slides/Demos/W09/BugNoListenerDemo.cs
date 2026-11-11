using System.Collections.Generic;
using UnityEngine;

/// <summary>Bug 1 from the slide: everything is playing and there is nothing to hear, because the
/// scene has no active AudioListener. L brings the ears back. This demo never reads the ears - there
/// is nothing to read - it measures the source instead.</summary>
public class BugNoListenerDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public GameObject earObject;
    public TextMesh headline;
    public TextMesh earState;
    public Transform levelMeter;
    public TextMesh levelText;

    private float sourceLevel;

    public override string Title { get { return "Bug 1: No AudioListener"; } }
    public override string Keys { get { return "L turn the AudioListener on / off"; } }

    private bool Listening { get { return earObject != null && earObject.activeSelf; } }

    public override void OnActivate()
    {
        if (earObject != null) earObject.SetActive(false);
        if (source != null) source.Play();
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
        // Never leave a second listener behind: the next demo brings its own.
        if (earObject != null) earObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && earObject != null) earObject.SetActive(!earObject.activeSelf);

        // No listener => no Ears() here; ask the source instead.
        sourceLevel = SourceLevel(source);
        PracticeBars.Fill(levelMeter, sourceLevel, 4.2f);
        ShowText();
    }

    private void ShowText()
    {
        if (headline != null)
        {
            headline.text = Listening ? "LISTENING" : "NO LISTENER";
            headline.color = Listening ? DemoColors.Green : DemoColors.Pink;
        }
        if (earState != null)
        {
            earState.text = Listening ? "listener ON" : "listener OFF";
            earState.color = Listening ? DemoColors.Green : DemoColors.Pink;
        }
        if (levelText != null) levelText.text = "source " + Level(sourceLevel, 10) + " " + Db(sourceLevel);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("listener     " + (Listening ? "ON - an active AudioListener" : "NONE - the EARS object is inactive"));
        lines.Add("source       " + (source != null && source.isPlaying ? "playing" : "stopped") +
                  "   level " + Level(sourceLevel) + " " + Db(sourceLevel));
        lines.Add("what you hear " + (Listening ? "the fire loop, fading as the ears stay put" : "SILENCE - nothing is listening"));
        lines.Add("no AudioListener = silence. The #1 audio bug.");
        lines.Add("L brings the ears back: a sound needs someone listening");
    }
}
