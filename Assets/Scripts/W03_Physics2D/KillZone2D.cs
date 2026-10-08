using UnityEngine;

/// <summary>
/// An invisible trigger below the level that respawns the player.
/// W3 review: Trigger2D event. W4 slide: the "kill plane" / out-of-bounds zone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KillZone2D : MonoBehaviour
{
    public PlayerController2D player;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (LabHud.Instance != null)
        {
            LabHud.Instance.LoseLife();
        }

        if (player != null)
        {
            player.Respawn();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}
