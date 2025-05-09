using System.Collections;
using DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class GameLevel1SceneManager : MonoBehaviour
{
    public GameObject startPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string NormalStageId = "Boat";
    [SerializeField] private string SecretStageId = "Cat";

    [Header("Actors")]
    [SerializeField] private GameObject sky;

    [SerializeField] private GameObject sea;
    [SerializeField] private GameObject largeBoat;
    [SerializeField] private GameObject characterWithWood;
    [SerializeField] private GameObject generatedTangramHolder;
    [SerializeField] private GameObject blackScreenImage;
    [SerializeField] private GameObject whale;
    [SerializeField] private GameObject sprout;

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


    [SerializeField] private DialogueManager dialogueManager;

    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve sproutMoveCurve;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        sky.SetActive(true);
        sea.SetActive(true);
        largeBoat.SetActive(true);
        characterWithWood.SetActive(false);
        whale.SetActive(false);
        sprout.SetActive(false);


        // Increment the current level index
        GameManager.Instance.IncrementCurrentLevelIndex();

        if (startPuzzleButton != null)
        {
            startPuzzleButton.SetActive(false); // hide initially
        }

        // StartPuzzle();

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
        yield return new WaitForSeconds(2.0f);

        // Play Dialogue 1
        bool isDialogueFinished = false;
        var dialogueAsset1 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName1);
        if (dialogueAsset1 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName1}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset1, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait a short time before starting the next dialogue
        yield return new WaitForSeconds(1.0f);

        // Play Dialogue 2
        isDialogueFinished = false;
        var dialogueAsset2 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName2);
        if (dialogueAsset2 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName2}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset2, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait a short time before starting the animation
        yield return new WaitForSeconds(1.0f);

        // Move the boat to the upper right corner (using ActorController)
        Vector3 targetPosition = new Vector3(6.0f, -1.0f, 0.0f);
        float moveAndScaleDuration = 2.5f;
        bool isBoatMovementFinished = false;
        var boatController = largeBoat.GetComponent<ActorController>();
        if (boatController == null)
        {
            Debug.LogError("BoatController not found on the large boat.");
            yield break;
        }

        boatController.MoveToPosition(targetPosition, moveAndScaleDuration, () => { isBoatMovementFinished = true; },
            null, true, false, 1.0f);

        // Scale down the boat at the same time
        Vector3 targetScale = new Vector3(0.45f, 0.45f, 1.0f);
        bool isBoatScaleFinished = false;
        boatController.ScaleTo(targetScale, moveAndScaleDuration, () => { isBoatScaleFinished = true; });

        // Wait until 1/4 of the boat movement is finished
        yield return new WaitForSeconds(moveAndScaleDuration / 4.0f);

        // Set the character with wood to active
        characterWithWood.SetActive(true);
        // Get the actor controller of the character with wood
        var characterController = characterWithWood.GetComponent<ActorController>();
        // Set the alpha of the character with wood to 0 (it self and also the child)
        characterController.SetAlphaInstantly(0f);
        // Fade in the character with wood
        float fadeDuration = 1.0f;
        bool isCharacterFadeFinished = false;
        characterController.FadeToAlpha(1.0f, fadeDuration, () => { isCharacterFadeFinished = true; });

        // Wait until both movements are finished for the boat and the fade in is finished for the character
        yield return new WaitUntil(() => isBoatMovementFinished && isBoatScaleFinished && isCharacterFadeFinished);

        // Wait a short time before starting the next dialogue
        yield return new WaitForSeconds(2.0f);

        // Play Dialogue 3
        isDialogueFinished = false;
        var dialogueAsset3 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName3);
        if (dialogueAsset3 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName3}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset3, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished);

        // Sink the boat
        // Set the alpha of the large boat to 0
        // Move the large boat to the bottom of the screen
        float fadeAndMoveDuration = 2.0f;
        bool isBoatFadeFinished = false;
        bool isBoatMoveFinished = false;
        targetPosition = new Vector3(6.0f, -6.0f, 0.0f);
        largeBoat.GetComponent<ActorController>()
            .FadeToAlpha(0f, fadeAndMoveDuration, () => { isBoatFadeFinished = true; });
        // Move the large boat to the bottom of the screen
        largeBoat.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, fadeAndMoveDuration, () => { isBoatMoveFinished = true; });
        // Wait until both fade and move are finished
        yield return new WaitUntil(() => isBoatFadeFinished && isBoatMoveFinished);
        // Disable the large boat
        largeBoat.SetActive(false);

        // Wait a short time before starting the next dialogue
        yield return new WaitForSeconds(1.0f);

        // Play Dialogue 4
        isDialogueFinished = false;
        var dialogueAsset4 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName4);
        if (dialogueAsset4 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName4}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset4, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished);

        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }

    private void PlayPostPuzzleAnimation(string stageID, System.Action onComplete)
    {
        // Play the post-puzzle animation based on the stage ID
        if (stageID == NormalStageId)
        {
            // Play the normal stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Normal Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayBoatAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else if (stageID == SecretStageId)
        {
            // Play the secret stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Secret Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayCatAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else
        {
            Debug.LogWarning($"No specific post-puzzle animation for stage: {stageID}");
        }
    }

    private IEnumerator PlayBoatAnimationCoroutine(System.Action onFinish)
    {
        // Set the Tangram holder's position
        generatedTangramHolder.transform.position = new Vector3(4.44000006f, -2.24000001f, -5.06389952f);
        // Add ActorController component to the generated tangram holder
        var tangramHolderActorController = generatedTangramHolder.AddComponent<ActorController>();

        // Add FloatingPathOffset component to the generated tangram holder
        var tangramHolderFloatingPathOffset = generatedTangramHolder.AddComponent<FloatingPathOffset>();
        tangramHolderFloatingPathOffset.amplitude = new Vector3(0.2f, 0.2f, 0f);
        tangramHolderFloatingPathOffset.frequency = new Vector3(4f, 4f, 0f);

        // Fade in the generated tangram holder
        bool isFadeComplete = false;
        tangramHolderActorController.FadeToAlpha(1f, stageManager.generatedTangramFlickerDuration,
            (() => isFadeComplete = true));
        // Wait until the fade is finished
        yield return new WaitUntil(() => isFadeComplete);

        // Wait for a short time before starting the animation
        yield return new WaitForSeconds(1.0f);

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

        // Add FloatingMotion component to the generated tangram holder, so it do the floating animation
        var tangramHolderFloatingMotion = generatedTangramHolder.AddComponent<FloatingMotion>();
        tangramHolderFloatingMotion.seed = 65;
        tangramHolderFloatingMotion.amplitude = new Vector3(1.5f, 0.2f, 0f);
        tangramHolderFloatingMotion.frequency = new Vector3(0.5f, 2f, 0f);

        // Set the child object of the generated tangram's sprite to the "Actor" sorting layer and set the order to 1
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

        // Move the generated tangram holder near the character
        bool isMoveComplete = false;
        Vector3 deltaPosition = new Vector3(-10.0f, 2.0f, 0.0f);
        float duration = 2.0f;
        tangramHolderActorController.MoveByDelta(deltaPosition, duration, () => { isMoveComplete = true; },
            usePathOffset: true, settleDuration: 0.5f);
        yield return new WaitUntil(() => isMoveComplete);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(2.0f);

        // Play Dialogue 5
        bool isDialogueFinished = false;
        var dialogueAsset5 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName5);
        if (dialogueAsset5 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName5}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset5, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);
        // Move the character with wood to the up, and make it fade out.
        isFadeOutComplete = false;
        isMoveComplete = false;
        deltaPosition = new Vector3(0.0f, 2.0f, 0.0f);
        duration = 2.0f;
        characterWithWood.GetComponent<ActorController>()
            .MoveByDelta(deltaPosition, duration, () => { isMoveComplete = true; });
        characterWithWood.GetComponent<ActorController>().FadeToAlpha(0, duration, () => { isFadeOutComplete = true; });

        // Wait until both fade and move are finished
        yield return new WaitUntil(() => isMoveComplete && isFadeOutComplete);

        // Disable the character with wood
        characterWithWood.SetActive(false);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Move the generated tangram holder to the center of the screen
        isMoveComplete = false;
        deltaPosition = new Vector3(3.0f, -1.0f, 0.0f);
        duration = 1.0f;
        tangramHolderActorController.MoveByDelta(deltaPosition, duration, () => { isMoveComplete = true; },
            usePathOffset: true, settleDuration: 0.5f);

        // Wait until the move is finished
        yield return new WaitUntil(() => isMoveComplete);
        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Play Dialogue 6
        isDialogueFinished = false;
        var dialogueAsset6 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName6);
        if (dialogueAsset6 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName6}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset6, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Play Dialogue 7
        isDialogueFinished = false;
        var dialogueAsset7 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName7);
        if (dialogueAsset7 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName7}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset7, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Whale appears from the bottom of the screen and alpha from 0 to 1
        isMoveComplete = false;
        bool isFadeInComplete = false;
        duration = 3.0f;
        deltaPosition = new Vector3(0.0f, 6.5f, 0.0f);
        whale.SetActive(true);
        whale.GetComponent<ActorController>().SetAlphaInstantly(0f); // Set the alpha to 0
        whale.GetComponent<ActorController>().FadeToAlpha(1, duration, () => { isFadeInComplete = true; });
        whale.GetComponent<ActorController>().MoveByDelta(deltaPosition, duration, () => { isMoveComplete = true; });
        // Wait until both fade and move are finished
        yield return new WaitUntil(() => isMoveComplete && isFadeInComplete);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Start the sprout animation (moving up and alpha from 0 to 1)
        isMoveComplete = false;
        isFadeInComplete = false;
        duration = 3.0f;
        deltaPosition = new Vector3(0.0f, 11.0f, 0.0f);
        sprout.SetActive(true);
        sprout.GetComponent<ActorController>().SetAlphaInstantly(0f); // Set the alpha to 0
        sprout.GetComponent<ActorController>()
            .FadeToAlpha(1, duration, () => { isFadeInComplete = true; }, sproutMoveCurve);
        sprout.GetComponent<ActorController>()
            .MoveByDelta(deltaPosition, duration, () => { isMoveComplete = true; }, sproutMoveCurve);

        // Wait for a period of time before starting the tangram animation
        yield return new WaitForSeconds(duration / 6.0f);

        // Start the tangram animation move
        bool isTangramMoveComplete = false;
        deltaPosition = new Vector3(0.0f, 12.0f, 0.0f);
        generatedTangramHolder.GetComponent<ActorController>().MoveByDelta(deltaPosition, duration,
            () => { isTangramMoveComplete = true; }, sproutMoveCurve);

        // Wait until both fade and move are finished
        yield return new WaitUntil(() => isMoveComplete && isFadeInComplete && isTangramMoveComplete);
        // Disable the tangram holder
        generatedTangramHolder.SetActive(false);

        // Play Dialogue 8
        isDialogueFinished = false;
        var dialogueAsset8 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName8);
        if (dialogueAsset8 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName8}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset8, Language.ZH, () => { isDialogueFinished = true; });

        // Wait for a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Fade away the sprout
        isFadeInComplete = false;
        duration = 1.0f;
        sprout.GetComponent<ActorController>().FadeToAlpha(0, duration, () => { isFadeInComplete = true; });

        // Wait until the fade is finished and the dialogue is finished
        yield return new WaitUntil(() => isDialogueFinished && isFadeInComplete);

        // Fade in the black screen
        isFadeInComplete = false;
        if (blackScreenImage != null)
        {
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.SceneEndFadeIn((() => { isFadeInComplete = true; }));
        }

        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeInComplete);

        // Wait for a short time before going to the next scene
        yield return new WaitForSeconds(GameManager.Instance.blackScreenStayDuration);

        // Call the onComplete action after the animation is finished
        onFinish?.Invoke();
    }


    private IEnumerator PlayCatAnimationCoroutine(System.Action onFinish)
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

        // Wait for a short time before starting the animation
        yield return new WaitForSeconds(1.0f);

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

        // Set the child object of the generated tangram's sprite to the "Actor" sorting layer and set the order to 1
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

        // Add CatMotion component to the cat
        var tangramHolderFloatingMotion = generatedTangramHolder.AddComponent<CatMotion>();
        tangramHolderFloatingMotion.seed = 65;
        tangramHolderFloatingMotion.swayAmplitude = new Vector3(0.1f, 0.1f, 0f);
        tangramHolderFloatingMotion.swayFrequency = new Vector3(5f, 5f, 0f);

        // Move the cat to right
        bool isMoveComplete = false;
        Vector3 deltaPosition = new Vector3(5.0f, 2.0f, 0.0f);
        float duration = 2.0f;
        tangramHolderActorController.MoveByDelta(deltaPosition, duration, () => { isMoveComplete = true; },
            usePathOffset: true, settleDuration: 0.5f);

        // Scale up the cat
        Vector3 targetScale = new Vector3(2.0f, 2.0f, 1.0f);
        bool isTangramScaleFinished = false;
        tangramHolderActorController.ScaleTo(targetScale, duration, () => { isTangramScaleFinished = true; });

        yield return new WaitUntil(() => isMoveComplete && isTangramScaleFinished);

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(2.0f);

        // Play Dialogue 9
        bool isDialogueFinished = false;
        var dialogueAsset9 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName9);
        if (dialogueAsset9 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName9}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset9, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Cat leans to the left
        bool isRotateComplete = false;
        isMoveComplete = false;
        Vector3 targetPosition = new Vector3(5.5999999f, -2.51999998f, 0);
        float targetRotation = 35.7799873f;
        duration = 2.0f;
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        tangramHolderActorController.RotateInPlace(targetRotation, duration, () => { isRotateComplete = true; });
        yield return new WaitUntil(() => isRotateComplete && isMoveComplete);

        // Wait a short time before starting the next animation
        // yield return new WaitForSeconds(0.3f);

        // Play Dialogue 10
        isDialogueFinished = false;
        var dialogueAsset10 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName10);
        if (dialogueAsset10 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName10}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset10, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Move down the sea and character, as if the cat is drinking the sea
        bool isSeaMoveComplete = false;
        bool isCharacterMoveComplete = false;
        Vector3 deltaSeaPosition = new Vector3(0.0f, -2.0f, 0.0f);
        duration = 1.0f;
        int repeatCount = 3;
        while (repeatCount > 0)
        {
            sea.GetComponent<ActorController>()
                .MoveByDelta(deltaSeaPosition, duration, () => { isSeaMoveComplete = true; });
        
            characterWithWood.GetComponent<ActorController>()
                .MoveByDelta(deltaSeaPosition, duration, () => { isCharacterMoveComplete = true; });
            yield return new WaitUntil(() => isSeaMoveComplete && isCharacterMoveComplete);
            isSeaMoveComplete = false;
            isCharacterMoveComplete = false;
            // Wait a short time before starting the next animation
            yield return new WaitForSeconds(1.0f);
            repeatCount--;
        }
        
        // Fade in the black screen
        bool isFadeInComplete = false;
        if (blackScreenImage != null)
        {
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.SceneEndFadeIn((() => { isFadeInComplete = true; }));
        }

        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeInComplete);

        // Wait for a short time before going to the next scene
        yield return new WaitForSeconds(GameManager.Instance.blackScreenStayDuration);

        // Call the onComplete action after the animation is finished
        onFinish?.Invoke();

        
    }
}