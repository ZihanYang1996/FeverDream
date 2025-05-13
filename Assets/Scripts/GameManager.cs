using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using DialogueSystem;

public class GameManager : MonoBehaviour
{
    // 单例
    private static GameManager _instance;
    // Whether the game is in a tutorial state
    public bool hasSeenTutorial = false;
    public bool justFinishedTutorial = false;  // Whether the game just finished the tutorial (used in DayScene)

    [Header("Game Settings")]
    public Language currentLanguage = Language.ZH;
    
    [Header("Stage Management")]
    [SerializeField] private StageData tutorialStage;
    [SerializeField] private List<StageData> mainStages;
    [SerializeField] public int blackScreenAlphaForPuzzle = 245; // Alpha value for the black screen when playing the puzzle
    private HashSet<string> completedStageIds = new HashSet<string>();
    
    [Header("Dialogue Settings")]
    [SerializeField] public StyleSettings defaultStyle;
    [SerializeField] public StyleSettings thoughtStyle;
    [SerializeField] public  StyleSettings narratorStyle;

    [Header("Scene Management")]
    [SerializeField] private SceneFlowController sceneFlowController;
    [SerializeField] public float blackScreenFadeDuration = 1f; // Duration for black screen fade in/out between scenes
    [SerializeField] public float blackScreenStayDuration = 1f; // Duration for black screen to stay before fading out
    public int totalNumberOfLevel { get; private set;  } = 2; // Total number of level in the game
    public int totalNumberOfHiddenLevel { get; private set;  } = 2; // Total number of hidden level in the game

    public int currentLevelIndex { get; private set;  } = 0; // Current level index, used by Wake Up Scene
    public int numOfCompletedLevel { get; private set; } = 0; // Number of completed level in the current loop
    public int numOfCompletedHiddenLevel { get; private set; } = 0; // Number of completed hidden level in the current loop

    public int numOfCompletedLevelLastLoop { get; private set; } = 0; // Number of completed level in the last loop
    public int numOfCompletedHiddenLevelLastLoop { get; private set; } = 0; // Number of completed hidden level in the last loop
    
    public string currentTime = "1AM"; // Current time in the game, used by Wake Up Scene
    
    [SerializeField] public Vector3 defaultStartPuzzelButtonPosition = new Vector3(750f, -350f, 0f); // Start puzzle button position in the scene
    
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
        
        // Reset the current time
        currentTime = "1AM";
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