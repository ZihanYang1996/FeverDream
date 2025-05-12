using UnityEngine;
using UnityEngine.UI;

public class WakeUpSceneManager : MonoBehaviour
{
    public Button continueButton;
    
    private int currentLevelIndex;
    private int numOfCompletedLevel;
    private int numOfCompletedHiddenLevel;
    private string currentTime;
    void Start()
    {
        currentLevelIndex = GameManager.Instance.currentLevelIndex;
        Debug.Log($"Wake up at level {currentLevelIndex}");

        numOfCompletedLevel = GameManager.Instance.numOfCompletedLevel;
        Debug.Log($"Completed {numOfCompletedLevel} levels");
        
        numOfCompletedHiddenLevel = GameManager.Instance.numOfCompletedHiddenLevel;
        Debug.Log($"Completed {numOfCompletedHiddenLevel} hidden levels");
        
        currentTime = GameManager.Instance.currentTime;

        // Bind the continue button to the ContinueGame method
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }
    }

    void ContinueGame()
    {
        if (currentTime == "1AM")
        {
            GameManager.Instance.GoToNextScene("1AM");
        }
        else if (currentTime == "3AM")
        {
            GameManager.Instance.GoToNextScene("3AM");
        }
        else if (currentTime == "5AM")
        {
            GameManager.Instance.GoToNextScene("5AM");
        }
        else if (currentTime == "7AM")
        {
            // At the last level of the game, for now.
            // Will loop back to DayScene, but reset the game state first.
            GameManager.Instance.ResetForNewLoop();
            GameManager.Instance.GoToNextScene("7AM");
        }
    }
}
