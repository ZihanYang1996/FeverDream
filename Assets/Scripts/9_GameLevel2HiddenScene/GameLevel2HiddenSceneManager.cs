using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class GameLevel2HiddenSceneManager : MonoBehaviour
{
    public GameObject startPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string correctStageId = "Heart";

    [SerializeField] private List<string> selectedStageIds = new List<string> { "Heart" };

    [Header("Acrtors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject blackScreenImage;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject generatedTangramHolder;

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        character.SetActive(false);

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
            Debug.Log($"[Game Level 2 Hidden] Puzzle '{stage.id}' completed!");
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
            Debug.Log("[Game Level 2 Hidden] Puzzle failed.");
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
        if (stageID == correctStageId)
        {
            // Play the normal stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Normal Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayHeartAnimationCoroutine((() => { onComplete?.Invoke(); })));
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


        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }

    private IEnumerator PlayHeartAnimationCoroutine(System.Action onComplete)
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

        // Move the generated tangram holder to the target position: Vector3(0.360000014,-0.49000001,-5.06389952)
        // Scale it to Vector3(2.45000005,2.45000005,2.45000005)
        bool isMoveComplete = false;
        bool isScaleComplete = false;
        float duration = 1.0f;
        Vector3 targetPosition = new Vector3(0.360000014f, -0.49000001f, -5.06389952f);
        Vector3 targetScale = new Vector3(2.45000005f, 2.45000005f, 2.45000005f);
        tangramHolderActorController.MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; });
        tangramHolderActorController.ScaleTo(targetScale, duration, () => { isScaleComplete = true; });
        // Wait until the move and scale are finished
        yield return new WaitUntil(() => isMoveComplete && isScaleComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Fade in the black screen
        isFadeComplete = false;
        fadeDuration = 1.0f;
        if (blackScreenImage != null)
        {
            blackScreenImage.SetActive(true);
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.StartFadeIn(fadeDuration, (() => { isFadeComplete = true; }));
        }

        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeComplete);

        // Sanp the camera to Vector3(-5.25,-26.8400002,-10)
        Vector3 targetCameraPosition = new Vector3(-5.25f, -26.8400002f, -10f);
        mainCamera.GetComponent<CameraController>().SnapTo(targetCameraPosition);

        // Enable the character and set its alpha to 0
        character.SetActive(true);
        character.GetComponent<ActorController>().SetAlphaInstantly(0f);

        // Wait for a moment
        yield return new WaitForSeconds(3f);

        // Fade out the black screen
        isFadeComplete = false;
        fadeDuration = 1.0f;
        if (blackScreenImage != null)
        {
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.StartFadeOut(fadeDuration, (() => { isFadeComplete = true; }));
        }

        // Wait for the fade-out to complete
        yield return new WaitUntil(() => isFadeComplete);

        // Wait for a moment
        yield return new WaitForSeconds(1f);

        // Fade in the character and move it to the target position: Vector3(-3.11999989,-30.8299999,0)
        isFadeComplete = false;
        isMoveComplete = false;
        character.GetComponent<WalkingPathOffset>().frequency = 3f;
        duration = 1.0f;
        Vector3 targetCharacterPosition = new Vector3(-3.11999989f, -30.8299999f, 0f);
        character.GetComponent<ActorController>()
            .FadeToAlpha(1f, duration, (() => { isFadeComplete = true; }));
        character.GetComponent<ActorController>()
            .MoveToPosition(targetCharacterPosition, duration, (() => { isMoveComplete = true; }), usePathOffset: true,
                settleDuration: 0.5f);
        
        // Wait until the fade and move are finished
        yield return new WaitUntil(() => isFadeComplete && isMoveComplete);
        
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

        // Play dialogue saying nothing happened

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

        // Call the onComplete action after the animation is finished
        onComplete?.Invoke();
    }
}