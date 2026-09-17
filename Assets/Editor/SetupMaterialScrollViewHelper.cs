using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class SetupMaterialScrollViewHelper : EditorWindow
{
    [MenuItem("Helper/Setup Material ScrollView")]
    [MenuItem("CONTEXT/MaterialManager/Setup ScrollView Materi")]
    public static void SetupScrollView()
    {
        // 1. Cari GameObject Materi (parent dari Isi Materi & Foto Soal)
        GameObject materiGo = GameObject.Find("Materi");
        if (materiGo == null)
        {
            // Coba cari melalui MateriHolder
            GameObject materiHolder = GameObject.Find("MateriHolder");
            if (materiHolder != null)
            {
                Transform t = materiHolder.transform.Find("Materi");
                if (t != null) materiGo = t.gameObject;
            }
        }

        if (materiGo == null)
        {
            Debug.LogError("[SetupMaterialScrollViewHelper] Tidak dapat menemukan GameObject 'Materi' di scene!");
            return;
        }

        // 2. Cari Isi Materi
        Transform isiMateriTrans = materiGo.transform.Find("Isi Materi");
        if (isiMateriTrans == null)
        {
            // Coba cari jika sudah berada di dalam Viewport/Content
            Transform existingContent = materiGo.transform.Find("MateriScrollView/Viewport/Content/Isi Materi");
            if (existingContent != null)
            {
                isiMateriTrans = existingContent;
                Debug.Log("[SetupMaterialScrollViewHelper] 'Isi Materi' sudah berada di dalam ScrollView.");
            }
        }

        if (isiMateriTrans == null)
        {
            Debug.LogError("[SetupMaterialScrollViewHelper] Tidak dapat menemukan GameObject 'Isi Materi'!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(materiGo, "Setup Material ScrollView");

        // 3. Buat atau dapatkan MateriScrollView
        GameObject scrollViewGo = null;
        Transform existingSvTrans = materiGo.transform.Find("MateriScrollView");
        if (existingSvTrans != null)
        {
            scrollViewGo = existingSvTrans.gameObject;
        }
        else
        {
            scrollViewGo = new GameObject("MateriScrollView", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(scrollViewGo, "Create MateriScrollView");
            scrollViewGo.transform.SetParent(materiGo.transform, false);

            // Letakkan di posisi index yang sama seperti Isi Materi sebelumnya
            int siblingIdx = isiMateriTrans.GetSiblingIndex();
            scrollViewGo.transform.SetSiblingIndex(siblingIdx);
        }

        // Setup RectTransform & LayoutElement pada MateriScrollView
        RectTransform svRt = scrollViewGo.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0f, 0f);
        svRt.anchorMax = new Vector2(1f, 1f);
        svRt.pivot = new Vector2(0.5f, 0.5f);

        LayoutElement svLe = scrollViewGo.GetComponent<LayoutElement>();
        if (svLe == null) svLe = scrollViewGo.AddComponent<LayoutElement>();
        svLe.preferredWidth = 502f;
        svLe.flexibleWidth = 1f;
        svLe.layoutPriority = 1;

        // Setup ScrollRect
        ScrollRect scrollRect = scrollViewGo.GetComponent<ScrollRect>();
        if (scrollRect == null) scrollRect = scrollViewGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 25f;

        // 4. Buat atau dapatkan Viewport
        Transform vpTrans = scrollViewGo.transform.Find("Viewport");
        GameObject viewportGo;
        if (vpTrans != null)
        {
            viewportGo = vpTrans.gameObject;
        }
        else
        {
            viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            Undo.RegisterCreatedObjectUndo(viewportGo, "Create Viewport");
            viewportGo.transform.SetParent(scrollViewGo.transform, false);
        }

        RectTransform vpRt = viewportGo.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.pivot = new Vector2(0f, 1f);
        vpRt.anchoredPosition = Vector2.zero;
        vpRt.sizeDelta = new Vector2(-16f, 0f); // Sisakan ruang 16px untuk scrollbar

        RectMask2D mask = viewportGo.GetComponent<RectMask2D>();
        if (mask == null) viewportGo.AddComponent<RectMask2D>();

        scrollRect.viewport = vpRt;

        // 5. Buat atau dapatkan Content
        Transform contentTrans = viewportGo.transform.Find("Content");
        GameObject contentGo;
        if (contentTrans != null)
        {
            contentGo = contentTrans.gameObject;
        }
        else
        {
            contentGo = new GameObject("Content", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(contentGo, "Create Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
        }

        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);

        ContentSizeFitter csf = contentGo.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        VerticalLayoutGroup vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(0, 4, 0, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 0f;

        scrollRect.content = contentRt;

        // 6. Masukkan Isi Materi ke dalam Content
        if (isiMateriTrans.parent != contentGo.transform)
        {
            isiMateriTrans.SetParent(contentGo.transform, false);
        }

        // Hapus LayoutElement dari Isi Materi jika ada, karena ukurannya sekarang diatur oleh Content
        LayoutElement oldLe = isiMateriTrans.GetComponent<LayoutElement>();
        if (oldLe != null)
        {
            DestroyImmediate(oldLe);
        }

        // Pastikan TMP Text menggunakan mode Overflow agar tingginya dihitung akurat oleh ContentSizeFitter
        TextMeshProUGUI tmp = isiMateriTrans.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = true;
        }

        RectTransform tmpRt = isiMateriTrans.GetComponent<RectTransform>();
        tmpRt.anchorMin = new Vector2(0f, 1f);
        tmpRt.anchorMax = new Vector2(1f, 1f);
        tmpRt.pivot = new Vector2(0.5f, 1f);

        // 7. Buat Scrollbar Vertical
        Transform sbTrans = scrollViewGo.transform.Find("Scrollbar Vertical");
        GameObject scrollbarGo;
        if (sbTrans != null)
        {
            scrollbarGo = sbTrans.gameObject;
        }
        else
        {
            scrollbarGo = new GameObject("Scrollbar Vertical", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            Undo.RegisterCreatedObjectUndo(scrollbarGo, "Create Scrollbar Vertical");
            scrollbarGo.transform.SetParent(scrollViewGo.transform, false);
        }

        RectTransform sbRt = scrollbarGo.GetComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(1f, 0f);
        sbRt.anchorMax = new Vector2(1f, 1f);
        sbRt.pivot = new Vector2(1f, 1f);
        sbRt.sizeDelta = new Vector2(10f, 0f);
        sbRt.anchoredPosition = Vector2.zero;

        // Cari sprite built-in untuk track dan handle
        Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (defaultSprite == null)
        {
            defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }

        Image bgImg = scrollbarGo.GetComponent<Image>();
        bgImg.sprite = defaultSprite;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.85f, 0.55f, 0.2f, 0.25f); // Warna track lembut tembus pandang selaras warna tema

        // Sliding Area
        Transform saTrans = scrollbarGo.transform.Find("Sliding Area");
        GameObject slidingAreaGo;
        if (saTrans != null)
        {
            slidingAreaGo = saTrans.gameObject;
        }
        else
        {
            slidingAreaGo = new GameObject("Sliding Area", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(slidingAreaGo, "Create Sliding Area");
            slidingAreaGo.transform.SetParent(scrollbarGo.transform, false);
        }

        RectTransform saRt = slidingAreaGo.GetComponent<RectTransform>();
        saRt.anchorMin = Vector2.zero;
        saRt.anchorMax = Vector2.one;
        saRt.sizeDelta = new Vector2(-4f, -4f);
        saRt.anchoredPosition = Vector2.zero;

        // Handle
        Transform hTrans = slidingAreaGo.transform.Find("Handle");
        GameObject handleGo;
        if (hTrans != null)
        {
            handleGo = hTrans.gameObject;
        }
        else
        {
            handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(handleGo, "Create Handle");
            handleGo.transform.SetParent(slidingAreaGo.transform, false);
        }

        RectTransform hRt = handleGo.GetComponent<RectTransform>();
        hRt.anchorMin = Vector2.zero;
        hRt.anchorMax = Vector2.one;
        hRt.sizeDelta = new Vector2(4f, 4f);
        hRt.anchoredPosition = Vector2.zero;

        Image handleImg = handleGo.GetComponent<Image>();
        handleImg.sprite = defaultSprite;
        handleImg.type = Image.Type.Sliced;
        handleImg.color = new Color(0.88f, 0.45f, 0.12f, 0.85f); // Warna handle oranye cerah elegan

        Scrollbar sb = scrollbarGo.GetComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        sb.handleRect = hRt;
        sb.targetGraphic = handleImg;

        scrollRect.verticalScrollbar = sb;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 4f;

        // 8. Sambungkan referensi ke MaterialManager
        MaterialManager mm = Object.FindFirstObjectByType<MaterialManager>();
        if (mm != null)
        {
            Undo.RecordObject(mm, "Update MaterialManager References");
            mm.contentText = tmp;
            mm.contentScrollRect = scrollRect;
            EditorUtility.SetDirty(mm);
            Debug.Log("[SetupMaterialScrollViewHelper] Berhasil menghubungkan ScrollRect dan contentText ke MaterialManager!");
        }
        else
        {
            Debug.LogWarning("[SetupMaterialScrollViewHelper] MaterialManager tidak ditemukan di scene.");
        }

        // 9. Simpan scene
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = scrollViewGo;

        Debug.Log("<color=green>[SetupMaterialScrollViewHelper] Setup ScrollView Materi BERHASIL!</color> Text materi sekarang bisa discroll dan memiliki scrollbar rapi.");
    }
}
