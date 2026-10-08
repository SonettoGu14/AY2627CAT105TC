using UnityEngine;

/// <summary>
/// CAT105TC · 标准 2D 角色控制器 —— 第 3 周（物理）+ 第 4 周（动画）
/// CAT105TC · standard 2D character controller — Week 3 (physics) + Week 4 (animation)
///
/// 怎么用 / How to use:
///   1. 新建空物体，命名 Player
///      Create an empty GameObject and name it "Player".
///   2. 给它加 Rigidbody2D（不加也行，挂脚本时会自动补）和 CapsuleCollider2D
///      Add a Rigidbody2D (or skip it — attaching this script adds one) and a CapsuleCollider2D.
///   3. 把角色图片放在子物体 Visual 上，再给 Visual 加 Animator
///      Put the character sprite on a child named "Visual", then give Visual an Animator.
///   4. 把这个脚本挂到 Player 上，把下面几个引用槽拖满
///      Attach this script to "Player" and drag the references into the fields below.
///
/// 操作 / Controls:  A / D（或 ← →）左右移动，Space 跳跃
///                   A / D (or the arrow keys) to move, Space to jump
///
/// 记住这条分工 / The one rule to remember:
///   输入读在 Update，物理写在 FixedUpdate。这是 W3 幻灯片上讲过的，不是随便分的：
///   帧率和物理帧率不一样，在 Update 里写物理会抖、会穿墙。
///   Read input in Update, write physics in FixedUpdate. That is a W3 slide, not a style choice:
///   the frame rate and the physics rate are not the same, so doing physics in Update jitters
///   and tunnels through walls.
///
/// 它驱动哪些 Animator 参数 / which Animator parameters it drives:
///   speed      (float) —— 当前水平速度（原始值，单位/秒），不是 0~1
///                        the current horizontal speed (raw units/sec), NOT a 0..1 value
///   isJumping  (bool)  —— 现在是不是在空中 / whether we are in the air right now
///   你 Animator 里的参数名必须和上面**完全一致，大小写也一样** —— 对不上不会报错，只是没反应。
///   The Animator parameters must match the names above **exactly, including case** — a typo
///   does not throw, it just silently does nothing.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class W4PlayerController : MonoBehaviour
{
    // ============================================================================================
    //  1. Inspector 里能调的参数 / tunable fields        ← W4「Field Modifier」知识点
    // ============================================================================================

    [Header("移动 / Movement")]
    [Tooltip("左右移动速度（单位/秒）。Animator 里 speed > 3 才会切到 Run\n" +
             "Horizontal move speed (units/sec). The Animator switches to Run above speed 3")]
    [SerializeField]                                        // private，但 Inspector 里照样能看到
    private float moveSpeed = 5f;

    [Header("跳跃 / Jump")]
    [Tooltip("起跳冲量。跳多高 = 这个值 ÷ 重力，所以要和 Rigidbody2D 的 Gravity Scale 一起调\n" +
             "Jump impulse. Jump height = this ÷ gravity, so tune it together with the Gravity Scale")]
    [Range(1f, 25f)]                                        // [Range] 会显示成一根滑条
    public float jumpForce = 11f;

    [Tooltip("上升途中松开 Space，重力就乘上这个值：松手越早跳得越矮\n" +
             "While rising with Space released, gravity is multiplied by this: release early, jump shorter")]
    [Range(0f, 1f)] public float lowJumpMultiplier = 0.45f;

    [Header("地面检测 / Ground check")]
    [Tooltip("哪些层算地面。默认 Everything（所有层）；只想让 Ground 层算地面就在这里勾选\n" +
             "Which layers count as ground. Defaults to Everything; narrow it down if you want")]
    public LayerMask groundLayer = ~0;

    [Tooltip("往下探多远算踩到了地面 / how far down to look for ground")]
    [Range(0.05f, 0.5f)] public float groundCheckDistance = 0.15f;

    [Header("引用 / References")]
    [Tooltip("用来左右翻转的 SpriteRenderer / the SpriteRenderer used for the left-right flip")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("由代码驱动的 Animator（W4）/ the Animator driven from code (W4)")]
    public Animator animator;

    // ============================================================================================
    //  2. Animator 参数名 / Animator parameter names
    //     改成常量而不是到处写字符串 —— 名字只出现在一个地方，拼错只可能拼错一次。
    //     Kept as constants instead of loose strings, so the name lives in exactly one place.
    // ============================================================================================

    private const string SpeedParam = "speed";        // float：当前水平速度 / current horizontal speed
    private const string JumpingParam = "isJumping";  // bool ：是否在空中 / are we in the air?

    // ============================================================================================
    //  3. 运行时状态 / runtime state —— 学生不用管这几个
    // ============================================================================================

    private Rigidbody2D rb;
    private Collider2D body;

    private float horizontal;      // 本帧左右输入 / this frame's horizontal input
    private bool grounded;         // 脚下现在有没有地面 / is there ground under our feet
    private bool jumpQueued;       // Update 里记下，FixedUpdate 里执行 / pressed in Update, run in FixedUpdate

    /// <summary>只读，给别的脚本用（比如 HUD）/ read-only, for other scripts such as a HUD.</summary>
    public bool Grounded { get { return grounded; } }
    public float Horizontal { get { return horizontal; } }

    // ============================================================================================
    //  4. Awake：抓引用，并把常见的坑填掉 / grab the references and pre-empt the usual mistakes
    // ============================================================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // 被撞了不要原地打转 —— 2D 角色几乎总是锁死旋转
        // Do not spin on collision - a 2D character almost always wants rotation frozen.
        rb.freezeRotation = true;

        // 引用没拖？那就自动去子物体里找，省得学生一忘就 NullReferenceException
        // Field left empty? Look in the children, so forgetting to drag it is harmless.
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        body = GetComponent<Collider2D>();
        if (body == null)
        {
            Debug.LogError("[W4PlayerController] 没有 Collider2D，角色会掉下去。" +
                           "  This GameObject has no Collider2D, so the character will fall through the floor.", this);
        }
    }

    // ============================================================================================
    //  5. Update：只读输入，不碰物理 / input only, no physics here     ← W3：输入放在 Update
    // ============================================================================================

    private void Update()
    {
        // 用 GetAxis（平滑）而不是 GetAxisRaw（瞬间）：
        // 平滑输入会让速度从 0 慢慢升上去，正好经过 Animator 的 Walking 区间（0 < speed < 3）。
        // GetAxis smoothly ramps the value in, so the speed climbs through the Animator's Walking
        // band (0 < speed < 3) before reaching Run. GetAxisRaw would jump straight to full speed
        // and you would only ever see Idle and Run.
        horizontal = Input.GetAxis("Horizontal");           // A / D 或 ← / →，返回 -1 ~ +1

        // 这里不是在物理帧里，所以只把「按了跳跃」记下来，真正起跳留到 FixedUpdate。
        // We are not in a physics frame here, so only remember that jump was pressed;
        // the actual jump happens in FixedUpdate.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
        }
    }

    // ============================================================================================
    //  6. FixedUpdate：所有物理都写在这里 / every physics change goes here     ← W3
    // ============================================================================================

    private void FixedUpdate()
    {
        grounded = CheckGrounded();

        // --- 左右移动：直接写 rigidbody.velocity。别用 transform.position 硬挪，那会穿过墙体
        // --- Move: write rigidbody.velocity directly. Never translate transform.position,
        // --- that skips the physics solver and tunnels through walls.
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // --- 跳跃：只在「按下的那一帧」+「踩在地上」时才生效（所以空中连按没用）
        // --- Jump: only on the frame it was pressed AND only while grounded,
        // --- which is what stops the endless mid-air jumping.
        if (jumpQueued && grounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);               // 先清 Y，让每次跳都一样高
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);   // W3：Impulse = 瞬间蹬一脚
        }
        jumpQueued = false;

        // --- 变高跳：上升中一松开 Space 就提前变重，于是跳得矮
        // --- Variable height: letting go of Space while rising adds gravity, so the jump is shorter.
        if (rb.velocity.y > 0f && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        UpdateAnimation();
    }

    // ============================================================================================
    //  7. 地面检测：一条向下的短射线 / ground check: one short ray pointing down
    //                                                              ← W3：Physics2D.Raycast
    // ============================================================================================

    private bool CheckGrounded()
    {
        if (body == null) return false;

        // 从「碰撞体的底部」往下打，而不是从 transform.position。
        // 这样无论图片的 Pivot 在脚底还是正中间，判断都是对的。
        // Cast from the bottom of the collider, not from transform.position, so it works
        // whether the sprite's pivot is at the feet or in the middle of the image.
        Bounds b = body.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y - 0.02f);  // 起点放到脚底外侧，免得打到自己
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;                                // 碰到东西 = 站在地上
    }

    // ============================================================================================
    //  8. 动画：用代码驱动 Animator / drive the Animator from code     ← W4：Animator 脚本控制
    // ============================================================================================

    private void UpdateAnimation()
    {
        // 左右翻转：SpriteRenderer 的 flipX。         ← W4：SpriteRenderer
        // Left-right flip via SpriteRenderer.flipX.
        if (spriteRenderer != null && Mathf.Abs(horizontal) > 0.01f)
        {
            spriteRenderer.flipX = horizontal < 0f;
        }

        if (animator == null) return;

        // speed 传的是**原始速度**（单位/秒），不是 0~1 的百分比 ——
        // 因为 Animator 里的阈值是 3（Walking → Run 在 speed > 3 时切）。
        // speed is the RAW velocity in units/sec, not a 0..1 percentage, because the Animator's
        // threshold is 3 (Walking -> Run fires above speed 3).
        animator.SetFloat(SpeedParam, Mathf.Abs(rb.velocity.x));

        // isJumping：只要没踩到地面就算在空中 —— 起跳、跳起来、走下悬崖都会变 true。
        // isJumping: anything not on the ground counts as airborne, so it covers jumping up,
        // falling back down, and walking off a ledge.
        animator.SetBool(JumpingParam, !grounded);
    }
}
