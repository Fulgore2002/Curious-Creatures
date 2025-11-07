using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndTrigger : MonoBehaviour
{
    private bool hasEnded = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasEnded) return;

        if (other.CompareTag("Player"))
        {
            hasEnded = true;

            
            var movement = other.GetComponent<Playercontroller>();
            if (movement != null)
                movement.enabled = false;

            
            StartCoroutine(LoadEndScene());
        }
    }

    private IEnumerator LoadEndScene()
    {
        yield return new WaitForSeconds(0.5f); // short delay before transition
        SceneManager.LoadScene("End Level");
    }
}
