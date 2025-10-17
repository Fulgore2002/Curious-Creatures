using UnityEngine;

public class Playercontroller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 20f;
    public float airAcceleration = 10f;
    public float deceleration = 25f;
    public float airDeceleration = 15f;

    [Header("Jump")]
    public float jumpForce = 13f;
    public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;

    [Header("Wall Interaction")]
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public float wallSlideStartSpeed = 0.5f;
    public float wallSlideMaxSpeed = 3f;
    public float wallSlideAcceleration = 4f;
    public float wallJumpForceX = 10f;
    public float wallJumpForceY = 14f;
    private float wallJumpDuration = 0.25f;

    private Rigidbody2D rb;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float currentSlideSpeed;

    private bool isGrounded;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool hasJumped;
    private int lastWallDir = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 4f;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
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

        // --- Jump Buffer ---
        if (jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // --- Wall Check ---
        bool touchingLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, groundLayer);
        bool touchingRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, groundLayer);

        bool pressingTowardLeft = touchingLeft && moveInput < 0;
        bool pressingTowardRight = touchingRight && moveInput > 0;

        bool isTouchingWall = touchingLeft || touchingRight;
        isWallSliding = (pressingTowardLeft || pressingTowardRight) && !isGrounded && rb.linearVelocity.y < 0;

        // --- Movement (with smooth acceleration) ---
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded
            ? (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration)
            : (Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration);

        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.deltaTime);

        if (!isWallJumping)
            rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);

        // --- Wall Sliding (soft acceleration downward) ---
        if (isWallSliding)
        {
            float pushDirection = touchingLeft ? 1f : -1f;
            currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, wallSlideMaxSpeed, wallSlideAcceleration * Time.deltaTime);
            rb.linearVelocity = new Vector2(pushDirection * 0.5f, -currentSlideSpeed);
        }
        else
        {
            currentSlideSpeed = wallSlideStartSpeed;
        }

        // --- Jump ---
        if (jumpBufferCounter > 0f)
        {
            // Wall Jump (requires switching walls)
            if (isWallSliding)
            {
                int wallDir = touchingLeft ? -1 : (touchingRight ? 1 : 0);
                if (wallDir != 0 && wallDir != lastWallDir)
                {
                    rb.linearVelocity = new Vector2(-wallDir * wallJumpForceX, wallJumpForceY);
                    isWallJumping = true;
                    hasJumped = true;
                    lastWallDir = wallDir;
                    jumpBufferCounter = 0f;
                    Invoke(nameof(ResetWallJump), wallJumpDuration);
                }
            }
            // Normal Jump
            else if ((isGrounded || coyoteTimeCounter > 0f) && !hasJumped)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
        }

        // --- Variable Jump Height (Ori-like control) ---
        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    void ResetWallJump()
    {
        isWallJumping = false;
    }
}
