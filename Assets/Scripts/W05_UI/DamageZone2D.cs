using UnityEngine;

/// <summary>
/// W5 slide "Health Bar", step 2: *where the damage comes from*. The slide says "add triggers for
/// objects that can harm the player and trigger the injured function" - this is that trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageZone2D : MonoBehaviour
{
    public Health health;
    public int damage = 20;
    public float cooldown = 1.2f;

    private float next;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerStay2D(Collider2D other)
    {
        if (health == null || !other.CompareTag("Player")) return;
        if (Time.time < next) return;
        next = Time.time + cooldown;
        health.Take(damage);
    }
}
