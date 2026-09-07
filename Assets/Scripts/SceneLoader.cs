using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Default Scene Configuration")]
    [SerializeField] private string arSceneName = "ARScene";
    [SerializeField] private string quizSceneName = "SampleScene";
    [SerializeField] private string materiSceneName = "SampleScene";

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
    /// Loads the Quiz scene configured in the inspector.
    /// </summary>
    public void LoadQuizScene()
    {
        LoadSceneByName(quizSceneName);
    }

    /// <summary>
    /// Loads the Material/Lesson scene configured in the inspector.
    /// </summary>
    public void LoadMateriScene()
    {
        LoadSceneByName(materiSceneName);
    }
}
