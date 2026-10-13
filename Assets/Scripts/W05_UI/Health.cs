using System;
using UnityEngine;

/// <summary>
/// W5: the player's hit points. Hazards and pickups write it; the health bar reads it.
///
/// Keep the value and the UI apart - the bar is a *view* of this, not the other way round. That is
/// what lets slide 29's "fade the whole panel" and slide 33's "let the bar catch up" both work.
/// </summary>
public class Health : MonoBehaviour
{
    public int max = 100;
    public int current = 100;

    /// <summary>Raised whenever the value changes; the UI subscribes to it.</summary>
    public event Action Changed;

    public float Normalized { get { return max <= 0 ? 0f : Mathf.Clamp01((float)current / max); } }
    public bool IsDead { get { return current <= 0; } }

    void Awake() { current = max; }

    public void Take(int amount)
    {
        if (amount <= 0) return;
        current = Mathf.Max(0, current - amount);
        Raise();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        current = Mathf.Min(max, current + amount);
        Raise();
    }

    public void Refill() { current = max; Raise(); }

    private void Raise() { if (Changed != null) Changed(); }
}
