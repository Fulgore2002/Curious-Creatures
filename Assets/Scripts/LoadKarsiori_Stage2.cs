using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadKarsiori_Stage2 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Loading Karsiori_Stage2...");
            SceneManager.LoadScene("Karsiori_Stage2");
        }
    }
}
