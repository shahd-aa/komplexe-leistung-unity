// 10.11.2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Unity.Cinemachine;

public class StateManager2 : MonoBehaviour
{
    // Main panels
    public GameObject titlePanel;
    public GameObject subtitlePanel;
    public GameObject quizPanel;
    public GameObject feedbackPanel;

    // Quiz elements
    public GameObject questionPanel;
    public GameObject answerOptionsPanel;
    public Button submitButton;
    public GameObject characterPanel;
    public Animator characterAnimator;
    public TMP_Text feedbackText;
    public TMP_Text explanationText;
    public Button nextButton;

    // Question data
    public QuestionData currentQuestion;
    public TMP_Text questionText; // The text component that shows question
    public AnswerSystem answerSystem; // Reference to answer system

    // References
    public SubtitleScript subtitleScript;
    public QuizIntroAnimation introAnim;
    public GameObject lettersPanel;
    public PlayableDirector timeline;
    public GameObject blurImage;

    // Cinemachine
    public CinemachineCamera followCamera;
    public CinemachineCamera frontCamera;
    public CinemachineBrain mainCameraBrain;

    // Character
    public GameObject characterUpset;
    public GameObject characterHappy;

    // Settings
    public float titleDisplayTime;
    public float delayBetweenAnswerOptions;
    public float scaleX, scaleY;
    public float tweenDuration;
    public float scaleUpFactor;
    private Vector3 originalSize;

    // Private
    private List<GameObject> answerOptions = new List<GameObject>();
    private List<RectTransform> answerOptionsRects = new List<RectTransform>();
    private int randomNumber;

    void Start()
    {
        // Ensure cameras and timeline are properly initialized
        followCamera.gameObject.SetActive(false);
        frontCamera.gameObject.SetActive(false);
        mainCameraBrain.enabled = false;

        timeline.gameObject.SetActive(false);

        // Get all answer options
        foreach (Transform child in answerOptionsPanel.transform)
        {
            answerOptions.Add(child.gameObject);
        }
        foreach (GameObject answerOption in answerOptions)
        {
            Vector3 fullScale = answerOption.transform.localScale;
            scaleX = fullScale.x;
            scaleY = fullScale.y;
            originalSize = fullScale;

            RectTransform rect = answerOption.GetComponent<RectTransform>();
            if (rect != null)
            {
                answerOptionsRects.Add(rect);
            }
        }

        // Setup
        HideAllPanels();
        StartCoroutine(TitleScreenSequence());
        nextButton.onClick.AddListener(OnNextQuestion);
    }

    void HideAllPanels()
    {
        titlePanel.SetActive(false);
        subtitlePanel.SetActive(false);
        quizPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        characterPanel.SetActive(false);
    }

    IEnumerator TitleScreenSequence()
    {
        timeline.time = 0;
        timeline.Stop();

        titlePanel.SetActive(true);
        yield return new WaitForSeconds(titleDisplayTime);
        titlePanel.SetActive(false);
        StartCutscene();
    }

    void StartCutscene()
    {
        subtitlePanel.SetActive(true);

        // Re-enable timeline
        timeline.gameObject.SetActive(true);

        followCamera.gameObject.SetActive(true);
        frontCamera.gameObject.SetActive(true);
        mainCameraBrain.enabled = true;

        timeline.stopped -= OnCutsceneEnded;
        timeline.stopped += OnCutsceneEnded;
        timeline.Play();
    }

    void OnCutsceneEnded(PlayableDirector pd)
    {
        Debug.Log("CUTSCENE ENDED!");
        pd.Stop();
        StartCoroutine(ShowQuizSequence());
    }

    IEnumerator ShowQuizSequence()
    {
        subtitlePanel.SetActive(false);

        // Show quiz panel
        quizPanel.SetActive(true);

        // Hide everything initially
        questionPanel.SetActive(false);
        answerOptionsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);
        blurImage.SetActive(false);
        characterHappy.SetActive(false);
        characterUpset.SetActive(false);

        // Letter popup
        yield return StartCoroutine(introAnim.PlayIntroAnimationCoroutine());

        // Load question data into UI
        LoadQuestionData();

        // Show quiz elements
        questionPanel.SetActive(true);
        answerOptionsPanel.SetActive(true);
        blurImage.SetActive(true);

        // Hide all options first
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(false);
        }

        // Show options one by one
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(true);
            PopOut(option.GetComponent<RectTransform>());
            yield return new WaitForSeconds(delayBetweenAnswerOptions);
        }

        // Show submit button
        submitButton.gameObject.SetActive(true);
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(answerSystem.CheckAnswer);
    }

    void PopOut(RectTransform rect)
    {
        Vector3 original = rect.localScale;
        Vector3 target = original * scaleUpFactor;

        rect.localScale = original; // Ensure starting point
        rect.DOScale(target, tweenDuration)
            .OnComplete(() => rect.DOScale(original, 0.5f));
    }

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
            if (buttonText)
            {
                buttonText.text = currentQuestion.answers[i];
            }
        }

        // Tell AnswerSystem which answer is correct
        answerSystem.correctIndex = currentQuestion.correctAnswerIndex;
    }

    public void OnAnswerResult(bool isCorrect)
    {
        TMP_Text submitText = submitButton.GetComponentInChildren<TMP_Text>();
        submitButton.gameObject.SetActive(false);
        answerOptionsPanel.gameObject.SetActive(false);
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);

        if (isCorrect)
        {
            feedbackText.text = currentQuestion.correctFeedback;
            explanationText.text = currentQuestion.explanation;
            characterPanel.SetActive(true);
            characterHappy.SetActive(true);
            nextButton.gameObject.SetActive(true);
            Debug.Log("CORRECT! " + currentQuestion.correctFeedback);
        }
        else
        {
            feedbackText.text = currentQuestion.wrongFeedback;
            explanationText.text = $"Correct Answer: {currentQuestion.correctAnswerIndex} {currentQuestion.explanation}";
            characterPanel.SetActive(true);
            characterUpset.SetActive(true);
            submitButton.gameObject.SetActive(true);
            if (submitText) submitText.text = "Retry";
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnRetry);
            Debug.Log("WRONG! " + currentQuestion.wrongFeedback);
        }
    }

    void OnRetry()
    {
        TMP_Text submitText = submitButton.GetComponentInChildren<TMP_Text>();
        feedbackPanel.SetActive(false);
        if (submitText) submitText.text = "Submit";
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(answerSystem.CheckAnswer);
        submitButton.gameObject.SetActive(true);
        Shuffle();
        answerSystem.ResetButtons();
    }

    void Shuffle()
    {
        for (int i = currentQuestion.answers.Count - 1; i > 1; i--)
        {
            randomNumber = Random.Range(0, i + 1);

            if (currentQuestion.correctAnswerIndex == i)
            {
                currentQuestion.correctAnswerIndex = randomNumber;
            }
            else if (currentQuestion.correctAnswerIndex == randomNumber)
            {
                currentQuestion.correctAnswerIndex = i;
            }

            (currentQuestion.answers[i], currentQuestion.answers[randomNumber]) =
                (currentQuestion.answers[randomNumber], currentQuestion.answers[i]);
        }
    }

    void OnNextQuestion()
    {
        nextButton.gameObject.SetActive(false);
        feedbackPanel.SetActive(false);
        Shuffle();
        StartCoroutine(ShowQuizSequence());
    }

    void OnDestroy()
    {
        if (timeline != null)
        {
            timeline.stopped -= OnCutsceneEnded;
        }
    }
}
