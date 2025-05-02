using UnityEngine;

public class GameLevel1SceneHiddenManager : MonoBehaviour
{
    public GameObject showPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string CorrectStageId = "Windmill";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (showPuzzleButton != null)
        {
            showPuzzleButton.SetActive(false); // hide initially
        }

        StartPuzzle();

        if (stageManager != null)
        {
            stageManager.OnPuzzleComplete += HandlePuzzleResult;
        }
        else
        {
            Debug.LogError("StageManager reference not assigned in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void GoToNextScene(string condition)
    {
        GameManager.Instance.GoToNextScene(condition);
    }

    public void StartPuzzle()
    {
        if (showPuzzleButton != null)
        {
            showPuzzleButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ShowPuzzleButton is not assigned in the inspector.");
        }

        StageManager stageManager = FindAnyObjectByType<StageManager>();
        if (stageManager != null)
        {
            var mainStages = GameManager.Instance.GetMainStages();
            stageManager.SetupStages(mainStages);
        }
        else
        {
            Debug.LogError("StageManager not found in the scene.");
        }
    }

    private void HandlePuzzleResult(bool success, StageData stage)
    {
        if (success)
        {
            Debug.Log($"[Game Leve 1] Puzzle '{stage.id}' completed!");
            GameManager.Instance.RegisterCompletedStage(stage);

            // Determine the scene transition condition based on the stage ID
            if (stage.id == CorrectStageId)
            {
                // Increment the number of completed hidden levels
                GameManager.Instance.IncrementCompletedHiddenLevel();
                
                GoToNextScene("CorrectSolved");
            }
            else
            {
                // Maybe in the future we will handle wrong stages as well
                GoToNextScene("WrongSolved");
            }
        }
        else
        {
            Debug.Log("[Game Leve 1] Puzzle failed.");
            GoToNextScene("TimeOut");
        }
    }
}