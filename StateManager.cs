using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class StateManager : Monobehavior {
  public GameObject panels[]; // insert titlepanel, subtitlepanel, questionpanel and pauseoverlay here
  public SubtitleScript subtitleScript; //i renamed it
  //UI ELEMENTS
  public Transform canvas;
  private List<GameObject> allUIElements = new List<GameObject>();
  public Button submitButton;
  public GameObject[] answerOptionsPanel;
  private GameObject titlePanel = panels[0];
  private GameObject subtitlePanel = panels[1];
  private GameObject questionPanel = panels[2];
  private GameObject quizPanel = panels[3];
  
  public PlayableDirector timeline;
  private float duration;
  
    void Start()
    {
        // Setup logic
      HideAllPanels();
      StartCoroutine(TitleScreenSequence());
    }

  // METHODS
  void HideAllPanels() 
  {
    foreach (GameObject panel in panels) {
      panel.SetActive(false);
    }
  }

  IEnumerator TitleScreenSequence() 
  {
    titlePanel.SetActive(true);
    yield return new WaitForSeconds(10f);
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
    pd.time = pd.duration //pause at last frame
    pd.Pause();
    ShowQuizSequence();
  }

  void ShowQuizSequence()
  {
    // show quiz panel
    quizPanel.SetActive(true);
    
    // show question first
    questionPanel.SetActive(true);
    // wait a specific time of duration
    yield return new WaitForSeconds(duration);
    // show answer options one by one
    answerOptionPanel.SetActive(true);

    foreach (option in answerOptionsPanel) {
      option.SetActive(true);
      
    }
  }
}
