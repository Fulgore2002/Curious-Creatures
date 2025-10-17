using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Playercontroller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 15f;
    public float airAcceleration = 10f;

    [Header("Jump")]
    public float jumpForce = 14f;        // was 13f
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.65f; // was 0.5f

    [Header("Wall Jump")]
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public float wallSlideSpeed = 2f;
    public float wallJumpForceX = 10f;
    public float wallJumpForceY = 14f;
    private float wallJumpDuration = 0.25f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool hasJumped;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int lastWallDir = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 4f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpReleased = Input.GetButtonUp("Jump");

        // --- Ground Check ---
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            hasJumped = false;
            lastWallDir = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // --- Wall Check ---
        bool touchingLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, groundLayer);
        bool touchingRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, groundLayer);
        bool pressingTowardLeft = touchingLeft && moveInput < 0;
        bool pressingTowardRight = touchingRight && moveInput > 0;
        isWallSliding = (pressingTowardLeft || pressingTowardRight) && !isGrounded && rb.linearVelocity.y < 0;

        // --- Jump Buffer ---
        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // --- Movement ---
        if (!isWallJumping)
        {
            float targetSpeed = moveInput * moveSpeed;
            float accel = isGrounded ? acceleration : airAcceleration;
            float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accel * Time.deltaTime);

            rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        }

        // --- Wall Slide ---
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }

        // --- Jump ---
        if (jumpBufferCounter > 0f)
        {
            // Wall Jump
            if (isWallSliding)
            {
                int wallDir = touchingLeft ? -1 : (touchingRight ? 1 : 0);
                if (wallDir != lastWallDir)
                {
                    rb.linearVelocity = new Vector2(-wallDir * wallJumpForceX, wallJumpForceY);
                    isWallJumping = true;
                    hasJumped = true;
                    lastWallDir = wallDir;
                    jumpBufferCounter = 0f;
                    Invoke(nameof(ResetWallJump), wallJumpDuration);
                }
            }
            // Ground Jump
            else if ((isGrounded || coyoteTimeCounter > 0f) && !hasJumped)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
        }

        // --- Variable Jump Height ---
        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    void ResetWallJump() => isWallJumping = false;
}
