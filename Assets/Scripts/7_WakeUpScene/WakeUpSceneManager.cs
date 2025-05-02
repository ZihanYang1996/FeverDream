using UnityEngine;
using UnityEngine.UI;

public class WakeUpSceneManager : MonoBehaviour
{
    public Button continueButton;
    
    private int currentLevelIndex;
    private int numOfCompletedLevel;
    private int numOfCompletedHiddenLevel;
    void Start()
    {
        currentLevelIndex = GameManager.Instance.currentLevelIndex;
        Debug.Log($"Wake up at level {currentLevelIndex}");

        numOfCompletedLevel = GameManager.Instance.numOfCompletedLevel;
        Debug.Log($"Completed {numOfCompletedLevel} levels");
        
        numOfCompletedHiddenLevel = GameManager.Instance.numofCompletedHiddenLevel;
        Debug.Log($"Completed {numOfCompletedHiddenLevel} hidden levels");

        // Bind the continue button to the ContinueGame method
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }
    }

    void ContinueGame()
    {
        if (currentLevelIndex == 1)
        {
            GameManager.Instance.GoToNextScene("AtLevel1");
        }
        else if (currentLevelIndex == 2)
        {
            // At the last level of the game, for now.
            // Will loop back to DayScene, but reset the game state first.
            GameManager.Instance.ResetForNewLoop();
            GameManager.Instance.GoToNextScene("AtLevel2");
        }
    }
}
