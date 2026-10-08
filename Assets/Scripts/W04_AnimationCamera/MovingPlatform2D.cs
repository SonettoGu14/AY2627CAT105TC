using UnityEngine;

/// <summary>
/// A kinematic platform that slides back and forth.
///
/// W3 review: Rigidbody2D behaviour modes - "Kinematic: does not move actively;
/// it needs to be moved by code (e.g. moving platform, elevator)".
/// W3 review: OnCollisionEnter2D / Exit2D.
///
/// IMPORTANT: the platform root has scale (1,1,1) - the size comes from the
/// BoxCollider2D size and a scaled child sprite. That is what lets us parent the
/// player to it without stretching the player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform2D : MonoBehaviour
{
    public Vector2 travel = new Vector2(4f, 0f);
    public float speed = 1.2f;

    private Rigidbody2D rb;
    private Vector2 start;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        start = rb.position;
    }

    private void FixedUpdate()
    {
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;   // 0..1..0
        rb.MovePosition(Vector2.Lerp(start, start + travel, t));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Standing on the platform -> ride along with it.
        if (collision.collider.CompareTag("Player") && collision.collider.transform.parent == null)
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null);
        }
    }
}
