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
            StartCoroutine(PlayCamelAnimationCoroutine((() => { onComplete?.Invoke(); })));
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

    private IEnumerator PlayCamelAnimationCoroutine(System.Action onComplete)
    {
        // Set the Tangram holder's position
        generatedTangramHolder.transform.position = new Vector3(-9.53999996f, 30.4799995f, -5.06389952f);
        // Add ActorController component to the generated tangram holder
        var tangramHolderActorController = generatedTangramHolder.AddComponent<ActorController>();
        // Add WalkingPathOffset component to the generated tangram holder
        var walkingPathOffset = generatedTangramHolder.AddComponent<WalkingPathOffset>();
        walkingPathOffset.amplitude = 0.7f;
        walkingPathOffset.frequency = 6f;
        walkingPathOffset.horizontalSway = 0.1f;


        // Fade in the generated tangram holder
        bool isFadeComplete = false;
        tangramHolderActorController.FadeToAlpha(1f, stageManager.generatedTangramFlickerDuration,
            (() => isFadeComplete = true));
        // Wait until the fade is finished
        yield return new WaitUntil(() => isFadeComplete);

        // Fade out the black screen
        bool isFadeOutComplete = false;
        float fadeDuration = 1.0f;
        if (blackScreenImage != null)
        {
            blackScreenImage.SetActive(true);
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.StartFadeOut(fadeDuration, (() => { isFadeOutComplete = true; }));
        }

        // Wait for the fade-out to complete
        yield return new WaitUntil(() => isFadeOutComplete);
        Debug.Log("Fade out complete");
        blackScreenImage.SetActive(false);

        // Set the child object of the generated tangram's sprite to the "Actor" sorting layer and set the order to 7
        var tangramHolderSpriteRenderer = generatedTangramHolder.GetComponentInChildren<SpriteRenderer>();
        if (tangramHolderSpriteRenderer != null)
        {
            tangramHolderSpriteRenderer.sortingLayerName = "Actors";
            tangramHolderSpriteRenderer.sortingOrder = 7;
        }
        else
        {
            Debug.LogError("Tangram holder does not have a child with a SpriteRenderer component.");
        }

        // Move the generated tangram holder to the target position: Vector3(0.360000014,-0.49000001,-5.06389952)
        // Scale it to Vector3(2.45000005,2.45000005,2.45000005)
        bool isMoveComplete = false;
        float duration = 1.0f;
        Vector3 targetPosition = new Vector3(-14.8000002f, 30.1599998f, -5.06389952f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        // Wait until the move and scale are finished
        yield return new WaitUntil(() => isMoveComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Character moves to the target position: Vector3(-15.6000004,30.8299999,0)
        isMoveComplete = false;
        Vector3 characterTargetPosition = new Vector3(-15.6000004f, 30.8299999f, 0);
        duration = 1.0f;
        character.GetComponent<ActorController>()
            .MoveToPosition(characterTargetPosition, duration, (() => { isMoveComplete = true; }),
                usePathOffset: true, settleDuration: 0.5f);
        yield return new WaitUntil(() => isMoveComplete);

        // Character is set to be a child of the generated tangram holder
        character.transform.SetParent(generatedTangramHolder.transform);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Move the generated tangram to the target position: Vector3(4,30.1599998,-5.06389952)
        isMoveComplete = false;
        targetPosition = new Vector3(4f, 30.1599998f, -5.06389952f);
        duration = 3.0f;
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; },
            usePathOffset: true, settleDuration: 0.5f);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Fade in the black screen
        bool isFadeInComplete = false;
        if (blackScreenImage != null)
        {
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.SceneEndFadeIn((() => { isFadeInComplete = true; }));
        }

        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeInComplete);

        // Call the onComplete action after the animation is finished
        onComplete?.Invoke();
    }
}