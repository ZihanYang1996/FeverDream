using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class GameLevel1SceneHiddenManager : MonoBehaviour
{
    public GameObject startPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string correctStageId = "Rocket";

    [SerializeField] private List<string> selectedStageIds = new List<string> { "Rocket" };

    [Header("Acrtors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject blackScreenImage;
    [SerializeField] private GameObject generatedTangramHolder;

    [Header("Dialogue")]
    [SerializeField] private string dialogueFileName1;

    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string dialogueFileName4;
    [SerializeField] private string dialogueFileName5;
    [SerializeField] private string dialogueFileName6;

    [SerializeField] private DialogueManager dialogueManager;

    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve rocketFlyCurve;

    // Special for this scene, used to determine when to make the puzzle button non-interactable
    private bool isPuzzleButtonAnimationComplete = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        character.SetActive(true);

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
            startPuzzleButton.GetComponent<ButtonEntranceEffect>().PlayEntranceAnimation((() =>
            {
                isPuzzleButtonAnimationComplete = true;
            }));
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
            List<StageData> selectedStages = mainStages.Where(stage => correctStageId.Contains(stage.id)).ToList();
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
            Debug.Log($"[Game Leve 1] Puzzle '{stage.id}' completed!");
            GameManager.Instance.RegisterCompletedStage(stage);

            // Determine the scene transition condition based on the stage ID
            if (stage.id == correctStageId)
            {
                // Increment the number of completed hidden levels
                GameManager.Instance.IncrementCompletedHiddenLevel();
                PlayPostPuzzleAnimation(stage.id, () =>
                {
                    // After the animation is finished, go to the next scene
                    GoToNextScene("CorrectSolved");
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
        if (stageID == correctStageId)
        {
            // Play the normal stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Normal Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayRocketAnimationCoroutine((() => { onComplete?.Invoke(); })));
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

        // Display the puzzle button (start puzzle)
        StartPuzzle();
        // But make it non-interactable
        // Wait for the animation to complete
        yield return new WaitUntil(() => isPuzzleButtonAnimationComplete);
        if (startPuzzleButton != null)
        {
            startPuzzleButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            Debug.LogWarning("StartPuzzleButton is not assigned in the inspector.");
        }

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

        // Make the puzzle button interactable
        if (startPuzzleButton != null)
        {
            startPuzzleButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            Debug.LogWarning("StartPuzzleButton is not assigned in the inspector.");
        }
    }

    private IEnumerator PlayRocketAnimationCoroutine(System.Action onFinish)
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

        // Move, rotate, and scale the generated tangram holder
        bool isMoveComplete = false;
        bool isRotateComplete = false;
        bool isScaleComplete = false;
        float duration = 2.0f;
        Vector3 targetPosition = new Vector3(1.08000004f, -1.47000003f, -5.06389952f);
        float targetRotation = -55f;
        Vector3 targetScale = new Vector3(2f, 2f, 2f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, (() => isMoveComplete = true));
        tangramHolderActorController.RotateTo(targetRotation, duration, (() => isRotateComplete = true));
        tangramHolderActorController.ScaleTo(targetScale, duration, (() => isScaleComplete = true));
        yield return new WaitUntil(() => isMoveComplete && isRotateComplete && isScaleComplete);

        // Wait for a short time before starting the next dialogue
        yield return new WaitForSeconds(1.0f);

        // Play Dialogue 3
        bool isDialogueFinished = false;
        var dialogueAsset3 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName3);
        if (dialogueAsset3 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName3}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset3, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait for a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Move the character to the generated tangram holder (to the right) then make it transparent
        isMoveComplete = false;
        isFadeComplete = false;
        duration = 2.0f;
        targetPosition = new Vector3(1f, -2.54999995f, 0);
        character.GetComponent<ActorController>().MoveToPosition(targetPosition, duration,
            (() => isMoveComplete = true), usePathOffset: true, settleDuration: 0.5f);
        character.GetComponent<ActorController>().FadeToAlpha(0f, duration, (() => isFadeComplete = true));
        yield return new WaitUntil(() => isMoveComplete && isFadeComplete);

        // Wait for a short time before starting the next dialogue
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
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait for a short time before starting the next animation
        yield return new WaitForSeconds(1.0f);

        // Play the rocket fly animation (but failed)
        // Step 1: 摇晃（位置轻微抖动）
        Vector3 originalPos = generatedTangramHolder.transform.localPosition;
        float shakeDuration = 1.0f;
        float shakeIntensity = 0.2f;
        float shakeElapsed = 0f;

        while (shakeElapsed < shakeDuration)
        {
            Vector2 offset = Random.insideUnitCircle * shakeIntensity;
            generatedTangramHolder.transform.localPosition = originalPos + new Vector3(offset.x, offset.y, 0f);
            shakeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Play next dialogue
        isDialogueFinished = false;
        var dialogueAsset5 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName5);
        if (dialogueAsset5 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName5}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset5, Language.ZH, () => { isDialogueFinished = true; });
        
        // Keep shaking until the dialogue is finished
        while (!isDialogueFinished)
        {
            Vector2 offset = Random.insideUnitCircle * shakeIntensity;
            generatedTangramHolder.transform.localPosition = originalPos + new Vector3(offset.x, offset.y, 0f);
            shakeElapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复原位置
        generatedTangramHolder.transform.localPosition = originalPos;

        // Step 2: 稍微向上移动
        Vector3 deltaPosition = new Vector3(0f, 0.5f, 0f);
        duration = 0.3f;
        bool step2Done = false;
        tangramHolderActorController.MoveByDelta(deltaPosition, duration, () =>
        {
            step2Done = true;
        });
        while (!step2Done) yield return null;

        // Step 3: 快速旋转 + 飞离（模拟失控）
        targetPosition = new Vector3(17.0499992f, 15.3599997f, -5.06389952f);
        duration = 1.5f;
        bool step3Done = false;
        tangramHolderActorController.RotateByDelta(180f, duration);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () =>
        {
            step3Done = true;
        }, curve:rocketFlyCurve);
        while (!step3Done) yield return null;
        
        // Play next dialogue
        isDialogueFinished = false;
        var dialogueAsset6 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName6);
        if (dialogueAsset6 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName6}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset6, Language.ZH, () => { isDialogueFinished = true; });
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        
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
}