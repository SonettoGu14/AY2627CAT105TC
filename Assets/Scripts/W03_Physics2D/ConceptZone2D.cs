using UnityEngine;

/// <summary>
/// One "review station" in the W3 playground. Each zone covers a topic from the W3 lecture.
/// Walking into it ticks the topic off the checklist on the HUD.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ConceptZone2D : MonoBehaviour
{
    public string conceptTitle = "Topic";
    public PlaygroundReview review;
    public SpriteRenderer markerRenderer;
    public Color pendingColor = new Color(0.95f, 0.80f, 0.30f, 0.16f);
    public Color doneColor = new Color(0.35f, 0.95f, 0.50f, 0.16f);

    public bool Done { get; private set; }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (markerRenderer != null) markerRenderer.color = pendingColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Done || !other.CompareTag("Player"))
        {
            return;
        }

        Done = true;
        if (markerRenderer != null) markerRenderer.color = doneColor;
        if (review != null) review.Complete(this);
    }
}
