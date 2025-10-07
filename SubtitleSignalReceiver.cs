using UnityEngine;
using UnityEngine.Playables;

public class SubtitleSignalReceiver : MonoBehaviour
{
    public LevelStateManager stateManager;
    
    // These methods will be called by Timeline Signals
    public void ShowSubtitle1()
    {
        stateManager.UpdateSubtitle("Ein Auto fährt mit drei Kisten auf dem Dach");
    }
    
    public void ShowSubtitle2()
    {
        stateManager.UpdateSubtitle("Das Auto nimmt eine scharfe Kurve");
    }
    
    public void ShowSubtitle3()
    {
        stateManager.UpdateSubtitle("Die Kisten fallen vom Dach!");
    }
}
