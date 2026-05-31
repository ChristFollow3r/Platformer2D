using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldLoader : MonoBehaviour
{



    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string saveName = "save1";

    [ContextMenu("New World")]
    public void NewWorld()
    {
        if (string.IsNullOrEmpty(saveName)) return;
        WorldSerializer.isNewWorld = true;
        WorldSerializer.WorldName = saveName;
        SceneManager.LoadScene(gameSceneName);
    }

    [ContextMenu("Load World")]
    public void LoadWorld()
    {
        if (string.IsNullOrEmpty(saveName)) return;
        WorldSerializer.isNewWorld = false;
        if (!WorldSerializer.Exists(saveName))
        {
            Debug.LogWarning($"[WorldLoader] No save found with name '{saveName}'");
            return;
        }

        WorldSerializer.WorldName = saveName;
        SceneManager.LoadScene(gameSceneName);
    }

    [ContextMenu("Delete World")]
    public void DeleteWorld()
    {
        if (!WorldSerializer.Exists(saveName))
        {
            Debug.LogWarning($"[WorldLoader] No save found with name '{saveName}'");
            return;
        }

        WorldSerializer.Delete(saveName);
        Debug.Log($"[WorldLoader] Deleted save '{saveName}'");
    }
}
