using System.Collections;
using DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class GameLevel2SceneManager : MonoBehaviour
{
    [FormerlySerializedAs("showPuzzleButton")]
    public GameObject startPuzzleButton;

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
        generatedTangramHolder.transform.position = new Vector3(-59.5200005f, -13.4899998f, -5.06389952f);
        // Add ActorController component to the generated tangram holder 
        var tangramHolderActorController = generatedTangramHolder.AddComponent<ActorController>();
        // Add TreeCutDetector component to the generated tangram holder
        var treeCutDetector = generatedTangramHolder.AddComponent<TreeCutDetector>();

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

        // Move the sword close to the character
        // Move the generated tangram holder to the target position: Vector3(-66.2799988,-15.29,0)
        // Rotate the generated tangram holder to the target rotation: 132
        // Scale the generated tangram holder to the target scale: Vector3(0.159999996,0.297142833,0.159999996)
        bool isMoveComplete = false;
        bool isRotateComplete = false;
        bool isScaleComplete = false;
        float duration = 1.0f;
        Vector3 targetPosition = new Vector3(-66.2799988f, -15.29f, 0f);
        float targetRotation = 135f;
        Vector3 targetScale = new Vector3(0.159999996f, 0.297142833f, 0.159999996f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }));
        tangramHolderActorController.RotateTo(targetRotation, duration, (() => { isRotateComplete = true; }));
        tangramHolderActorController.ScaleTo(targetScale, duration, (() => { isScaleComplete = true; }));
        yield return new WaitUntil(() => isMoveComplete && isRotateComplete && isScaleComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // The sword fly up
        // Move the generated tangram holder to the target position: Vector3(-64.2300034,-14.3100004,-5.06389952)
        isMoveComplete = false;
        duration = 0.3f;
        targetPosition = new Vector3(-64.2300034f, -14.3100004f, -5.06389952f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }),
            curve: cameraSwordCurve);
        yield return new WaitUntil(() => isMoveComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // The sword ratate
        // Rotate the generated tangram holder to the target rotation: -270 (delta is -420) (split into several steps)
        float deltaRotation = -90f;
        duration = 0.2f;
        isRotateComplete = false;
        tangramHolderActorController.RotateByDelta(deltaRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete);
        deltaRotation = -90f;
        duration = 0.2f;
        isRotateComplete = false;
        tangramHolderActorController.RotateByDelta(deltaRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete);
        deltaRotation = -90f;
        duration = 0.2f;
        isRotateComplete = false;
        tangramHolderActorController.RotateByDelta(deltaRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete);
        deltaRotation = -90f;
        duration = 0.2f;
        isRotateComplete = false;
        tangramHolderActorController.RotateByDelta(deltaRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete);
        deltaRotation = -45f;
        duration = 0.2f;
        isRotateComplete = false;
        tangramHolderActorController.RotateByDelta(deltaRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete);


        // Wait for a moment
        yield return new WaitForSeconds(1f);


        // The sword launch to the right
        // Make the sword start cutting the tree
        treeCutDetector.StartCutting();
        // Make the camera follow the generated tangram holder
        mainCamera.GetComponent<CameraController>().FollowTarget(generatedTangramHolder.transform, 0.1f);
        // Move the generated tangram holder to the target position: Vector3(50.0999985,-14.3100004,-5.06389952)
        duration = 2f;
        isMoveComplete = false;
        targetPosition = new Vector3(50.0999985f, -14.3100004f, -5.06389952f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }));
        yield return new WaitUntil(() => isMoveComplete);
        
        // Deactivate the generated tangram holder
        generatedTangramHolder.SetActive(false);

        // Stop the camera from following the generated tangram holder
        mainCamera.GetComponent<CameraController>().StopFollowing();


        // Wait for a moment
        yield return new WaitForSeconds(2f);

        // Teleport the character to the place on the left side of the screen (out of the screen)
        // Teleport the chacter to the target position: Vector3(15,-16,0)
        targetPosition = new Vector3(15f, -16f, 0f);
        character.GetComponent<ActorController>().TeleportToPosition(targetPosition);

        // Character move into the screen
        // Move the character to the target position: Vector3(37,-16,0)
        duration = 1.0f;
        isMoveComplete = false;
        targetPosition = new Vector3(37f,-16f,0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }), usePathOffset: true,
                settleDuration: 0.5f);
        yield return new WaitUntil(() => isMoveComplete);
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Character move out of the screen on right
        // // Move the character to the target position: Vector3(46,-16,0)
        duration = 1.0f;
        isMoveComplete = false;
        targetPosition = new Vector3(46f, -16f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }), usePathOffset: true,
                settleDuration: 0.5f);
        yield return new WaitUntil(() => isMoveComplete);
    }
}