using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "2D Rendering: SpriteRenderer" (slide 7), split into three single-concept demos:
///   Color   - SpriteRenderer.color tints the sprite (a dye: it can only darken)
///   Flip    - flipX / flipY mirror the sprite
///   Sorting - Sorting Layer + Order decides which sprite is in front
/// </summary>
public class SpriteRendererDemo : DemoBase
{
    public enum Mode { Color, Flip, Sorting }

    public Mode mode = Mode.Color;
    public SpriteRenderer target;          // Color + Flip
    public SpriteRenderer front;           // Sorting
    public SpriteRenderer back;

    private int orderOffset;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Color: return "SpriteRenderer.color tints the sprite";
                case Mode.Flip: return "SpriteRenderer.flipX / flipY mirror the sprite";
                default: return "Sorting Order decides which sprite is in front";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Color: return "R / G / B  change the tint      Shift+R/G/B  reduce it";
                case Mode.Flip: return "F  flipX      V  flipY";
                default: return "W  raise the front sprite's order      S  lower it";
            }
        }
    }

    private void Update()
    {
        if (mode == Mode.Color && target != null)
        {
            Color c = target.color;
            if (Input.GetKey(KeyCode.R)) c.r += 0.5f * Time.deltaTime;
            if (Input.GetKey(KeyCode.G)) c.g += 0.5f * Time.deltaTime;
            if (Input.GetKey(KeyCode.B)) c.b += 0.5f * Time.deltaTime;
            if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.LeftShift)) c.r -= 1.5f * Time.deltaTime;
            if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.LeftShift)) c.g -= 1.5f * Time.deltaTime;
            if (Input.GetKey(KeyCode.B) && Input.GetKey(KeyCode.LeftShift)) c.b -= 1.5f * Time.deltaTime;
            target.color = new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), 1f);
        }
        else if (mode == Mode.Flip && target != null)
        {
            if (Input.GetKeyDown(KeyCode.F)) target.flipX = !target.flipX;
            if (Input.GetKeyDown(KeyCode.V)) target.flipY = !target.flipY;
        }
        else if (mode == Mode.Sorting && front != null)
        {
            if (Input.GetKeyDown(KeyCode.W)) orderOffset += 1;
            if (Input.GetKeyDown(KeyCode.S)) orderOffset -= 1;
            front.sortingOrder = orderOffset;
        }
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Color:
                if (target == null) return;
                lines.Add("target.color = " + Col(target.color));
                lines.Add("sprite       = " + (target.sprite != null ? target.sprite.name : "-"));
                lines.Add("");
                lines.Add("colour is a dye: it multiplies the sprite's");
                lines.Add("own colours, so it can only get darker.");
                break;

            case Mode.Flip:
                if (target == null) return;
                lines.Add("target.flipX = " + B(target.flipX));
                lines.Add("target.flipY = " + B(target.flipY));
                lines.Add("");
                lines.Add("only the rendering is mirrored -");
                lines.Add("the transform, the collider and physics");
                lines.Add("are untouched.");
                break;

            default:
                if (front == null || back == null) return;
                lines.Add("front.sortingOrder = " + front.sortingOrder);
                lines.Add("back.sortingOrder  = " + back.sortingOrder);
                lines.Add("in front           = " + (front.sortingOrder >= back.sortingOrder ? "front (orange)" : "back (blue)"));
                lines.Add("");
                lines.Add("SpriteRenderer sorting is not the same as");
                lines.Add("the GameObject's Layer - the Layer");
                lines.Add("classifies, the Sorting Layer draws.");
                break;
        }
    }
}
