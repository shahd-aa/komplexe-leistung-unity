using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")] // menu in editor to create questions without script
public class QuestionData : ScriptableObject
{
    [Header("Question Content")]
    [TextArea(3, 6)] // makes text box bigger 
    public string questionText;
    
    [Header("Answers")]
    [Tooltip("Add 2-6 answer options")]
    public List<string> answers = new List<string>();
    
    [Header("Correct Answer")]
    [Tooltip("Index of correct answer (0-based)")]
    public int correctAnswerIndex;
    
    [Header("Feedback")]
    [TextArea(2, 4)]
    public string explanation;
    
    // validation method - unity calls this automatically
    private void OnValidate()
    {
        // ensure at least 1 answers
        if (answers.Count < 1)
        {
            Debug.LogWarning($"Question '{name}' needs at least 1 answer!");
        }
        
        // Ensure not more than 6 answers (UI constraint)
        if (answers.Count > 6)
        {
            Debug.LogWarning($"Question '{name}' has too many answers (max 6)!");
        }
        
        // Validate correct answer index
        if (correctAnswerIndex < 0 || correctAnswerIndex >= answers.Count) // checks if index is negative or out of bound
        {
            Debug.LogError($"Question '{name}' has invalid correctAnswerIndex! Must be between 0 and {answers.Count - 1}");
            correctAnswerIndex = 0; // Reset to safe value
        }
    }
    
    // Optional: Helper method to check if answer is correct
    public bool IsCorrect(int selectedIndex)
    {
        return selectedIndex == correctAnswerIndex;
    }
    
    // Optional: Get the correct answer text
    public string GetCorrectAnswer()
    {
        if (correctAnswerIndex >= 0 && correctAnswerIndex < answers.Count)
            return answers[correctAnswerIndex];
        return "";
    }
}
