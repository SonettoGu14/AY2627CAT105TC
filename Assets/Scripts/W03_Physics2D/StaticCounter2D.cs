using UnityEngine;

/// <summary>
/// W3 review: a static field belongs to the CLASS, not to an instance.
/// Every coin in the scene bumps the same counter, and the HUD shows it.
///
/// W3 slide "When to use Statics": global counters (how many collectibles), global state.
/// </summary>
public class StaticCounter2D : MonoBehaviour
{
    /// <summary>Lives on the class itself - shared by every instance.</summary>
    public static int TotalCollected;

    /// <summary>Reset the static when Play starts, so it doesn't leak between runs in the Editor.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        TotalCollected = 0;
    }

    [SerializeField] private int perPickup = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        TotalCollected += perPickup;   // class-name.member - no instance needed
        if (LabHud.Instance != null)
        {
            LabHud.Instance.SetStateText("static StaticCounter2D.TotalCollected = " + TotalCollected);
        }
        Destroy(gameObject);
    }
}
