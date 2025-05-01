using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 单例
    private static GameManager _instance;
    // Whether the game is in a tutorial state
    public bool hasSeenTutorial = false;
    public bool justFinishedTutorial = false;  // Whether the game just finished the tutorial (used in DayScene)

    [Header("Stage Management")]
    [SerializeField] private StageData tutorialStage;
    [SerializeField] private List<StageData> mainStages;
    private HashSet<string> completedStageIds = new HashSet<string>();

    [Header("Scene Management")]
    [SerializeField] private SceneFlowController sceneFlowController;
    
    public StageData GetTutorialStage() => tutorialStage;
    public List<StageData> GetMainStages() => mainStages;

    public void RegisterCompletedStage(StageData stage)
    {
        if (!string.IsNullOrEmpty(stage.id))
        {
            completedStageIds.Add(stage.id);
        }
    }

    public bool HasCompletedStage(string id) => completedStageIds.Contains(id);

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                SetupInstance();
            }
            return _instance;
        }
    }


    private void Awake()
    {
        // Make sure this is the only instance of GameManager
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject); // Don't destroy this object when loading a new scene
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Destroy this object if another instance already exists
        }
    }

    // Some logic to handle game state
    public void ResetForNewLoop()
    {
        hasSeenTutorial = false;
        completedStageIds.Clear();
    }
    
    private static void SetupInstance()
    {
        _instance = Object.FindFirstObjectByType<GameManager>();
        if (_instance == null)
        {
            GameObject gameObj = new GameObject("GameManager (Generated)");
            _instance = gameObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameObj);
        }
    }
    
    public void GoToNextScene(string condition)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = sceneFlowController.ResolveNextScene(currentScene, condition);
        SceneManager.LoadScene(nextScene);
    }
}