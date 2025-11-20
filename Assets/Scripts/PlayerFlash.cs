using UnityEngine;
using System.Collections;

public class PlayerFlash : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Color flashColor = Color.white;
    public float flashDuration = 0.15f;
    public float invincibleTime = 3f;

    private Color originalColor;
    public bool isInvincible = false;

    private int enemyLayer;
    private int playerLayer;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        enemyLayer = LayerMask.NameToLayer("Enemy");
        playerLayer = LayerMask.NameToLayer("Player");
    }

    public void FlashNow()
    {
        if (!isInvincible)
            StartCoroutine(InvincibleCoroutine());
    }

    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        // Disable collision between player and enemies
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float endTime = Time.time + invincibleTime;

        // FLASH repeatedly during invincibility
        while (Time.time < endTime)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        // Restore
        spriteRenderer.color = originalColor;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        isInvincible = false;
    }
}
