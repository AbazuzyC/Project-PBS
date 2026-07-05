using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SetupGameObjectsHelper : EditorWindow
{
    [MenuItem("Helper/Setup UI GameObjects")]
    public static void SetupUI()
    {
        // 1. Find MainMenuCanvas in the scene
        GameObject mainMenuCanvasGo = GameObject.Find("MainMenuCanvas");
        if (mainMenuCanvasGo == null)
        {
            Debug.LogError("Setup Error: Could not find MainMenuCanvas in the active scene!");
            return;
        }

        // 2. Find MulaiButton in the scene
        UnityEngine.UI.Button mulaiButtonComponent = null;
        Transform mulaiButtonTransform = mainMenuCanvasGo.transform.Find("MulaiButton");
        if (mulaiButtonTransform != null)
        {
            mulaiButtonComponent = mulaiButtonTransform.GetComponent<UnityEngine.UI.Button>();
        }
        else
        {
            // Search globally if not nested directly under MainMenuCanvas
            GameObject mulaiButtonGo = GameObject.Find("MulaiButton");
            if (mulaiButtonGo != null)
            {
                mulaiButtonComponent = mulaiButtonGo.GetComponent<UnityEngine.UI.Button>();
            }
        }

        if (mulaiButtonComponent == null)
        {
            Debug.LogError("Setup Error: Could not find MulaiButton in the scene (or it is missing a UnityEngine.UI.Button component)!");
            return;
        }

        // 3. Find or Create ModeSelectionUI GameObject
        GameObject modeSelectionUIGo = GameObject.Find("ModeSelectionUI");
        if (modeSelectionUIGo == null)
        {
            modeSelectionUIGo = new GameObject("ModeSelectionUI");
            Undo.RegisterCreatedObjectUndo(modeSelectionUIGo, "Create ModeSelectionUI");
        }

        // Add or get the UIDocument component
        UIDocument uiDoc = modeSelectionUIGo.GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            uiDoc = modeSelectionUIGo.AddComponent<UIDocument>();
        }

        // Assign UXML VisualTreeAsset
        VisualTreeAsset uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/ModeSelectionUI.uxml");
        if (uxmlAsset != null)
        {
            uiDoc.visualTreeAsset = uxmlAsset;
        }
        else
        {
            Debug.LogWarning("Setup Warning: Could not find ModeSelectionUI.uxml at 'Assets/UI Toolkit/ModeSelectionUI.uxml'");
        }

        // Assign PanelSettings Asset
        PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");
        if (panelSettings != null)
        {
            uiDoc.panelSettings = panelSettings;
        }
        else
        {
            Debug.LogWarning("Setup Warning: Could not find PanelSettings.asset at 'Assets/UI Toolkit/PanelSettings.asset'");
        }

        // 4. Find or Create UIController GameObject in the scene
        GameObject uiControllerGo = GameObject.Find("UIController");
        if (uiControllerGo == null)
        {
            uiControllerGo = new GameObject("UIController");
            Undo.RegisterCreatedObjectUndo(uiControllerGo, "Create UIController");
        }

        // Add or get the MainMenuUIButtons controller script component
        MainMenuUIButtons controller = uiControllerGo.GetComponent<MainMenuUIButtons>();
        if (controller == null)
        {
            controller = uiControllerGo.AddComponent<MainMenuUIButtons>();
        }

        // Add or get the ModeSceneManager script component
        ModeSceneManager sceneManager = uiControllerGo.GetComponent<ModeSceneManager>();
        if (sceneManager == null)
        {
            sceneManager = uiControllerGo.AddComponent<ModeSceneManager>();
        }

        // 5. Assign serialized fields using SerializedObject to support Undo history and mark scene dirty
        SerializedObject soController = new SerializedObject(controller);
        soController.FindProperty("mainMenuCanvas").objectReferenceValue = mainMenuCanvasGo;
        soController.FindProperty("mulaiButton").objectReferenceValue = mulaiButtonComponent;
        soController.FindProperty("modeSelectionUIDocument").objectReferenceValue = uiDoc;
        soController.ApplyModifiedProperties();

        SerializedObject soManager = new SerializedObject(sceneManager);
        soManager.FindProperty("modeSelectionUIDocument").objectReferenceValue = uiDoc;
        soManager.ApplyModifiedProperties();

        // Mark active scene dirty so changes are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("UI Setup Completed Successfully! Created ModeSelectionUI and configured MainMenuUIButtons and ModeSceneManager scripts with the correct field references.");
    }
}
