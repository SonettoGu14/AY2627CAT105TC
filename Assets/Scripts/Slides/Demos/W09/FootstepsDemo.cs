using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Week 9, the practice half: the footstep manager (slide 7), the AudioMixer (slide 8: groups and
/// routing), the five classic audio bugs (slide 9) and the walkaround (slide 10).
///
/// Each demo teaches exactly ONE thing and says it in the readout. The builder (W09PracticeDemos)
/// does all the furnishing; these components only run the interaction and report what you hear.
///
/// The measuring rule: SourceLevel() is "what the source puts out" (before the mixer and before the
/// ears), Ears() is "what the listener actually gets". Whatever a demo claims about volume or
/// panning is measured, not assumed - except where there is no listener at all, which is itself the
/// lesson (bug_no_listener must never call Ears(), Unity would log an error every frame).
/// </summary>
public class FootstepsDemo : AudioDemoBase
{
    /// <summary>The throttle: one step every 0.4 s, whatever the frame rate.</summary>
    public const float Interval = 0.6f;   // a walking pace (~100 steps/min), not a march

    [Header("Wired by the builder")]
    public AudioClip[] dirt = new AudioClip[0];
    public AudioClip[] grass = new AudioClip[0];
    public AudioClip[] snow = new AudioClip[0];
    public AudioClip[] gravel = new AudioClip[0];
    public Transform player;
    public Transform[] prints = new Transform[0];
    public TextMesh terrainText;
    public TextMesh stepText;
    public Color[] terrainColors = new Color[0];

    private static readonly string[] TerrainNames = { "dirt", "grass", "snow", "gravel" };

    private int terrain;
    private int steps;
    private int printIndex;
    private float nextStep;
    private float lastPitch = 1f;
    private string lastClip = "-";

    public override string Title { get { return "Footstep Manager"; } }
    public override string Keys { get { return "LEFT/RIGHT walk   Z dirt  X grass  C snow  V gravel   R reset"; } }

    public override void OnActivate()
    {
        terrain = 0;
        steps = 0;
        printIndex = 0;
        nextStep = 0f;
        lastPitch = 1f;
        lastClip = "-";
        if (player != null) player.localPosition = new Vector3(0f, -0.4f, 0f);
        if (source != null) source.Stop();
        HidePrints();
        ShowText();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) SetTerrain(0);
        if (Input.GetKeyDown(KeyCode.X)) SetTerrain(1);
        if (Input.GetKeyDown(KeyCode.C)) SetTerrain(2);
        if (Input.GetKeyDown(KeyCode.V)) SetTerrain(3);
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetTerrain(0);
            steps = 0;
            printIndex = 0;
            HidePrints();
        }

        if (player == null) return;

        // The slide's Update(): move on the axis, and fire on a timer - never on the frame.
        float axis = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(axis) > 0.01f)
        {
            Vector3 p = player.localPosition;
            p.x = Mathf.Clamp(p.x + axis * 3.2f * Time.deltaTime, -7.2f, 7.2f);
            player.localPosition = p;

            if (Time.time >= nextStep) Step();
        }

        ShowText();
    }

    private void Step()
    {
        AudioClip[] clips = ClipsFor(terrain);
        if (source == null || clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(clip, 0.9f);

        lastPitch = source.pitch;
        lastClip = clip.name;
        steps++;
        nextStep = Time.time + Interval;
        DropPrint();
    }

    private AudioClip[] ClipsFor(int index)
    {
        if (index == 0) return dirt;
        if (index == 1) return grass;
        if (index == 2) return snow;
        return gravel;
    }

    private void SetTerrain(int index)
    {
        terrain = Mathf.Clamp(index, 0, TerrainNames.Length - 1);
        ShowText();
    }

    private void DropPrint()
    {
        if (prints == null || prints.Length == 0) return;
        Transform print = prints[printIndex % prints.Length];
        printIndex++;
        if (print == null) return;

        print.localPosition = new Vector3(player.localPosition.x, -0.94f, 0f);
        SpriteRenderer sr = print.GetComponent<SpriteRenderer>();
        if (sr != null && terrainColors != null && terrain < terrainColors.Length) sr.color = terrainColors[terrain];
        print.gameObject.SetActive(true);
    }

    private void HidePrints()
    {
        if (prints == null) return;
        for (int i = 0; i < prints.Length; i++)
            if (prints[i] != null) prints[i].gameObject.SetActive(false);
    }

    private void ShowText()
    {
        if (terrainText != null)
            terrainText.text = "TERRAIN   " + TerrainNames[terrain].ToUpperInvariant() + "   (" + (terrain + 1) + ")";
        if (stepText != null)
            stepText.text = "steps " + steps + "    pitch " + lastPitch.ToString("0.00") + "    one per 0.4 s";
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("terrain      " + TerrainNames[terrain] + "   (1-4 switches the clip set)");
        lines.Add("steps fired  " + steps + "   one every " + Interval.ToString("0.0") + " s while moving");
        lines.Add("last step    " + lastClip + "   pitch " + lastPitch.ToString("0.00"));
        lines.Add("the pattern  3 clips per terrain, random pick, pitch +/-10%");
        lines.Add("hold LEFT/RIGHT: the 0.4 s timer, not the frame rate, sets the pace");
    }
}
