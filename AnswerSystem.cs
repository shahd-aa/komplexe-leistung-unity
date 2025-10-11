using UnityEngine;
using UnityEngine.UI,;

public class AnswerSystem : MonoBehaviour {
  // REFERENCES
  public Transform AnswerOptionsPanel;
  private List<Button> answerButtons = new List<Button>();
  public StateManager stateManager;
  
  // INDICES
  private int correctIndex;
  private int selectedIndex;
  
  //COLORS 
  public Color wrongColor = Color.red;
  public Color correctColor = Color.green;
  public Color selectedColor = Color.blue;

  public void Start()
  {
    foreach (Transform child in AnswerOptionsPanel) {
      
    }
  }

  // METHODS
  void SetupButtons()
  {
    // attach click listeners to all buttons
  }

  void OnAnswerClicked(int index)
  {
    // runs when player clicked an answer
  }

  void HighlightButton(int index)
  {
    // runs when button is selected
  }

  void UnhighlightAll(int index)
  {
    // unhighlights all buttins
  }

  void CheckAnswer()
  {
    // compares selected to correct (called by submit button)
  }

  void ShowCorrectAnswer()
  {
    // highlights correct answer
  }

  void ShowWrongAnswer()
  {
    // highlights wrong answers
  }
}
