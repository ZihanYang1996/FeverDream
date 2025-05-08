using UnityEngine;

public class GameLevel2SceneManager : MonoBehaviour
{
    public GameObject showPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string NormalStageId = "Rocket";
    [SerializeField] private string SecretStageId = "Horse";
    [SerializeField] private GameObject testingGameObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Increment the current level index
        GameManager.Instance.IncrementCurrentLevelIndex();
        
        if (showPuzzleButton != null)
        {
            showPuzzleButton.SetActive(false); // hide initially
        }

        testingGameObject.GetComponent<ActorController>().MoveByDelta(new Vector3(20, 0, 0), 5, null, null, true);

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
            if (stage.id == NormalStageId)
            {
                // Increment the completed level count
                GameManager.Instance.IncrementCompletedLevel();
                
                GoToNextScene("NormalSolved");
            }
            else if (stage.id == SecretStageId)
            {
                // Increment the completed level count
                GameManager.Instance.IncrementCompletedLevel();
                
                GoToNextScene("SecretSolved");
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