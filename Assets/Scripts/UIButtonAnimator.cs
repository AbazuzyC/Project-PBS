using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animation Settings")]
    [Tooltip("Seberapa kecil/besar tombol saat ditekan (misal 0.9 berarti mengecil 10%)")]
    public float scaleTo = 0.9f;
    [Tooltip("Durasi animasi (detik)")]
    public float duration = 0.15f;
    public Ease easeType = Ease.OutQuad;

    [Header("Audio Settings")]
    [Tooltip("Putar suara klik otomatis dari AudioManager saat tombol ditekan")]
    public bool playClickSound = true;

    private Vector3 originalScale;

    private void Awake()
    {
        // Simpan ukuran asli saat game dimulai
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        // Kembalikan ke ukuran semula jika objek dinonaktifkan saat animasi berjalan
        transform.DOKill();
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playClickSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        // Hentikan animasi sebelumnya agar tidak bertumpuk
        transform.DOKill();
        // Animasi mengecil
        transform.DOScale(originalScale * scaleTo, duration).SetEase(easeType).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        // Animasi kembali ke ukuran normal dengan efek membal (OutBack) biar lebih terasa 'juicy'
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true);
    }
}
