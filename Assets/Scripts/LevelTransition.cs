using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransition : MonoBehaviour
{
    [Tooltip("Exact scene name (must be in Build Settings)")]
    public string targetScene = "Karsiori_Stage2";

    [Tooltip("Delay before starting to load (seconds)")]
    public float delayBeforeLoad = 0.5f;

    [Tooltip("If true, will try to disable a PlayerController script on the player during the load")]
    public bool temporarilyDisablePlayerController = false;

    [Tooltip("Name of the player controller component (case sensitive). Example: PlayerController")]
    public string playerControllerScriptName = "Playercontroller";

    private bool hasTransitioned = false;

    private void Reset()
    {
        // sensible defaults when first attached
        targetScene = "Karsiori_Stage2";
        delayBeforeLoad = 0.5f;
        temporarilyDisablePlayerController = false;
        playerControllerScriptName = "Playercontroller";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTransitioned) return;

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"LevelTransition: collided with non-player object '{other.gameObject.name}'");
            return;
        }

        hasTransitioned = true;
        Debug.Log($"LevelTransition: Player entered trigger. Preparing to load '{targetScene}' in {delayBeforeLoad} seconds.");

        // Optionally disable player's movement script (only temporarily)
        MonoBehaviour playerController = null;
        if (temporarilyDisablePlayerController && !string.IsNullOrEmpty(playerControllerScriptName))
        {
            var comp = other.GetComponent(playerControllerScriptName);
            if (comp is MonoBehaviour mb)
            {
                playerController = mb;
                mb.enabled = false;
                Debug.Log("LevelTransition: Disabled player controller '" + playerControllerScriptName + "'.");
            }
            else
            {
                Debug.LogWarning($"LevelTransition: Could not find component named '{playerControllerScriptName}' on player to disable.");
            }
        }

        StartCoroutine(LoadSceneAsyncCoroutine(other.gameObject, playerController));
    }

    private IEnumerator LoadSceneAsyncCoroutine(GameObject player, MonoBehaviour disabledController)
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        // Start async load
        var asyncOp = SceneManager.LoadSceneAsync(targetScene);
        if (asyncOp == null)
        {
            Debug.LogError($"LevelTransition: Failed to start loading scene '{targetScene}'. Make sure it is added to Build Settings.");
            if (disabledController != null) disabledController.enabled = true;
            yield break;
        }

        // Optionally allow the scene to load in the background and activate immediately when ready:
        asyncOp.allowSceneActivation = true;

        // wait until load completes
        while (!asyncOp.isDone)
        {
            // progress will go 0..0.9, then isDone becomes true once activation finished
            yield return null;
        }

        // If we disabled the player's controller and you want it re-enabled when returning to this scene (unlikely),
        // you'd re-enable it here only if you plan to stay in this object. Usually we don't re-enable after a scene change.
        Debug.Log($"LevelTransition: Scene '{targetScene}' loaded.");
    }
}
