using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Loading END LEVEL NOW");

            // HARD LOAD — no delays, no coroutine
            SceneManager.LoadScene("End Level");
        }
    }
}
