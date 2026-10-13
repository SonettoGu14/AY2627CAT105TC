using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5 slide "Health Bar": a Slider is the bar and a Text is the number.
///
/// The bar EASES toward the real value instead of snapping. That is deliberate: you can see the two
/// disagree for a moment, which is the difference between "the number" and "what is drawn", and it
/// is the same Lerp the lecture uses for a smooth fill.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    public Health health;
    public Slider bar;
    public Image fill;
    public TMP_Text label;
    public float smoothing = 6f;

    private float target = 1f;

    void OnEnable()
    {
        if (health != null) health.Changed += Pull;
        Pull();
        if (bar != null) bar.value = target;      // no ease on the first frame
    }

    void OnDisable() { if (health != null) health.Changed -= Pull; }

    private void Pull() { if (health != null) target = health.Normalized; }

    void Update()
    {
        if (health == null) return;
        if (bar != null) bar.value = Mathf.Lerp(bar.value, target, Time.deltaTime * smoothing);
        if (fill != null)
            fill.color = bar != null && bar.value > 0.6f ? UiPalette.Green
                       : bar != null && bar.value > 0.3f ? UiPalette.Gold
                       : UiPalette.Pink;
        if (label != null) label.text = health.current + " / " + health.max;
    }
}
