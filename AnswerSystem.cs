using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnswerSystem : MonoBehaviour 
{
    // REFERENCES
    public Transform AnswerOptionsPanel;
    private List<Button> answerButtons = new List<Button>();
    public StateManager stateManager;
    
    // INDICES
    public int correctIndex; // SET THIS IN INSPECTOR!
    private int selectedIndex;
    
    // COLORS 
    public Color wrongColor = Color.red;
    public Color correctColor = Color.green;
    public Color selectedColor = Color.blue;
    public Color defaultColor = Color.white;
    
    public void Start()
    {
        SetupButtons();
        selectedIndex = -1; // means no selection
    }
    
    // METHODS
    void SetupButtons()
    {
        // Get children (each answer option) from answeroptionspanel
        foreach (Transform child in AnswerOptionsPanel) 
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null) 
            {
                answerButtons.Add(btn);
            }
        }
        
        // Attach click listeners to all buttons
        for (int i = 0; i < answerButtons.Count; i++) 
        {
            int index = i; // capturing the index of button
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
        }
        
        UnhighlightAll();
    }
    
    void OnAnswerClicked(int index)
    {
        // Runs when player clicked an answer
        selectedIndex = index;
        UnhighlightAll();
        HighlightButton(index);
    }
    
    void HighlightButton(int index)
    {
        // Runs when button is selected
        Button button = answerButtons[index];
        Image buttonImage = button.GetComponent<Image>();
        buttonImage.color = selectedColor;
    }
    
    void UnhighlightAll()
    {
        // Unhighlights all buttons
        foreach (Button button in answerButtons) 
        {
            Image buttonImage = button.GetComponent<Image>();
            buttonImage.color = defaultColor;
        }
    }
    
    public void CheckAnswer()
    {
        // Compares selected to correct (called by submit button)
        
        // Step 1: Check if anything selected
        if (selectedIndex < 0) 
        {
            Debug.Log("Error! Please choose an answer.");
            return; // Stop here!
        }
        
        // Step 2: Compare selected to correct
        bool isCorrect = (selectedIndex == correctIndex);
        
        // Step 3: Show visual feedback
        if (isCorrect)
        {
            ShowCorrectAnswer();
        }
        else
        {
            ShowWrongAnswer();
            ShowCorrectAnswer(); // Also show which one was right!
        }
        
        // Step 4: Disable all buttons
        DisableAllButtons();
        
        // Step 5: Tell StateManager the result
        stateManager.OnAnswerResult(isCorrect);
    }
    
    void ShowCorrectAnswer()
    {
        // Highlights correct answer in green
        Button correctButton = answerButtons[correctIndex];
        Image correctButtonImage = correctButton.GetComponent<Image>();
        correctButtonImage.color = correctColor;
    }
    
    void ShowWrongAnswer()
    {
        // Highlights the SELECTED wrong answer in red
        Button wrongButton = answerButtons[selectedIndex];
        Image wrongButtonImage = wrongButton.GetComponent<Image>();
        wrongButtonImage.color = wrongColor;
    }
    
    void DisableAllButtons()
    {
        // Make all buttons unclickable
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }
    }
}
