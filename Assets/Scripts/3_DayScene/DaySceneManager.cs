using UnityEngine;

public class DaySceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryManager storyManager;

    [Header("Story Parts")]
    [SerializeField] private StoryStep[] part1Steps; // 第三幕前半段
    [SerializeField] private string tutorialScene = "TutorialScene"; // 新手教学
    [SerializeField] private StoryStep[] part2Steps; // 第三幕后半段


    private bool tutorialCompleted = false;

    private void Start()
    {
        if (!GameManager.Instance.hasSeenTutorial)
        {
            PlayPart1ThenTutorial();
        }
        else if (GameManager.Instance.justFinishedTutorial)
        {
            PlayRemainingPart();
            GameManager.Instance.justFinishedTutorial = false; // Reset the flag
        }
        else
        {
            PlayWholePartDirectly();
        }
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(tutorialScene);
    }

    private void PlayWholePartDirectly()
    {
        StoryStep[] combined = new StoryStep[part1Steps.Length + part2Steps.Length];
        part1Steps.CopyTo(combined, 0);
        part2Steps.CopyTo(combined, part1Steps.Length);

        storyManager.Play(combined);
    }

    private void PlayRemainingPart()
    {
        storyManager.Play(part2Steps, onComplete: () =>
        {
            GoToFirstLevel();
        });
    }
    
    // This method is called when the story is finished, to load the first level
    private void GoToFirstLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLevel1Scene");
    }
}