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
using UnityEngine.SceneManagement;

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
    public GameObject characterPanel;
    public Animator characterAnimator;
    public TMP_Text feedbackText;
    public TMP_Text explanationText;

    public Button nextButton;
    public Button submitButton;
    public Button retryButton;

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
    public GameObject confettiRight;
    public GameObject confettiLeft;

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
        DisableBackgroundRaycasts();
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
        confettiRight.SetActive(false);
        confettiLeft.SetActive(false);
    }

    IEnumerator TitleScreenSequence()
    {
        timeline.time = 0;
        timeline.Pause();

        titlePanel.SetActive(true);
        yield return new WaitForSeconds(titleDisplayTime);
        titlePanel.SetActive(false);
        StartCutscene();
    }

    void StartCutscene()
    {
        subtitlePanel.SetActive(true);
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

        //Hold at the last frame
        pd.time = 0;
        pd.Pause();

        StartCoroutine(ShowQuizSequence());
    }

    IEnumerator ShowQuizSequence()
    {
        // Delay before popping up the UI 
        yield return new WaitForSeconds(2f);

        subtitlePanel.SetActive(false);

        // Show quiz panel
        quizPanel.SetActive(true);

        // Hide everything initially
        questionPanel.SetActive(false);
        answerOptionsPanel.SetActive(false);
        submitButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        blurImage.SetActive(false);
        confettiRight.SetActive(false);
        confettiLeft.SetActive(false);
        characterHappy.SetActive(false);
        characterUpset.SetActive(false);

        // Letter popup
        yield return StartCoroutine(introAnim.PlayIntroAnimationCoroutine());

        // Load question data into UI
        LoadQuestionData();

        // Show quiz elements
        RemoveCameraNoise();
        questionPanel.SetActive(true);
        answerOptionsPanel.SetActive(true);
        blurImage.SetActive(true);

        // FIX: Disable raycast on blur image so it doesn't block buttons
        Image blurImg = blurImage.GetComponent<Image>();
        if (blurImg != null)
        {
            blurImg.raycastTarget = false;
        }

        // Move blur to back
        blurImage.transform.SetAsFirstSibling();

        // Hide all options first
        foreach (GameObject option in answerOptions)
        {
            option.SetActive(false);
        }

        // Show options one by one
        Transform panelTransform = answerOptionsPanel.transform;

        for (int i = 0; i < panelTransform.childCount; i++)
        {
            Transform answerTransform = panelTransform.GetChild(i);
            GameObject answerOption = answerTransform.gameObject;

            answerOption.SetActive(true);

            // Make button interactable
            Button btn = answerOption.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
            }

            // Animate the answer option
            PopOut(answerOption.GetComponent<RectTransform>());

            yield return new WaitForSeconds(delayBetweenAnswerOptions);
        }

        // Bring panel to front
        answerOptionsPanel.transform.SetAsLastSibling();

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
            confettiRight.SetActive(true);
            confettiLeft.SetActive(true);
            nextButton.gameObject.SetActive(true);
            Debug.Log("CORRECT! " + currentQuestion.correctFeedback);
        }
        else
        {
            nextButton.gameObject.SetActive(false);
            feedbackText.text = currentQuestion.wrongFeedback;
            explanationText.text = $"Correct Answer: {currentQuestion.correctAnswerIndex + 1}. {currentQuestion.explanation}";
            characterPanel.SetActive(true);
            characterUpset.SetActive(true);

            submitButton.gameObject.SetActive(false);
            retryButton.gameObject.SetActive(true);

            retryButton.onClick.AddListener(OnRetry);

            Debug.Log("WRONG! " + currentQuestion.wrongFeedback);
        }
    }

    void OnRetry()
    {
        Debug.Log("OnRetry called!");

        // Hide feedback panels
        feedbackPanel.SetActive(false);
        characterPanel.SetActive(false);
        characterHappy.SetActive(false);
        characterUpset.SetActive(false);

        // Hide retry button
        retryButton.gameObject.SetActive(false);
        retryButton.onClick.RemoveAllListeners();

        // Show quiz elements
        questionPanel.SetActive(true);
        answerOptionsPanel.SetActive(true);

        // Shuffle answers
        Shuffle();

        // Reset answer system
        answerSystem.ResetButtons();

        // Reload question with shuffled answers
        LoadQuestionData();

        // Show submit button (not retry)
        submitButton.gameObject.SetActive(true);
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(answerSystem.CheckAnswer);
    }

    void Shuffle()
    {
        // store the correct answer value before shuffling
        string correctAnswer = currentQuestion.answers[currentQuestion.correctAnswerIndex];

        int n = currentQuestion.answers.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            // swap answers[i] and answers[j]
            (currentQuestion.answers[i], currentQuestion.answers[j]) =
                (currentQuestion.answers[j], currentQuestion.answers[i]);
        }

        // update correctAnswerIndex after shuffle
        currentQuestion.correctAnswerIndex = currentQuestion.answers.IndexOf(correctAnswer);
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

    void RemoveCameraNoise()
    {
        var perlin = followCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        if (perlin != null)
        {
            perlin.enabled = false;
        }
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    void DisableBackgroundRaycasts()
    {
        // Disable raycast on background panels
        if (quizPanel != null)
        {
            Image panelImg = quizPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
        }

        if (feedbackPanel != null)
        {
            Image panelImg = feedbackPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
        }

        if (questionPanel != null)
        {
            Image panelImg = questionPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
        }
    }
}
