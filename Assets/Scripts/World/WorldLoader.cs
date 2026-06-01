using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldLoader : MonoBehaviour
{



    [SerializeField] public string gameSceneName = "Game";

    [ContextMenu("New World")]
    public void NewWorld(string name, string seed)
    {
        if (string.IsNullOrEmpty(name)) return;
        WorldSerializer.isNewWorld = true;
        WorldSerializer.Seed = seed;
        WorldSerializer.WorldName = name;
        SceneManager.LoadScene(gameSceneName);
        //TODO: handle async
    }

    [ContextMenu("Load World")]
    public void LoadWorld(string saveName)
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
        //TODO: handle async
    }

    [ContextMenu("Delete World")]
    public void DeleteWorld(string saveName)
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
