using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainMenuUIButtons : MonoBehaviour
{
    [Header("Main Menu Canvas References")]
    [SerializeField] private GameObject mainMenuCanvas; // Assign MainMenuCanvas in Inspector
    [SerializeField] private UnityEngine.UI.Button mulaiButton; // Assign MulaiButton in Inspector

    [Header("Mode Selection UI Toolkit")]
    [SerializeField] private UIDocument modeSelectionUIDocument; // Assign ModeSelectionUI Document GameObject in Inspector

    private VisualElement modeSelectionOverlay;

    void Start()
    {
        // Setup Mode Selection UI Document state
        if (modeSelectionUIDocument != null)
        {
            var root = modeSelectionUIDocument.rootVisualElement;
            if (root != null)
            {
                modeSelectionOverlay = root.Q<VisualElement>("mode-selection-root");
                if (modeSelectionOverlay != null)
                {
                    // Hide the mode selection overlay on startup
                    modeSelectionOverlay.style.display = DisplayStyle.None;
                }

                // Query buttons inside Mode Selection to print click actions (placeholders for gameplay scenes)
                var arPlayBtn = root.Q<UnityEngine.UIElements.Button>("ar-play-button");
                var quizPlayBtn = root.Q<UnityEngine.UIElements.Button>("quiz-play-button");
                var materiStudyBtn = root.Q<UnityEngine.UIElements.Button>("materi-study-button");

                if (arPlayBtn != null) arPlayBtn.clicked += () => Debug.Log("Game AR - MAIN Clicked");
                if (quizPlayBtn != null) quizPlayBtn.clicked += () => Debug.Log("Kuis - MAIN Clicked");
                if (materiStudyBtn != null) materiStudyBtn.clicked += () => Debug.Log("Materi - BELAJAR Clicked");
            }
        }

        // Bind UGUI Mulai button event
        if (mulaiButton != null)
        {
            mulaiButton.onClick.AddListener(OnMulaiButtonClicked);
        }
    }

    private void OnMulaiButtonClicked()
    {
        Debug.Log("Mulai Button Clicked: Navigating to Mode Selection.");

        // Hide Main Menu UGUI Canvas
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        // Show Mode Selection UI Document
        if (modeSelectionOverlay != null)
        {
            modeSelectionOverlay.style.display = DisplayStyle.Flex;
        }
    }
}

