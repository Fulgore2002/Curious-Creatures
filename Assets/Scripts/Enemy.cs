using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public string playerTag = "Player";

    [Header("Attack")]
    private float attackCooldown = 0.2f;
    private float nextAttackTime = 0f;

    [Header("Knockback")]
    public float hitStunTime = 0.25f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isStunned = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 2f; // ✅ gravity stays on

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning("[Enemy] Player not found!");
    }

    void Update()
    {
        if (player == null || isStunned) return;

        float dir = Mathf.Sign(player.position.x - rb.position.x);

        rb.linearVelocity = new Vector2(
            dir * moveSpeed,
            rb.linearVelocity.y
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerFlash pf = other.GetComponentInParent<PlayerFlash>();
        if (pf != null)
            TryDamage(pf);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerFlash pf = other.GetComponentInParent<PlayerFlash>();
        if (pf != null)
            TryDamage(pf);
    }

    void TryDamage(PlayerFlash pf)
    {
        if (pf.isInvincible) return;

        if (Time.time >= nextAttackTime)
        {
            pf.FlashNow();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // ✅ CALLED BY PlayerAttack
    public void TakeHit(Vector2 attackerPos, float force)
    {
        if (isStunned) return;

        Vector2 knockDir = ((Vector2)transform.position - attackerPos).normalized;

        StopAllCoroutines();
        StartCoroutine(HitStun(knockDir * force));
    }

    IEnumerator HitStun(Vector2 knockback)
    {
        isStunned = true;

        rb.linearVelocity = Vector2.zero; // reset
        rb.AddForce(new Vector2(knockback.x, 2f), ForceMode2D.Impulse);

        yield return new WaitForSeconds(hitStunTime);

        isStunned = false;
    }
}
