using UnityEngine;

public class Playercontroller : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public float coyoteTime = 0.2f;
    public float wallSlideSpeed = 1.5f;
    public float wallJumpForceX = 8f;
    public float wallJumpForceY = 12f;
    private float wallJumpDuration = 0.2f;

    private Rigidbody2D rb;
    private float coyoteTimeCounter;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool hasJumped;

    private bool jumpBuffered;
    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    private int lastWallDir = 0; // -1 = left wall, 1 = right wall, 0 = none

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");

        // --- Ground Check ---
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // --- Coyote Time ---
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            if (!wasGrounded)
            {
                hasJumped = false;
                lastWallDir = 0; // reset when grounded
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // --- Wall Check ---
        bool touchingLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, groundLayer);
        bool touchingRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, groundLayer);
        isTouchingWall = touchingLeft || touchingRight;

        // --- Wall Sliding ---
        bool pressingTowardLeft = touchingLeft && move < 0;
        bool pressingTowardRight = touchingRight && move > 0;
        isWallSliding = (pressingTowardLeft || pressingTowardRight) && !isGrounded && rb.linearVelocity.y < 0;

        // --- Buffered Jump Input ---
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // --- Movement ---
        if (!isWallJumping)
        {
            if ((touchingLeft && move < 0) || (touchingRight && move > 0))
                move *= 0.2f;

            rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
        }

        // --- Wall Slide ---
        if (isWallSliding)
        {
            float pushDirection = touchingLeft ? 1f : -1f;
            float slideY = Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue);
            rb.linearVelocity = new Vector2(pushDirection * 0.5f, slideY);
        }

        // --- Jump ---
        if (jumpBufferCounter > 0f)
        {
            // Wall Jump (requires switching walls)
            if (isWallSliding)
            {
                int currentWallDir = touchingLeft ? -1 : (touchingRight ? 1 : 0);

                // Only allow jump if it's a different wall than last time
                if (currentWallDir != 0 && currentWallDir != lastWallDir)
                {
                    float direction = currentWallDir == -1 ? 1 : -1;
                    rb.linearVelocity = new Vector2(direction * wallJumpForceX, wallJumpForceY);
                    isWallJumping = true;
                    hasJumped = true;
                    lastWallDir = currentWallDir;
                    jumpBufferCounter = 0f;
                    Invoke(nameof(ResetWallJump), wallJumpDuration);
                }
            }
            // Normal jump
            else if ((isGrounded || coyoteTimeCounter > 0f) && !hasJumped)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
        }
    }

    void ResetWallJump()
    {
        isWallJumping = false;
    }
}
