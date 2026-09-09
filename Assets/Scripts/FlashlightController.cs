using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the FlashButton (Toggle) here")]
    public Toggle flashToggle;

    private void Start()
    {
        // Jika toggle sudah di-assign di Inspector, tambahkan listener
        if (flashToggle != null)
        {
            flashToggle.onValueChanged.AddListener(OnFlashToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (flashToggle != null)
        {
            flashToggle.onValueChanged.RemoveListener(OnFlashToggleChanged);
        }
    }

    /// <summary>
    /// Dipanggil otomatis saat status Toggle berubah (On / Off)
    /// </summary>
    public void OnFlashToggleChanged(bool isOn)
    {
        SetFlashlight(isOn);
    }

    /// <summary>
    /// Logika utama untuk menyalakan/mematikan Flashlight Vuforia
    /// </summary>
    private void SetFlashlight(bool turnOn)
    {
        Debug.Log($"[FlashlightController] Menerima perintah untuk: {(turnOn ? "MENYALAKAN" : "MEMATIKAN")} Flashlight");

#if UNITY_EDITOR
        Debug.Log("[FlashlightController] (Info) Sedang testing di Editor. Sebagian besar webcam PC TIDAK memiliki flashlight, namun logic script tetap berjalan.");
#endif

        string[] assemblyNames = new string[]
        {
            "VuforiaEngine",
            "VuforiaScripts",
            "Vuforia.Unity.Engine"
        };

        foreach (string asmName in assemblyNames)
        {
            try 
            {
                // Coba API Vuforia 10+ (VuforiaBehaviour.Instance.CameraDevice.SetFlash)
                System.Type behaviourType = System.Type.GetType("Vuforia.VuforiaBehaviour, " + asmName);
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
                                        Debug.Log($"[FlashlightController] Flashlight berhasil via Vuforia 10+ API. Status: {(turnOn ? "ON" : "OFF")} | Success: {success}");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                // Fallback: Coba API Vuforia lama (CameraDevice.Instance.SetFlashTorchMode)
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
                                bool success = (bool)method.Invoke(instance, new object[] { turnOn });
                                Debug.Log($"[FlashlightController] Flashlight berhasil dieksekusi via Vuforia Legacy API. Status: {(turnOn ? "ON" : "OFF")} | Success: {success}");
                                return; 
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FlashlightController] Percobaan via assembly '{asmName}' gagal: {e.Message}");
            }
        }
        
        Debug.LogWarning("[FlashlightController] Gagal menyalakan/mematikan flashlight. Pastikan Vuforia sudah jalan dan API-nya sesuai.");
    }
}
