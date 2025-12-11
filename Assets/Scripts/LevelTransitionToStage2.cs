using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransitionToStage2 : MonoBehaviour
{
    private bool hasTransitioned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTransitioned) return;

        if (other.CompareTag("Player"))
        {
            hasTransitioned = true;

            // No disabling player movement here!

            StartCoroutine(LoadNextStage());
        }
    }

    private IEnumerator LoadNextStage()
    {
        yield return new WaitForSeconds(0.5f); // small delay
        SceneManager.LoadScene("Karsiori_Stage2");
    }
}