using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5 slides 32-33 — the two "put it together" demos, the same shape as W4's platformer on the last
/// teaching slide: a small scene plus the UI the slide asks for.
///
///   Dialogue  - walk the character to a trigger and the dialogue box fades in (Canvas Group)
///   HealthBar - the character takes damage at a hazard and the bar drops, eased, so you can see
///               the difference between the value and what is drawn
///
/// Both keep a world object (drawn by the camera) and a panel (drawn by a Canvas) so the two halves
/// of the finished thing are visible at once.
/// </summary>
public class WidgetDemo : DemoBase
{
    public enum Mode { Dialogue, HealthBar }

    public Mode mode = Mode.Dialogue;

    public SpriteRenderer hero;
    public SpriteRenderer zone;        // the trigger marker in the world
    public CanvasGroup group;          // the panel that fades
    public TMP_Text panelText;         // the dialogue line
    public Slider bar;                 // the health bar
    public Image barFill;
    public TMP_Text barLabel;

    public float zoneX = 3.4f;
    public float triggerRadius = 0.7f;

    private float health = 100f;
    private float shown = 1f;          // what the bar currently draws (eases toward health)
    private int hits;

    public override string Title
    {
        get
        {
            return mode == Mode.Dialogue
                ? "A dialogue box, triggered by a position"
                : "A health bar, triggered by taking damage";
        }
    }

    public override string Keys
    {
        get
        {
            return mode == Mode.Dialogue
                ? "A / D  walk the character into the marker"
                : "T  take damage        H  heal";
        }
    }

    public override void OnActivate()
    {
        health = 100f; shown = 1f; hits = 0;
        if (hero != null) hero.transform.localPosition = new Vector3(-4.6f, -1.2f, 0f);
        if (group != null) group.alpha = 0f;
        if (panelText != null) panelText.text = "Oh! You made it.\nThe UI learned to fade in.";
        if (bar != null) bar.value = 1f;
        if (barFill != null) barFill.color = UiPalette.Green;
    }

    void Update()
    {
        if (mode == Mode.Dialogue) UpdateDialogue();
        else UpdateHealth();
    }

    private void UpdateDialogue()
    {
        if (hero != null)
        {
            float dx = 0f;
            if (Input.GetKey(KeyCode.A)) dx = -0.05f;
            if (Input.GetKey(KeyCode.D)) dx = 0.05f;
            Vector3 p = hero.transform.localPosition;
            p.x = Mathf.Clamp(p.x + dx, -5.4f, 5.4f);
            hero.transform.localPosition = p;
        }
        bool inZone = hero != null && Mathf.Abs(hero.transform.localPosition.x - zoneX) <= triggerRadius;
        // the Canvas Group is the whole lesson: one number fades the finished panel in
        if (group != null) group.alpha = Mathf.Lerp(group.alpha, inZone ? 1f : 0f, 0.08f);
    }

    private void UpdateHealth()
    {
        if (Input.GetKeyDown(KeyCode.T)) { health = Mathf.Max(0f, health - 20f); hits++; }
        if (Input.GetKeyDown(KeyCode.H)) health = Mathf.Min(100f, health + 20f);

        // the bar eases toward the real value, which is why it is worth showing both numbers
        shown = Mathf.Lerp(shown, health / 100f, 0.08f);
        if (bar != null) bar.value = shown;
        if (barFill != null)
            barFill.color = shown > 0.6f ? UiPalette.Green : shown > 0.3f ? UiPalette.Gold : UiPalette.Pink;
        if (zone != null) zone.color = hits > 0 ? UiPalette.Pink : new Color(0.55f, 0.6f, 0.7f, 1f);
    }

    public override void Readout(List<string> lines)
    {
        if (mode == Mode.Dialogue)
        {
            float x = hero != null ? hero.transform.localPosition.x : 0f;
            bool inZone = Mathf.Abs(x - zoneX) <= triggerRadius;
            lines.Add("group.alpha = " + F(group != null ? group.alpha : 0f, 2));
            lines.Add("");
            lines.Add("hero x     = " + F(x, 2));
            lines.Add("trigger at = " + F(zoneX, 1) + "  (radius " + F(triggerRadius, 1) + ")");
            lines.Add("in trigger = " + B(inZone));
            lines.Add("");
            lines.Add("1. build the box (shape, text, animation)");
            lines.Add("2. add a trigger in the scene");
            lines.Add("3. the trigger fades the Canvas Group in");
        }
        else
        {
            lines.Add("health       = " + F(health, 0) + " / 100");
            lines.Add("bar drawn at = " + F(shown * 100f, 0) + "   (it eases, so it lags)");
            lines.Add("hits taken   = " + hits);
            lines.Add("");
            lines.Add("1. build the bar - a Slider's value is 0..1");
            lines.Add("2. put a hazard in the scene");
            lines.Add("3. the hazard calls TakeDamage(), which sets health");
            lines.Add("   and lets the bar catch up");
        }
    }
}
