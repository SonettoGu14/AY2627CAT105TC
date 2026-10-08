using UnityEngine;

/// <summary>
/// The end-of-level trigger. W3 review: Trigger2D event + Tag.
/// W4: finishing the level flips a flag in the HUD.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GoalZone2D : MonoBehaviour
{
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
            LabHud.Instance.SetGoalReached(true);
        }
    }
}
