using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slides 8-10, one concept each:
///   Mask  - SpriteMask + MaskInteraction: the image only shows inside (or only outside) the mask
///   Slice - the sheet was sliced into many sprites; step through them
///   Swap  - assigning spriteRenderer.sprite from code
/// </summary>
public class SpriteDemo : DemoBase
{
    public enum Mode { Mask, Slice, Swap }

    public Mode mode = Mode.Mask;

    // Mask
    public SpriteMask mask;
    public SpriteRenderer masked;
    public SpriteRenderer maskVisual;
    private int interaction;

    // Slice / Swap
    public SpriteRenderer target;
    public Sprite[] frames;          // the 45 sliced sprites (Slice) or the poses (Swap)
    private int frame;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Mask: return "SpriteMask + MaskInteraction hides everything outside the mask";
                case Mode.Slice: return "dicing one sheet into 45 sprites (Sprite Editor)";
                default: return "swap the image from code: spriteRenderer.sprite = ...";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Mask: return "A / D  move the mask      Space  cycle MaskInteraction";
                case Mode.Slice: return "A / D  step through the sliced sprites";
                default: return "Space  assign the next sprite";
            }
        }
    }

    public override void OnActivate()
    {
        interaction = masked != null ? (int)masked.maskInteraction : 0;
        ApplyInteraction();
    }

    private void Update()
    {
        if (mode == Mode.Mask)
        {
            if (mask != null)
            {
                Vector3 p = mask.transform.localPosition;
                if (Input.GetKey(KeyCode.A)) p.x -= 2.5f * Time.deltaTime;
                if (Input.GetKey(KeyCode.D)) p.x += 2.5f * Time.deltaTime;
                p.x = Mathf.Clamp(p.x, -3.5f, 3.5f);
                mask.transform.localPosition = p;
                if (maskVisual != null) maskVisual.transform.localPosition = p;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                interaction = (interaction + 1) % 3;
                ApplyInteraction();
            }
        }
        else if (frames != null && frames.Length > 0)
        {
            if (mode == Mode.Slice)
            {
                if (Input.GetKeyDown(KeyCode.A)) frame = (frame - 1 + frames.Length) % frames.Length;
                if (Input.GetKeyDown(KeyCode.D)) frame = (frame + 1) % frames.Length;
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                frame = (frame + 1) % frames.Length;
            }
            if (target != null) target.sprite = frames[frame];
        }
    }

    private void ApplyInteraction()
    {
        if (masked != null)
            masked.maskInteraction = (SpriteMaskInteraction)interaction;
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Mask:
                if (masked == null) return;
                lines.Add("masked.maskInteraction = " + masked.maskInteraction);
                lines.Add("mask at x              = " + F(mask != null ? mask.transform.localPosition.x : 0f, 1));
                lines.Add("");
                lines.Add("None              - the mask is ignored");
                lines.Add("VisibleInsideMask - only inside the mask shows");
                lines.Add("VisibleOutsideMask- only outside shows");
                break;

            case Mode.Slice:
                lines.Add("sprite = " + (target != null && target.sprite != null ? target.sprite.name : "-"));
                lines.Add("frame  = " + (frame + 1) + " / " + (frames != null ? frames.Length : 0));
                lines.Add("");
                lines.Add("these are all sub-sprites of ONE texture:");
                lines.Add("Sprite Mode = Multiple, sliced in the");
                lines.Add("Sprite Editor. The PNG never changed.");
                break;

            default:
                lines.Add("spriteRenderer.sprite = " + (target != null && target.sprite != null ? target.sprite.name : "-"));
                lines.Add("frame                 = " + (frame + 1) + " / " + (frames != null ? frames.Length : 0));
                lines.Add("");
                lines.Add("exactly what the slide's snippet does -");
                lines.Add("one assignment, no Animator involved.");
                break;
        }
    }
}
