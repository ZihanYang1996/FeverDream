using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [SerializeField] private List<string> selectedStageIds = new List<string> { "Sword", "Butterfly", "Heart" };


    [Header("Actors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject mushroomHouse;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject sandStorm;

    [SerializeField] private GameObject blackScreenImage;
    [SerializeField] private GameObject generatedTangramHolder;

    [Header("Dialogue")]
    [SerializeField] private string dialogueFileName1;

    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string dialogueFileName4;
    [SerializeField] private string dialogueFileName5;
    [SerializeField] private string dialogueFileName6;
    [SerializeField] private string dialogueFileName7;
    [SerializeField] private string dialogueFileName8;
    [SerializeField] private string dialogueFileName9;
    [SerializeField] private string dialogueFileName10;
    [SerializeField] private string dialogueFileName11;
    [SerializeField] private string dialogueFileName12;
    [SerializeField] private string dialogueFileName13;
    [SerializeField] private string dialogueFileName14;
    [SerializeField] private string dialogueFileName15;
    [SerializeField] private string dialogueFileName16;
    [SerializeField] private string dialogueFileName17;
    [SerializeField] private string dialogueFileName18;

    [SerializeField] private DialogueManager dialogueManager;

    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve swordFlyCurve;

    [SerializeField] private AnimationCurve cameraFocusCurve;

    [SerializeField] private AnimationCurve characterFallCurve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        character.SetActive(true);
        mushroomHouse.SetActive(false);
        sandStorm.SetActive(false);

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
            
            // Filter the main stages to only include the correct stage
            List<StageData> selectedStages = mainStages.Where(stage => selectedStageIds.Contains(stage.id)).ToList();
            stageManager.SetupStages(selectedStages);
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
            Debug.Log($"[Game Level 2] Puzzle '{stage.id}' completed!");
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
                    // Set the current time to 3AM
                    GameManager.Instance.currentTime = "3AM";
                    // After the animation is finished, go to the next scene
                    GoToNextScene("WrongSolved");
                });
            }
        }
        else
        {
            Debug.Log("[Game Level 2] Puzzle failed.");
            StartCoroutine(PlayFailedAnimationCoroutine(() =>
            {
                // Set the current time to 3AM
                GameManager.Instance.currentTime = "3AM";
                // After the animation is finished, go to the next scene
                GoToNextScene("TimeOut");
            }));
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
            StartCoroutine(PlayButterflyAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else
        {
            StartCoroutine(PlayFailedAnimationCoroutine((() => { onComplete?.Invoke(); })));
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
        
        // Play Dialogue 1
        bool isDialogueFinished = false;
        var dialogueAsset1 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName1);
        if (dialogueAsset1 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName1}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset1, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
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

        // Play Dialogue 2
        isDialogueFinished = false;
        var dialogueAsset2 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName2);
        if (dialogueAsset2 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName2}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset2, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
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
        
        // Play Dialogue 3
        isDialogueFinished = false;
        var dialogueAsset3 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName3);
        if (dialogueAsset3 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName3}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset3, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1.0f);
        
        // Play Dialogue 4
        isDialogueFinished = false;
        var dialogueAsset4 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName4);
        if (dialogueAsset4 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName4}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset4, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1.0f);
        

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

        // Play Dialogue 5
        isDialogueFinished = false;
        var dialogueAsset5 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName5);
        if (dialogueAsset5 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName5}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset5, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
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
        
        // Play Dialogue 6
        isDialogueFinished = false;
        var dialogueAsset6 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName6);
        if (dialogueAsset6 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName6}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset6, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1.0f);
        
        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }

    private IEnumerator PlaySwordAnimationCoroutine(System.Action onFinish)
    {
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
        
        // Play Dialogue 7
        bool isDialogueFinished = false;
        var dialogueAsset7 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName7);
        if (dialogueAsset7 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName7}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset7, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // The sword fly up
        // Move the generated tangram holder to the target position: Vector3(-64.2300034,-14.3100004,-5.06389952)
        isMoveComplete = false;
        duration = 2f;
        targetPosition = new Vector3(-64.2300034f, -14.3100004f, -5.06389952f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }),
            curve: swordFlyCurve);
        
        // Wait for a moment
        yield return new WaitForSeconds(0.8f);
        
        // Play Dialogue 8
        isDialogueFinished = false;
        var dialogueAsset8 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName8);
        if (dialogueAsset8 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName8}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset8, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        yield return new WaitUntil(() => isMoveComplete);  // Wait until the sword fly up is complete
        
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
        targetPosition = new Vector3(37f, -16f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }), usePathOffset: true,
                settleDuration: 0.5f);
        yield return new WaitUntil(() => isMoveComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Play Dialogue 9
        isDialogueFinished = false;
        var dialogueAsset9 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName9);
        if (dialogueAsset9 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName9}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset9, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
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
        onFinish?.Invoke();
    }

    private IEnumerator PlayButterflyAnimationCoroutine(System.Action onComplete)
    {
        // Add ActorController component to the generated tangram holder 
        var tangramHolderActorController = generatedTangramHolder.AddComponent<ActorController>();

        // Add ButterflyPathOffset component to the generated tangram holder
        var butterflyPathOffset = generatedTangramHolder.AddComponent<ButterflyPathOffset>();
        butterflyPathOffset.horizontalAmplitude = 0.25f;
        butterflyPathOffset.verticalAmplitude = 0.25f;
        butterflyPathOffset.horizontalFrequency = 3f;
        butterflyPathOffset.verticalFrequency = 3f;

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

        // Activate the mushroom house
        mushroomHouse.SetActive(true);

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

        // Scale the generated tangram holder to the target scale: Vector3(0.200000003,0.200000003,0.200000003)
        bool isScaleComplete = false;
        float duration = 1.0f;
        Vector3 targetScale = new Vector3(0.200000003f, 0.200000003f, 0.200000003f);
        tangramHolderActorController.ScaleTo(targetScale, duration, () => { isScaleComplete = true; });
        yield return new WaitUntil(() => isScaleComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Move the butterfly close to the character
        // Move the generated tangram holder to the target position: Vector3(-64.8000031,-13.8999996,-5.5501132)
        bool isMoveComplete = false;
        Vector3 targetPosition = new Vector3(-65f, -14f, 0f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }),
            usePathOffset: true, settleDuration: 0.1f);
        yield return new WaitUntil(() => isMoveComplete);
        
        // Wait for a moment
        yield return new WaitForSeconds(0.5f);

        // Add ButterflyMotion component to the generated tangram holder
        var butterflyMotion = generatedTangramHolder.AddComponent<ButterflyMotion>();
        butterflyMotion.moveSpeed = 3f;
        butterflyMotion.moveRadius = 1f;

        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 10
        bool isDialogueFinished = false;
        var dialogueAsset10 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName10);
        if (dialogueAsset10 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName10}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset10, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 11
        isDialogueFinished = false;
        var dialogueAsset11 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName11);
        if (dialogueAsset11 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName11}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset11, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1.5f);

        // The butterfly fly to the right then hover Vector3(-54f, -14f, 0f)
        isMoveComplete = false;
        targetPosition = new Vector3(-54f, -14f, 0f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }),
            usePathOffset: true, settleDuration: 0.1f);
        yield return new WaitUntil(() => isMoveComplete);
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Play Dialogue 12
        isDialogueFinished = false;
        var dialogueAsset12 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName12);
        if (dialogueAsset12 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName12}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset12, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Butterfly fly to the right to the mushroom house Vector3(13,-14.5,0)
        // Change the butterfly's path offset frequency
        butterflyPathOffset.horizontalFrequency = 20f;
        butterflyPathOffset.verticalFrequency = 20f;
        bool isButterflyMoveComplete = false;
        duration = 8.5f;
        targetPosition = new Vector3(13f, -14.5f, 0f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration,
            (() => { isButterflyMoveComplete = true; }),
            usePathOffset: true, settleDuration: 1f);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Camera move to the character's position then start following the character (but don't follow y axis)
        bool isCameraMoveComplete = false;
        duration = 1.0f;
        Vector3 axisToFollow = new Vector3(1, 0, 0);
        mainCamera.GetComponent<CameraController>().MoveToAndFollowActor(character.transform, duration,
            cameraFocusCurve,
            (() => { isCameraMoveComplete = true; }), axisToFollow);

        // Wait for a moment
        yield return new WaitUntil((() => isCameraMoveComplete));

        // Character move to the mushroom house Vector3(7,-16,0)
        // Change the character's path offset frequency
        character.GetComponent<WalkingPathOffset>().frequency = 30f;
        bool isCharacterMoveComplete = false;
        duration = 8.0f;
        targetPosition = new Vector3(7f, -16f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, (() => { isCharacterMoveComplete = true; }), usePathOffset: true,
                settleDuration: 0.5f);

        yield return new WaitUntil(() => isCharacterMoveComplete && isButterflyMoveComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 13
        isDialogueFinished = false;
        var dialogueAsset13 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName13);
        if (dialogueAsset13 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName13}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset13, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Butterfly disappear
        isFadeComplete = false;
        duration = 0.5f;
        tangramHolderActorController.FadeToAlpha(0f, duration,
            (() => isFadeComplete = true));
        yield return new WaitUntil(() => isFadeComplete);
        // Deactivate the generated tangram holder
        generatedTangramHolder.SetActive(false);
        
        // Play Dialogue 14
        isDialogueFinished = false;
        var dialogueAsset14 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName14);
        if (dialogueAsset14 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName14}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset14, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Character move to the mushroom house Vector3(13.0600004,-14.8999996,0)
        // Character disappear at the same time
        // Change the character's path offset frequency
        character.GetComponent<WalkingPathOffset>().frequency = 3f;
        isMoveComplete = false;
        isFadeComplete = false;
        duration = 1f;
        targetPosition = new Vector3(13.0600004f, -14.8999996f, 0f);
        character.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, (() => { isMoveComplete = true; }), usePathOffset: true,
                settleDuration: 0.5f);
        character.GetComponent<ActorController>()
            .FadeToAlpha(0f, duration, (() => { isFadeComplete = true; }));
        yield return new WaitUntil(() => isMoveComplete && isFadeComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1.5f);

        // Start the sandstorm
        sandStorm.SetActive(true);
        fadeDuration = 3f;
        sandStorm.GetComponent<ScrollEffect>().StartScroll(true, fadeDuration);

        // Wait for a moment
        yield return new WaitForSeconds(5f);


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

    private IEnumerator PlayFailedAnimationCoroutine(System.Action onComplete)
    {
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

        // Play Dialogue 15
        bool isDialogueFinished = false;
        var dialogueAsset15 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName15);
        if (dialogueAsset15 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName15}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset15, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 16
        isDialogueFinished = false;
        var dialogueAsset16 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName16);
        if (dialogueAsset16 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName16}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset16, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);
        
        // Move the sandstorm to the character's position Vector3(-66.5999985,-11.6999998,0)
        // then play the sandstorm animation
        sandStorm.SetActive(true);
        sandStorm.transform.position = new Vector3(-66.5999985f, -11.6999998f, 0f);
        fadeDuration = 3f;
        sandStorm.GetComponent<ScrollEffect>().StartScroll(true, fadeDuration);

        // Play Dialogue 17
        isDialogueFinished = false;
        var dialogueAsset17 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName17);
        if (dialogueAsset17 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName17}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset17, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 18
        isDialogueFinished = false;
        var dialogueAsset18 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName18);
        if (dialogueAsset18 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName18}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset18, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
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
        
        // Play the dialogue

        // Call the onComplete action after the animation is finished
        onComplete?.Invoke();
    }
}