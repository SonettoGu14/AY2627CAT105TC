using UnityEngine;

/// <summary>
/// W3 slide: "&lt;/&gt; Implement colour change after collision!" and
/// "&lt;/&gt; Implement colour change only when colliding with one (or more) specific objects!".
///
/// Attach to the Player. The player flashes orange on any collision, and gold when it
/// touches the object carrying <see cref="specialTag"/> (we use the "Finish" tag).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CollisionColorDemo2D : MonoBehaviour
{
    [Header("Colour demo")]
    public SpriteRenderer target;                                  // usually the player's SpriteRenderer
    public Color normalColor = Color.white;
    public Color anyHitColor = new Color(1.0f, 0.45f, 0.15f);
    public Color specialHitColor = new Color(1.0f, 0.85f, 0.20f);

    [Header("The special object")]
    public string specialTag = "Finish";

    private float restoreAt;

    private void Reset()
    {
        target = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (target == null) target = GetComponent<SpriteRenderer>();
        if (target != null) normalColor = target.color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (target == null)
        {
            return;
        }

        bool isSpecial = !string.IsNullOrEmpty(specialTag) && collision.gameObject.CompareTag(specialTag);
        target.color = isSpecial ? specialHitColor : anyHitColor;
        restoreAt = Time.time + 0.3f;

        if (LabHud.Instance != null)
        {
            LabHud.Instance.SetStateText(
                "OnCollisionEnter2D  <-  " + collision.gameObject.name +
                (isSpecial ? "\n(compareTag(\"" + specialTag + "\") matched)" : ""));
        }
    }

    private void Update()
    {
        if (target != null && restoreAt > 0f && Time.time >= restoreAt)
        {
            target.color = normalColor;
            restoreAt = 0f;
        }
    }
}
