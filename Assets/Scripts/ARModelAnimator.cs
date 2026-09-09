using UnityEngine;
using DG.Tweening;

public class ARModelAnimator : MonoBehaviour
{
    [Tooltip("Drag 3D model Anda ke sini (biasanya child dari ImageTarget)")]
    public Transform modelTransform;
    
    public float popOutDuration = 0.5f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (modelTransform != null)
        {
            // Simpan ukuran asli model
            originalScale = modelTransform.localScale;
            
            // Set ukuran ke 0 di awal supaya tidak tiba-tiba muncul ("pop")
            modelTransform.localScale = Vector3.zero; 
        }
    }

    /// <summary>
    /// Panggil fungsi ini di dalam event "On Target Found ()" milik Image Target
    /// </summary>
    public void PopOut()
    {
        if (modelTransform != null)
        {
            modelTransform.DOKill(); // Hentikan animasi sebelumnya (jika ada)
            modelTransform.localScale = Vector3.zero;
            
            // Animasi membesar ke ukuran aslinya
            modelTransform.DOScale(originalScale, popOutDuration).SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// Panggil fungsi ini di dalam event "On Target Lost ()" milik Image Target
    /// </summary>
    public void Shrink()
    {
        if (modelTransform != null)
        {
            modelTransform.DOKill();
            
            // Animasi mengecil menjadi 0 saat marker hilang
            modelTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
        }
    }
}
