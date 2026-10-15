using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5: lives, drawn as a row of Images - each one the character's own sprite.
///
/// It watches the player's Health instead of a hazard calling it directly: the bar drains while the
/// player is being hurt, and when it empties a life is spent and the bar is handed back full. At zero
/// lives everything refills and the player returns to the start, so the demo needs no game-over
/// screen. That keeps the whole loop inside two components (Health + this) and one bar.
/// </summary>
public class HudLives : MonoBehaviour
{
    public Image[] icons;          // left to right
    public int lives;
    public Health health;
    public PlayerController2D player;
    public Vector2 respawnPoint = new Vector2(-7f, -0.5f);

    void OnEnable() { if (health != null) health.Changed += OnHealthChanged; }
    void OnDisable() { if (health != null) health.Changed -= OnHealthChanged; }

    void Start()
    {
        if (lives <= 0 || lives > icons.Length) lives = icons.Length;
        Refresh();
    }

    /// <summary>Health hit zero: spend a life, then hand the bar back full.</summary>
    private void OnHealthChanged()
    {
        if (health == null || !health.IsDead) return;
        Lose(1);
        health.Refill();
    }

    public void Lose(int amount = 1)
    {
        lives = Mathf.Max(0, lives - amount);
        if (lives == 0)
        {
            lives = icons.Length;      // out of lives: refill and go back to the start
            Respawn();
        }
        Refresh();
    }

    private void Respawn()
    {
        if (player == null) return;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.velocity = Vector2.zero; rb.position = respawnPoint; }
        player.transform.position = respawnPoint;
    }

    /// <summary>Only the first `lives` icons stay enabled - the simplest way to draw a counter.</summary>
    private void Refresh()
    {
        for (int i = 0; i < icons.Length; i++)
            if (icons[i] != null) icons[i].enabled = i < lives;
    }
}
