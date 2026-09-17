using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Reflection;

public class ARCameraUIController : MonoBehaviour
{
    private UIDocument _uiDocument;
    
    // UI Toolkit Elements
    private Button _backButton;
    private Button _flashlightButton;
    private VisualElement _scanLine;

    // UGUI Fallback Elements
    private UnityEngine.UI.Toggle _uguiFlashToggle;
    private UnityEngine.UI.Button _uguiBackButton;

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
            _uiDocument = GetComponentInChildren<UIDocument>();
        }

        if (_uiDocument != null)
        {
            var root = _uiDocument.rootVisualElement;
            if (root != null)
            {
                _backButton = root.Q<Button>("BackButton");
                _flashlightButton = root.Q<Button>("FlashlightButton");
                _scanLine = root.Q<VisualElement>("ScanLine");

                if (_backButton != null) _backButton.clicked += OnBackClicked;
                if (_flashlightButton != null) _flashlightButton.clicked += OnFlashlightClicked;
            }
        }
        else
        {
            // Scene uses standard Unity Canvas (UGUI) instead of UI Toolkit
            SetupUGUIFallbacks();
        }
    }

    private void SetupUGUIFallbacks()
    {
        // Try finding UGUI FlashButton / Toggle in the scene
        var toggles = FindObjectsByType<UnityEngine.UI.Toggle>(FindObjectsSortMode.None);
        foreach (var t in toggles)
        {
            if (t.gameObject.name.ToLower().Contains("flash"))
            {
                _uguiFlashToggle = t;
                _uguiFlashToggle.onValueChanged.RemoveListener(OnUGUIFlashToggleChanged);
                _uguiFlashToggle.onValueChanged.AddListener(OnUGUIFlashToggleChanged);
                break;
            }
        }

        // Try finding UGUI BackButton in the scene if not already handled
        var buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b.gameObject.name.ToLower().Contains("back"))
            {
                _uguiBackButton = b;
                _uguiBackButton.onClick.RemoveListener(OnBackClicked);
                _uguiBackButton.onClick.AddListener(OnBackClicked);
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (_backButton != null) _backButton.clicked -= OnBackClicked;
        if (_flashlightButton != null) _flashlightButton.clicked -= OnFlashlightClicked;

        if (_uguiFlashToggle != null)
        {
            _uguiFlashToggle.onValueChanged.RemoveListener(OnUGUIFlashToggleChanged);
        }
        if (_uguiBackButton != null)
        {
            _uguiBackButton.onClick.RemoveListener(OnBackClicked);
        }
    }

    private System.Collections.IEnumerator Start()
    {
        // Wait one frame for the scene to be fully loaded
        yield return null;

        EnsureVuforiaRunning();
    }

    private void EnsureVuforiaRunning()
    {
        // Vuforia is automatically initialized by VuforiaConfiguration / VuforiaEditorAutoInit.
        // We only ensure the VuforiaBehaviour on the ARCamera is enabled if present.
        try
        {
            System.Type vuforiaBehaviourType = FindVuforiaType("Vuforia.VuforiaBehaviour");
            if (vuforiaBehaviourType != null)
            {
                var vuforiaBehaviour = FindFirstObjectByType(vuforiaBehaviourType) as Behaviour;
                if (vuforiaBehaviour != null)
                {
                    if (!vuforiaBehaviour.enabled)
                    {
                        vuforiaBehaviour.enabled = true;
                        Debug.Log("ARCameraUIController: VuforiaBehaviour found and enabled.");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("ARCameraUIController: EnsureVuforiaRunning failed: " + e.Message);
        }
    }

    private void Update()
    {
        // Animate the UI Toolkit Scan Line moving up and down if present
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
        SceneManager.LoadScene("MainMenu");
    }

    private void OnUGUIFlashToggleChanged(bool isOn)
    {
        _isFlashlightOn = isOn;
        SetFlashlight(_isFlashlightOn);
    }

    private void OnFlashlightClicked()
    {
        _isFlashlightOn = !_isFlashlightOn;
        SetFlashlight(_isFlashlightOn);
    }

    private void SetFlashlight(bool turnOn)
    {
        // 1. Try Vuforia 10+ CameraDevice via VuforiaBehaviour
        try
        {
            System.Type behaviourType = FindVuforiaType("Vuforia.VuforiaBehaviour");
            if (behaviourType != null)
            {
                var instanceProp = behaviourType.GetProperty("Instance");
                if (instanceProp != null)
                {
                    var behaviourInstance = instanceProp.GetValue(null);
                    if (behaviourInstance != null)
                    {
                        var cameraDeviceProp = behaviourType.GetProperty("CameraDevice");
                        if (cameraDeviceProp != null)
                        {
                            var cameraDeviceInstance = cameraDeviceProp.GetValue(behaviourInstance);
                            if (cameraDeviceInstance != null)
                            {
                                var setFlashMethod = cameraDeviceInstance.GetType().GetMethod("SetFlash", new System.Type[] { typeof(bool) });
                                if (setFlashMethod != null)
                                {
                                    bool success = (bool)setFlashMethod.Invoke(cameraDeviceInstance, new object[] { turnOn });
                                    Debug.Log($"[ARCameraUIController] Flashlight set: {turnOn}, success: {success}");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ARCameraUIController] SetFlash error: {ex.Message}");
        }

        // 2. Fallback: Legacy Vuforia CameraDevice.Instance.SetFlashTorchMode
        try
        {
            System.Type cameraDeviceType = FindVuforiaType("Vuforia.CameraDevice");
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
                            bool success = (bool)method.Invoke(instance, new object[] { turnOn });
                            Debug.Log($"[ARCameraUIController] Legacy flashlight set: {turnOn}, success: {success}");
                            return;
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ARCameraUIController] Legacy torch error: {ex.Message}");
        }

        Debug.Log("[ARCameraUIController] Flashlight mode toggled (webcam/editor simulation).");
    }

    private static System.Type FindVuforiaType(string typeFullName)
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
