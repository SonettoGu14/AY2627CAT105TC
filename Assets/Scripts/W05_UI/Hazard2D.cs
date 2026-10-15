using UnityEngine;

/// <summary>
/// W5: a red zone that costs a life. It is the "trigger" half of the pattern the slides describe -
/// an object in the scene calls into the UI, and the UI never looks for the object.
///
/// Uses OnTriggerEnter2D (not Stay) plus a short cooldown, so touching it once costs one life.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard2D : MonoBehaviour
{
    public HudLives lives;
    public float cooldown = 1f;

    private float next;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (lives == null || !other.CompareTag("Player")) return;
        if (Time.time < next) return;
        next = Time.time + cooldown;
        lives.Lose(1);
    }
}
