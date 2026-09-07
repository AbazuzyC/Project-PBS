using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPagination : MonoBehaviour
{
    [Header("Page Settings")]
    [Tooltip("Masukkan GameObject 'Page' yang berisi frame kuis ke sini secara berurutan.")]
    public RectTransform[] pages;
    
    [Header("UI Buttons")]
    public Button nextButton;
    public Button prevButton;
    
    [Header("Animation Settings")]
    public float transitionDuration = 0.5f;
    [Tooltip("Jarak pergeseran antar halaman (misal selebar layar/canvas)")]
    public float pageSpacing = 1500f; 

    private int currentPageIndex = 0;

    void Start()
    {
        // Setup posisi awal setiap page
        for (int i = 0; i < pages.Length; i++)
        {
            if (i == 0)
            {
                // Page pertama ada di tengah
                pages[i].anchoredPosition = Vector2.zero;
            }
            else
            {
                // Page lainnya ditaruh di sebelah kanan (luar layar)
                pages[i].anchoredPosition = new Vector2(pageSpacing, 0);
            }
        }
        
        UpdateButtonStates();

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            // Geser page saat ini ke sebelah kiri
            pages[currentPageIndex].DOAnchorPosX(-pageSpacing, transitionDuration).SetEase(Ease.InOutQuad);
            
            currentPageIndex++;
            
            // Posisikan page baru di kanan sebelum di-slide ke tengah
            pages[currentPageIndex].anchoredPosition = new Vector2(pageSpacing, 0); 
            // Geser page baru ke tengah
            pages[currentPageIndex].DOAnchorPosX(0, transitionDuration).SetEase(Ease.InOutQuad);
            
            UpdateButtonStates();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            // Geser page saat ini ke sebelah kanan
            pages[currentPageIndex].DOAnchorPosX(pageSpacing, transitionDuration).SetEase(Ease.InOutQuad);
            
            currentPageIndex--;
            
            // Posisikan page baru di kiri sebelum di-slide ke tengah
            pages[currentPageIndex].anchoredPosition = new Vector2(-pageSpacing, 0); 
            // Geser page baru ke tengah
            pages[currentPageIndex].DOAnchorPosX(0, transitionDuration).SetEase(Ease.InOutQuad);
            
            UpdateButtonStates();
        }
    }

    private void UpdateButtonStates()
    {
        // Matikan tombol Prev jika di page pertama, dan Next jika di page terakhir
        if (prevButton != null) prevButton.interactable = currentPageIndex > 0;
        if (nextButton != null) nextButton.interactable = currentPageIndex < pages.Length - 1;
    }
}
