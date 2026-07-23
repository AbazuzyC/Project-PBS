using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour 
{
    [Header("UI References")]
    public TextMeshProUGUI questionNumberTopText; // Untuk teks "SOAL 1" di atas
    public TextMeshProUGUI questionCounterText;   // Untuk teks "1 / 10" di bawah
    public TextMeshProUGUI questionText;
    public Image questionImage;
    public TextMeshProUGUI[] optionTexts; // Teks jawaban A, B, C, D
    
    [Header("Button References")]
    public Button prevButton;
    public Button nextButton;

    private QuizScriptable currentQuiz;
    private int currentQuestionIndex = 0;

    public void StartQuiz(QuizScriptable selectedQuiz) 
    {
        currentQuiz = selectedQuiz;
        currentQuestionIndex = 0;
        UpdateQuestionUI();
    }

    // Fungsi ini dipisah biar gampang dipanggil pas Next/Prev
    private void UpdateQuestionUI() 
    {
        QuestionData qData = currentQuiz.questions[currentQuestionIndex];
        
        // Update teks soal dan indikator nomor
        questionNumberTopText.text = "SOAL " + (currentQuestionIndex + 1);
        questionCounterText.text = (currentQuestionIndex + 1) + " / " + currentQuiz.questions.Count;
        questionText.text = qData.questionText;
        
        // Atur gambar
        if (qData.questionImage != null) {
            questionImage.sprite = qData.questionImage;
            questionImage.gameObject.SetActive(true);
        } else {
            questionImage.gameObject.SetActive(false);
        }

        // Update teks di tombol A, B, C, D
        for (int i = 0; i < optionTexts.Length; i++) {
            optionTexts[i].text = qData.options[i];
        }

        // LOGIC TOMBOL PREV & NEXT
        // Kalo index 0 (soal 1), prev gak bisa dipencet. 
        prevButton.interactable = currentQuestionIndex > 0;
        
        // Kalo index mentok di soal terakhir, next gak bisa dipencet.
        nextButton.interactable = currentQuestionIndex < currentQuiz.questions.Count - 1;
    }

    // Dipanggil saat klik tombol Next (>)
    public void NextQuestion()
    {
        if (currentQuestionIndex < currentQuiz.questions.Count - 1)
        {
            currentQuestionIndex++;
            UpdateQuestionUI();
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
        QuestionData qData = currentQuiz.questions[currentQuestionIndex];
        
        // Cek apakah jawaban player sama dengan kunci jawaban di ScriptableObject
        if (selectedOptionIndex == qData.correctAnswerIndex)
        {
            Debug.Log("BENAR! Player menjawab opsi index: " + selectedOptionIndex);
            // TODO: Tambah skor atau panggil animasi benar di sini
        }
        else
        {
            Debug.Log("SALAH! Player menjawab opsi index: " + selectedOptionIndex + ", Kunci jawaban: " + qData.correctAnswerIndex);
            // TODO: Kasih feedback salah
        }
    }
}