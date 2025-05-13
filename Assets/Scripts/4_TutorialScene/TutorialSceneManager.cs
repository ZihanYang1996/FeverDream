using System.Collections;
using UnityEngine;
using DialogueSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialSceneManager : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] public SpriteRenderer backgroundImage;

    [SerializeField] public GameObject blackScreenImage;

    [SerializeField] public GameObject startPuzzleButton;
    [SerializeField] private float moveButtonDuration = 1.5f;
    [SerializeField] private float buttonPulseCount = 3;
    [SerializeField] private float buttonPulseScale = 1.2f;
    [SerializeField] private float buttonPulseDuration = 0.2f;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private GameObject generatedTangramHolder;

    [SerializeField] private UICurtain uiCurtain;

    [Header("Background Images")]
    [SerializeField] public Sprite backgroundImage1;

    [SerializeField] public Sprite backgroundImage2;

    [SerializeField] public Sprite backgroundImage3;
    [SerializeField] public Sprite[] backgroundImage4;

    [Header("Dialogue")]
    [SerializeField] private float delayBetweenBackgroundAndDialogue = 1.0f;

    [SerializeField] private string dialogueFileName1;
    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string[] dialogueFileName4;
    [SerializeField] private DialogueManager dialogueManager;

    private Coroutine animationCoroutine;
    private Vector3 originalStartPuzzleButtonPosition;
    private Vector3 targetStartPuzzleButtonPosition;
    private RectTransform startPuzzleButtonRect;
    private Button startPuzzleButtonComponent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (startPuzzleButton != null)
        {
            startPuzzleButton.SetActive(false); // hide initially
            startPuzzleButtonRect = startPuzzleButton.GetComponent<RectTransform>(); // Get the RectTransform component
            originalStartPuzzleButtonPosition = startPuzzleButtonRect.anchoredPosition3D; // Store the original position
            startPuzzleButtonComponent = startPuzzleButton.GetComponent<Button>();
            startPuzzleButtonComponent.interactable = false;
            targetStartPuzzleButtonPosition = GameManager.Instance.defaultStartPuzzelButtonPosition;
        }

        if (blackScreenImage != null)
        {
            blackScreenImage.SetActive(false); // hide initially
        }

        if (backgroundImage != null)
        {
            backgroundImage.sprite = null; // Set to a default sprite or null
        }
        else
        {
            Debug.LogError("BackgroundImage reference not assigned in the inspector.");
        }

        // Stop any previous animation coroutine
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Start the pre-puzzle animation
        animationCoroutine = StartCoroutine(PlayPrePuzzleAnimation1());


        if (stageManager != null)
        {
            stageManager.OnPuzzleComplete += HandlePuzzleResult;
        }
        else
        {
            Debug.LogError("StageManager reference not assigned in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ReturnToDayScene()
    {
        GameManager.Instance.hasSeenTutorial = true;
        GameManager.Instance.justFinishedTutorial = true;
        GameManager.Instance.GoToNextScene(SceneTransitionConditions.Default);
    }

    public void StartPuzzle()
    {
        if (startPuzzleButton != null)
        {
            startPuzzleButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ShowPuzzleButton is not assigned in the inspector.");
        }

        StageManager stageManager = FindAnyObjectByType<StageManager>();
        if (stageManager != null)
        {
            var tutorialStage = GameManager.Instance.GetTutorialStage();
            stageManager.SetupStages(new System.Collections.Generic.List<StageData> { tutorialStage });
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
            if (stage)
            {
                Debug.Log($"[Tutorial] Puzzle '{stage.id}' completed!");
            }
            else
            {
                Debug.LogError("[Tutorial] Stage is null.");
            }

            GameManager.Instance.RegisterCompletedStage(stage);
        }
        else
        {
            Debug.Log("[Tutorial] Puzzle failed. But it's fine.");
        }

        StartCoroutine(PlayPostPuzzleAnimation1(success));
    }

    private IEnumerator PlayPrePuzzleAnimation1()
    {
        // Fade out the black screen
        yield return uiCurtain.FadeOut();

        // Play the first background image (Black screen)
        backgroundImage.sprite = backgroundImage1;
        // Wait for a moment before dialogue
        yield return new WaitForSeconds(delayBetweenBackgroundAndDialogue);
        
        // Play Dialogue 1
        bool isDialogueComplete = false;
        var dialogueAsset1 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName1);
        if (dialogueAsset1 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName1}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset1, () => { isDialogueComplete = true; });
        yield return new WaitUntil(() => isDialogueComplete);

        // Fade in the black screen
        yield return uiCurtain.FadeIn();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Start the second animation
        animationCoroutine = StartCoroutine(PlayPrePuzzleAnimation2());
    }

    private IEnumerator PlayPrePuzzleAnimation2()
    {
        // Play the first background image
        backgroundImage.sprite = backgroundImage2;

        // Fade out the curtain
        yield return uiCurtain.FadeOut();

        // Wait for a moment before dialogue
        yield return new WaitForSeconds(delayBetweenBackgroundAndDialogue);
        
        // Play Dialogue 2
        bool isDialogueComplete = false;
        var dialogueAsset2 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName2);
        if (dialogueAsset2 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName2}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset2, () => { isDialogueComplete = true; });
        yield return new WaitUntil(() => isDialogueComplete);

        // Fade in the curtain
        yield return uiCurtain.FadeIn();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Start the second animation
        animationCoroutine = StartCoroutine(PlayPrePuzzleAnimation3());
    }

    private IEnumerator PlayPrePuzzleAnimation3()
    {
        // Show the puzzle button
        StartPuzzle();

        // Play the first background image
        backgroundImage.sprite = backgroundImage3;

        // Fade out the curtain
        yield return uiCurtain.FadeOut();

        // Activate the button
        startPuzzleButton.SetActive(true);
        // Wait for a moment before dialogue
        yield return new WaitForSeconds(delayBetweenBackgroundAndDialogue);
        
        // Play Dialogue 3
        bool isDialogueComplete = false;
        var dialogueAsset3 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName3);
        if (dialogueAsset3 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName3}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset3, () => { isDialogueComplete = true; });
        yield return new WaitUntil(() => isDialogueComplete);


        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Start the button move animation
        animationCoroutine = StartCoroutine(MoveButton());
    }

    private IEnumerator MoveButton()
    {
        float elapsed = 0f;
        while (elapsed < moveButtonDuration)
        {
            float t = elapsed / moveButtonDuration;
            startPuzzleButtonRect.anchoredPosition =
                Vector3.Lerp(originalStartPuzzleButtonPosition, targetStartPuzzleButtonPosition, t);
            elapsed += Time.deltaTime;
            // Logging for debugging
            Debug.Log($"[Tutorial] Moving button: {startPuzzleButton.transform.position}");
            yield return null;
        }

        // Ensure final position is set
        startPuzzleButtonRect.anchoredPosition = targetStartPuzzleButtonPosition;

        // Flash the button
        for (int i = 0; i < buttonPulseCount; i++)
        {
            // 缩放放大
            float timer = 0f;
            while (timer < buttonPulseDuration)
            {
                timer += Time.deltaTime;
                float t = timer / buttonPulseDuration;
                float scale = Mathf.Lerp(1f, buttonPulseScale, Mathf.SmoothStep(0f, 1f, t));
                startPuzzleButtonRect.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            // 缩放缩小
            timer = 0f;
            while (timer < buttonPulseDuration)
            {
                timer += Time.deltaTime;
                float t = timer / buttonPulseDuration;
                float scale = Mathf.Lerp(buttonPulseScale, 1f, Mathf.SmoothStep(0f, 1f, t));
                startPuzzleButtonRect.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }

        // Enable the button after the animation
        startPuzzleButtonComponent.interactable = true;
    }

    private IEnumerator PlayPostPuzzleAnimation1(bool success)
    {
        // Add ActorController component to the generated tangram holder
        var tangramHolderActorController = generatedTangramHolder.AddComponent<ActorController>();

        // Fade in the generated tangram holder, if success
        if (success)
        {
            bool isFadeComplete = false;
            tangramHolderActorController.FadeToAlpha(1f, stageManager.generatedTangramFlickerDuration,
                (() => isFadeComplete = true));

            // Wait until the fade is finished
            yield return new WaitUntil(() => isFadeComplete);
        }


        // Fade in the black screen
        bool isFadeInComplete = false;
        float fadeOutDuration = 0.5f;
        if (blackScreenImage != null)
        {
            blackScreenImage.SetActive(true);
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.StartFadeIn(fadeOutDuration, (() => { isFadeInComplete = true; }));
        }

        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeInComplete);

        // Small delay before starting the animation
        yield return new WaitForSeconds(0.5f);
        // Play the first background image
        backgroundImage.sprite = backgroundImage1;
        // Wait for a moment before dialogue
        yield return new WaitForSeconds(delayBetweenBackgroundAndDialogue);
        // Play Dialogue 4
        bool isDialogueComplete = false;
        string currentDialogueFileName = success ? dialogueFileName4[0] : dialogueFileName4[1];
        var dialogueAsset4 = DialogueLoader.LoadFromResources("Dialogue/" + currentDialogueFileName);
        if (dialogueAsset4 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName4}");
            yield break;
        }
        dialogueManager.PlayDialogue(dialogueAsset4,
            () => { isDialogueComplete = true; });
        yield return new WaitUntil(() => isDialogueComplete);

        // Fade in the black screen
        yield return uiCurtain.FadeIn();

        // Return to the day scene
        ReturnToDayScene();
    }
}