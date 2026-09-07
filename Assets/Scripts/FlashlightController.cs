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

        // Jika testing di Editor, catat bahwa webcam PC biasanya tidak support flashlight
#if UNITY_EDITOR
        Debug.Log("[FlashlightController] (Info) Sedang testing di Editor. Sebagian besar webcam PC TIDAK memiliki flashlight, namun logic script tetap berjalan.");
#endif

        // Menggunakan reflection agar tidak error meskipun versi Vuforia berbeda
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
                                bool success = (bool)method.Invoke(instance, new object[] { turnOn });
                                Debug.Log($"[FlashlightController] Flashlight berhasil dieksekusi via Vuforia API. Status: {(turnOn ? "ON" : "OFF")} | Success (didukung oleh hardware): {success}");
                                return; // Keluar dari loop jika berhasil
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
        
        Debug.LogWarning("[FlashlightController] Gagal menyalakan/mematikan flashlight. Pastikan kamera Vuforia sudah aktif di Scene AR ini.");
    }
}
