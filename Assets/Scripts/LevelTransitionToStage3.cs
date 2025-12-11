using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransitionToStage3 : MonoBehaviour
{
    private bool hasTransitioned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTransitioned) return;

        if (other.CompareTag("Player"))
        {
            hasTransitioned = true;

            // No player disabling here!

            StartCoroutine(LoadNextStage());
        }
    }

    private IEnumerator LoadNextStage()
    {
        yield return new WaitForSeconds(0.5f); // small delay before transition
        SceneManager.LoadScene("Karsiori_Stage3");
    }
}