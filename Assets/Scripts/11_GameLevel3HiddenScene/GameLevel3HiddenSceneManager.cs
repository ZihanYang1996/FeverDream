using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLevel3HiddenSceneManager : MonoBehaviour
{
    public GameObject startPuzzleButton;
    [SerializeField] private StageManager stageManager;

    [SerializeField] private string correctStageId = "Tree";

    [SerializeField] private List<string> selectedStageIds = new List<string> { "Tree" };

    [Header("Acrtors")]
    [SerializeField] private GameObject character;

    [SerializeField] private GameObject blackScreenImage;

    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject generatedTangramHolder;
    
    [Header("AnimationCurves")]
    [SerializeField] private AnimationCurve treeGrowCurve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial state of the scene
        mainCamera.GetComponent<CameraController>().SnapTo(new Vector3(51.5f, -8.5f, -10f));
        mainCamera.GetComponent<CameraController>().ZoomInstantlyTo(2.6f);
        blackScreenImage.SetActive(true);
        blackScreenImage.GetComponent<BlackScreenController>().SetAlphaInstantly(1f);

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
            Debug.Log("[Game Leve 3 Hidden] Puzzle failed.");
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
        if (stageID == correctStageId)
        {
            // Play the normal stage post-puzzle animation
            Debug.Log($"Playing post-puzzle animation for Normal Stage: {stageID}");
            // Add your animation logic here
            StartCoroutine(PlayTreeAnimationCoroutine((() => { onComplete?.Invoke(); })));
        }
        else
        {
            Debug.LogWarning($"No specific post-puzzle animation for stage: {stageID}");
        }
    }

    private IEnumerator PlayPrePuzzleAnimationCoroutine()
    {
        // Play dialogue about drinking water

        // Wait for a few seconds
        yield return new WaitForSeconds(1.0f);

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

        // Display the puzzle button (start puzzle)
        StartPuzzle();
    }

    private IEnumerator PlayTreeAnimationCoroutine(System.Action onComplete)
    {
        // Set the Tangram holder's position
        // Move the generated tangram holder to the target position: Vector3(52.9399986,-8.97999954,-5.06389952)
        // Scale it to Vector3(0.310000002,0.310000002,0.310000002)
        generatedTangramHolder.transform.position = new Vector3(52.9399986f, -8.97999954f, -5.06389952f);
        generatedTangramHolder.transform.localScale = new Vector3(0.310000002f, 0.310000002f, 0.310000002f);

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

        // Move the tree (generated tangram holder) to the target position: Vector3(50.3199997,-8.13000011,-5.06389952)
        bool isTreeMoveComplete = false;
        float duration = 1.0f;
        Vector3 treeTargetPosition = new Vector3(50.3199997f, -8.13000011f, -5.06389952f);
        tangramHolderActorController.MoveToPosition(treeTargetPosition, duration,
            (() => { isTreeMoveComplete = true; }));
        // Wait until the tree move is finished
        yield return new WaitUntil(() => isTreeMoveComplete);

        // Character climbs the tree
        // Character moves to the tree: Vector3(50.2330017,-9.11200047,0)
        bool isCharacterMoveComplete = false;
        duration = 1.0f;
        Vector3 characterTargetPosition = new Vector3(50.2330017f, -9.11200047f, 0);
        character.GetComponent<ActorController>().MoveToPosition(characterTargetPosition, duration,
            (() => { isCharacterMoveComplete = true; }), usePathOffset: true, settleDuration: 0.5f);
        // Wait until the character move is finished
        yield return new WaitUntil(() => isCharacterMoveComplete);

        // Wait for a few seconds
        yield return new WaitForSeconds(1.0f);

        // Character climbs the tree: Vector3(50.2330017,-8,0)
        isCharacterMoveComplete = false;
        duration = 1.0f;
        characterTargetPosition = new Vector3(50.2330017f, -8, 0);
        character.GetComponent<ActorController>().MoveToPosition(characterTargetPosition, duration,
            (() => { isCharacterMoveComplete = true; }));
        // Wait until the character move is finished
        yield return new WaitUntil(() => isCharacterMoveComplete);

        // Camera shake
        bool isCameraShakeComplete = false;
        duration = 2.0f;
        float intensity = 0.001f;
        float frequency = 20f;
        mainCamera.GetComponent<CameraController>()
            .Shake(intensity, duration, frequency, () => { isCameraShakeComplete = true; });

        // Tree grows, move to Vector3(51.2000008,67.1999969,-5.06389952), scale to Vector3(18,18,18)
        isTreeMoveComplete = false;
        bool isTreeScaleComplete = false;
        duration = 12.0f;
        treeTargetPosition = new Vector3(51.2000008f, 67.1999969f, -5.06389952f);
        Vector3 treeTrgetScale = new Vector3(18, 18, 18);
        tangramHolderActorController.MoveToPosition(treeTargetPosition, duration,
            (() => { isTreeMoveComplete = true; }), treeGrowCurve);
        tangramHolderActorController.ScaleTo(treeTrgetScale, duration,
            (() => { isTreeScaleComplete = true; }), treeGrowCurve);

        // Character moves up as well, move to Vector3(51.2000008,65,-5.06389952)
        isCharacterMoveComplete = false;
        duration = 12.0f;
        characterTargetPosition = new Vector3(51.2000008f, 65, -5.06389952f);
        character.GetComponent<ActorController>().MoveToPosition(characterTargetPosition, duration,
            (() => { isCharacterMoveComplete = true; }), treeGrowCurve);

        // Wait for camera shake to complete
        yield return new WaitUntil(() => isCameraShakeComplete);

        // Camera zooms out, move to Vector3(0,13.5,0), size changes to 50
        bool isCameraMoveComplete = false;
        bool isCameraZoomComplete = false;
        duration = 8.0f;
        Vector3 cameraTargetPosition = new Vector3(0, 13.5f, -10);
        float cameraTargetSize = 50;
        mainCamera.GetComponent<CameraController>().MoveTo(cameraTargetPosition, duration,
            onComplete: (() => { isCameraMoveComplete = true; }));
        mainCamera.GetComponent<CameraController>().ZoomTo(cameraTargetSize, duration,
            onComplete: (() => { isCameraZoomComplete = true; }));

        // Wait for a few seconds then fade out the character
        yield return new WaitForSeconds(4.0f);
        bool isCharacterFadeComplete = false;
        duration = 6.0f;
        character.GetComponent<ActorController>().FadeToAlpha(0f, duration,
            (() => { isCharacterFadeComplete = true; }));

        // Wait until the tree move and scale are finished
        yield return new WaitUntil(() => isTreeMoveComplete && isTreeScaleComplete);

        // Wait until the character move is finished
        yield return new WaitUntil(() => isCharacterMoveComplete && isCharacterFadeComplete);

        // Wait until the camera move and zoom are finished
        yield return new WaitUntil(() => isCameraMoveComplete && isCameraZoomComplete);

        // Wait for a few seconds
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