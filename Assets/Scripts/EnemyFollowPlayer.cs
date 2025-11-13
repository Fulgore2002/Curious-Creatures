using UnityEngine;

public class EnemyFollowPlayer : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Transform player;

    void Start()
    {
        // Find the player by tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            player = p.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player is tagged 'Player'.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // Move toward the player
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );
    }
}