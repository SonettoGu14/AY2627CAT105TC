using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// W5: lives, drawn as a row of Images - each one the character's own sprite.
///
/// Losing a life just switches the last visible icon off, so the whole component is one loop. At
/// zero it refills and puts the player back at the start, which keeps the demo short enough to
/// follow in a lab (no game-over screen, no restart button).
/// </summary>
public class HudLives : MonoBehaviour
{
    public Image[] icons;          // left to right
    public int lives;
    public PlayerController2D player;
    public Vector2 respawnPoint = new Vector2(-7f, -0.5f);

    void Start()
    {
        if (lives <= 0 || lives > icons.Length) lives = icons.Length;
        Refresh();
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
