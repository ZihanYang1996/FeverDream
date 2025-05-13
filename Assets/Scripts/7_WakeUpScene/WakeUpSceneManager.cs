using System.Collections;
using DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class WakeUpSceneManager : MonoBehaviour
{
    [Header("Background Images")]
    [SerializeField] private GameObject background1Am;
    [SerializeField] private GameObject background3Am;
    [SerializeField] private GameObject background5Am;
    [SerializeField] private GameObject background7Am;

    [SerializeField] private GameObject blackScreenImage;
    
    [Header("Dialogue")]
    [SerializeField] private string dialogueFileName1;

    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string dialogueFileName4;

    [SerializeField] private DialogueManager dialogueManager;

    private int currentLevelIndex;
    private int numOfCompletedLevel;
    private int numOfCompletedHiddenLevel;
    private string currentTime;

    void Start()
    {
        // Set all the background images to inactive
        background1Am.SetActive(false);
        background3Am.SetActive(false);
        background5Am.SetActive(false);
        background7Am.SetActive(false);

        currentLevelIndex = GameManager.Instance.currentLevelIndex;
        Debug.Log($"Wake up at level {currentLevelIndex}");

        numOfCompletedLevel = GameManager.Instance.numOfCompletedLevel;
        Debug.Log($"Completed {numOfCompletedLevel} levels");

        numOfCompletedHiddenLevel = GameManager.Instance.numOfCompletedHiddenLevel;
        Debug.Log($"Completed {numOfCompletedHiddenLevel} hidden levels");

        currentTime = GameManager.Instance.currentTime;

        ContinueGame();
    }

    void ContinueGame()
    {
        if (currentTime == "1AM")
        {
            // Set the background1Am to active
            background1Am.SetActive(true);
            StartCoroutine(PlayAnimationCoroutine((() => { GameManager.Instance.GoToNextScene("1AM"); })));
        }
        else if (currentTime == "3AM")
        {
            // Set the background3Am to active
            background3Am.SetActive(true);
            StartCoroutine(PlayAnimationCoroutine((() => { GameManager.Instance.GoToNextScene("3AM"); })));
        }
        else if (currentTime == "5AM")
        {
            // Set the background5Am to active
            background5Am.SetActive(true);
            StartCoroutine(PlayAnimationCoroutine((() => { GameManager.Instance.GoToNextScene("5AM"); })));
        }
        else if (currentTime == "7AM")
        {
            // At the last level of the game, for now.
            // Will loop back to DayScene, but reset the game state first.
            background7Am.SetActive(true);
            StartCoroutine(PlayAnimationCoroutine((() =>
            {
                GameManager.Instance.ResetForNewLoop();
                GameManager.Instance.GoToNextScene("7AM");
            })));
        }
    }

    private IEnumerator PlayAnimationCoroutine(System.Action onComplete = null)
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

        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        if (currentTime == "1AM")
        {
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
        }
        else if (currentTime == "3AM")
        {
            // Play Dialogue 2
            bool isDialogueFinished = false;
            var dialogueAsset2 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName2);
            if (dialogueAsset2 == null)
            {
                Debug.LogError($"Failed to load dialogue: {dialogueFileName2}");
                yield break;
            }
            dialogueManager.PlayDialogue(dialogueAsset2, () => { isDialogueFinished = true; });
            yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished
        }
        else if (currentTime == "5AM")
        {
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
        }
        else if (currentTime == "7AM")
        {
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
        }
        
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);
        
        // Fade in the black screen
        bool isFadeInComplete = false;
        if (blackScreenImage != null)
        {
            blackScreenImage.GetComponent<BlackScreenController>()
                ?.SceneEndFadeIn((() => { isFadeInComplete = true; }));
        }

        // Wait for the fade-in to complete
        yield return new WaitUntil(() => isFadeInComplete);
        

        // Invoke the continue game method
        onComplete?.Invoke();
    }
}