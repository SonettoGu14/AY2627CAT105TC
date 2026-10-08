using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slide 2, the "Implement a 2D physics controller" review, one idea per demo:
///   Move   - move by writing rb.velocity
///   Jump   - jump with AddForce(..., ForceMode2D.Impulse)
///   Ground - ground detection with Physics2D.Raycast
///
/// Each demo carries its own minimal controller on purpose: the point is to show ONE idea,
/// so Move has no jump and Jump has no walk.
/// </summary>
public class ControllerDemo : DemoBase
{
    public enum Mode { Move, Jump, Ground }

    public Mode mode = Mode.Move;

    public Rigidbody2D body;
    public Transform groundCheckFrom;
    public LineRenderer groundRay;
    public LayerMask groundLayer = ~0;
    public float groundRayLength = 0.6f;
    public float speed = 5f;
    public float jumpForce = 9f;

    private bool grounded;
    private bool jumpQueued;

    public override string Title
    {
        get
        {
            switch (mode)
            {
                case Mode.Move: return "move by writing rb.velocity (never transform.position)";
                case Mode.Jump: return "jump with AddForce(..., ForceMode2D.Impulse)";
                default: return "ground detection with Physics2D.Raycast";
            }
        }
    }

    public override string Keys
    {
        get
        {
            switch (mode)
            {
                case Mode.Move: return "A / D  move";
                case Mode.Jump: return "Space  jump";
                default: return "A / D  walk off the ledge";
            }
        }
    }

    private void Update()
    {
        if (mode == Mode.Jump && Input.GetKeyDown(KeyCode.Space)) jumpQueued = true;
    }

    private void FixedUpdate()
    {
        if (body == null) return;

        grounded = CheckGrounded();

        if (mode == Mode.Move)
        {
            float h = Input.GetAxisRaw("Horizontal");
            body.velocity = new Vector2(h * speed, body.velocity.y);
        }
        else if (mode == Mode.Jump)
        {
            if (jumpQueued && grounded)
            {
                body.velocity = new Vector2(body.velocity.x, 0f);
                body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            jumpQueued = false;
        }
        else
        {
            float h = Input.GetAxisRaw("Horizontal");
            body.velocity = new Vector2(h * speed, body.velocity.y);
        }

        if (groundRay != null && groundCheckFrom != null)
        {
            Vector2 origin = groundCheckFrom.position;
            Vector2 dir = Vector2.down;
            float len = grounded ? Mathf.Abs(groundRayLength * 0.55f) : groundRayLength;
            groundRay.positionCount = 2;
            groundRay.SetPosition(0, new Vector3(origin.x, origin.y, -1f));
            groundRay.SetPosition(1, new Vector3(origin.x, origin.y - len, -1f));
            Color c = grounded ? new Color32(0x7C, 0xE3, 0x8B, 0xFF) : new Color32(0xFF, 0x6B, 0x6B, 0xFF);
            groundRay.startColor = c;
            groundRay.endColor = c;
        }
    }

    private bool CheckGrounded()
    {
        if (groundCheckFrom == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(groundCheckFrom.position, Vector2.down, groundRayLength, groundLayer);
        return hit.collider != null;
    }

    public override void Readout(List<string> lines)
    {
        switch (mode)
        {
            case Mode.Move:
                lines.Add("rb.velocity = new Vector2(h * speed, rb.velocity.y)");
                lines.Add("");
                lines.Add("h            = " + F(Input.GetAxisRaw("Horizontal"), 2) + "   (A = -1, D = +1)");
                lines.Add("speed        = " + F(speed, 1));
                lines.Add("rb.velocity  = " + V(body != null ? body.velocity : Vector2.zero));
                lines.Add("transform.x  = " + F(body != null ? body.position.x : 0f, 2));
                lines.Add("");
                lines.Add("we never touch transform.position - the solver");
                lines.Add("owns the body's motion once it has a Rigidbody2D.");
                break;

            case Mode.Jump:
                lines.Add("rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse)");
                lines.Add("");
                lines.Add("jumpForce    = " + F(jumpForce, 1));
                lines.Add("IsGrounded   = " + B(grounded));
                lines.Add("rb.velocity  = " + V(body != null ? body.velocity : Vector2.zero));
                lines.Add("");
                lines.Add("Impulse is a one-shot kick: force * mass,");
                lines.Add("applied on the next physics step.");
                break;

            default:
                lines.Add("Physics2D.Raycast(pos, Vector2.down, 0.6f, groundLayer)");
                lines.Add("");
                lines.Add("IsGrounded   = " + B(grounded));
                lines.Add("ray length   = " + F(groundRayLength, 2) + "   (A / D to walk off)");
                lines.Add("");
                lines.Add("green ray = the floor is under us,");
                lines.Add("red ray  = nothing there, so no jump.");
                break;
        }
    }
}
