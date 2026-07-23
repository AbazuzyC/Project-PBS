using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SetupGameObjectsHelper : EditorWindow
{
    [MenuItem("Helper/Setup UI GameObjects")]
    public static void SetupUI()
    {
        // 1. Find or Create ModeSelectionUI GameObject
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

        // 2. Find or Create UIController GameObject in the scene
        GameObject uiControllerGo = GameObject.Find("UIController");
        if (uiControllerGo == null)
        {
            uiControllerGo = new GameObject("UIController");
            Undo.RegisterCreatedObjectUndo(uiControllerGo, "Create UIController");
        }

        // Add or get the ModeSceneManager script component
        ModeSceneManager sceneManager = uiControllerGo.GetComponent<ModeSceneManager>();
        if (sceneManager == null)
        {
            sceneManager = uiControllerGo.AddComponent<ModeSceneManager>();
        }

        // 3. Assign serialized fields using SerializedObject to support Undo history and mark scene dirty
        SerializedObject soManager = new SerializedObject(sceneManager);
        soManager.FindProperty("modeSelectionUIDocument").objectReferenceValue = uiDoc;
        soManager.ApplyModifiedProperties();

        // Mark active scene dirty so changes are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("UI Setup Completed Successfully! Created ModeSelectionUI and configured ModeSceneManager script with the correct field references.");
    }
}
