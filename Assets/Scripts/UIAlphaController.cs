using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

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
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();

        parentCanvas = GetComponentInParent<Canvas>();

        AutoDiscoverTargetsIfEmpty();
    }

    private void AutoDiscoverTargetsIfEmpty()
    {
        if (target3DObjects == null || target3DObjects.Length == 0)
        {
            var animators = FindObjectsByType<ARModelAnimator>(FindObjectsSortMode.None);
            if (animators != null && animators.Length > 0)
            {
                target3DObjects = new Transform[animators.Length];
                for (int i = 0; i < animators.Length; i++)
                {
                    target3DObjects[i] = animators[i].transform;
                }
            }
        }
    }

    private void Update()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (targetImage == null || panelRect == null || mainCamera == null)
            return;

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        Camera uiCamera = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        bool isBehind = false;

        if (target3DObjects != null && target3DObjects.Length > 0)
        {
            foreach (var obj in target3DObjects)
            {
                if (IsObjectBehindPanel(obj, panelRect, mainCamera, uiCamera))
                {
                    isBehind = true;
                    break;
                }
            }
        }

        // Apply smooth transition for the alpha
        Color color = targetImage.color;
        float targetAlpha = isBehind ? transparentAlpha : opaqueAlpha;
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);
        if (Mathf.Abs(color.a - targetAlpha) < 0.002f)
        {
            color.a = targetAlpha;
        }
        targetImage.color = color;
    }

    private bool IsObjectBehindPanel(Transform obj, RectTransform panel, Camera cam, Camera uiCam)
    {
        if (obj == null || !obj.gameObject.activeInHierarchy)
            return false;

        // 1. Check world scale (if shrunk to zero, it is hidden)
        if (obj.lossyScale.sqrMagnitude < 0.0001f)
            return false;

        // 2. Check ARModelAnimator (if present and shrunk)
        ARModelAnimator animator = obj.GetComponentInParent<ARModelAnimator>();
        if (animator == null) animator = obj.GetComponentInChildren<ARModelAnimator>();
        if (animator != null && animator.modelTransform != null)
        {
            if (animator.modelTransform.localScale.sqrMagnitude < 0.0001f)
                return false;
        }

        // 3. Check Vuforia Observer tracking status
        ObserverBehaviour observer = obj.GetComponentInParent<ObserverBehaviour>();
        if (observer == null) observer = obj.GetComponentInChildren<ObserverBehaviour>();
        if (observer != null)
        {
            var status = observer.TargetStatus.Status;
            if (status != Status.TRACKED && status != Status.EXTENDED_TRACKED)
                return false;
        }

        // 4. Check Renderers
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        bool hasVisibleRenderer = false;
        Bounds combinedBounds = new Bounds();
        bool boundsInit = false;

        foreach (var r in renderers)
        {
            if (r != null && r.enabled && r.gameObject.activeInHierarchy)
            {
                hasVisibleRenderer = true;
                if (!boundsInit)
                {
                    combinedBounds = r.bounds;
                    boundsInit = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }
        }

        // If there are renderers attached, at least one must be actively enabled
        if (renderers.Length > 0 && !hasVisibleRenderer)
            return false;

        // 5. Check if the object is in front of the camera
        Vector3 centerWorld = boundsInit ? combinedBounds.center : obj.position;
        Vector3 centerScreen = cam.WorldToScreenPoint(centerWorld);
        if (centerScreen.z <= 0 || centerScreen.z > cam.farClipPlane)
            return false;

        // Check if the center is directly inside the panel
        if (RectTransformUtility.RectangleContainsScreenPoint(panel, centerScreen, uiCam))
            return true;

        // If bounds available, check screen-space bounding box overlap
        if (boundsInit)
        {
            Vector3 min = combinedBounds.min;
            Vector3 max = combinedBounds.max;
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            int inFrontCount = 0;

            for (int i = 0; i < 8; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(corners[i]);
                if (sp.z > 0)
                {
                    inFrontCount++;
                    if (RectTransformUtility.RectangleContainsScreenPoint(panel, sp, uiCam))
                        return true;

                    if (sp.x < minX) minX = sp.x;
                    if (sp.x > maxX) maxX = sp.x;
                    if (sp.y < minY) minY = sp.y;
                    if (sp.y > maxY) maxY = sp.y;
                }
            }

            if (inFrontCount > 0)
            {
                Vector3[] panelCorners = new Vector3[4];
                panel.GetWorldCorners(panelCorners);

                Vector2 panelMin = panelCorners[0];
                Vector2 panelMax = panelCorners[2];
                if (uiCam != null)
                {
                    panelMin = RectTransformUtility.WorldToScreenPoint(uiCam, panelCorners[0]);
                    panelMax = RectTransformUtility.WorldToScreenPoint(uiCam, panelCorners[2]);
                }

                Rect panelScreenRect = Rect.MinMaxRect(
                    Mathf.Min(panelMin.x, panelMax.x),
                    Mathf.Min(panelMin.y, panelMax.y),
                    Mathf.Max(panelMin.x, panelMax.x),
                    Mathf.Max(panelMin.y, panelMax.y)
                );

                Rect objScreenRect = Rect.MinMaxRect(minX, minY, maxX, maxY);

                return panelScreenRect.Overlaps(objScreenRect);
            }
        }

        return false;
    }
}
