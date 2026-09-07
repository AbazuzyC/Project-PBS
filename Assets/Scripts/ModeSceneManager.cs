using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ModeSceneManager : MonoBehaviour
{
    [SerializeField] private UIDocument modeSelectionUIDocument;

    [Header("Scene Configuration")]
    [SerializeField] private string arSceneName = "ARScene";
    [SerializeField] private string quizSceneName = "SampleScene";
    [SerializeField] private string materiSceneName = "SampleScene";

    [Header("Permission Handler")]
    [Tooltip("Assign the GameObject with VuforiaPermissionHandler. If not assigned, will try to find one.")]
    [SerializeField] private VuforiaPermissionHandler permissionHandler;

    private void OnEnable()
    {
        if (modeSelectionUIDocument == null)
        {
            modeSelectionUIDocument = GetComponent<UIDocument>();
        }

        if (modeSelectionUIDocument != null)
        {
            var root = modeSelectionUIDocument.rootVisualElement;
            if (root != null)
            {
                var arPlayBtn = root.Q<Button>("ar-play-button");
                var quizPlayBtn = root.Q<Button>("quiz-play-button");
                var materiStudyBtn = root.Q<Button>("materi-study-button");

                if (arPlayBtn != null)
                {
                    arPlayBtn.clicked -= OnARPlayClicked;
                    arPlayBtn.clicked += OnARPlayClicked;
                }
                if (quizPlayBtn != null)
                {
                    quizPlayBtn.clicked -= OnQuizPlayClicked;
                    quizPlayBtn.clicked += OnQuizPlayClicked;
                }
                if (materiStudyBtn != null)
                {
                    materiStudyBtn.clicked -= OnMateriStudyClicked;
                    materiStudyBtn.clicked += OnMateriStudyClicked;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (modeSelectionUIDocument != null)
        {
            var root = modeSelectionUIDocument.rootVisualElement;
            if (root != null)
            {
                var arPlayBtn = root.Q<Button>("ar-play-button");
                var quizPlayBtn = root.Q<Button>("quiz-play-button");
                var materiStudyBtn = root.Q<Button>("materi-study-button");

                if (arPlayBtn != null) arPlayBtn.clicked -= OnARPlayClicked;
                if (quizPlayBtn != null) quizPlayBtn.clicked -= OnQuizPlayClicked;
                if (materiStudyBtn != null) materiStudyBtn.clicked -= OnMateriStudyClicked;
            }
        }
    }

    private void OnARPlayClicked()
    {
        Debug.Log("ModeSceneManager: AR button clicked, checking permissions...");
        
        // Try to find the permission handler if not assigned
        if (permissionHandler == null)
        {
            permissionHandler = FindFirstObjectByType<VuforiaPermissionHandler>();
        }

        if (permissionHandler != null)
        {
            permissionHandler.RequestCameraAndLoadAR();
        }
        else
        {
            // Fallback: load directly if no permission handler found
            Debug.LogWarning("ModeSceneManager: No VuforiaPermissionHandler found, loading AR scene directly.");
            SceneManager.LoadScene(arSceneName);
        }
    }

    private void OnQuizPlayClicked()
    {
        Debug.Log("ModeSceneManager: Transitioning to Quiz scene: " + quizSceneName);
        SceneManager.LoadScene(quizSceneName);
    }

    private void OnMateriStudyClicked()
    {
        Debug.Log("ModeSceneManager: Transitioning to Materi scene: " + materiSceneName);
        SceneManager.LoadScene(materiSceneName);
    }
}
