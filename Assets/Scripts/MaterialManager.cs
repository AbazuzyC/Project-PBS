using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialManager : MonoBehaviour 
{
    [Header("UI References")]
    public TextMeshProUGUI mainTitleText;  // Teks paling atas: "MATERI KUPU KUPU"
    public TextMeshProUGUI pageTitleText;  // Teks pita: "TAHAP 1: TELUR"
    public TextMeshProUGUI contentText;    // Teks paragraf materi
    public Image pageImage;                // Gambar materi
    public TextMeshProUGUI pageCounterText;// Teks bawah: "1 DARI 4"
    
    [Header("Button References")]
    public Button prevButton;
    public Button nextButton;

    private TopicCategorySO currentTopic;
    private int currentPageIndex = 0;

    // Panggil fungsi ini dari tombol pilihan topik (Kupu Kupu, Lalat, dll)
    public void StartMaterial(TopicCategorySO selectedTopic) 
    {
        currentTopic = selectedTopic;
        currentPageIndex = 0;
        
        mainTitleText.text = currentTopic.topicName;
        UpdatePageUI();
    }

    private void UpdatePageUI() 
    {
        PageData pData = currentTopic.pages[currentPageIndex];
        
        // Update Teks
        pageTitleText.text = pData.pageTitle;
        contentText.text = pData.contentText;
        pageCounterText.text = (currentPageIndex + 1) + " DARI " + currentTopic.pages.Count;

        // Logic Gambar vs Tanpa Gambar
        if (pData.pageImage != null) 
        {
            pageImage.sprite = pData.pageImage;
            pageImage.gameObject.SetActive(true);
        } 
        else 
        {
            // Jika gambar tidak diisi di Inspector, matikan objek gambarnya.
            // (Pastikan lu pakai Horizontal Layout Group di Parent UI-nya biar teks otomatis melebar saat gambar mati)
            pageImage.gameObject.SetActive(false);
        }

        // Logic Tombol Next & Prev
        prevButton.interactable = currentPageIndex > 0;
        nextButton.interactable = currentPageIndex < currentTopic.pages.Count - 1;
    }

    // Sambungkan ke event OnClick tombol >
    public void NextPage() 
    {
        if (currentPageIndex < currentTopic.pages.Count - 1) 
        {
            currentPageIndex++;
            UpdatePageUI();
        }
    }

    // Sambungkan ke event OnClick tombol <
    public void PrevPage() 
    {
        if (currentPageIndex > 0) 
        {
            currentPageIndex--;
            UpdatePageUI();
        }
    }
}