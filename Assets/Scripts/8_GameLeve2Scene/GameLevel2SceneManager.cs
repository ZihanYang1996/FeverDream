using System.Collections;
using DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class GameLevel2SceneManager : MonoBehaviour
{
    [FormerlySerializedAs("showPuzzleButton")] public GameObject startPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string NormalStageId = "Sword";
    [SerializeField] private string SecretStageId = "Butterfly";

    [Header("Actors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject blackScreenImage;
    [SerializeField] private GameObject generatedTangramHolder;

    [Header("Dialogue")]
    [SerializeField] private string dialogueFileName1;

    [SerializeField] private DialogueManager dialogueManager;

    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve cameraSwordCurve;

    [SerializeField] private AnimationCurve cameraFocusCurve;

    [SerializeField] private AnimationCurve characterFallCurve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        character.SetActive(true);

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
            StartCoroutine(PlaySwordAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else if (stageID == SecretStageId)
        {
            // Play the secret stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Secret Stage: {stageID}");
            // Add your animation logic here
            // StartCoroutine(PlayButterflyAnimationCoroutine((() => { onComplete?.Invoke(); })));
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

        // Character fall from the sky
        bool isMoveComplete = false;
        float duration = 5.0f;
        Vector3 targetPosition = new Vector3(-100f, -13f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }), curve: characterFallCurve);
        // // Character rotation
        bool isRotateComplete = false;
        float singleRotationDuration = duration / 2f;
        float singleRotationAngle = 180f;
        character.GetComponent<ActorController>()
            .RotateByDelta(singleRotationAngle, singleRotationDuration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete);
        // Another rotation
        character.GetComponent<ActorController>()
            .RotateByDelta(singleRotationAngle, singleRotationDuration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete & isMoveComplete);

        // Character rotate dleta -150 degree
        isRotateComplete = false;
        float rotateDelta = -150f;
        duration = 1.0f;
        character.GetComponent<ActorController>()
            .RotateByDelta(rotateDelta, duration, (() => { isRotateComplete = true; }));
        // Character bounce up and down, Vector3(-95,-10,0), Vector3(-92,-16,0)
        isMoveComplete = false;
        targetPosition = new Vector3(-95f, -10f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration / 3f, (() => { isMoveComplete = true; }));
        yield return new WaitUntil(() => isMoveComplete);
        isMoveComplete = false;
        targetPosition = new Vector3(-92f, -18f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration * 2 / 3f, (() => { isMoveComplete = true; }));
        yield return new WaitUntil(() => isMoveComplete && isRotateComplete);

        // Stop the camera from following the character
        mainCamera.GetComponent<CameraController>().StopFollowing();

        // Wait for a moment
        yield return new WaitForSeconds(1.0f);

        // Character stand up (rotate to -5 degree)
        duration = 1.0f;
        isRotateComplete = false;
        float targetRotation = -5f;
        character.GetComponent<ActorController>()
            .RotateTo(targetRotation, duration, (() => { isRotateComplete = true; }));

        // Character move up and left a little bit
        Vector3 deltaPosition = new Vector3(-2f, 2f, 0f);
        isMoveComplete = false;
        character.GetComponent<ActorController>()
            .MoveByDelta(deltaPosition, duration, (() => { isMoveComplete = true; }));


        yield return new WaitUntil(() => isRotateComplete && isMoveComplete);


        // Camera move up a liittle bit
        isMoveComplete = false;
        targetPosition = new Vector3(-91f, -12f, -10f);
        mainCamera.GetComponent<CameraController>()
            .MoveTo(targetPosition, duration, cameraFocusCurve, (() => { isMoveComplete = true; }));
        yield return new WaitUntil(() => isMoveComplete);

        // Actor and Camera move right, then left, then right (stop on the left side of the screen)
        // Move right
        float characterDuration = 5.0f;
        float lagDuration = 0.5f;
        float cameraDuration = characterDuration - lagDuration;
        
        // Set character's move offset frequency
        character.GetComponent<WalkingPathOffset>().frequency = 20;

        bool isCharacterMoveComplete = false;
        bool isCameraMoveComplete = false;
        Vector3 characterTargetPosition = new Vector3(-30f, -16f, 0f);
        Vector3 cameraTargetPosition = new Vector3(-48f, -12f, -10f);
        character.GetComponent<ActorController>()
            .MoveToPosition(characterTargetPosition, characterDuration, (() => { isCharacterMoveComplete = true; }),
                usePathOffset: true, settleDuration: 0.1f);
        yield return new WaitForSeconds(lagDuration);
        mainCamera.GetComponent<CameraController>()
            .MoveTo(cameraTargetPosition, cameraDuration, curve: cameraFocusCurve,
                onComplete: (() => { isCameraMoveComplete = true; }));
        yield return new WaitUntil(() => isCharacterMoveComplete && isCameraMoveComplete);

        // Move left
        characterDuration = 5.0f;
        lagDuration = 1.5f;
        cameraDuration = characterDuration - lagDuration;
        
        // Flip the character
        character.GetComponent<ActorController>().FlipActor();
        
        isCharacterMoveComplete = false;
        isCameraMoveComplete = false;
        characterTargetPosition = new Vector3(-90f, -16f, 0);
        cameraTargetPosition = new Vector3(-74, -12f, -10f);
        character.GetComponent<ActorController>()
            .MoveToPosition(characterTargetPosition, characterDuration, (() => { isCharacterMoveComplete = true; }),
                usePathOffset: true, settleDuration: 0.1f);
        yield return new WaitForSeconds(lagDuration);
        mainCamera.GetComponent<CameraController>()
            .MoveTo(cameraTargetPosition, cameraDuration, curve: cameraFocusCurve,
                onComplete: (() => { isCameraMoveComplete = true; }));
        yield return new WaitUntil(() => isCharacterMoveComplete && isCameraMoveComplete);

        // Move right
        characterDuration = 2.0f;
        lagDuration = 0.7f;
        cameraDuration = characterDuration - lagDuration;
        
        // Flip the character
        character.GetComponent<ActorController>().FlipActor();
        // Set character's move offset frequency
        character.GetComponent<WalkingPathOffset>().frequency = 10;

        isCharacterMoveComplete = false;
        isCameraMoveComplete = false;
        characterTargetPosition = new Vector3(-68, -16f, 0);
        cameraTargetPosition = new Vector3(-64, -12f, -10f);
        character.GetComponent<ActorController>()
            .MoveToPosition(characterTargetPosition, characterDuration, (() => { isCharacterMoveComplete = true; }),
                usePathOffset: true, settleDuration: 0.5f);
        yield return new WaitForSeconds(lagDuration);
        mainCamera.GetComponent<CameraController>()
            .MoveTo(cameraTargetPosition, cameraDuration,
                onComplete: (() => { isCameraMoveComplete = true; }));
        yield return new WaitUntil(() => isCharacterMoveComplete && isCameraMoveComplete);

        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }

    private IEnumerator PlaySwordAnimationCoroutine(System.Action onComplete)
    {
        // Set the Tangram holder's position
        generatedTangramHolder.transform.position = new Vector3(4.51000023f, -1.47000003f, -5.06389952f);
        // Add ActorController component to the generated tangram holder
        var tangramHolderActorController = generatedTangramHolder.AddComponent<ActorController>();

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
    }
}