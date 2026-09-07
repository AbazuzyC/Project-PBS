using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Reflection;

public class ARCameraUIController : MonoBehaviour
{
    private UIDocument _uiDocument;
    
    // UI Elements
    private Button _backButton;
    private Button _flashlightButton;
    private VisualElement _scanLine;

    // Flashlight State
    private bool _isFlashlightOn = false;

    // Scanline State
    private float _scanLinePos = 0f;
    private float _scanLineSpeed = 300f; // pixels per second
    private bool _scanLineMovingDown = true;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            Debug.LogError("ARCameraUIController requires a UIDocument component on the same GameObject.");
            return;
        }
        
        var root = _uiDocument.rootVisualElement;
        
        _backButton = root.Q<Button>("BackButton");
        _flashlightButton = root.Q<Button>("FlashlightButton");
        _scanLine = root.Q<VisualElement>("ScanLine");

        // Back Button Event
        if (_backButton != null)
        {
            _backButton.clicked += OnBackClicked;
        }

        // Flashlight Button Event
        if (_flashlightButton != null)
        {
            _flashlightButton.clicked += OnFlashlightClicked;
        }
    }

    private void OnDisable()
    {
        if (_backButton != null) _backButton.clicked -= OnBackClicked;
        if (_flashlightButton != null) _flashlightButton.clicked -= OnFlashlightClicked;
    }

    private System.Collections.IEnumerator Start()
    {
        // Wait one frame for the scene to be fully loaded
        yield return null;

        InitializeVuforia();
    }

    private void InitializeVuforia()
    {
        // The correct assembly names for Vuforia in this project:
        //   - "Vuforia.Unity.Engine" for VuforiaBehaviour, ImageTargetBehaviour, etc.
        //   - "VuforiaScripts" for DefaultObserverEventHandler, etc.
        string[] assemblyNames = new string[]
        {
            "Vuforia.Unity.Engine",  // Actual assembly in this project
            "Vuforia.Unity.Engine.dll",
            "VuforiaEngine",         // Fallback for other Vuforia versions
            "VuforiaScripts"
        };

        bool initialized = false;

        // --- Approach 1: VuforiaApplication.Instance.Initialize() ---
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
                                Debug.Log($"Vuforia initialized via VuforiaApplication.Instance.Initialize() [assembly: {asmName}]");
                                initialized = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"VuforiaApplication init attempt failed for assembly '{asmName}': {e.Message}");
            }
        }

        // --- Approach 2: Find and enable VuforiaBehaviour in the scene ---
        // This is the most reliable way — VuforiaBehaviour on the AR Camera handles everything
        try
        {
            foreach (string asmName in assemblyNames)
            {
                System.Type vuforiaBehaviourType = System.Type.GetType("Vuforia.VuforiaBehaviour, " + asmName);
                if (vuforiaBehaviourType != null)
                {
                    // Find all VuforiaBehaviour components in the scene
                    var vuforiaBehaviour = FindFirstObjectByType(vuforiaBehaviourType) as Behaviour;
                    if (vuforiaBehaviour != null)
                    {
                        if (!vuforiaBehaviour.enabled)
                        {
                            vuforiaBehaviour.enabled = true;
                            Debug.Log($"VuforiaBehaviour found and enabled [assembly: {asmName}]");
                        }
                        else
                        {
                            Debug.Log($"VuforiaBehaviour already enabled [assembly: {asmName}]");
                        }
                        initialized = true;
                        break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("VuforiaBehaviour enable attempt failed: " + e.Message);
        }

        if (!initialized)
        {
            Debug.LogError("Failed to initialize Vuforia! No valid Vuforia types found in any assembly.");
        }
    }

    private void Update()
    {
        // Animate the Scan Line moving up and down
        if (_scanLine != null && _scanLine.parent != null)
        {
            float maxDistance = _scanLine.parent.resolvedStyle.height - _scanLine.resolvedStyle.height - 20f;
            
            if (maxDistance > 0)
            {
                if (_scanLineMovingDown)
                {
                    _scanLinePos += _scanLineSpeed * Time.deltaTime;
                    if (_scanLinePos >= maxDistance) _scanLineMovingDown = false;
                }
                else
                {
                    _scanLinePos -= _scanLineSpeed * Time.deltaTime;
                    if (_scanLinePos <= 0f) _scanLineMovingDown = true;
                }
                
                _scanLine.style.translate = new StyleTranslate(new Translate(0, _scanLinePos, 0));
            }
        }
    }

    private void OnBackClicked()
    {
        // Change "MainMenu" to your actual main menu scene name if different
        SceneManager.LoadScene("MainMenu");
    }

    private void OnFlashlightClicked()
    {
        _isFlashlightOn = !_isFlashlightOn;
        
        // Use reflection to toggle Vuforia flashlight so it works across different Vuforia versions 
        // without causing compile errors if the namespace changes.
        string[] assemblyNames = new string[]
        {
            "Vuforia.Unity.Engine",
            "VuforiaEngine",
            "VuforiaScripts"
        };

        foreach (string asmName in assemblyNames)
        {
            try 
            {
                System.Type cameraDeviceType = System.Type.GetType("Vuforia.CameraDevice, " + asmName);
                if (cameraDeviceType != null)
                {
                    var instanceProp = cameraDeviceType.GetProperty("Instance");
                    if (instanceProp != null)
                    {
                        var instance = instanceProp.GetValue(null);
                        if (instance != null)
                        {
                            var method = cameraDeviceType.GetMethod("SetFlashTorchMode");
                            if (method != null)
                            {
                                bool success = (bool)method.Invoke(instance, new object[] { _isFlashlightOn });
                                Debug.Log("Flashlight toggled: " + _isFlashlightOn + " Success: " + success);
                                return;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Flashlight toggle attempt failed for assembly '{asmName}': {e.Message}");
            }
        }
        
        Debug.LogWarning("Could not toggle flashlight using reflection. Vuforia API might be missing or changed.");
    }
}
