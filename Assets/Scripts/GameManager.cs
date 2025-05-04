using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
    public int totalNumberOfLevel { get; private set;  } = 2; // Total number of level in the game
    public int totalNumberOfHiddenLevel { get; private set;  } = 2; // Total number of hidden level in the game

    public int currentLevelIndex { get; private set;  } = 0; // Current level index, used by Wake Up Scene
    public int numOfCompletedLevel { get; private set; } = 0; // Number of completed level in the current loop
    public int numOfCompletedHiddenLevel { get; private set; } = 0; // Number of completed hidden level in the current loop

    public int numOfCompletedLevelLastLoop { get; private set; } = 0; // Number of completed level in the last loop
    public int numOfCompletedHiddenLevelLastLoop { get; private set; } = 0; // Number of completed hidden level in the last loop
    public void IncrementCompletedLevel()
    {
        numOfCompletedLevel++;
    }
    public void IncrementCompletedHiddenLevel()
    {
        numOfCompletedHiddenLevel++;
    }
    
    public void IncrementCurrentLevelIndex()
    {
        currentLevelIndex++;
    }
    
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
        // Reset the completed stages for the next loop
        numOfCompletedLevelLastLoop = numOfCompletedLevel;
        numOfCompletedLevel = 0;
        
        // Reset the completed hidden stages for the next loop
        numOfCompletedHiddenLevelLastLoop = numOfCompletedHiddenLevel;
        numOfCompletedHiddenLevel = 0;
        
        // Reset the current scene index
        currentLevelIndex = 0;
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