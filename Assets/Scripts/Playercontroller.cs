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

    private Rigidbody2D rb;
    private float coyoteTimeCounter;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    public float wallJumpForceX = 8f;
    public float wallJumpForceY = 12f;
    private bool isWallJumping;
    private float wallJumpDuration = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        // Disable horizontal input briefly after wall jump
        if (!isWallJumping)
            rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Coyote time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Wall check
        bool touchingLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, groundLayer);
        bool touchingRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, groundLayer);
        isTouchingWall = touchingLeft || touchingRight;

        // Wall sliding
        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0;

        if (isWallSliding)
            rb.linearVelocity = new Vector2(0, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            if (isWallSliding)
            {
                // Wall jump direction
                float direction = touchingLeft ? 1 : -1;
                rb.linearVelocity = new Vector2(direction * wallJumpForceX, wallJumpForceY);
                isWallJumping = true;
                Invoke(nameof(ResetWallJump), wallJumpDuration);
            }
            else if (coyoteTimeCounter > 0f)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                coyoteTimeCounter = 0f;
            }
        }
    }

    void ResetWallJump()
    {
        isWallJumping = false;
    }
}