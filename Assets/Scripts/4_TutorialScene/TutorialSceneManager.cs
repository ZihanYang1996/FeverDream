using UnityEngine;

public class TutorialSceneManager : MonoBehaviour
{
    public GameObject showPuzzleButton;
    [SerializeField] private StageManager stageManager;

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

    public void ReturnToDayScene()
    {
        GameManager.Instance.hasSeenTutorial = true;
        GameManager.Instance.justFinishedTutorial = true;
        GameManager.Instance.GoToNextScene(SceneTransitionConditions.Default);
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
            var tutorialStage = GameManager.Instance.GetTutorialStage();
            stageManager.SetupStages(new System.Collections.Generic.List<StageData> { tutorialStage });
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
            if (stage)
            {
                Debug.Log($"[Tutorial] Puzzle '{stage.id}' completed!");
            }
            else
            {
                Debug.LogError("[Tutorial] Stage is null.");
            }
            
            GameManager.Instance.RegisterCompletedStage(stage);
            ReturnToDayScene();
        }
        else
        {
            Debug.Log("[Tutorial] Puzzle failed. But it's fine.");
            ReturnToDayScene();
        }
    }
}