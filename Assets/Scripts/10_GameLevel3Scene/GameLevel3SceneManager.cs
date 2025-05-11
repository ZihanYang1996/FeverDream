using System.Collections;
using UnityEngine;

public class GameLevel3SceneManager : MonoBehaviour
{
    public GameObject startPuzzleButton;

    [SerializeField] private StageManager stageManager;

    [SerializeField] private string NormalStageId = "Camel";
    [SerializeField] private string SecretStageId = "Bottle";

    [Header("Actors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject blackScreenImage;
    [SerializeField] private GameObject generatedTangramHolder;

    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve cameraFocusCurve;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        character.SetActive(false);
        mainCamera.GetComponent<CameraController>().SnapTo(new Vector3(4f, 22f, -10f));
        mainCamera.GetComponent<CameraController>().ZoomInstantlyTo(28f);


        // Increment the current level index
        GameManager.Instance.IncrementCurrentLevelIndex();

        if (startPuzzleButton != null)
        {
            startPuzzleButton.SetActive(false); // hide initially
        }

        if (stageManager != null)
        {
            stageManager.OnPuzzleComplete += HandlePuzzleResult;
        }
        else
        {
            Debug.LogError("StageManager reference not assigned in the inspector.");
        }

        // Start the pre-puzzle animation
        StartCoroutine(PlayPrePuzzleAnimationCoroutine());
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
        if (startPuzzleButton != null)
        {
            startPuzzleButton.SetActive(true);
            startPuzzleButton.GetComponent<ButtonEntranceEffect>().PlayEntranceAnimation();
        }
        else
        {
            Debug.LogWarning("ShowPuzzleButton is not assigned in the inspector.");
        }

        // StageManager stageManager = FindAnyObjectByType<StageManager>();
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
                // Play the post-puzzle animation
                PlayPostPuzzleAnimation(stage.id, () =>
                {
                    // After the animation is finished, go to the next scene
                    GoToNextScene("NormalSolved");
                });
            }
            else if (stage.id == SecretStageId)
            {
                // Increment the completed level count
                GameManager.Instance.IncrementCompletedLevel();
                // Play the post-puzzle animation
                PlayPostPuzzleAnimation(stage.id, () =>
                {
                    // After the animation is finished, go to the next scene
                    GoToNextScene("SecretSolved");
                });
            }
            else
            {
                // Play the post-puzzle animation
                PlayPostPuzzleAnimation(stage.id, () =>
                {
                    // After the animation is finished, go to the next scene
                    GoToNextScene("WrongSolved");
                });
            }
        }
        else
        {
            Debug.Log("[Game Leve 1] Puzzle failed.");
            GoToNextScene("TimeOut");
        }
    }

    private void PlayPostPuzzleAnimation(string stageID, System.Action onComplete)
    {
        // Play the post-puzzle animation based on the stage ID
        if (stageID == NormalStageId)
        {
            // Play the normal stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Normal Stage: {stageID}");
            // Add your animation logic here
            // StartCoroutine(PlayCamelAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else if (stageID == SecretStageId)
        {
            // Play the secret stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Secret Stage: {stageID}");
            // Add your animation logic here
            // StartCoroutine(PlayBottleAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else
        {
            Debug.LogWarning($"No specific post-puzzle animation for stage: {stageID}");
        }
    }

    private IEnumerator PlayPrePuzzleAnimationCoroutine()
    {
        // Start with black screen fade out
        if (blackScreenImage != null)
        {
            bool isSceneTransitionComplete = false;
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.SceneStartFadeOut((() => { isSceneTransitionComplete = true; }));
            yield return new WaitUntil(() => isSceneTransitionComplete);
        }
        else
        {
            Debug.LogError("Black screen image not assigned in the inspector.");
        }

        // Delay before starting the animation
        yield return new WaitForSeconds(1.0f);

        // Zoom in (size = 8) and move the camera to Vector3(-14,32,-10)
        bool isCameraMoveComplete = false;
        bool isCameraZoomComplete = false;
        Vector3 targetPosition = new Vector3(-14f, 32f, -10f);
        float targetSize = 8f;
        float duration = 2f;
        mainCamera.GetComponent<CameraController>()
            .MoveTo(targetPosition, duration, cameraFocusCurve, (() => { isCameraMoveComplete = true; }));
        mainCamera.GetComponent<CameraController>()
            .ZoomTo(targetSize, duration, cameraFocusCurve, (() => { isCameraZoomComplete = true; }));
        yield return new WaitUntil(() => isCameraMoveComplete && isCameraZoomComplete);

        // Start the character animation move to Vector3(-20,28,0)
        character.SetActive(true);
        character.GetComponent<WalkingPathOffset>().frequency = 10;
        character.GetComponent<WalkingPathOffset>().amplitude = 0.2f;
        bool isCharacterMoveComplete = false;
        Vector3 characterTargetPosition = new Vector3(-20f, 28f, 0f);
        float characterDuration = 5f;
        character.GetComponent<ActorController>()
            .MoveToPosition(characterTargetPosition, characterDuration, (() => { isCharacterMoveComplete = true; }),
                usePathOffset: true, settleDuration: 0.5f);
        yield return new WaitUntil(() => isCharacterMoveComplete);


        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }
}