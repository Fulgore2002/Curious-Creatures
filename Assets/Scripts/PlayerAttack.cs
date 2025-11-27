using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1f;
    public int attackDamage = 1;
    public float attackCooldown = 0.4f;

    [Header("Knockback")]
    public float knockbackForce = 8f;

    [Header("References")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    private float nextAttackTime;
    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Keyboard (G) OR Controller (X)
        if ((Input.GetKeyDown(KeyCode.G) || Input.GetButtonDown("Fire1"))
            && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;

        if (anim != null)
            anim.SetTrigger("Attack");

        // Detect enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider2D enemy in hits)
        {
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            Rigidbody2D er = enemy.GetComponent<Rigidbody2D>();

            if (eh != null)
                eh.TakeDamage(attackDamage);

            // Knockback
            if (er != null)
            {
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                er.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    // Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
