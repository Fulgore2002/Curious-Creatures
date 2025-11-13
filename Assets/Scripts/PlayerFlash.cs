using UnityEngine;
using System.Collections;

public class PlayerFlash : MonoBehaviour
{
    [Tooltip("Optional: assign manually if SpriteRenderer is not on this GameObject")]
    public SpriteRenderer spriteRenderer;

    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        // If not assigned, search this GameObject and children (handles animated children)
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[PlayerFlash] No SpriteRenderer found on player or children of '{gameObject.name}'. Flash will not work until one is assigned.");
        }
        else
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Public method other objects can call
    public void FlashNow()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[PlayerFlash] Flash requested but SpriteRenderer is null on '{gameObject.name}'.");
            return;
        }

        if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine());
        }
    }

    private IEnumerator FlashCoroutine()
    {
        isFlashing = true;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }
}
