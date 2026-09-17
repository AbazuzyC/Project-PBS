#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Ensures the correct physical webcam is selected for Vuforia in the Unity Editor,
/// preventing conflicts with scanners/printers (e.g., EPSON) or virtual devices,
/// and handles initialization safely without causing premature camera starvation.
/// </summary>
public static class VuforiaEditorAutoInit
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeVuforiaEarlyInEditor()
    {
        Debug.Log("VuforiaEditorAutoInit: Checking webcam and initialization in Editor...");

        // 1. Ensure the correct webcam device is selected
        EnsureCorrectWebCamSelected();

        // 2. Check if Vuforia requires manual initialization (only if delayed initialization is enabled)
        System.Type vuforiaAppType = FindType("Vuforia.VuforiaApplication");
        if (vuforiaAppType == null)
        {
            Debug.LogWarning("VuforiaEditorAutoInit: Failed to find VuforiaApplication.");
            return;
        }

        try
        {
            var instanceProp = vuforiaAppType.GetProperty("Instance");
            if (instanceProp != null)
            {
                var instance = instanceProp.GetValue(null);
                if (instance != null)
                {
                    var isInitializedProp = vuforiaAppType.GetProperty("IsInitialized");
                    if (isInitializedProp != null && (bool)isInitializedProp.GetValue(instance))
                    {
                        Debug.Log("VuforiaEditorAutoInit: Vuforia is already initialized.");
                        return;
                    }

                    // Check if Delayed Initialization is enabled in VuforiaConfiguration
                    if (IsDelayedInitializationEnabled())
                    {
                        var initMethod = vuforiaAppType.GetMethod("Initialize", System.Type.EmptyTypes)
                                      ?? vuforiaAppType.GetMethod("Initialize");
                        if (initMethod != null)
                        {
                            initMethod.Invoke(instance, null);
                            Debug.Log("VuforiaEditorAutoInit: Vuforia initialized early for delayed initialization support.");
                            return;
                        }
                    }
                    else
                    {
                        Debug.Log("VuforiaEditorAutoInit: Delayed initialization is disabled; letting ARCamera initialize Vuforia on scene load.");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"VuforiaEditorAutoInit: Error initializing Vuforia: {e.Message}");
        }
    }

    private static void EnsureCorrectWebCamSelected()
    {
        try
        {
            var configType = FindType("Vuforia.VuforiaConfiguration");
            if (configType == null) return;

            var configInstanceProp = configType.GetProperty("Instance");
            var configInstance = configInstanceProp?.GetValue(null);
            if (configInstance == null) return;

            var webcamProp = configType.GetProperty("WebCam");
            var webcamInstance = webcamProp?.GetValue(configInstance);
            if (webcamInstance == null) return;

            var deviceNameProp = webcamInstance.GetType().GetProperty("DeviceNameSetInEditor");
            if (deviceNameProp == null) return;

            string currentDevice = deviceNameProp.GetValue(webcamInstance) as string;

            var devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                Debug.LogWarning("VuforiaEditorAutoInit: No webcam devices detected by Unity!");
                return;
            }

            string bestCam = null;
            foreach (var d in devices)
            {
                string dName = d.name;
                string lower = dName.ToLower();

                // Skip printers, scanners, or virtual capture devices that have no video stream
                if (lower.Contains("epson") || lower.Contains("scanner") || lower.Contains("print"))
                    continue;

                if (bestCam == null)
                {
                    bestCam = dName;
                }
                else if (lower.Contains("webcam") || lower.Contains("integrated") || lower.Contains("camera") || lower.Contains("vga"))
                {
                    bestCam = dName;
                }
            }

            if (!string.IsNullOrEmpty(bestCam) && currentDevice != bestCam)
            {
                deviceNameProp.SetValue(webcamInstance, bestCam);
                Debug.Log($"VuforiaEditorAutoInit: Set Vuforia WebCam device to '{bestCam}' (was: '{currentDevice ?? "None"}')");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"VuforiaEditorAutoInit: Error setting WebCam device: {e.Message}");
        }
    }

    private static bool IsDelayedInitializationEnabled()
    {
        try
        {
            var configType = FindType("Vuforia.VuforiaConfiguration");
            if (configType == null) return false;

            var configInstanceProp = configType.GetProperty("Instance");
            var configInstance = configInstanceProp?.GetValue(null);
            if (configInstance == null) return false;

            var vuforiaProp = configType.GetProperty("Vuforia");
            var vuforiaInstance = vuforiaProp?.GetValue(configInstance);
            if (vuforiaInstance == null) return false;

            var delayedProp = vuforiaInstance.GetType().GetProperty("DelayedInitialization");
            if (delayedProp != null)
            {
                return (bool)delayedProp.GetValue(vuforiaInstance);
            }
        }
        catch {}
        return false;
    }

    private static System.Type FindType(string typeFullName)
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetType(typeFullName);
                if (type != null) return type;
            }
            catch {}
        }
        return null;
    }
}
#endif
