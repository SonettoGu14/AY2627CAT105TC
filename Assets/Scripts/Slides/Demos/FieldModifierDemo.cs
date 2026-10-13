using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 6 "Field Modifier", one modifier per demo:
///   Serialize       - [SerializeField] makes a PRIVATE field visible in the Inspector
///   HideInspector   - [HideInInspector] keeps a PUBLIC field out of the Inspector
///   Range           - [Range(0,10)] draws the field as a slider
///
/// The value drives a visible bar either way, so pressing the keys shows the field changing.
/// </summary>
public class FieldModifierDemo : DemoBase
{
    public enum Mode { Serialize, HideInspector, Range }

    public Mode mode = Mode.Serialize;
    public Transform bar;                 // scaled by the value
    public SpriteRenderer barSprite;
    public float max = 10f;

    // ---- the three fields, declared exactly as the slide describes them ----
    [SerializeField] private int coinCount = 3;          // private, but shown in the Inspector
    [HideInInspector] public int hiddenScore = 5;        // public, but hidden from the Inspector
    [Range(0f, 10f)] public float power = 4f;            // shown as a slider

    private const float BarWidth = 6f;
    private const float BarLeft = -3f;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Serialize: return "[SerializeField] shows a PRIVATE field in the Inspector";
                case Mode.HideInspector: return "[HideInInspector] keeps a PUBLIC field out of the Inspector";
                default: return "[Range(0,10)] draws the field as a slider";
            }
        }
    }

    public override string Keys { get { return "A / D  change the value"; } }

    private void Update()
    {
        int dir = 0;
        if (Input.GetKeyDown(KeyCode.A)) dir = -1;
        if (Input.GetKeyDown(KeyCode.D)) dir = 1;
        if (dir == 0) return;

        switch (mode)
        {
            case Mode.Serialize:
                coinCount = Mathf.Clamp(coinCount + dir, 0, (int)max);
                break;
            case Mode.HideInspector:
                hiddenScore = Mathf.Clamp(hiddenScore + dir, 0, (int)max);
                break;
            default:
                power = Mathf.Clamp(power + dir, 0f, max);
                break;
        }
    }

    private void LateUpdate()
    {
        if (bar == null) return;
        float v = Value01();
        bar.localScale = new Vector3(BarWidth * v, bar.localScale.y, 1f);
        bar.localPosition = new Vector3(BarLeft + BarWidth * v * 0.5f, bar.localPosition.y, 0f);
        if (barSprite != null)
        {
            barSprite.color = mode == Mode.Serialize ? new Color32(0x7C, 0xE3, 0x8B, 0xFF)
                            : mode == Mode.HideInspector ? new Color32(0xFF, 0xB4, 0x54, 0xFF)
                            : new Color32(0x6F, 0xA8, 0xFF, 0xFF);
        }
    }

    private float Value01()
    {
        switch (mode)
        {
            case Mode.Serialize: return coinCount / max;
            case Mode.HideInspector: return hiddenScore / max;
            default: return power / max;
        }
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Serialize:
                lines.Add("[SerializeField] private int coinCount");
                lines.Add("coinCount = " + coinCount);
                lines.Add("");
                lines.Add("private  -> code is the only writer,");
                lines.Add("but [SerializeField] puts it in the");
                lines.Add("Inspector so you can tune it by hand.");
                break;

            case Mode.HideInspector:
                lines.Add("[HideInInspector] public int hiddenScore");
                lines.Add("hiddenScore = " + hiddenScore);
                lines.Add("");
                lines.Add("public -> reachable from other scripts,");
                lines.Add("but the Inspector will not show it, so");
                lines.Add("nothing on the outside can change it.");
                break;

            default:
                lines.Add("[Range(0, 10)] public float power");
                lines.Add("power = " + F(power, 1));
                lines.Add("");
                lines.Add("the same float, drawn as a slider instead");
                lines.Add("of a text box - range + guard rail in one.");
                break;
        }
    }
}
