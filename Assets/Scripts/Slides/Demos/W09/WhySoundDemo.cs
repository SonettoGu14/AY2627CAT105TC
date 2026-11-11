using System.Collections.Generic;
using UnityEngine;


// ==============================================================================================
//  WHY SOUND: two 3D emitters hidden behind fog. The picture is identical either way; the ears
//  are what tell you where the coin is. SPACE mutes, E lifts the fog.
// ==============================================================================================

public class WhySoundDemo : AudioDemoBase
{
    public SpriteRenderer fogLeft;
    public SpriteRenderer fogRight;

    private bool audible = true;
    private bool revealed;
    private float nextCoin;

    public override string Title { get { return "Why sound: the picture cannot tell you where"; } }
    public override string Keys { get { return "SPACE mute    E reveal"; } }

    public override void OnActivate()
    {
        audible = true;
        revealed = false;
        nextCoin = 0f;
        if (source != null) { source.Stop(); source.mute = false; source.pitch = 1f; }
        if (secondSource != null)
        {
            secondSource.Stop();
            secondSource.mute = false;
            secondSource.Play();
        }
        Apply();
    }

    public override void OnDeactivate()
    {
        if (source != null) { source.Stop(); source.mute = false; }
        if (secondSource != null) { secondSource.Stop(); secondSource.mute = false; }
        SetFogAlpha(1f);
    }

    private void Update()
    {
        if (source == null || secondSource == null) return;

        if (Input.GetKeyDown(KeyCode.Space)) { audible = !audible; Apply(); }
        if (Input.GetKeyDown(KeyCode.E)) { revealed = !revealed; Apply(); }

        // The coin fires on its own so "which side?" is a question you can ask repeatedly.
        nextCoin -= Time.deltaTime;
        if (nextCoin <= 0f && source.clip != null)
        {
            source.PlayOneShot(source.clip);
            nextCoin = 2.6f;
        }
    }

    private void Apply()
    {
        if (source != null) source.mute = !audible;
        if (secondSource != null) secondSource.mute = !audible;
        // Audible: the fog stays shut so only the ears know. Muted: lift it a little to show that
        // the two emitters were there all along - you just cannot place them by ear.
        SetFogAlpha(revealed ? 0.02f : (audible ? 1f : 0.5f));
    }

    private void SetFogAlpha(float a)
    {
        if (fogLeft != null) fogLeft.color = AudioDemoUi.Alpha(fogLeft.color, a);
        if (fogRight != null) fogRight.color = AudioDemoUi.Alpha(fogRight.color, a);
    }

    public override void Readout(List<string> lines)
    {
        lines.Add(audible
            ? "SOUND ON: eyes on the fog - which side is the coin?"
            : "SOUND OFF: same picture, and no idea what is out there");

        float left = 0f, right = 0f;
        if (listener != null) Ears(out left, out right);
        lines.Add("L " + Level(left, 10) + "  R " + Level(right, 10));
        lines.Add("pan  " + PanText(Pan(left, right)) + (audible ? "" : "   (silent: no direction)"));
        lines.Add(revealed
            ? "revealed: the coin is LEFT, the fire loop is RIGHT"
            : "nothing on screen says left or right - the ears do");
        lines.Add("the loop sits right; the coin swings the mix left");
    }
}
