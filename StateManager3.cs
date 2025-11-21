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

public class StateManager3 : MonoBehaviour
{
    // UI 
    public GameObject titlePanel;
    public GameObject subtitlePanel;
    public GameObject sliderPanel;
    public GameObject feedbackPanel;
    public GameObject characterPanel;
    public GameObject notePanel;
    public GameObject taskPanel;
    public Button nextButton;

    // References
    public StateManager2 stateManagerScript;
    public SubtitleScript subtitleScript;
    public SwingController swingController;
    public PlayableDirector timeline;
    public GameObject blurImage;
    public GameObject confettiRight;
    public GameObject confettiLeft;

    // character
    public GameObject characterHappy;

    // Settings
    public float titleDisplayTime;
    public float tweenDuration;
    public float scaleUpFactor;

    // Cinemachine
    public CinemachineCamera followCamera;
    public CinemachineCamera frontCamera;
    public CinemachineBrain mainCameraBrain;

    // Timeline Control Variables
    private bool timelineIsPlaying = false;
    private bool cutsceneCompleted = false;

    void Start()
    {
        // Ensure cameras and timeline are properly initialized
        followCamera.gameObject.SetActive(false);
        frontCamera.gameObject.SetActive(false);
        mainCameraBrain.enabled = false;

        // Setup
        HideAllPanels();
        StartCoroutine(TitleScreenSequence());
        nextButton.onClick.AddListener(() => stateManagerScript.LoadNextScene());
        InitializeTimeline();
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
        if (cutsceneCompleted)
        {
            Debug.LogWarning("Cutscene already completed!");
            return;
        }

        subtitlePanel.SetActive(true);
        followCamera.gameObject.SetActive(true);
        frontCamera.gameObject.SetActive(true);
        mainCameraBrain.enabled = true;

        // Start timeline properly
        PlayTimeline();
    }

    IEnumerator ShowLevelSequence()
    {
        // Delay before popping up the UI 
        yield return new WaitForSeconds(2f);

        subtitlePanel.SetActive(false);

        taskPanel.SetActive(true);
        PopOut(taskPanel.GetComponent<RectTransform>());

        yield return new WaitForSeconds(5f);
        taskPanel.SetActive(false);

        sliderPanel.SetActive(true);
        notePanel.SetActive(true);
    }

    public void ShowFeedback()
    {
        feedbackPanel.SetActive(true);
        characterPanel.SetActive(true);
        confettiLeft.SetActive(true);
        confettiRight.SetActive(true);
    }

    // ---------------------------- TIMELINE ------------------------------------
    void PlayTimeline()
    {
        if (timeline == null) return;

        timeline.time = 0;
        timeline.Evaluate(); // Update to starting frame
        timelineIsPlaying = true;
        timeline.Play();

        Debug.Log("Timeline started playing");
    }

    void InitializeTimeline()
    {
        if (timeline != null)
        {
            // Force timeline to stop and reset
            timeline.Stop();
            timeline.time = 0;
            timeline.Evaluate(); // Force it to update to time 0

            // Subscribe to events
            timeline.stopped -= OnCutsceneEnded;
            timeline.stopped += OnCutsceneEnded;
            timeline.played -= OnTimelineStarted;
            timeline.played += OnTimelineStarted;

            Debug.Log($"Timeline initialized. Duration: {timeline.duration}");
        }
    }

    void OnTimelineStarted(PlayableDirector pd)
    {
        Debug.Log("Timeline play event triggered");
        timelineIsPlaying = true;
    }

    void OnCutsceneEnded(PlayableDirector pd)
    {
        Debug.Log($"CUTSCENE ENDED! Time: {pd.time}, Duration: {pd.duration}");

        timelineIsPlaying = false;
        cutsceneCompleted = true;

        // FORCE pause at last frame
        pd.Pause();
        pd.time = pd.duration; // Explicitly set to end
        pd.Evaluate(); // Force update

        // Optionally disable timeline to prevent any updates
        pd.playableGraph.GetRootPlayable(0).SetSpeed(0);

        StartCoroutine(ShowLevelSequence());
    }

    // ----------------------------- EXTRAS ---------------------------------------------
    void RemoveCameraNoise()
    {
        var perlin = followCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
        if (perlin != null)
        {
            perlin.enabled = false;
        }
    }

    void HideAllPanels()
    {
        titlePanel.SetActive(false);
        subtitlePanel.SetActive(false);
        feedbackPanel.SetActive(false);
        characterPanel.SetActive(false);
        confettiRight.SetActive(false);
        confettiLeft.SetActive(false);
        blurImage.SetActive(false);
        sliderPanel.SetActive(false);
        notePanel.SetActive(false);
        taskPanel.SetActive(false);
    }

    void PopOut(RectTransform rect)
    {
        Vector3 original = rect.localScale;
        Vector3 target = original * scaleUpFactor;

        rect.localScale = original; // Ensure starting point
        rect.DOScale(target, tweenDuration)
            .OnComplete(() => rect.DOScale(original, 0.5f));
    }
}
