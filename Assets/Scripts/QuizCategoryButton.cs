using UnityEngine;
using UnityEngine.UI;

public class QuizCategoryButton : MonoBehaviour 
{
    public QuizScriptable categoryData; // Drag asset Quiz_KupuKupu ke sini via Inspector
    public QuizManager quizManager;     // Reference ke QuizManager utama

    private void Start() 
    {
        GetComponent<Button>().onClick.AddListener(OnCategoryClicked);
    }

    private void OnCategoryClicked() 
    {
        quizManager.StartQuiz(categoryData);
    }
}