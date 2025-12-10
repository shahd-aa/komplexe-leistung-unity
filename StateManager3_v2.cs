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

public class StateManager3_v2 : MonoBehaviour
{
    public PlayableDirector timeline;
    public GameObject slowMoPanel;
    public GameObject ball;

    public float jumpToThrowTime = 6.9833f;
    public Vector3 startPos;
    public Quaternion startRot;
    public Vector3 startScale;

    // static fields
    private static bool pendingSlowMoReplay = false;
    private static double pendingTimelineTime = 0.0;
    private static float pendingTimeScale = 1f;

    void Start()
    {
        // if a slowmo replay was requested before the reload, handle it and don't auto-start the cutscene
        if (pendingSlowMoReplay)
        {
            // clear the flag so next loads are normal
            pendingSlowMoReplay = false;

            // show UI if needed
            if (slowMoPanel != null) slowMoPanel.SetActive(true);

            // apply timescale and jump the timeline to the requested time
            Time.timeScale = pendingTimeScale;
            if (timeline != null)
            {
                StartCoroutine(StartDelay(1f));
                timeline.time = pendingTimelineTime;
                timeline.Evaluate(); // apply the values at that time immediately
                timeline.Play();
            }

            return;
        }

        slowMoPanel.SetActive(false);
        StartCutscene();
    }

    private void StartCutscene()
    {
        Debug.Log("cutscene is playing !");
        timeline.stopped -= OnCutsceneEnded;
        timeline.stopped += OnCutsceneEnded;
        timeline.Play();
    }

    IEnumerator StartDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private void OnCutsceneEnded(PlayableDirector pd)
    {
        Debug.Log("CUTSCENE ENDED!");
        pd.Stop();
        StartCoroutine(SlowMoReplay());
    }

    IEnumerator SlowMoReplay()
    {
        yield return new WaitForSeconds(1.5f);
        if (slowMoPanel != null) slowMoPanel.SetActive(true);

        RequestReloadAndSlowMo(jumpToThrowTime, 0.7f);

        // Resume the timeline
        timeline.time = jumpToThrowTime;
        timeline.Evaluate();
        timeline.Play();
    }

    // call this from your coroutine instead of FullReset directly
    public void RequestReloadAndSlowMo(double timeToJump, float timeScale = 0.7f)
    {
        // set static flags so the newly loaded scene knows what to do
        pendingSlowMoReplay = true;
        pendingTimelineTime = timeToJump;
        pendingTimeScale = timeScale;

        // housekeeping
        DOTween.KillAll();
        AudioListener.pause = false;

        // reload the scene (this will cause Start() to run again on scene objects)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
