﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Visual (Child Components)")]
    [Tooltip("Assign the child GameObject that holds the Animator and SpriteRenderer.")]
    public Transform spriteChild;
    public Animator childAnimator;
    public SpriteRenderer childSpriteRenderer;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ✅ Use explicitly assigned child components if set
        if (childAnimator != null && childSpriteRenderer != null)
        {
            anim = childAnimator;
            spriteRenderer = childSpriteRenderer;
        }
        else if (spriteChild != null)
        {
            anim = spriteChild.GetComponent<Animator>();
            spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
        }
        else
        {
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

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

    bool DetectGround()
    {
        bool grounded;
        foreach (LayerMask layer in collisionLayers)
        {
            grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, layer);
            if(grounded)
            {
                return true;
            }
        }
        return false;
    }

    private void DetectTile()
    {
<<<<<<< Updated upstream
        Vector3Int cellPosition = interactionTilemaps[0].WorldToCell(transform.position);
        TileBase tileUnderPlayer = interactionTilemaps[0].GetTile(cellPosition);
=======
        if (interactionTilemap == null) return;

        Vector3Int cellPosition = interactionTilemap.WorldToCell(transform.position);
        TileBase tileUnderPlayer = interactionTilemap.GetTile(cellPosition);
>>>>>>> Stashed changes

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
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpReleased = Input.GetButtonUp("Jump");

        // --- Ground Check ---
<<<<<<< Updated upstream
        bool wasGrounded = isGrounded;
        isGrounded = DetectGround();

=======
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
>>>>>>> Stashed changes
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
            // Ground Jump
            else if ((isGrounded || coyoteTimeCounter > 0f) && !hasJumped)
            {
                Debug.Log("Jump!");
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

        // ✅ Flip child sprite only — keeps collider position fixed
        if (spriteChild != null)
        {
            Vector3 scale = spriteChild.localScale;
            if (moveInput > 0) scale.x = Mathf.Abs(scale.x);
            else if (moveInput < 0) scale.x = -Mathf.Abs(scale.x);
            spriteChild.localScale = scale;
        }
    }

    // 🎞️ Animation handling
    void UpdateAnimationStates()
    {
        if (anim == null) return;

        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat("Speed", horizontalSpeed);
        anim.SetBool("isGrounded", isGrounded);

        if (!isGrounded && rb.linearVelocity.y > 0.1f)
            anim.SetBool("isJumping", true);
        else if (isGrounded)
            anim.SetBool("isJumping", false);
    }
}
