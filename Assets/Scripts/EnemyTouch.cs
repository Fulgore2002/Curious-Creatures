using UnityEngine;
using System.Collections; // Needed for IEnumerator

public class EnemyTouch : MonoBehaviour
{
    [Tooltip("Tag to identify the player")]
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        // Quick log to verify collisions
        Debug.Log($"[EnemyTouch] TriggerEnter: {gameObject.name} collided with {other.gameObject.name}");

        // Check tag first (fast)
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[EnemyTouch] Ignored {other.gameObject.name} because it's not tagged '{playerTag}'.");
            return;
        }

        // Try to get PlayerFlash directly on the collider object
        PlayerFlash pf = other.GetComponent<PlayerFlash>();

        // If not found, check parent (handles cases where collider is on a child)
        if (pf == null && other.transform.parent != null)
        {
            pf = other.transform.parent.GetComponent<PlayerFlash>();
        }

        // If still not found, try GetComponentInChildren on the root object
        if (pf == null)
        {
            pf = other.GetComponentInChildren<PlayerFlash>();
        }

        if (pf != null)
        {
            Debug.Log($"[EnemyTouch] Found PlayerFlash on {pf.gameObject.name} — calling FlashNow()");
            pf.FlashNow();
        }
        else
        {
            Debug.LogWarning($"[EnemyTouch] No PlayerFlash component found on '{other.gameObject.name}' or parents/children. Attach PlayerFlash to the player root.");
        }
    }
}