using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayer;
    public float attackCooldown = 0.3f;

    [Header("Knockback")]
    public float knockbackForce = 6f;

    private float nextAttackTime = 0f;

    void Update()
    {
        // Keyboard G OR Controller X
        bool attackPressed =
            Input.GetKeyDown(KeyCode.G) ||
            Input.GetButtonDown("Fire1"); // X button by default

        if (attackPressed && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        // Detect enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeHit(transform.position, knockbackForce);
            }
        }
    }

    // Visualize hitbox
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
