using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;

public class LevelStateManager : MonoBehaviour
{
    [Header("State Panels")]
    public CanvasGroup titlePanel;
    public GameObject subtitlePanel;
    public GameObject pauseOverlay;
    public GameObject questionPanel;
    public GameObject characterContainer; // For 3D character
    public GameObject resultPanel;
    
    [Header("Title Screen")]
    public TMP_Text titleText;
    public string levelName = "Level #1";
    public float titleFadeInTime = 0.5f;
    public float titleDisplayTime = 2f;
    public float titleFadeOutTime = 0.5f;
    
    [Header("Subtitle")]
    public TMP_Text subtitleText;
    private Queue<string> subtitleQueue = new Queue<string>();
    
    [Header("Character (3D Model)")]
    public Camera characterCamera; // Separate camera for character
    public Animator characterAnimator;
    public string happyAnimationTrigger = "Happy";
    public string sadAnimationTrigger = "Sad";
    
    [Header("References")]
    public PlayableDirector timeline;
    public QuizManager quizManager;
    public SignalReceiver signalReceiver;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip titleWhoosh;
    
    private void Start()
    {
        HideAllPanels();
        
        // Register signal listeners
        SetupSignals();
        
        // Start with title
        StartCoroutine(TitleSequence());
    }
    
    private void HideAllPanels()
    {
        titlePanel.alpha = 0;
        titlePanel.gameObject.SetActive(false);
        
        subtitlePanel.SetActive(false);
        pauseOverlay.SetActive(false);
        questionPanel.SetActive(false);
        characterContainer.SetActive(false);
        resultPanel.SetActive(false);
    }
    
    private void SetupSignals()
    {
        // You'll connect these in Timeline
        // We'll set this up in the next step
    }
    
    private IEnumerator TitleSequence()
    {
        titlePanel.gameObject.SetActive(true);
        titleText.text = levelName;
        
        if (titleWhoosh != null)
            audioSource.PlayOneShot(titleWhoosh);
        
        // Fade in with scribbly effect
        yield return StartCoroutine(ScribblyFadeIn());
        
        // Hold
        yield return new WaitForSeconds(titleDisplayTime);
        
        // Fade out
        yield return StartCoroutine(FadeOut(titlePanel, titleFadeOutTime));
        
        titlePanel.gameObject.SetActive(false);
        
        // Start cutscene
        StartCutscene();
    }
    
    private IEnumerator ScribblyFadeIn()
    {
        // Animate the TMP maxVisibleCharacters for scribbly effect
        titleText.maxVisibleCharacters = 0;
        titlePanel.alpha = 1;
        
        int totalChars = titleText.text.Length;
        float charDelay = titleFadeInTime / totalChars;
        
        for (int i = 0; i <= totalChars; i++)
        {
            titleText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }
    }
    
    private IEnumerator FadeOut(CanvasGroup group, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            group.alpha = 1 - (elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        group.alpha = 0;
    }
    
    private void StartCutscene()
    {
        subtitlePanel.SetActive(true);
        
        timeline.Play();
        timeline.stopped += OnCutsceneFinished;
    }
    
    // Call this from Timeline Signals
    public void UpdateSubtitle(string text)
    {
        subtitleText.text = text;
    }
    
    private void OnCutsceneFinished(PlayableDirector director)
    {
        // Pause the timeline so last frame stays visible
        timeline.Pause();
        
        // Show pause overlay (semi-transparent)
        pauseOverlay.SetActive(true);
        
        // Show question UI ON TOP of frozen cutscene
        questionPanel.SetActive(true);
        quizManager.StartQuiz();
    }
    
    // Call this from QuizManager when answer selected
    public void ShowCharacterFeedback(bool isCorrect)
    {
        // Hide quiz UI temporarily
        questionPanel.SetActive(false);
        
        // Show 3D character
        characterContainer.SetActive(true);
        
        // Trigger animation
        if (isCorrect)
            characterAnimator.SetTrigger(happyAnimationTrigger);
        else
            characterAnimator.SetTrigger(sadAnimationTrigger);
        
        // Let animation play for 3 seconds
        StartCoroutine(HideCharacterAfterDelay(isCorrect));
    }
    
    private IEnumerator HideCharacterAfterDelay(bool isCorrect)
    {
        yield return new WaitForSeconds(3f);
        
        characterContainer.SetActive(false);
        
        // Show result panel
        questionPanel.SetActive(true); // Show it again
        resultPanel.SetActive(true);
        quizManager.ShowResult(isCorrect);
    }
}
