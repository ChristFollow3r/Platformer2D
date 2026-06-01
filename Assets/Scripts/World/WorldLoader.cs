using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        StartCoroutine(LoadSceneAsync(gameSceneName));
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
        StartCoroutine(LoadSceneAsync(gameSceneName));
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

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Scene loaderScene = gameObject.scene;

        // Load new scene additively so both exist simultaneously
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        load.allowSceneActivation = false;
        MainMenuUI.Singleton.SetLoading();
        // Wait until the scene is ready (progress stops at 0.9 until activation is allowed)
        while (load.progress < 0.9f)
        {
            MainMenuUI.Singleton.loadFill.style.width = Length.Percent(load.progress * 100f);
            yield return null;
        }
        yield return new WaitForSeconds(1);
        // Activate the new scene
        load.allowSceneActivation = true;
        yield return load; // wait for activation to complete

        // Set the new scene as active
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        // Unload the loader scene
        yield return SceneManager.UnloadSceneAsync(loaderScene);
    }
}
