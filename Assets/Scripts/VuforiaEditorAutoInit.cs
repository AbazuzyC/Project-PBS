#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// This script forces Vuforia to initialize immediately when playing in the Unity Editor.
/// This allows you to keep "Delayed Initialization" ENABLED in Vuforia settings (for Android),
/// but still have the webcam work perfectly when testing in the Editor.
/// </summary>
public static class VuforiaEditorAutoInit
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeVuforiaEarlyInEditor()
    {
        Debug.Log("VuforiaEditorAutoInit: Forcing early initialization in Editor to support Webcam...");
        
        string[] assemblyNames = new string[]
        {
            "Vuforia.Unity.Engine",
            "Vuforia.Unity.Engine.dll",
            "VuforiaEngine",
            "VuforiaScripts"
        };

        foreach (string asmName in assemblyNames)
        {
            try
            {
                System.Type vuforiaAppType = System.Type.GetType("Vuforia.VuforiaApplication, " + asmName);
                if (vuforiaAppType != null)
                {
                    var instanceProp = vuforiaAppType.GetProperty("Instance");
                    if (instanceProp != null)
                    {
                        var instance = instanceProp.GetValue(null);
                        if (instance != null)
                        {
                            var initMethod = vuforiaAppType.GetMethod("Initialize");
                            if (initMethod != null)
                            {
                                initMethod.Invoke(instance, null);
                                Debug.Log($"Vuforia successfully auto-initialized in Editor [assembly: {asmName}]");
                                return;
                            }
                        }
                    }
                }
            }
            catch (System.Exception) {}
        }
        
        Debug.LogWarning("VuforiaEditorAutoInit: Failed to find VuforiaApplication to auto-initialize.");
    }
}
#endif
