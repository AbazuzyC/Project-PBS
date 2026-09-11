using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Tambahkan namespace DOTween

public class QuizManager : MonoBehaviour 
{
    [Header("UI References")]
    public TextMeshProUGUI questionNumberTopText; // Untuk teks "SOAL 1" di atas
    public TextMeshProUGUI questionCounterText;   // Untuk teks "1 / 10" di bawah
    public TextMeshProUGUI questionText;
    public Image questionImage;
    
    [Header("Quiz Confirmation UI")]
    public GameObject quizConfirmationPanel; // Panel pop-up konfirmasi
    public TextMeshProUGUI textTotalSoalJawab; // Teks "8/10" di konfirmasi
    
    [Header("Finished Quiz UI")]
    public GameObject finishedQuizPanel; // Panel FinishedQuiz
    public TextMeshProUGUI textSkor;
    public TextMeshProUGUI textAfirmasi;

    [Header("Panel Navigation")]
    public GameObject quizSelectedPanel; // GameObject "QuizSelected"
    public GameObject changeQuizLevelPanel; // GameObject "ChangeQuizLevel"
    public QuizScriptable[] allQuizzesList; // Untuk fitur tombol Next Quiz

    [Header("Options References")]
    public TextMeshProUGUI[] optionTexts; // Teks jawaban A, B, C, D
    public Button[] optionButtons;        // Button komponen dari opsi A, B, C, D
    public Image[] optionImages;          // Image komponen dari opsi A, B, C, D (untuk ganti warna/sprite)

    [Header("Sprites Feedback")]
    public Sprite defaultOptionSprite;
    public Sprite correctOptionSprite;
    public Sprite wrongOptionSprite;
    
    [Header("Next Button Feedback")]
    public Sprite nextButtonSprite;
    public Sprite doneButtonSprite;

    [Header("Button References")]
    public Button prevButton;
    public Button nextButton;

    private QuizScriptable currentQuiz;
    private int currentQuestionIndex = 0;
    private int[] playerAnswers; // Array untuk menyimpan jawaban player. -1 = belum dijawab
    private Vector3[] originalOptionScales; // Untuk menyimpan ukuran asli tombol
    private CanvasGroup[] optionCanvasGroups; // Untuk mengatur opacity tombol + textnya
    private GameObject[] optionLetterObjects; // Referensi ke GameObject teks A, B, C, D
    private Vector3 originalConfirmationScale = Vector3.one; // Ukuran asli frame konfirmasi
    private Vector3 originalFinishedScale = Vector3.one; // Ukuran asli frame FinishedQuiz

    private void Awake()
    {
        // Simpan scale asli panel konfirmasi kalau ada
        if (quizConfirmationPanel != null)
        {
            Transform frame = quizConfirmationPanel.transform.Find("Frame");
            if (frame != null)
                originalConfirmationScale = frame.localScale;
            else
                originalConfirmationScale = quizConfirmationPanel.transform.localScale;
        }

        // Simpan scale asli panel finished quiz kalau ada
        if (finishedQuizPanel != null)
        {
            Transform frame = finishedQuizPanel.transform.Find("Frame");
            if (frame != null)
                originalFinishedScale = frame.localScale;
            else
                originalFinishedScale = finishedQuizPanel.transform.localScale;
        }

        // Simpan ukuran asli (scale) dari setiap tombol opsi saat game mulai
        // Dan siapkan CanvasGroup agar opacity text + gambar bisa diatur barengan
        if (optionButtons != null)
        {
            originalOptionScales = new Vector3[optionButtons.Length];
            optionCanvasGroups = new CanvasGroup[optionButtons.Length];
            optionLetterObjects = new GameObject[optionButtons.Length];

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null)
                {
                    originalOptionScales[i] = optionButtons[i].transform.localScale;

                    // Tambahkan CanvasGroup jika belum ada
                    CanvasGroup cg = optionButtons[i].GetComponent<CanvasGroup>();
                    if (cg == null)
                    {
                        cg = optionButtons[i].gameObject.AddComponent<CanvasGroup>();
                    }
                    optionCanvasGroups[i] = cg;

                    // Timpa warna "Disabled" bawaan Unity Button supaya tidak otomatis jadi abu-abu/transparan sendiri
                    ColorBlock cb = optionButtons[i].colors;
                    cb.disabledColor = Color.white; // Putih = warna asli sprite
                    optionButtons[i].colors = cb;

                    // Cari teks huruf A, B, C, D secara otomatis (cari TextMeshProUGUI yg bukan teks jawaban utama)
                    TextMeshProUGUI[] childTexts = optionButtons[i].GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var t in childTexts)
                    {
                        if (optionTexts != null && i < optionTexts.Length && t != optionTexts[i])
                        {
                            optionLetterObjects[i] = t.gameObject;
                            break;
                        }
                    }
                }
            }
        }
    }

    public void StartQuiz(QuizScriptable selectedQuiz) 
    {
        currentQuiz = selectedQuiz;
        currentQuestionIndex = 0;
        
        // Inisialisasi status jawaban (-1 menandakan belum dijawab)
        if (currentQuiz != null && currentQuiz.questions != null)
        {
            playerAnswers = new int[currentQuiz.questions.Count];
            for (int i = 0; i < playerAnswers.Length; i++)
            {
                playerAnswers[i] = -1;
            }
        }

        UpdateQuestionUI();
    }

    // Fungsi ini dipisah biar gampang dipanggil pas Next/Prev
    private void UpdateQuestionUI() 
    {
        if (currentQuiz == null || currentQuiz.questions == null || currentQuiz.questions.Count == 0) return;

        QuestionData qData = currentQuiz.questions[currentQuestionIndex];
        
        // Update teks soal dan indikator nomor
        if (questionNumberTopText != null) questionNumberTopText.text = "SOAL " + (currentQuestionIndex + 1);
        if (questionCounterText != null) questionCounterText.text = (currentQuestionIndex + 1) + " / " + currentQuiz.questions.Count;
        if (questionText != null) questionText.text = qData.questionText;
        
        // Atur gambar
        if (qData.questionImage != null && questionImage != null) {
            questionImage.sprite = qData.questionImage;
            
            // Pastikan gambar tidak stretch
            questionImage.preserveAspect = true;
            
            // Hitung ukuran agar maksimal 200x100 dan rasio tetap terjaga
            float originalWidth = qData.questionImage.rect.width;
            float originalHeight = qData.questionImage.rect.height;
            float ratio = originalWidth / originalHeight;

            float maxWidth = 200f;
            float maxHeight = 100f;
            
            float targetWidth = maxWidth;
            float targetHeight = targetWidth / ratio;

            if (targetHeight > maxHeight)
            {
                targetHeight = maxHeight;
                targetWidth = targetHeight * ratio;
            }

            questionImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

            questionImage.gameObject.SetActive(true);
        } else if (questionImage != null) {
            questionImage.gameObject.SetActive(false);
        }

        int answeredIndex = playerAnswers != null && currentQuestionIndex < playerAnswers.Length ? playerAnswers[currentQuestionIndex] : -1;

        // Update opsi jawaban
        for (int i = 0; i < optionTexts.Length; i++) {
            if (optionTexts[i] != null && qData.options.Length > i)
                optionTexts[i].text = qData.options[i];

            if (i < optionButtons.Length && optionButtons[i] != null)
            {
                // Pastikan tidak ada animasi berjalan sisa soal sblmnya
                optionButtons[i].transform.DOKill(true);
                
                // Kembalikan ke scale aslinya (bukan Vector3.one, supaya tidak mengecil)
                if (originalOptionScales != null && i < originalOptionScales.Length)
                {
                    optionButtons[i].transform.localScale = originalOptionScales[i];
                }

                // Jika soal INI belum dijawab
                if (answeredIndex == -1)
                {
                    optionButtons[i].interactable = true;
                    if (optionCanvasGroups != null && i < optionCanvasGroups.Length && optionCanvasGroups[i] != null)
                        optionCanvasGroups[i].alpha = 1.0f; // Opacity 100%

                    if (i < optionImages.Length && optionImages[i] != null && defaultOptionSprite != null)
                        optionImages[i].sprite = defaultOptionSprite;

                    // Munculkan kembali huruf (A, B, C, D)
                    if (optionLetterObjects != null && i < optionLetterObjects.Length && optionLetterObjects[i] != null)
                        optionLetterObjects[i].SetActive(true);
                }
                // Jika soal INI sudah dijawab
                else
                {
                    optionButtons[i].interactable = false; // Disable klik
                    
                    if (optionCanvasGroups != null && i < optionCanvasGroups.Length && optionCanvasGroups[i] != null)
                        optionCanvasGroups[i].alpha = 0.9f; // Opacity 90% supaya gak terlalu invisible
                    
                    if (i < optionImages.Length && optionImages[i] != null)
                    {
                        // Default state
                        optionImages[i].sprite = defaultOptionSprite;
                        bool shouldHideLetter = false;

                        // Jika ini adalah opsi yang dipilih player
                        if (i == answeredIndex)
                        {
                            if (answeredIndex == qData.correctAnswerIndex)
                                optionImages[i].sprite = correctOptionSprite;
                            else
                                optionImages[i].sprite = wrongOptionSprite;
                                
                            shouldHideLetter = true;
                        }
                        
                        // Opsional: Tunjukin jawaban yang benar walaupun dia salah
                        if (i == qData.correctAnswerIndex)
                        {
                            optionImages[i].sprite = correctOptionSprite;
                            shouldHideLetter = true;
                        }

                        // Sembunyikan huruf (A, B, C, D) jika gambar berubah jadi benar/salah
                        if (optionLetterObjects != null && i < optionLetterObjects.Length && optionLetterObjects[i] != null)
                        {
                            optionLetterObjects[i].SetActive(!shouldHideLetter);
                        }
                    }
                }
            }
        }

        // LOGIC TOMBOL PREV & NEXT
        if (prevButton != null) prevButton.interactable = currentQuestionIndex > 0;
        
        if (nextButton != null) 
        {
            bool isLastQuestion = currentQuestionIndex >= currentQuiz.questions.Count - 1;
            
            // Ganti sprite ke 'Done' kalo soal terakhir
            if (nextButton.image != null)
            {
                if (isLastQuestion && doneButtonSprite != null)
                    nextButton.image.sprite = doneButtonSprite;
                else if (!isLastQuestion && nextButtonSprite != null)
                    nextButton.image.sprite = nextButtonSprite;
            }

            // Selalu interactable, karena kalo udah di soal terakhir, mencet tombol ini harusnya buat "Finish Quiz"
            nextButton.interactable = true;
        }
    }

    // Dipanggil saat klik tombol Next (>)
    public void NextQuestion()
    {
        if (currentQuiz == null) return;

        if (currentQuestionIndex < currentQuiz.questions.Count - 1)
        {
            currentQuestionIndex++;
            UpdateQuestionUI();
        }
        else
        {
            // Kalo dipencet pas di soal terakhir (Tombol Done)
            Debug.Log("KUIS SELESAI! Menampilkan konfirmasi...");
            
            // Hitung berapa soal yang sudah dijawab
            int answeredCount = 0;
            if (playerAnswers != null)
            {
                foreach (int answer in playerAnswers)
                {
                    if (answer != -1) answeredCount++;
                }
            }

            // Update teks total terjawab
            if (textTotalSoalJawab != null)
            {
                textTotalSoalJawab.text = answeredCount.ToString() + "/" + currentQuiz.questions.Count.ToString();
            }

            // Tampilkan pop-up konfirmasi
            if (quizConfirmationPanel != null)
            {
                quizConfirmationPanel.SetActive(true);

                // Cari object "Frame" di dalam panel konfirmasi biar animasinya cuma di frame (nggak ikut nge-scale background gelap)
                Transform frame = quizConfirmationPanel.transform.Find("Frame");
                if (frame != null)
                {
                    // Animasi Pop-up pakai DOTween di Frame, menuju scale aslinya biar nggak mengecil
                    frame.localScale = Vector3.zero;
                    frame.DOScale(originalConfirmationScale, 0.4f).SetEase(Ease.OutBack);
                }
                else
                {
                    // Kalau nggak ketemu "Frame", animasiin keseluruhan panelnya ke scale aslinya
                    quizConfirmationPanel.transform.localScale = Vector3.zero;
                    quizConfirmationPanel.transform.DOScale(originalConfirmationScale, 0.4f).SetEase(Ease.OutBack);
                }
            }
        }
    }

    // Dipanggil saat klik tombol Prev (<)
    public void PrevQuestion()
    {
        if (currentQuestionIndex > 0)
        {
            currentQuestionIndex--;
            UpdateQuestionUI();
        }
    }

    // Dipanggil saat klik opsi jawaban (A, B, C, atau D)
    public void CheckAnswer(int selectedOptionIndex)
    {
        // Fix Bug: Kalo script belum diinisialisasi (misal play dari scene langsung), return aja
        if (playerAnswers == null || currentQuiz == null) 
        {
            Debug.LogWarning("Quiz belum di-start via StartQuiz()! Balik ke MainMenu atau klik kategori dulu.");
            return;
        }

        // Kalo udah pernah dijawab, ignore klik ini
        if (playerAnswers[currentQuestionIndex] != -1) return;

        QuestionData qData = currentQuiz.questions[currentQuestionIndex];
        
        // Simpan jawaban
        playerAnswers[currentQuestionIndex] = selectedOptionIndex;

        // Disable semua tombol agar tidak bisa diganti & kurangi opacity
        for (int i = 0; i < optionButtons.Length; i++) {
            if (optionButtons[i] != null)
                optionButtons[i].interactable = false;
            
            if (optionCanvasGroups != null && i < optionCanvasGroups.Length && optionCanvasGroups[i] != null)
                optionCanvasGroups[i].alpha = 0.9f;
        }

        // Cek apakah jawaban player sama dengan kunci jawaban di ScriptableObject
        if (selectedOptionIndex == qData.correctAnswerIndex)
        {
            Debug.Log("BENAR! Player menjawab opsi index: " + selectedOptionIndex);
            if (optionImages[selectedOptionIndex] != null && correctOptionSprite != null)
                optionImages[selectedOptionIndex].sprite = correctOptionSprite;

            // Sembunyikan huruf di tombol yang benar
            if (optionLetterObjects != null && selectedOptionIndex < optionLetterObjects.Length && optionLetterObjects[selectedOptionIndex] != null)
                optionLetterObjects[selectedOptionIndex].SetActive(false);

            // Animasi DOTween Benar (Punch Scale membesar sedikit)
            if (optionButtons[selectedOptionIndex] != null)
                optionButtons[selectedOptionIndex].transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 5, 1);
        }
        else
        {
            Debug.Log("SALAH! Player menjawab opsi index: " + selectedOptionIndex + ", Kunci jawaban: " + qData.correctAnswerIndex);
            if (optionImages[selectedOptionIndex] != null && wrongOptionSprite != null)
                optionImages[selectedOptionIndex].sprite = wrongOptionSprite;

            // Sembunyikan huruf di tombol yang salah
            if (optionLetterObjects != null && selectedOptionIndex < optionLetterObjects.Length && optionLetterObjects[selectedOptionIndex] != null)
                optionLetterObjects[selectedOptionIndex].SetActive(false);

            // Tunjukkan jawaban yang benar (opsional, biar pemain tau jawaban yg betul)
            if (optionImages[qData.correctAnswerIndex] != null && correctOptionSprite != null)
            {
                optionImages[qData.correctAnswerIndex].sprite = correctOptionSprite;
                // Sembunyikan huruf di tombol yang benar juga
                if (optionLetterObjects != null && qData.correctAnswerIndex < optionLetterObjects.Length && optionLetterObjects[qData.correctAnswerIndex] != null)
                    optionLetterObjects[qData.correctAnswerIndex].SetActive(false);
            }

            // Animasi DOTween Salah (Shake Position getar ke kanan-kiri)
            if (optionButtons[selectedOptionIndex] != null)
                optionButtons[selectedOptionIndex].transform.DOShakePosition(0.4f, new Vector3(10f, 0, 0), 20, 90, false, true);
        }
    }

    // Dipanggil saat klik tombol "Yakin Selesaikan?" di popup konfirmasi
    public void ConfirmFinishQuiz()
    {
        // Sembunyikan panel konfirmasi
        if (quizConfirmationPanel != null) quizConfirmationPanel.SetActive(false);

        // Hitung Skor
        int correctAnswers = 0;
        int totalQuestions = currentQuiz != null && currentQuiz.questions != null ? currentQuiz.questions.Count : 0;

        if (totalQuestions > 0 && playerAnswers != null)
        {
            for (int i = 0; i < totalQuestions; i++)
            {
                if (i < playerAnswers.Length && playerAnswers[i] == currentQuiz.questions[i].correctAnswerIndex)
                {
                    correctAnswers++;
                }
            }
        }

        int score = totalQuestions > 0 ? Mathf.RoundToInt(((float)correctAnswers / totalQuestions) * 100) : 0;

        if (textSkor != null) textSkor.text = score.ToString();

        // Teks Afirmasi dan Warna
        if (textAfirmasi != null)
        {
            if (score == 100)
            {
                textAfirmasi.text = "SEMPURNA!";
                if (ColorUtility.TryParseHtmlString("#42824B", out Color color))
                    textAfirmasi.color = color;
            }
            else if (score >= 70)
            {
                textAfirmasi.text = "KEREN!!";
                if (ColorUtility.TryParseHtmlString("#FF8A14", out Color color))
                    textAfirmasi.color = color;
            }
            else
            {
                textAfirmasi.text = "COBA LAGI!";
                if (ColorUtility.TryParseHtmlString("#CE3842", out Color color))
                    textAfirmasi.color = color;
            }
        }

        // Tampilkan Finished Panel
        if (finishedQuizPanel != null)
        {
            finishedQuizPanel.SetActive(true);

            // Pop-up Animation
            Transform frame = finishedQuizPanel.transform.Find("Frame");
            if (frame != null)
            {
                frame.localScale = Vector3.zero;
                frame.DOScale(originalFinishedScale, 0.4f).SetEase(Ease.OutBack);
            }
            else
            {
                finishedQuizPanel.transform.localScale = Vector3.zero;
                finishedQuizPanel.transform.DOScale(originalFinishedScale, 0.4f).SetEase(Ease.OutBack);
            }
        }
    }

    // Dipanggil saat klik tombol Retry
    public void RetryQuiz()
    {
        if (finishedQuizPanel != null) finishedQuizPanel.SetActive(false);
        if (currentQuiz != null) StartQuiz(currentQuiz);
    }

    // Dipanggil saat klik tombol Back / Selection
    public void BackToQuizSelection()
    {
        if (finishedQuizPanel != null) finishedQuizPanel.SetActive(false);
        if (quizSelectedPanel != null) quizSelectedPanel.SetActive(false);
        if (changeQuizLevelPanel != null) changeQuizLevelPanel.SetActive(true);
    }

    // Dipanggil saat klik tombol Next Quiz
    public void PlayNextQuiz()
    {
        if (allQuizzesList == null || allQuizzesList.Length == 0 || currentQuiz == null) return;

        int currentIndex = System.Array.IndexOf(allQuizzesList, currentQuiz);
        if (currentIndex != -1 && currentIndex < allQuizzesList.Length - 1)
        {
            // Ada quiz selanjutnya
            if (finishedQuizPanel != null) finishedQuizPanel.SetActive(false);
            StartQuiz(allQuizzesList[currentIndex + 1]);
        }
        else
        {
            // Udah ga ada quiz lagi, balik ke menu
            BackToQuizSelection();
        }
    }
}