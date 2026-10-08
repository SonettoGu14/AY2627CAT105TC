using UnityEngine;

/// <summary>
/// W3 slide "RigidBody2D" - the three behaviour modes:
///   Dynamic   : fully simulated (falls, is pushed, is stopped by colliders)
///   Kinematic : does not move by itself; it must be moved by code (a lift / moving platform)
///   Static    : never moves at all (the ground, the walls)
///
/// Three copies of this component in the scene, one per mode, make the difference obvious.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class RigidbodyModeDemo2D : MonoBehaviour
{
    public enum Mode { Dynamic, Kinematic, Static }

    public Mode mode = Mode.Dynamic;
    public Vector2 kinematicTravel = new Vector2(1.5f, 0f);
    public float speed = 1.2f;
    public string displayName = "Dynamic";

    private Rigidbody2D rb;
    private Vector2 start;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        start = rb.position;

        if (mode == Mode.Kinematic)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else if (mode == Mode.Static)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void FixedUpdate()
    {
        if (mode != Mode.Kinematic)
        {
            return;
        }

        // Kinematic bodies only move when code moves them.
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;
        rb.MovePosition(Vector2.Lerp(start, start + kinematicTravel, t));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
        {
            return;
        }
        if (LabHud.Instance != null)
        {
            LabHud.Instance.SetStateText("Rigidbody2D mode: " + displayName);
        }
    }
}
