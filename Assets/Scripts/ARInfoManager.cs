using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ARInfoManager : MonoBehaviour
{
    [Header("Main Panel")]
    [Tooltip("Panel utama ARInformation yang akan muncul/hilang saat target terscan")]
    public GameObject arInfoPanel;

    [Header("UI References")]
    public TextMeshProUGUI pageTitleText;  // Teks subjudul (contoh: Tahap 1)
    public TextMeshProUGUI contentText;    // Teks isi materi

    [Header("Button References")]
    public Button prevButton;
    public Button nextButton;

    private TopicCategorySO currentTopic;
    private int currentPageIndex = 0;

    private void Start()
    {
        // Pastikan panel mati di awal sebelum marker terscan
        if (arInfoPanel != null)
        {
            arInfoPanel.SetActive(false);
        }

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
    }

    /// <summary>
    /// Panggil fungsi ini dari event OnTargetFound milik ImageTarget di Vuforia.
    /// Jangan lupa assign TopicCategorySO yang sesuai di Inspector event-nya.
    /// </summary>
    public void ShowARInfo(TopicCategorySO topic)
    {
        if (topic == null || topic.pages.Count == 0) return;

        currentTopic = topic;
        currentPageIndex = 0;

        if (arInfoPanel != null)
        {
            arInfoPanel.SetActive(true);
            
            // Pop out animation with DOTween
            arInfoPanel.transform.DOKill(); // Hentikan animasi sebelumnya jika ada
            arInfoPanel.transform.localScale = Vector3.zero;
            arInfoPanel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

        UpdatePageUI();
    }

    /// <summary>
    /// Panggil fungsi ini dari event OnTargetLost milik ImageTarget di Vuforia.
    /// </summary>
    public void HideARInfo()
    {
        currentTopic = null;
        
        if (arInfoPanel != null)
        {
            // Shrink animation with DOTween sebelum di non-aktifkan
            arInfoPanel.transform.DOKill();
            arInfoPanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => 
            {
                arInfoPanel.SetActive(false);
            });
        }
    }

    private void UpdatePageUI()
    {
        if (currentTopic == null || currentTopic.pages.Count == 0) return;

        PageData pData = currentTopic.pages[currentPageIndex];

        // Update Teks
        if (pageTitleText != null) pageTitleText.text = pData.pageTitle;
        if (contentText != null) contentText.text = pData.contentText;

        // Logic Tombol Navigasi
        if (prevButton != null) prevButton.interactable = currentPageIndex > 0;
        if (nextButton != null) nextButton.interactable = currentPageIndex < currentTopic.pages.Count - 1;
    }

    public void NextPage()
    {
        if (currentTopic != null && currentPageIndex < currentTopic.pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageUI();
        }
    }

    public void PrevPage()
    {
        if (currentTopic != null && currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageUI();
        }
    }
}
