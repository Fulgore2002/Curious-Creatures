using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;   // Singleton reference for easy access
    public Image healthBarImage;        // Reference to your health bar Image
    public Sprite[] healthSprites;      // Array of sprites

    private void Awake()
    {
        // Ensure only one UIManager exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Call this when the player’s health changes
    public void SetHealth(int health)
    {
        healthBarImage.sprite = healthSprites[Mathf.Clamp(health, 0, healthSprites.Length - 1)];
    }
}
