using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class StateManager : MonoBehaviour 
{
    // Main panels
    public GameObject titlePanel;
    public GameObject subtitlePanel;
    public GameObject quizPanel;
    // THIS IS NEW ↓
    public GameObject feedbackPanel;
    
    // Quiz elements
    public GameObject questionPanel;
    public GameObject answerOptionsPanel;
    public Button submitButton;
    // THIS IS NEW ↓
    public GameObject characterPanel;
    public TMP_Text feedbackText;
    public TMP_Text explanationText;
    public Button nextButton;
    // animator reference here later
    
    // Question data
    public QuestionData currentQuestion;
    public TMP_Text questionText; // The text component that shows question
    public AnswerSystem answerSystem; // Reference to answer system
    
    // References
    public SubtitleScript subtitleScript;
    public PlayableDirector timeline;
    
    // Settings
    public float titleDisplayTime = 10f;
    public float delayBetweenAnswerOptions = 0.5f;
    
    // Private
    private List<GameObject> answerOptions = new List<GameObject>();
    
    void Start()
    {
        // Get all answer options
        foreach (Transform child in answerOptionsPanel.transform)
        {
            answerOptions.Add(child.gameObject);
        }
        
        // Setup
        HideAllPanels();
        StartCoroutine(TitleScreenSequence());
    }
    
    void HideAllPanels() 
    {
        titlePanel.SetActive(false);
        subtitlePanel.SetActive(false);
        quizPanel.SetActive(false);
        // THIS IS NEW ↓
        feedbackPanel.SetActive(false);
    }
    
    IEnumerator TitleScreenSequence() 
    {
        titlePanel.SetActive(true);
        yield return new WaitForSeconds(titleDisplayTime);
        titlePanel.SetActive(false);
        StartCutscene();
    }
    
    void StartCutscene()
    {
        subtitlePanel.SetActive(true);
        timeline.Play();
        timeline.stopped += OnCutsceneEnded;
    }
    
    void OnCutsceneEnded(PlayableDirector pd)
    {
        subtitlePanel.SetActive(false);
        pd.time = pd.duration;
        pd.Pause();
        StartCoroutine(ShowQuizSequence());
    }
    
    IEnumerator ShowQuizSequence()
    {
        // Show quiz panel
        quizPanel.SetActive(true);
        
        // Hide everything initially
        questionPanel.SetActive(false);
        answerOptionsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);
        
        // NEW! Load question data into UI
        LoadQuestionData();
        
        // Small delay
        yield return new WaitForSeconds(0.5f);
        
        // Show question
        questionPanel.SetActive(true);
        
        // Wait
        yield return new WaitForSeconds(0.8f);
        
        // Show container
        answerOptionsPanel.SetActive(true);
        
        // Hide all options first
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(false);
        }
        
        // Show options one by one
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(true);
            yield return new WaitForSeconds(delayBetweenAnswerOptions);
        }
        
        // Show submit
        submitButton.gameObject.SetActive(true);
    }
    
    // Load question data into UI
    void LoadQuestionData()
    {
        // Set question text
        questionText.text = currentQuestion.questionText;
        
        // Set answer button texts
        for (int i = 0; i < currentQuestion.answers.Count; i++)
        {
            // Get the button
            GameObject buttonObj = answerOptions[i];
            
            // Find the Text (TMP) child
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            
            // Set the text
            if (buttonText != null)
            {
                buttonText.text = currentQuestion.answers[i];
            }
        }
        
        // Tell AnswerSystem which answer is correct
        answerSystem.correctIndex = currentQuestion.correctAnswerIndex;
    }
    
    // Called by AnswerSystem when answer is submitted
    public void OnAnswerResult(bool isCorrect)
    {
    // THIS IS NEW ↓
      submitButton.SetActive(false);
      feedbackPanel.SetActive(true);
        if (isCorrect)
        {
            feedbackText.text = currentQuestion.wrongFeedback;
            explanationText.text = currentQuestion.explanation;
            // trigger animation
            nextButton.SetActive(true);
            Debug.Log("CORRECT! " + currentQuestion.correctFeedback);
            // TODO: Show feedback panel with Next button
        }
        else
        {
            feedbackText.text = currentQuestion.wrongFeedback;
            explanationText.text = "Correct Answer: ${currentQuestion.correctAnswerIndex}" + currentQuestion.explanation;
            // trigger animation
            submitButton.SetActive(true);
            submitButton.text = "Retry";
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(onRetry);
            Debug.Log("WRONG! " + currentQuestion.wrongFeedback);
            // TODO: Show feedback panel with Retry button
        }
    }
}
