using System.Collections.Generic;
using UnityEngine;

/// <summary>Bug 2 from the slide: Min Distance 14 with Max Distance 10. The two radii are the wrong
/// way round, so the fade region collapses and the source is silent (or never fades) right next to
/// the listener. The pink ring is Min, the blue one Max - Max sits inside Min.</summary>
public class BugMinDistanceDemo : AudioDemoBase
{
    [Header("Wired by the builder")]
    public Transform levelMeter;
    public TextMesh levelText;

    private const float PathY = 0.2f;
    private const float MinDistance = 14f;
    private const float MaxDistance = 10f;

    private float distance;
    private float level;

    public override string Title { get { return "Bug 2: Min Distance Too Large"; } }
    public override string Keys { get { return "LEFT/RIGHT walk the ears in   R reset position"; } }

    public override void OnActivate()
    {
        if (listener != null) listener.transform.localPosition = new Vector3(-7.4f, PathY, 0f);
        if (source != null) source.Play();
    }

    public override void OnDeactivate()
    {
        if (source != null) source.Stop();
    }

    private void Update()
    {
        if (listener == null) return;

        if (Input.GetKeyDown(KeyCode.R)) listener.transform.localPosition = new Vector3(-7.4f, PathY, 0f);

        float axis = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(axis) > 0.01f)
        {
            Vector3 p = listener.transform.localPosition;
            p.x = Mathf.Clamp(p.x + axis * 4f * Time.deltaTime, -7.4f, 8f);
            p.y = PathY;
            listener.transform.localPosition = p;
        }

        distance = source != null
            ? Vector2.Distance(listener.transform.position, source.transform.position)
            : 0f;

        float left, right;
        Ears(out left, out right);
        level = Mathf.Max(left, right);

        PracticeBars.Fill(levelMeter, level, 5.4f);
        if (levelText != null)
            levelText.text = F(distance) + " units   " + Level(level, 10) + " " + Db(level) + "   " + State();
    }

    private string State()
    {
        if (level <= 0.001f) return "SILENT";
        return level > 0.35f ? "LOUD" : "QUIET";
    }

    public override void Readout(List<string> lines)
    {
        lines.Add("distance     " + F(distance) + " units   (LEFT/RIGHT walks the ears)");
        lines.Add("min 14 (pink)   max 10 (blue): the wrong way round");
        lines.Add("measured     " + Level(level) + " " + Db(level) + "   state " + State());
        lines.Add("Min must be smaller than Max: swapped, the fade collapses.");
        lines.Add("the ears never leave Min's side of the fold - listen for it");
    }
}
