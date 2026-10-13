using TMPro;
using UnityEngine;

/// <summary>
/// W5 slide "Create a dialogue box": walk to a position and the box appears.
///
/// The trigger does not animate anything itself - it just tells a Canvas Group to fade, and the
/// group takes the whole panel (background, text, buttons) with it. That is the slide's point:
/// one number moves a finished interface.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueZone2D : MonoBehaviour
{
    public CanvasGroup panel;
    public TMP_Text line;
    [TextArea] public string text = "Hello!";

    public float fade = 6f;

    private bool inside;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }
    void Start() { if (panel != null) panel.alpha = 0f; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        inside = true;
        if (line != null) line.text = text;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) inside = false;
    }

    void Update()
    {
        if (panel == null) return;
        panel.alpha = Mathf.Lerp(panel.alpha, inside ? 1f : 0f, Time.deltaTime * fade);

        // the other two Canvas Group switches, so the box stops eating clicks when it is invisible
        panel.interactable = inside;
        panel.blocksRaycasts = inside;
    }
}
