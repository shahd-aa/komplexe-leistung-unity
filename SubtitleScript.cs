using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Playables;

public class SubtitleScript: MonoBehaviour
{
    // Reference to PlayableDirector
    public PlayableDirector playableDirector;
    
    // Reference to subtitle text
    public TMP_Text subtitleText;
    
    // Reference to subtitle panel
    public GameObject subtitlePanel;
    
    // Reference to the actual subtitle text
    public List<string> subtitles = new List<string>();
    
    void Start()
    {
        // Shows subtitle panel
        subtitlePanel.SetActive(true);
        
        // Starts timeline playing
        playableDirector.Play();
        
        // Adds listener for when timeline ends
        playableDirector.stopped += OnTimelineEnded;
    }
    
    public void ShowSubtitle(int index) 
    {
        if (index >= 0 && index < subtitles.Count)
        {
            subtitleText.text = subtitles[index];
        }
    }
    
    public void ShowSubtitle0()
    {
        ShowSubtitle(0);
    }
    
    public void ShowSubtitle1()
    {
        ShowSubtitle(1);
    }
    
    public void ShowSubtitle2()
    {
        ShowSubtitle(2);
    }
    
    public void ShowSubtitle3()
    {
        ShowSubtitle(3);
    }
    
    private void OnTimelineEnded(PlayableDirector director)
    {
        Debug.Log("Timeline ended!");
    }
    
    void Update()
    {
        
    }
}
