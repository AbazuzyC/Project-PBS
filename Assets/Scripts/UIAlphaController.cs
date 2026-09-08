using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIAlphaController : MonoBehaviour
{
    [Tooltip("The Image component you want to make transparent")]
    public Image targetImage;

    [Tooltip("The 3D Object(s) to check. Drag your 3D leaf/butterfly models here.")]
    public Transform[] target3DObjects;

    [Tooltip("Alpha value when the 3D object is behind the panel")]
    [Range(0f, 1f)]
    public float transparentAlpha = 0.5f;

    [Tooltip("Alpha value when it is opaque (normal)")]
    [Range(0f, 1f)]
    public float opaqueAlpha = 1f;

    [Tooltip("Speed of the fade transition")]
    public float fadeSpeed = 10f;

    private RectTransform panelRect;
    private Camera mainCamera;
    private Canvas parentCanvas;

    private void Start()
    {
        panelRect = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (targetImage == null || panelRect == null || mainCamera == null)
            return;

        bool isBehind = false;

        if (target3DObjects != null && target3DObjects.Length > 0)
        {
            Camera uiCamera = null;
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = parentCanvas.worldCamera;
            }

            foreach (var obj in target3DObjects)
            {
                if (obj != null && obj.gameObject.activeInHierarchy)
                {
                    // Convert 3D object world position to screen position
                    Vector3 screenPoint = mainCamera.WorldToScreenPoint(obj.position);

                    // screenPoint.z > 0 means the object is in front of the AR camera (not behind the user)
                    if (screenPoint.z > 0)
                    {
                        // Check if the screen position is inside the UI Panel
                        if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, screenPoint, uiCamera))
                        {
                            isBehind = true;
                            break;
                        }
                    }
                }
            }
        }

        // Apply smooth transition for the alpha
        Color color = targetImage.color;
        float targetAlpha = isBehind ? transparentAlpha : opaqueAlpha;
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);
        targetImage.color = color;
    }
}
