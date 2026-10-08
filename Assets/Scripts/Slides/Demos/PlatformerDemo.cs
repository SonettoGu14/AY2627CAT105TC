using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 22's demo ("Animation production practice"): the whole W4 platformer, which is the lab
/// the students write that week. It exists here as the one demo that puts everything together,
/// rather than as a generic backdrop for the whole deck.
/// </summary>
public class PlatformerDemo : DemoBase
{
    public override string Title
    {
        get { return "the W4 platformer - everything from this lecture, running together"; }
    }

    public override string Keys { get { return "A / D  move      Space  jump"; } }

    public override void Readout(List<string> lines)
    {
        lines.Add("This is the scene W4Lab builds - the lab you");
        lines.Add("write this week, not a slide-only demo.");
        lines.Add("");
        lines.Add("Everything from the lecture is in here:");
        lines.Add("  rb.velocity movement + AddForce(Impulse) jump");
        lines.Add("  the Physics2D.Raycast ground check");
        lines.Add("  sprite animation driven through an Animator");
        lines.Add("  an orthographic camera following the character");
    }
}
