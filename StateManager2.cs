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
    [Header("Main Panels")]
    public GameObject titlePanel;
    public GameObject subtitlePanel;
    public GameObject quizPanel;
    public GameObject feedbackPanel;

    // Quiz elements
    [Header("Quiz Elements")]
    public GameObject questionPanel;
    public GameObject answerOptionsPanel;
    public GameObject characterPanel;
    public GameObject placeholderCharacter;
    public TMP_Text feedbackText;
    public TMP_Text explanationText;

    public Button nextButton;
    public Button submitButton;
    public Button retryButton;

    // Question data
    [Header("Question Data")]
    public QuestionData currentQuestion;
    public TMP_Text questionText; // The text component that shows question
    public AnswerSystem answerSystem; // Reference to answer system

    // References
    [Header("References")]
    public SubtitleScript subtitleScript;
    public QuizIntroAnimation introAnim;
    public GameObject lettersPanel;
    public PlayableDirector timeline;
    public GameObject blurImage;
    public GameObject confettiRight;
    public GameObject confettiLeft;
    public GameObject arrowsPanel;

    // Cinemachine
    [Header("Cinemachine")]
    public CinemachineCamera followCamera;
    public CinemachineCamera frontCamera;
    public CinemachineCamera endCamera;
    public CinemachineBrain mainCameraBrain;

    // Character
    [Header("Character")]
    public GameObject characterUpset;
    public GameObject characterHappy;

    // Settings
    [Header("Settings")]
    public float titleDisplayTime = 2f;
    public float delayBetweenAnswerOptions = 0.1f;
    public float scaleX, scaleY;
    public float tweenDuration = 0.35f;
    public float scaleUpFactor = 1.15f;
    private Vector3 originalSize;

    // Private
    private readonly List<GameObject> answerOptions = new List<GameObject>();
    private readonly List<RectTransform> answerOptionsRects = new List<RectTransform>();

    void Awake()
    {
        // sanity warnings for missing references
        if (answerOptionsPanel == null) Debug.LogWarning("answerOptionsPanel not assigned", this);
        if (submitButton == null) Debug.LogWarning("submitButton not assigned", this);
        if (nextButton == null) Debug.LogWarning("nextButton not assigned", this);
        if (timeline == null) Debug.LogWarning("timeline not assigned", this);
        if (answerSystem == null) Debug.LogWarning("answerSystem not assigned", this);
    }

    protected virtual void Start()
    {
        // safe camera setup
        if (followCamera != null) followCamera.gameObject.SetActive(false);
        if (frontCamera != null) frontCamera.gameObject.SetActive(false);
        if (mainCameraBrain != null) mainCameraBrain.enabled = false;

        // populate answers safely
        PopulateAnswerOptions();

        // Setup
        DisableBackgroundRaycasts();
        HideAllPanels();

        // attach next
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(LoadNextScene);
        }

        // start sequence
        if (titlePanel != null && timeline != null)
            StartCoroutine(TitleScreenSequence());
        else
            StartCoroutine(ShowQuizSequence());
    }

    void OnDestroy()
    {
        if (timeline != null)
            timeline.stopped -= OnCutsceneEnded;

        if (nextButton != null) nextButton.onClick.RemoveAllListeners();
        if (submitButton != null) submitButton.onClick.RemoveAllListeners();
        if (retryButton != null) retryButton.onClick.RemoveAllListeners();
    }

    public void HideAllPanels()
    {
        SetActiveSafe(titlePanel, false);
        SetActiveSafe(subtitlePanel, false);
        SetActiveSafe(quizPanel, false);
        SetActiveSafe(feedbackPanel, false);
        SetActiveSafe(characterPanel, false);
        SetActiveSafe(confettiRight, false);
        SetActiveSafe(confettiLeft, false);
        SetActiveSafe(arrowsPanel, false);
    }

    IEnumerator TitleScreenSequence()
    {
        if (timeline != null)
        {
            timeline.Pause();
        }
        SetActiveSafe(titlePanel, true);
        yield return new WaitForSeconds(Mathf.Max(0f, titleDisplayTime));
        SetActiveSafe(titlePanel, false);
        StartCutscene();
    }

    void StartCutscene()
    {
        bool showSubtitlePanel = true;

        SetActiveSafe(subtitlePanel, showSubtitlePanel);

        if (followCamera != null) followCamera.gameObject.SetActive(true);
        if (frontCamera != null) frontCamera.gameObject.SetActive(true);
        if (mainCameraBrain != null) mainCameraBrain.enabled = true;
        if (placeholderCharacter != null) placeholderCharacter.SetActive(false);
        SetActiveSafe(arrowsPanel, true);

        if (timeline != null)
        {
            timeline.stopped -= OnCutsceneEnded;
            timeline.stopped += OnCutsceneEnded;
            timeline.Play();
        }
        else
        {
            StartCoroutine(ShowQuizSequence());
        }
    }

    protected virtual void OnCutsceneEnded(PlayableDirector pd)
    {
        Debug.Log("CUTSCENE ENDED!");

        if (pd != null)
        {
            pd.Pause();
        }

        // Manually set camera priorities
        if (followCamera != null)
        {
            followCamera.gameObject.SetActive(false); // Turn it off
        }
        if (frontCamera != null)
        {
            frontCamera.gameObject.SetActive(false); // Turn it off
        }
        if (endCamera != null)
        {
            endCamera.gameObject.SetActive(false); // Turn it off
        }
        if (placeholderCharacter != null)
        {
            placeholderCharacter.SetActive(true);
        }

        StartCoroutine(ShowQuizSequence());
    }

    public IEnumerator ShowQuizSequence()
    {
        yield return new WaitForSeconds(2f);

        SetActiveSafe(subtitlePanel, false);
        SetActiveSafe(quizPanel, true);

        // hide everything initially
        SetActiveSafe(questionPanel, false);
        SetActiveSafe(answerOptionsPanel, false);
        SetActiveSafe(submitButton?.gameObject, false);
        SetActiveSafe(retryButton?.gameObject, false);
        SetActiveSafe(blurImage, false);
        SetActiveSafe(confettiRight, false);
        SetActiveSafe(confettiLeft, false);
        SetActiveSafe(characterHappy, false);
        SetActiveSafe(characterUpset, false);
        SetActiveSafe(arrowsPanel, false);

        if (introAnim != null)
            yield return StartCoroutine(introAnim.PlayIntroAnimationCoroutine());

        LoadQuestionData();

        RemoveCameraNoise();
        SetActiveSafe(questionPanel, true);
        SetActiveSafe(answerOptionsPanel, true);
        SetActiveSafe(blurImage, true);

        // ensure blur doesn't block buttons
        if (blurImage != null)
        {
            Image blurImg = blurImage.GetComponent<Image>();
            if (blurImg != null) blurImg.raycastTarget = false;
            blurImage.transform.SetAsFirstSibling();
        }

        // hide all options first
        foreach (GameObject option in answerOptions)
            SetActiveSafe(option, false);

        // show options one by one from panel children to match hierarchy
        if (answerOptionsPanel != null)
        {
            Transform panelTransform = answerOptionsPanel.transform;
            for (int i = 0; i < panelTransform.childCount; i++)
            {
                Transform answerTransform = panelTransform.GetChild(i);
                if (answerTransform == null) continue;

                GameObject answerOption = answerTransform.gameObject;
                SetActiveSafe(answerOption, true);

                Button btn = answerOption.GetComponent<Button>();
                if (btn != null) btn.interactable = true;

                RectTransform rect = answerOption.GetComponent<RectTransform>();
                if (rect != null) PopOut(rect);

                yield return new WaitForSeconds(Mathf.Max(0f, delayBetweenAnswerOptions));
            }

            answerOptionsPanel.transform.SetAsLastSibling();
        }

        // show submit button and wire safely
        if (submitButton != null && answerSystem != null)
        {
            SetActiveSafe(submitButton.gameObject, true);
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => answerSystem.CheckAnswer());
        }
        else if (submitButton != null)
        {
            Debug.LogWarning("submitButton present but answerSystem is missing - no checks will run", this);
        }
    }

    public void PopOut(RectTransform rect)
    {
        if (rect == null) return;
        Vector3 original = rect.localScale;
        Vector3 target = original * Mathf.Max(0.01f, scaleUpFactor);
        rect.localScale = original;
        rect.DOScale(target, Mathf.Max(0.01f, tweenDuration))
            .OnComplete(() => rect.DOScale(original, 0.5f));
    }

    // populate answerOptions from panel children safely
    public void PopulateAnswerOptions()
    {
        answerOptions.Clear();
        answerOptionsRects.Clear();

        if (answerOptionsPanel == null)
        {
            Debug.LogWarning("answerOptionsPanel is null - cannot populate options", this);
            return;
        }

        foreach (Transform child in answerOptionsPanel.transform)
        {
            if (child == null || child.gameObject == null) continue;
            answerOptions.Add(child.gameObject);
            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect != null)
            {
                answerOptionsRects.Add(rect);
                if (originalSize == Vector3.zero)
                {
                    originalSize = rect.localScale;
                    scaleX = originalSize.x;
                    scaleY = originalSize.y;
                }
            }
        }

        if (answerOptions.Count == 0)
            Debug.LogWarning("no answer option children found under answerOptionsPanel", this);
    }

    void LoadQuestionData()
    {
        if (currentQuestion == null)
        {
            Debug.LogError("currentQuestion is null", this);
            return;
        }

        if (currentQuestion.answers == null || currentQuestion.answers.Count == 0)
        {
            Debug.LogError("currentQuestion has no answers", this);
            return;
        }

        if (questionText != null)
            questionText.text = currentQuestion.questionText ?? string.Empty;

        int itemsToUse = Mathf.Min(currentQuestion.answers.Count, answerOptions.Count);
        if (itemsToUse == 0)
        {
            Debug.LogError("no UI answer options available to populate", this);
            return;
        }

        for (int i = 0; i < itemsToUse; i++)
        {
            GameObject buttonObj = answerOptions[i];
            if (buttonObj == null) continue;
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null)
                buttonText.text = currentQuestion.answers[i] ?? string.Empty;
            else
                Debug.LogWarning($"no TMP_Text on answer option {i}", this);
        }

        if (answerSystem != null)
        {
            answerSystem.correctIndex = Mathf.Clamp(currentQuestion.correctAnswerIndex, 0, Mathf.Max(0, currentQuestion.answers.Count - 1));
        }
        else
        {
            Debug.LogWarning("answerSystem missing - answer checks won't work", this);
        }
    }

    public void OnAnswerResult(bool isCorrect)
    {
        SetActiveSafe(submitButton?.gameObject, false);
        SetActiveSafe(answerOptionsPanel, false);
        SetActiveSafe(questionPanel, false);
        SetActiveSafe(feedbackPanel, true);

        if (isCorrect)
        {
            if (feedbackText != null) feedbackText.text = currentQuestion?.correctFeedback ?? string.Empty;
            if (explanationText != null) explanationText.text = currentQuestion?.explanation ?? string.Empty;
            SetActiveSafe(characterPanel, true);
            SetActiveSafe(characterHappy, true);
            SetActiveSafe(confettiRight, true);
            SetActiveSafe(confettiLeft, true);
            if (nextButton != null) SetActiveSafe(nextButton.gameObject, true);
            Debug.Log("CORRECT! " + (currentQuestion?.correctFeedback ?? ""));
        }
        else
        {
            if (nextButton != null) SetActiveSafe(nextButton.gameObject, false);
            if (feedbackText != null) feedbackText.text = currentQuestion?.wrongFeedback ?? string.Empty;
            //if (explanationText != null) explanationText.text = "(explanation placeholder)";
            SetActiveSafe(characterPanel, true);
            SetActiveSafe(characterUpset, true);

            if (retryButton != null)
            {
                SetActiveSafe(retryButton.gameObject, true);
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetry);
            }

            Debug.Log("WRONG! " + (currentQuestion?.wrongFeedback ?? ""));
        }
    }

    void OnRetry()
    {
        Debug.Log("OnRetry called!");

        SetActiveSafe(feedbackPanel, false);
        SetActiveSafe(characterPanel, false);
        SetActiveSafe(characterHappy, false);
        SetActiveSafe(characterUpset, false);

        if (retryButton != null)
        {
            SetActiveSafe(retryButton.gameObject, false);
            retryButton.onClick.RemoveAllListeners();
        }

        SetActiveSafe(questionPanel, true);
        SetActiveSafe(answerOptionsPanel, true);

        Shuffle();

        if (answerSystem != null) answerSystem.ResetButtons();

        LoadQuestionData();

        if (submitButton != null && answerSystem != null)
        {
            SetActiveSafe(submitButton.gameObject, true);
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => answerSystem.CheckAnswer());
        }
    }

    void Shuffle()
    {
        if (currentQuestion == null || currentQuestion.answers == null || currentQuestion.answers.Count <= 1) return;

        string correctAnswer = currentQuestion.answers[Mathf.Clamp(currentQuestion.correctAnswerIndex, 0, currentQuestion.answers.Count - 1)];
        int n = currentQuestion.answers.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (currentQuestion.answers[i], currentQuestion.answers[j]) = (currentQuestion.answers[j], currentQuestion.answers[i]);
        }

        currentQuestion.correctAnswerIndex = Mathf.Clamp(currentQuestion.answers.IndexOf(correctAnswer), 0, currentQuestion.answers.Count - 1);
    }

    void OnNextQuestion()
    {
        if (nextButton != null) SetActiveSafe(nextButton.gameObject, false);
        SetActiveSafe(feedbackPanel, false);
        Shuffle();
        StartCoroutine(ShowQuizSequence());
    }

    void RemoveCameraNoise()
    {
        if (followCamera == null) return;
        var perlin = followCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        if (perlin != null) perlin.enabled = false;
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void DisableBackgroundRaycasts()
    {
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

    // small helper
    void SetActiveSafe(GameObject obj, bool value)
    {
        if (obj == null) return;
        if (obj.activeSelf == value) return;
        obj.SetActive(value);
    }
}
