using TMPro;
using UnityEngine;

/// <summary>
/// W5: the smallest useful piece of UI - a Text that counts pickups.
///
/// The coins are W4's Collectible2D. It raises <see cref="Collectible2D.Picked"/>, so this counter
/// never needs to know what a coin is, and a coin never needs to know a HUD exists. That is the same
/// idea as the readout panel in the slides: the thing being shown does not draw itself.
/// </summary>
public class HudCoinCounter : MonoBehaviour
{
    public TMP_Text label;
    public string prefix = "Coins  ";
    public int count;

    void OnEnable() { Collectible2D.Picked += OnPicked; }
    void OnDisable() { Collectible2D.Picked -= OnPicked; }
    void Start() { Refresh(); }

    private void OnPicked(int value)
    {
        count += value;
        Refresh();
    }

    private void Refresh() { if (label != null) label.text = prefix + count; }
}
