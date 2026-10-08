using UnityEngine;

/// <summary>
/// The centrepiece script for the W3 + W4 labs.
///
/// W3 - "Implement a 2D physics controller":
///     * move by writing  Rigidbody2D.velocity          (never transform.position)
///     * jump with        Rigidbody2D.AddForce(..., ForceMode2D.Impulse)
///     * ground check with Physics2D.Raycast
///
/// W4 - "Field Modifier" + "Animator: Scripting":
///     * [SerializeField] / [HideInInspector] / [Range] field modifiers
///     * drive the Animator from code with SetFloat / SetBool / SetTrigger
///
/// Attach to the Player GameObject (needs a Rigidbody2D + Collider2D + Animator).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    // ---------------------------------------------------------------------------------
    // W4 slide "Field Modifier": the three modifiers we use in class.
    // ---------------------------------------------------------------------------------

    [Header("Movement  (W3: write rb.velocity)")]
    [SerializeField]                        // a private field, but shown in the Inspector
    private float moveSpeed = 7.0f;

    [Header("Jump  (W3: AddForce + ForceMode2D.Impulse)")]
    [Range(1f, 25f)]                        // shows a slider instead of a text box
    public float jumpForce = 11.0f;

    [Tooltip("While the jump key is released on the way up, gravity is multiplied by this.")]
    [Range(0f, 1f)] public float lowJumpMultiplier = 0.45f;

    [HideInInspector]                       // a public field, but kept OUT of the Inspector
    public Vector2 currentVelocity;

    [Header("Ground check  (W3: Physics2D.Raycast)")]
    public LayerMask groundLayer;           // Ground + OneWay in the scenes
    [Range(0.05f, 0.5f)] public float groundCheckDistance = 0.18f;

    [Header("Animation  (W4: Animator scripting)")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    // Cached parameter hashes: cheaper than passing strings every frame.
    private static readonly int SpeedHash       = Animator.StringToHash("Speed");
    private static readonly int VerticalHash    = Animator.StringToHash("VerticalSpeed");
    private static readonly int GroundedHash    = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash        = Animator.StringToHash("Jump");

    private Rigidbody2D rb;
    private Vector3 spawnPoint;
    private float horizontal;
    private bool grounded;
    private bool jumpQueued;

    public float Horizontal { get { return horizontal; } }
    public bool Grounded { get { return grounded; } }
    public float MoveSpeed { get { return moveSpeed; } }
    /// <summary>0..1, handy for the HUD speed bar and the Animator "Speed" blob.</summary>
    public float Speed01 { get { return Mathf.Clamp01(Mathf.Abs(rb != null ? rb.velocity.x : 0f) / Mathf.Max(0.0001f, moveSpeed)); } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        spawnPoint = transform.position;
    }

    private void Update()
    {
        // W3 slide: "Be sure to handle input in Update instead of FixedUpdate!"
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        grounded = CheckGrounded();

        // Move horizontally: keep the vertical velocity the solver gave us.
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (jumpQueued && grounded)
        {
            // Zero Y first so every jump reaches the same height.
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            if (animator != null) animator.SetTrigger(JumpHash);
        }
        jumpQueued = false;

        // Variable jump height ("release early, jump shorter").
        if (rb.velocity.y > 0.01f && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        UpdateAnimation();
    }

    private void LateUpdate()
    {
        currentVelocity = rb.velocity;
    }

    /// <summary>W3 / W4 slide: a short downward ray is the standard "can I jump?" pattern.</summary>
    public bool CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)transform.position + Vector2.up * 0.05f,
            Vector2.down,
            groundCheckDistance,
            groundLayer);
        return hit.collider != null;
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer != null && Mathf.Abs(horizontal) > 0.01f)
        {
            spriteRenderer.flipX = horizontal < 0f;   // W4: SpriteRenderer "Flip"
        }

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, Speed01);
            animator.SetFloat(VerticalHash, rb.velocity.y);
            animator.SetBool(GroundedHash, grounded);
        }
    }

    /// <summary>Teleport back to the last spawn point (used by the kill zone).</summary>
    public void Respawn()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = spawnPoint;
        transform.position = spawnPoint;
    }

    public void SetSpawn(Vector3 point)
    {
        spawnPoint = point;
    }
}
