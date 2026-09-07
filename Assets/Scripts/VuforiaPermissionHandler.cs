using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Handles camera permission requests BEFORE loading the AR scene.
/// Attach this to a GameObject in the MainMenu scene.
/// With Vuforia's "Delayed Initialization" enabled, Vuforia will NOT start
/// until VuforiaApplication.Instance.Initialize() is called in the AR scene.
/// 
/// Flow:
///   1. User clicks "Game AR" button → calls RequestCameraAndLoadAR()
///   2. This script requests camera permission (Android) 
///   3. Once granted → loads the AR scene
///   4. ARCameraUIController.Awake() in AR scene calls Vuforia Initialize()
/// </summary>
public class VuforiaPermissionHandler : MonoBehaviour
{
    [SerializeField] private string arSceneName = "ARScene";

    private bool _waitingForPermission = false;

    /// <summary>
    /// Call this from your AR button instead of directly loading the AR scene.
    /// </summary>
    public void RequestCameraAndLoadAR()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("VuforiaPermissionHandler: Requesting camera permission...");
            _waitingForPermission = true;

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDenied += OnPermissionDenied;
            callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedPermanently;

            Permission.RequestUserPermission(Permission.Camera, callbacks);
        }
        else
        {
            Debug.Log("VuforiaPermissionHandler: Camera permission already granted.");
            LoadARScene();
        }
#else
        // In Editor or non-Android platforms, just load the scene directly
        Debug.Log("VuforiaPermissionHandler: Non-Android platform, loading AR scene directly.");
        LoadARScene();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnPermissionGranted(string permission)
    {
        Debug.Log("VuforiaPermissionHandler: Camera permission GRANTED!");
        _waitingForPermission = false;
        LoadARScene();
    }

    private void OnPermissionDenied(string permission)
    {
        Debug.LogWarning("VuforiaPermissionHandler: Camera permission DENIED. Cannot start AR.");
        _waitingForPermission = false;
        // Optionally show a UI message to the user explaining why camera is needed
    }

    private void OnPermissionDeniedPermanently(string permission)
    {
        Debug.LogWarning("VuforiaPermissionHandler: Camera permission DENIED permanently. " +
                         "User must enable it manually in Settings.");
        _waitingForPermission = false;
        // Optionally show a dialog directing user to app settings
    }
#endif

    private void LoadARScene()
    {
        Debug.Log("VuforiaPermissionHandler: Loading AR scene: " + arSceneName);
        SceneManager.LoadScene(arSceneName);
    }
}
