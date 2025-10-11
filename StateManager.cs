using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class StateManager : Monobehavior {
  public GameObject panels[]; // insert titlepanel, subtitlepanel, questionpanel and pauseoverlay here
  public SubtitleScript subtitleScript; //i renamed it
  public PlayableDirector timeline;

      void Start()
    {
        // Setup logic
    }

  // METHODS
  void HideAllPanels() 
  {
    foreach (GameObject panel in panels) {
      panel.SetActive(false);
    }
  }
}
