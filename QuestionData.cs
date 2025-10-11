using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]
public class QuestionData : ScriptableObject
{
    [Header("Question Content")]
    [TextArea(3, 6)]
    public string questionText;
    
    [Header("Answers")]
    public List<string> answers = new List<string>();
    
    [Header("Correct Answer")]
    public int correctAnswerIndex;
    
    [Header("Feedback")]
    [TextArea(2, 4)]
    public string correctFeedback = "Richtig! Gut gemacht!";
    
    [TextArea(2, 4)]
    public string wrongFeedback = "Leider falsch. Versuch es nochmal!";
    
    [TextArea(2, 4)]
    public string explanation;
    
    private void OnValidate()
    {
        if (answers.Count < 2)
            Debug.LogWarning($"Question '{name}' needs at least 2 answers!");
        
        if (correctAnswerIndex < 0 || correctAnswerIndex >= answers.Count)
        {
            Debug.LogError($"Question '{name}' has invalid correctAnswerIndex!");
            correctAnswerIndex = 0;
        }
    }
    
    public bool IsCorrect(int selectedIndex)
    {
        return selectedIndex == correctAnswerIndex;
    }
}
