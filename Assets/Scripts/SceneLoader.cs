using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Default Scene Configuration")]
    [SerializeField] private string arSceneName = "ARScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Loads a scene by its exact name.
    /// Drag this function to your Button's OnClick() list in the Inspector and type the scene name in the box.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene Loader Error: The target scene name is empty!");
            return;
        }

        Debug.Log($"Scene Loader: Transitioning to scene '{sceneName}'");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Loads the AR Game scene configured in the inspector.
    /// </summary>
    public void LoadARScene()
    {
        LoadSceneByName(arSceneName);
    }

    /// <summary>
    /// Loads the Main Menu scene configured in the inspector.
    /// </summary>
    public void LoadMainMenuScene()
    {
        LoadSceneByName(mainMenuSceneName);
    }

    /// <summary>
    /// Closes the application or exits Play Mode if running in the Unity Editor.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Scene Loader: Quitting application...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
