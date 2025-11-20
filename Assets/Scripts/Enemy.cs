using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public string playerTag = "Player";

    private Transform player;
    private float attackCooldown = 0.2f; // how often enemy *tries* to damage
    private float nextAttackTime = 0f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.freezeRotation = true;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
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
        // PLAYER IS INVINCIBLE — LET THEM PASS THROUGH
        if (pf.isInvincible)
            return;

        if (Time.time >= nextAttackTime)
        {
            pf.FlashNow();
            nextAttackTime = Time.time + attackCooldown;
        }
    }
}
