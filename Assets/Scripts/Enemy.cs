using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Follow Settings")]
    public float moveSpeed = 2f;
    private Transform player;

    [Header("Player Detection")]
    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    void Start()
    {
        // --- FIND PLAYER ---
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);

        if (p != null)
        {
            player = p.transform;
            Debug.Log("[Enemy] Player found! Following.");
        }
        else
        {
            Debug.LogWarning("[Enemy] Player not found! Make sure the player is tagged 'Player'.");
        }
    }

    void Update()
    {
        // --- FOLLOW PLAYER ---
        if (player == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Enemy] TriggerEnter: {gameObject.name} collided with {other.gameObject.name}");

        // Only react to player
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[Enemy] Ignored {other.gameObject.name}, wrong tag.");
            return;
        }

        // Try to grab PlayerFlash component from different locations
        PlayerFlash pf = other.GetComponent<PlayerFlash>();

        if (pf == null && other.transform.parent != null)
            pf = other.transform.parent.GetComponent<PlayerFlash>();

        if (pf == null)
            pf = other.GetComponentInChildren<PlayerFlash>();

        if (pf != null)
        {
            Debug.Log($"[Enemy] Found PlayerFlash on {pf.gameObject.name} — FlashNow()");
            pf.FlashNow();
        }
        else
        {
            Debug.LogWarning($"[Enemy] No PlayerFlash found on {other.gameObject.name} or parents/children.");
        }
    }
}
