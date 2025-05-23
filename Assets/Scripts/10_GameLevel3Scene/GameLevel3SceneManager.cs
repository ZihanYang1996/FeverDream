using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem;
using UnityEngine;

public class GameLevel3SceneManager : MonoBehaviour
{
    public GameObject startPuzzleButton;

    [SerializeField] private StageManager stageManager;

    [SerializeField] private string NormalStageId = "Camel";
    [SerializeField] private string SecretStageId = "Bottle";
    
    [SerializeField] private List<string> selectedStageIds = new List<string> { "Camel", "Bottle", "Tree" };

    [Header("Actors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject water;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject blackScreenImage;
    [SerializeField] private GameObject generatedTangramHolder;

    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve cameraFocusCurve;
    [SerializeField] private AnimationCurve bottleSlipCurve;
    [SerializeField] private AnimationCurve bottleDropCurve;

    [Header("Dialogue")]
    [SerializeField] private string dialogueFileName1;

    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string dialogueFileName4;
    [SerializeField] private string dialogueFileName5;
    [SerializeField] private string dialogueFileName6;
    [SerializeField] private string dialogueFileName7;
    [SerializeField] private string dialogueFileName8;

    [SerializeField] private DialogueManager dialogueManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        character.SetActive(false);
        water.SetActive(false);
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
            Debug.Log($"[Game Level 3] Puzzle '{stage.id}' completed!");
            GameManager.Instance.RegisterCompletedStage(stage);

            // Determine the scene transition condition based on the stage ID
            if (stage.id == NormalStageId)
            {
                // Increment the completed level count
                GameManager.Instance.IncrementCompletedLevel();
                // Play the post-puzzle animation
                PlayPostPuzzleAnimation(stage.id, () =>
                {
                    // Set the current time to 7AM
                    GameManager.Instance.currentTime = "7AM";
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
                    // Set the current time to 5AM
                    GameManager.Instance.currentTime = "5AM";
                    // After the animation is finished, go to the next scene
                    GoToNextScene("WrongSolved");
                });
            }
        }
        else
        {
            Debug.Log("[Game Level 3] Puzzle failed.");
            StartCoroutine(PlayFailedAnimationCoroutine(() =>
            {
                // Set the current time to 5AM
                GameManager.Instance.currentTime = "5AM";
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
            StartCoroutine(PlayCamelAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else if (stageID == SecretStageId)
        {
            // Play the secret stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Secret Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayBottleAnimationCoroutine((() => { onComplete?.Invoke(); })));
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
        
        // Wait for the character to finish moving as well
        yield return new WaitUntil(() => isCharacterMoveComplete);
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
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

        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }

    private IEnumerator PlayCamelAnimationCoroutine(System.Action onComplete)
    {
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
        bool isMoveComplete = false;
        float duration = 1.0f;
        Vector3 targetPosition = new Vector3(-14.8000002f, 30.1599998f, -5.06389952f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        // Wait until the move and scale are finished
        yield return new WaitUntil(() => isMoveComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 3
        bool isDialogueFinished = false;
        var dialogueAsset3 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName3);
        if (dialogueAsset3 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName3}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset3, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
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

    private IEnumerator PlayBottleAnimationCoroutine(System.Action onComplete)
    {
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

        // Move the generated tangram holder to the target position: Vector3(-18.7600002,29.3099995,0)
        // Scale it to Vector3(0.15f,0.15f,0.15f)
        bool isMoveComplete = false;
        bool isScaleComplete = false;
        float duration = 1.0f;
        Vector3 targetPosition = new Vector3(-18.7600002f, 29.3099995f, 0f);
        Vector3 targetScale = new Vector3(0.15f, 0.15f, 0.15f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        tangramHolderActorController.ScaleTo(targetScale, duration, (() => { isScaleComplete = true; }));
        yield return new WaitUntil(() => isScaleComplete && isMoveComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 4
        bool isDialogueFinished = false;
        var dialogueAsset4 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName4);
        if (dialogueAsset4 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName4}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset4, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Bottle slipped out of the hand
        // Move it to Vector3(-18.4699993,28.8999996,0)
        // Rotate it to -20
        isMoveComplete = false;
        bool isRotateComplete = false;
        targetPosition = new Vector3(-18.4699993f, 28.8999996f, 0f);
        float targetRotation = -20f;
        duration = 3f;
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; },
            bottleSlipCurve);
        tangramHolderActorController.RotateTo(targetRotation, duration,
            (() => { isRotateComplete = true; }), bottleSlipCurve);
        
        // Wait for a moment
        yield return new WaitForSeconds(0.8f);
        
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
        
        // Wait for the bottle to finish moving and rotating
        yield return new WaitUntil(() => isMoveComplete && isRotateComplete);

        // Drop the bottle on the ground
        // Move it to Vector3(-18.4699993,27,0)
        // Rotate it to -35
        isMoveComplete = false;
        isRotateComplete = false;
        targetPosition = new Vector3(-18.4699993f, 27f, 0f);
        targetRotation = -35f;
        duration = 0.5f;
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; },
            bottleDropCurve);
        tangramHolderActorController.RotateTo(targetRotation, duration, (() => { isRotateComplete = true; }),
            bottleDropCurve);
        yield return new WaitUntil(() => isRotateComplete && isMoveComplete);

        // Bottle bounce up
        // Move it to Vector3(-18.2000008,28,0)
        // Rotate it to -55
        isMoveComplete = false;
        isRotateComplete = false;
        targetPosition = new Vector3(-18.2000008f, 28f, 0f);
        targetRotation = -55f;
        duration = 0.2f;
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        tangramHolderActorController.RotateTo(targetRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete && isMoveComplete);
        // Bottle land on the ground
        // Move it to Vector3(-17.7999992,27,0)
        // Rotate it to -95
        isMoveComplete = false;
        isRotateComplete = false;
        targetPosition = new Vector3(-17.7999992f, 27f, 0f);
        targetRotation = -95f;
        duration = 0.2f;
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        tangramHolderActorController.RotateTo(targetRotation, duration, (() => { isRotateComplete = true; }));
        yield return new WaitUntil(() => isRotateComplete && isMoveComplete);
        
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
        yield return new WaitForSeconds(1f);

        // Water start to leak
        // Ensure it at Vector3(-17.5650005,26.6490002,0)
        // Ensure the scale is Vector3(0.0700000003,0.0700000003,0.0700000003)
        // Ensure the alpha is 0
        water.SetActive(true);
        water.transform.position = targetPosition;
        water.transform.localScale = targetScale;
        water.GetComponent<ActorController>().SetAlphaInstantly(0f);
        // Water fade in
        bool isFadeInComplete = false;
        duration = 2.0f;
        water.GetComponent<ActorController>().FadeToAlpha(1f, duration,
            (() => { isFadeInComplete = true; }));
        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeInComplete);

        // Wait for a moment
        yield return new WaitForSeconds(0.5f);

        // Water get bigger
        // Scale it to Vector3(0.159999996,0.159999996,0.159999996)
        // Move to the right a bit
        isScaleComplete = false;
        isMoveComplete = false;
        targetScale = new Vector3(0.159999996f, 0.159999996f, 0.159999996f);
        Vector3 deltaPosition = new Vector3(0.1f, 0f, 0f);
        duration = 1.0f;
        water.GetComponent<ActorController>().ScaleTo(targetScale, duration,
            (() => { isScaleComplete = true; }));
        water.GetComponent<ActorController>().MoveByDelta(deltaPosition, duration,
            (() => { isMoveComplete = true; }));
        // Wait for the scale to complete
        yield return new WaitUntil(() => isScaleComplete && isMoveComplete);
        
        // Wait for a moment
        yield return new WaitForSeconds(0.5f);

        // Water get bigger
        // Scale it ot Vector3(0.280000001,0.280000001,0.280000001)
        isScaleComplete = false;
        isMoveComplete = false;
        targetScale = new Vector3(0.280000001f, 0.280000001f, 0.280000001f);
        deltaPosition = new Vector3(1f, 0f, 0f);
        duration = 1.0f;
        water.GetComponent<ActorController>().ScaleTo(targetScale, duration,
            (() => { isScaleComplete = true; }));
        water.GetComponent<ActorController>().MoveByDelta(deltaPosition, duration,
            (() => { isMoveComplete = true; }));
        // Wait for the scale to complete
        yield return new WaitUntil(() => isScaleComplete && isMoveComplete);
        
        // Wait for a moment
        yield return new WaitForSeconds(0.5f);

        // Play Dialogue 7
        isDialogueFinished = false;
        var dialogueAsset7 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName7);
        if (dialogueAsset7 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName7}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset7, () => { isDialogueFinished = true; });
        
        // Water get bigger
        // Scale it to Vector3(0.550000012,0.550000012,0.550000012)
        isScaleComplete = false;
        isMoveComplete = false;
        targetScale = new Vector3(0.550000012f, 0.550000012f, 0.550000012f);
        deltaPosition = new Vector3(1.5f, 0f, 0f);
        duration = 1.0f;
        water.GetComponent<ActorController>().ScaleTo(targetScale, duration,
            (() => { isScaleComplete = true; }));
        water.GetComponent<ActorController>().MoveByDelta(deltaPosition, duration,
            (() => { isMoveComplete = true; }));
        // Wait for the scale to complete
        yield return new WaitUntil(() => isScaleComplete && isMoveComplete);
        
        // Wait until the dialogue is finished
        yield return new WaitUntil(() => isDialogueFinished);
        
        // Fade in the black screen
        isFadeInComplete = false;
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

        // Play Dialogue 8
        bool isDialogueFinished = false;
        var dialogueAsset8 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName8);
        if (dialogueAsset8 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName8}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset8, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

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