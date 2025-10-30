using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Playercontroller : MonoBehaviour
{
    [Header("General Stats")]
    public int health = 10;
    public int maxHealth = 10;
    public int mana = 10;
    public int maxMana = 10;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 15f;
    public float airAcceleration = 10f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.65f;

    [Header("Wall Jump")]
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float checkRadius = 0.2f;
    public LayerMask[] collisionLayers;
    public float wallSlideSpeed = 2f;
    public float wallJumpForceX = 10f;
    public float wallJumpForceY = 14f;
    private float wallJumpDuration = 0.25f;

    [Header("Tilemaps")]
    public Tilemap[] interactionTilemaps;
    public float damageCooldown = 3f;
    private float lastDamageTime = 0f;
    public TileBase[] thornsTiles;
    private Dictionary<TileBase, Action> tileActions;

    // --- Private ---
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded = false;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool hasJumped;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int lastWallDir = 0;

    private float moveInput;
    private float downInput;
    private bool jumpPressed;
    private bool jumpReleased;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 4f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        tileActions = new Dictionary<TileBase, Action>();
        foreach (TileBase thorn in thornsTiles)
        {
            tileActions[thorn] = () => Health(1, false);
        }
    }

    void Update()
    {
        MovementController();
        UpdateAnimationStates();

        if (Input.GetKeyDown(KeyCode.H))
        {
            Health(1, true);
        }

        DetectTile();
    }

    void ResetWallJump() => isWallJumping = false;

    /// <summary>
    /// Checks for ground below player
    /// </summary>
    /// <returns>Returns True if there's ground</returns>
    bool DetectGround()
    {        
        bool grounded = false;
        foreach (LayerMask layer in collisionLayers)
        {
            if (!Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Player"), (int)Mathf.Log(layer.value, 2)))
            {
                grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, layer);
                if (grounded)
                {
                    return grounded;
                }
            }
        }
        return grounded;
    }

    private void DetectTile()
    {
        Vector3Int cellPosition = interactionTilemaps[0].WorldToCell(transform.position);
        TileBase tileUnderPlayer = interactionTilemaps[0].GetTile(cellPosition);

        if (tileUnderPlayer == null) return;

        if (tileActions.ContainsKey(tileUnderPlayer))
        {
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                tileActions[tileUnderPlayer]?.Invoke();
                lastDamageTime = Time.time;
            }
        }
    }

    void Health(int amount, bool Heal)
    {
        if (Heal) health += amount;
        else health -= amount;

        health = Mathf.Clamp(health, 0, maxHealth);
        UIManager.Instance.SetHealth(health);
    }

    void MovementController()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        downInput = Input.GetAxisRaw("Vertical");
        jumpPressed = Input.GetButtonDown("Jump");
        jumpReleased = Input.GetButtonUp("Jump");

        // --- Ground Check ---
        isGrounded = DetectGround();

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
        bool touchingLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, collisionLayers[0]);
        bool touchingRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, collisionLayers[0]);
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
            else if (Physics2D.OverlapCircle(groundCheck.position, checkRadius, collisionLayers[1]) && downInput < 0 && !hasJumped)
            {
                DropThruPlatform();
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

        // --- Sprite Flip ---
        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;
    }

    // 🎞️ Animation handling
    void UpdateAnimationStates()
    {
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat("Speed", horizontalSpeed);
        anim.SetBool("isGrounded", isGrounded);

        // Optional: add airborne trigger for future jump animations
        if (!isGrounded && rb.linearVelocity.y > 0.1f)
            anim.SetBool("isJumping", true);
        else if (isGrounded)
            anim.SetBool("isJumping", false);
    }

    /// <summary>
    /// If the player is on a platform and presses jump holding down, then fall through the platform
    /// </summary>
    void DropThruPlatform()
    {        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platforms"), true);
        hasJumped = true;
    }
}
