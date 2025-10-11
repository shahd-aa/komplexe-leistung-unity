using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SubtitleScript : MonoBehaviour
{
    public TMP_Text subtitleText;
    public List<string> subtitles = new List<string>();
    
    // Just displays subtitle - doesn't control timeline!
    public void ShowSubtitle(int index) 
    {
        if (index >= 0 && index < subtitles.Count)
        {
            subtitleText.text = subtitles[index];
        }
    }
    
    // Wrapper methods for Timeline Signals
    public void ShowSubtitle0() { ShowSubtitle(0); }
    public void ShowSubtitle1() { ShowSubtitle(1); }
    public void ShowSubtitle2() { ShowSubtitle(2); }
    public void ShowSubtitle3() { ShowSubtitle(3); }
}
