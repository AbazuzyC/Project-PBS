using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct QuestionData 
{
    [TextArea]
    public string questionText;
    public Sprite questionImage; // Kosongin kalau soalnya gak butuh gambar
    public string[] options;     // Isi 4 untuk A, B, C, D
    public int correctAnswerIndex; // 0 = A, 1 = B, 2 = C, 3 = D
}

[CreateAssetMenu(fileName = "New Quiz Category", menuName = "Quiz System/Quiz Category")]
public class QuizScriptable: ScriptableObject 
{
    public string categoryName; // Contoh: "Kupu Kupu"
    public List<QuestionData> questions;
}