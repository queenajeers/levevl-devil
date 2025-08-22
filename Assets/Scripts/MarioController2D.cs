using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MarioController2D : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Max horizontal speed while running.")]
    [SerializeField] private float maxRunSpeed = 8f;

    [Tooltip("How quickly you reach top speed on ground.")]
    [SerializeField] private float groundAcceleration = 60f;

    [Tooltip("How quickly you stop on ground when no input.")]
    [SerializeField] private float groundDeceleration = 70f;

    [Tooltip("Horizontal accel while airborne.")]
    [SerializeField] private float airAcceleration = 40f;

    [Tooltip("Horizontal decel while airborne (let-go drift).")]
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jump")]
    [Tooltip("Initial upward velocity added on jump.")]
    [SerializeField] private float jumpVelocity = 14f;

    [Tooltip("Extra gravity when you release jump early (for snappy jumps).")]
    [SerializeField] private float jumpCutMultiplier = 2.2f;

    [Tooltip("Stronger gravity on the way down (tighter arc).")]
    [SerializeField] private float fallGravityMultiplier = 1.6f;

    [Tooltip("Max downward speed (prevents infinite acceleration).")]
    [SerializeField] private float maxFallSpeed = -22f;

    [Tooltip("Allow jump shortly after leaving edge (seconds).")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Tooltip("Queue a jump pressed slightly before landing (seconds).")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    [Tooltip("Feet transform position to check ground.")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Radius for ground overlap check.")]
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Tooltip("Which layers count as ground.")]
    [SerializeField] private LayerMask groundLayers;

    [Header("Tuning")]
    [Tooltip("If true, face the direction of travel by flipping localScale.x")]
    [SerializeField] private bool flipSprite = true;

    private Rigidbody2D rb;

    // Input/state
    private float moveInput;                // -1..1 from horizontal input
    private bool jumpPressed;
    private bool jumpHeld;

    // Timers for jump features
    private float coyoteTimer;
    private float jumpBufferTimer;

    // Cache & constants
    private const float EPS = 0.0001f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (groundCheck == null)
        {
            Debug.LogWarning($"{nameof(MarioController2D)}: Assign a GroundCheck Transform (child at feet).");
        }

        // Recommended Rigidbody2D settings for tight platforming
        rb.gravityScale = 1f;          // base gravity, we’ll scale it manually
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        // --- INPUT (get in Update) ---
        moveInput = Input.GetAxisRaw("Horizontal"); // -1, 0, 1 from default axes
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld    = Input.GetButton("Jump");

        // Jump Buffer: remember a jump pressed for a short window
        if (jumpPressed) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        // Coyote Time: remember grounded for a short window after leaving ground
        if (IsGrounded()) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        // Handle jump start in Update (but apply physics in FixedUpdate)
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            DoJump();
        }

        // Optional: flip sprite to face movement
        if (flipSprite && Mathf.Abs(rb.linearVelocity.x) > 0.05f)
        {
            var s = transform.localScale;
            s.x = Mathf.Sign(rb.linearVelocity.x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    private void FixedUpdate()
    {
        // --- HORIZONTAL MOVEMENT ---
        float targetSpeed = moveInput * maxRunSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        // Pick accel rate (ground vs air, and accel vs decel)
        bool onGround = IsGrounded();
        float accelRate =
            Mathf.Abs(targetSpeed) > EPS
                ? (onGround ? groundAcceleration : airAcceleration)
                : (onGround ? groundDeceleration : airDeceleration);

        float movement = accelRate * speedDiff; // units/s^2 * (m/s) => m/s^2

        // Apply horizontal force to reach/maintain target speed
        rb.AddForce(new Vector2(movement, 0f), ForceMode2D.Force);

        // Clamp max horizontal speed
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxRunSpeed, maxRunSpeed);

        // --- VERTICAL / GRAVITY TUNING ---
        float gravity = Physics2D.gravity.y * rb.gravityScale;

        // Falling: stronger gravity
        if (rb.linearVelocity.y < 0f)
        {
            gravity *= fallGravityMultiplier;
        }
        // Jump cut: if the player releases the button early, increase gravity
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            gravity *= jumpCutMultiplier;
        }

        // Apply our custom gravity each physics step
        rb.AddForce(new Vector2(0f, gravity - Physics2D.gravity.y * rb.gravityScale), ForceMode2D.Force);

        // Clamp max fall speed
        float clampedY = Mathf.Max(rb.linearVelocity.y, maxFallSpeed);

        rb.linearVelocity = new Vector2(clampedX, clampedY);
    }

    private void DoJump()
    {
        // Consume buffers
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;

        // Set upward velocity directly for consistent height
        Vector2 v = rb.linearVelocity;
        v.y = jumpVelocity;
        rb.linearVelocity = v;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers) != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // --- Public setters if you want to tweak at runtime from other scripts/UI ---
    public void SetMaxRunSpeed(float v) => maxRunSpeed = Mathf.Max(0f, v);
    public void SetJumpVelocity(float v) => jumpVelocity = Mathf.Max(0f, v);
    public void SetAcceleration(float ground, float air) { groundAcceleration = ground; airAcceleration = air; }
    public void SetDeceleration(float ground, float air) { groundDeceleration = ground; airDeceleration = air; }
}
