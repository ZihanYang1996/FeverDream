using UnityEngine;

public class DaySceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryManager storyManager;

    [Header("Story Parts")]
    [SerializeField] private StoryStep[] part1Steps; // 第三幕前半段

    [SerializeField] private StoryStep[] part2Steps; // 第三幕后半段

    private string sceneTransitionCondition = "FirstTime"; // 过渡条件

    private void Start()
    {
        // Log the game state (for debugging purposes)
        Debug.Log($"Current Level Index: {GameManager.Instance.currentLevelIndex}");
        Debug.Log($"Completed Levels: {GameManager.Instance.numOfCompletedLevel}");
        Debug.Log($"Completed Hidden Levels: {GameManager.Instance.numofCompletedHiddenLevel}");
        Debug.Log($"Last Loop Completed Levels: {GameManager.Instance.numOfCompletedLevelLastLoop}");
        Debug.Log($"Last Loop Completed Hidden Levels: {GameManager.Instance.numOfCompletedHiddenLevel}");
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