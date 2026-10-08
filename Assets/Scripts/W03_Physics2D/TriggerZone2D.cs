using UnityEngine;

/// <summary>
/// W3 review: Trigger2D events.
/// The zone is non-solid; when the Player walks in it lights up and counts how many
/// objects are inside. Nothing is pushed - that is the whole point of a Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TriggerZone2D : MonoBehaviour
{
    public SpriteRenderer visual;
    public Color idleColor = new Color(0.30f, 0.65f, 1.00f, 0.22f);
    public Color activeColor = new Color(0.30f, 1.00f, 0.55f, 0.45f);
    public bool reportToHud = true;

    private int inside;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (visual != null) visual.color = idleColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        inside += 1;
        Apply();
        if (reportToHud && LabHud.Instance != null)
        {
            LabHud.Instance.SetStateText("OnTriggerEnter2D  ->  " + other.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        inside = Mathf.Max(0, inside - 1);
        Apply();
        if (reportToHud && LabHud.Instance != null)
        {
            LabHud.Instance.SetStateText("OnTriggerExit2D  <-  " + other.name);
        }
    }

    private void Apply()
    {
        if (visual != null)
        {
            visual.color = inside > 0 ? activeColor : idleColor;
        }
    }
}
