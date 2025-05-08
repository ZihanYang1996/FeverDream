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

    [Header("Dialogue")]
    [SerializeField] private string dialogueFileName1;

    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string dialogueFileName4;

    [SerializeField] private DialogueManager dialogueManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        sky.SetActive(true);
        sea.SetActive(true);
        largeBoat.SetActive(true);
        characterWithWood.SetActive(false);


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

        boatController.MoveToPosition(targetPosition, moveAndScaleDuration, () => { isBoatMovementFinished = true; });

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
        largeBoat.GetComponent<ActorController>().FadeToAlpha(0f, fadeAndMoveDuration, () => { isBoatFadeFinished = true; });
        // Move the large boat to the bottom of the screen
        largeBoat.GetComponent<ActorController>().MoveToPosition(targetPosition, fadeAndMoveDuration, () => { isBoatMoveFinished = true; });
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
        if (stageID == "Boat")
        {
            // Play the normal stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Normal Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayBoatAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else if (stageID == "Cat")
        {
            // Play the secret stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Secret Stage: {stageID}");
            // Add your animation logic here
        }
        else
        {
            Debug.LogWarning($"No specific post-puzzle animation for stage: {stageID}");
        }
    }

    private IEnumerator PlayBoatAnimationCoroutine(System.Action onFinish)
    {
        // Wait for a short time before starting the animation
        yield return new WaitForSeconds(1.0f);

        // Fade in the black screen
        bool isFadeOutComplete = false;
        if (blackScreenImage != null)
        {
            blackScreenImage.SetActive(true);
            blackScreenImage.GetComponent<BlackScreenController>()?.StartFadeOut((() => { isFadeOutComplete = true; }));
        }
        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeOutComplete);
        Debug.Log("Fade out complete");
        blackScreenImage.SetActive(false);
        
        

        // Call the onComplete action after the animation is finished
        // onFinish?.Invoke();
        // yield return new WaitForSeconds(1.0f);
    }
}