using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One slide demo. Each concrete demo teaches exactly ONE thing (the instructor's rule: split by
/// knowledge point, no composite effects) and reports what it is doing so the readout panel can
/// show it live next to the demo.
///
/// Concrete demos live in the *Demo.cs files next to this one; SlideDeckBuilder builds the objects
/// for each and DemoStage activates the one belonging to the current slide.
/// </summary>
public abstract class DemoBase : MonoBehaviour
{
    /// <summary>What this demo teaches, e.g. "SpriteRenderer.color tints the sprite".</summary>
    public abstract string Title { get; }

    /// <summary>One-line key hint, e.g. "A / D  change red".</summary>
    public virtual string Keys { get { return ""; } }

    /// <summary>Live lines for the readout panel. Called every frame; keep it cheap.</summary>
    public abstract void Readout(List<string> lines);

    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }

    // ---- small formatting helpers so every demo reads the same way ----
    protected static string B(bool v) { return v ? "true" : "false"; }
    protected static string F(float v, int decimals = 2) { return v.ToString("0." + new string('0', decimals)); }
    protected static string V(Vector2 v) { return "(" + F(v.x, 1) + ", " + F(v.y, 1) + ")"; }
    protected static string V(Vector3 v) { return "(" + F(v.x, 1) + ", " + F(v.y, 1) + ", " + F(v.z, 1) + ")"; }
    protected static string Col(Color c) { return "(" + F(c.r, 2) + ", " + F(c.g, 2) + ", " + F(c.b, 2) + ")"; }
}
