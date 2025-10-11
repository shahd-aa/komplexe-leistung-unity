using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;

public class StateManager : MonoBehaviour 
{
    // Main panels
    public GameObject titlePanel;
    public GameObject subtitlePanel;
    public GameObject quizPanel;
    
    // Quiz elements
    public GameObject questionPanel;
    public GameObject answerOptionsPanel;
    public Button submitButton;
    
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
        // Get all answer options from the container
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
        // Show quiz panel (overlay)
        quizPanel.SetActive(true);
        
        // Hide everything inside initially
        questionPanel.SetActive(false);
        answerOptionsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);
        
        // Small delay
        yield return new WaitForSeconds(0.5f);
        
        // POP! Question appears
        questionPanel.SetActive(true);
        
        // Wait a moment
        yield return new WaitForSeconds(0.8f);
        
        // Make container visible
        answerOptionsPanel.SetActive(true);
        
        // Hide all options first
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(false);
        }
        
        // POP! Show options one by one
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(true);
            yield return new WaitForSeconds(delayBetweenAnswerOptions);
        }
        
        // Show submit button
        submitButton.gameObject.SetActive(true);
    }
}
