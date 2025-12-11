using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadKarsiori_Stage3 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Loading Karsiori_Stage3...");
            SceneManager.LoadScene("Karsiori_Stage3");
        }
    }
}
