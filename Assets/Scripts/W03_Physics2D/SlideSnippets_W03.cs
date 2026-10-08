using UnityEngine;

/// <summary>
/// The exact code from the W3 lecture ("Physics in Unity"), gathered in one place so it
/// can be opened live during the class. Nothing here is attached to a GameObject - it is
/// a cheat sheet to read (and to copy from).
///
/// Method names say which slide they come from.
/// </summary>
public static class SlideSnippets_W03
{
    /// <summary>Slide "OnCollisionEnter2D": change colour after a collision.</summary>
    public static void ColorOnHit(SpriteRenderer sprite, Collision2D collision)
    {
        // collision.collider    -> the other object's collider
        // collision.contacts    -> where the two objects touched
        // collision.gameObject  -> the other object
        sprite.color = Color.red;
    }

    /// <summary>Slide "OnCollisionEnter2D": change colour ONLY for one specific object.</summary>
    public static void ColorOnSpecificObject(SpriteRenderer sprite, Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Finish"))   // CompareTag is cheaper than == "Finish"
        {
            sprite.color = Color.yellow;
        }
    }

    /// <summary>Slide "Trigger2D Event": the parameter is a Collider2D, not a Collision2D.</summary>
    public static void TriggerEntered(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger entered by " + other.name);
        }
    }

    /// <summary>Slide "Physics2D.Raycast function".</summary>
    public static bool RaycastForward(Transform from)
    {
        // RaycastHit2D Physics2D.Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask)
        RaycastHit2D hit = Physics2D.Raycast(from.position, Vector2.right, 2.0f);
        return hit.collider != null;              // it stops at the first object it touches
    }

    /// <summary>Slide "LayerMask": an int whose 32 bits say which layers are included.</summary>
    public static int BuildLayerMask()
    {
        return 1 << 9;                            // "1 << n" is the layermask of the nth layer
    }

    /// <summary>Slide "Implement a 2D physics controller": move with rigidbody.velocity.</summary>
    public static void MoveWithVelocity(Rigidbody2D rb, float horizontal)
    {
        rb.velocity = new Vector2(horizontal * 5f, rb.velocity.y);
    }

    /// <summary>Slide "Implement a 2D physics controller": jump with AddForce.</summary>
    public static void JumpWithAddForce(Rigidbody2D rb)
    {
        rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
    }
}
