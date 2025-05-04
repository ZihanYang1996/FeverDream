using UnityEngine;
using　System.Collections.Generic;

public class DaySceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryManager storyManager;

    [Header("Story Parts")]
    [SerializeField] private StoryStep[] step1;
    [SerializeField] private StoryStep step2;
    [SerializeField] private StoryStep[] step3;
    [SerializeField] private StoryStep step4;
    
    private StoryStep[] part1Steps; // Before the tutorial

    [SerializeField] private StoryStep[] part2Steps; // After the tutorial

    private string sceneTransitionCondition = "FirstTime"; // 过渡条件

    private void Start()
    {
        // Log the game state (for debugging purposes)
        Debug.Log($"Current Level Index: {GameManager.Instance.currentLevelIndex}");
        Debug.Log($"Completed Levels: {GameManager.Instance.numOfCompletedLevel}");
        Debug.Log($"Completed Hidden Levels: {GameManager.Instance.numOfCompletedHiddenLevel}");
        Debug.Log($"Last Loop Completed Levels: {GameManager.Instance.numOfCompletedLevelLastLoop}");
        Debug.Log($"Last Loop Completed Hidden Levels: {GameManager.Instance.numOfCompletedHiddenLevelLastLoop}");
        
        // Choose the Step 1 based on the game state
        List<StoryStep> tempPart1Steps = new List<StoryStep>(); // Before the tutorial
        if (GameManager.Instance.numOfCompletedLevelLastLoop == 0)
        {
            tempPart1Steps.Add(step1[0]);
        }
        else if (GameManager.Instance.numOfCompletedLevelLastLoop == 1)
        {
            tempPart1Steps.Add(step1[1]);
        }
        else if (GameManager.Instance.numOfCompletedLevelLastLoop == 2)
        {
            tempPart1Steps.Add(step1[2]);
        }
        else if (GameManager.Instance.numOfCompletedLevelLastLoop == 3)
        {
            tempPart1Steps.Add(step1[3]);
        }
        else
        {
            // Error handling: if the last loop completed levels are not in the expected range
            Debug.LogError("DaySceneManager: Invalid number of completed levels in the last loop.");
        }
        
        // Add the Step2 to the part1Steps
        tempPart1Steps.Add(step2);
        
        // Choose the Step 3 based on the game state
        if (GameManager.Instance.numOfCompletedLevelLastLoop == 0)
        {
            tempPart1Steps.Add(step3[0]);
        }
        else if (GameManager.Instance.numOfCompletedLevelLastLoop == 1)
        {
            tempPart1Steps.Add(step3[1]);
        }
        else if (GameManager.Instance.numOfCompletedLevelLastLoop == 2)
        {
            tempPart1Steps.Add(step3[2]);
        }
        else if (GameManager.Instance.numOfCompletedLevelLastLoop == 3)
        {
            tempPart1Steps.Add(step3[3]);
        }
        else
        {
            // Error handling: if the last loop completed levels are not in the expected range
            Debug.LogError("DaySceneManager: Invalid number of completed levels in the last loop.");
        }
            
        // Add the Step4 to the part1Steps
        tempPart1Steps.Add(step4);
        
        // Convert the List to an array
        part1Steps = tempPart1Steps.ToArray();
        
        if (!GameManager.Instance.hasSeenTutorial)
        {
            sceneTransitionCondition = "FirstTime"; // It's the first time playing, so we need to show the tutorial
            PlayPart1ThenTutorial();
        }
        else if (GameManager.Instance.justFinishedTutorial)
        {
            sceneTransitionCondition = "Normal"; // Tutorial has been completed, so we can proceed normally
            PlayRemainingPart();
            GameManager.Instance.justFinishedTutorial = false; // Reset the flag
        }
        else
        {
            sceneTransitionCondition = "Normal"; // No tutorial needed, proceed normally
            PlayWholePartDirectly();
        }

        // Decide
    }

    private void PlayPart1ThenTutorial()
    {
        storyManager.Play(part1Steps, onComplete: () =>
        {
            GameManager.Instance.hasSeenTutorial = true;
            OpenTutorial();
        });
    }

    private void OpenTutorial()
    {
        GameManager.Instance.GoToNextScene(sceneTransitionCondition);
    }

    private void PlayWholePartDirectly()
    {
        StoryStep[] combined = new StoryStep[part1Steps.Length + part2Steps.Length];
        part1Steps.CopyTo(combined, 0);
        part2Steps.CopyTo(combined, part1Steps.Length);

        storyManager.Play(combined, onComplete: () => { GoToFirstLevel(); });
    }

    private void PlayRemainingPart()
    {
        storyManager.Play(part2Steps, onComplete: () => { GoToFirstLevel(); });
    }

    // This method is called when the story is finished, to load the first level
    private void GoToFirstLevel()
    {
        GameManager.Instance.GoToNextScene(sceneTransitionCondition);
    }
}