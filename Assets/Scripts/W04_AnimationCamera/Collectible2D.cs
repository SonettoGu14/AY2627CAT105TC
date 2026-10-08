using UnityEngine;

/// <summary>
/// A collectible coin. W3 review: Trigger2D events + Tag.
/// W4: Instantiate & Destroy.
///
/// Setup: BoxCollider2D / CircleCollider2D with "Is Trigger" ticked, and the Player has a Rigidbody2D.
/// The coin itself does NOT need a Rigidbody (a static trigger still fires when a Rigidbody enters).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Collectible2D : MonoBehaviour
{
    [SerializeField] private int value = 1;

    [Header("W4: Instantiate a burst on pickup")]
    public Sprite burstSprite;
    public Color burstColor = new Color(1f, 0.85f, 0.2f, 1f);

    private void Reset()
    {
        // Sensible defaults if the component is added in the Editor.
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // W3 slide: CompareTag is faster than string comparison and allocates no garbage.
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (LabHud.Instance != null)
        {
            LabHud.Instance.AddCoin(value);
        }

        SpawnBurst();
        Destroy(gameObject);
    }

    private void SpawnBurst()
    {
        if (burstSprite == null)
        {
            return;
        }

        GameObject go = new GameObject("CoinBurst");
        go.transform.position = transform.position;
        go.transform.localScale = Vector3.one * 0.2f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = burstSprite;
        sr.color = burstColor;
        sr.sortingOrder = 5;

        go.AddComponent<Burst2D>();
    }
}
