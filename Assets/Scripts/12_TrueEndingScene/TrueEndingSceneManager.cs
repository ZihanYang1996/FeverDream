using System.Collections;
using DialogueSystem;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Serialization;

public class TrueEndingSceneManager : MonoBehaviour
{
    [SerializeField] public GameObject blackScreenImage;

    [Header("Background Images")]
    [SerializeField] public GameObject classroomWithoutTeacher;

    [SerializeField] public GameObject classroomWithTeacher;
    [SerializeField] public GameObject assignment;
    
    [Header("Actor")]
    [SerializeField] private GameObject strikethrough;
    [SerializeField] private GameObject gameTitle;


    [Header("Dialogue")]
    [SerializeField] private float delayBetweenBackgroundAndDialogue = 1.0f;

    [SerializeField] private string dialogueFileName1;
    [SerializeField] private string dialogueFileName2;
    [SerializeField] private string dialogueFileName3;
    [SerializeField] private string dialogueFileName4;
    [SerializeField] private string dialogueFileName5;

    [SerializeField] private DialogueManager dialogueManager;
    
    [Header("Animation Curve")]
    [SerializeField] private AnimationCurve zoomInAssignmentCurve;
    [SerializeField] private AnimationCurve strikethroughCurve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set initial state of the background images
        blackScreenImage.SetActive(true);
        blackScreenImage.GetComponent<BlackScreenController>().SetAlphaInstantly(1f);

        classroomWithoutTeacher.SetActive(true);
        classroomWithTeacher.SetActive(true);
        // Make classroomWithTeacher's sprite invisible
        classroomWithTeacher.GetComponent<ActorController>().SetAlphaInstantly(0f);
        assignment.SetActive(false);
        
        strikethrough.SetActive(false);
        
        gameTitle.SetActive(false);

        StartCoroutine(PlayTrueEndingScript());
    }

    private IEnumerator PlayTrueEndingScript()
    {
        // Wait for a short time
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
        
        // Blinking effect (use black screen image)
        int blinkCount = 3;
        float blinkDuration = 1.0f;
        float blackscreenDuration = 1.0f;
        for (int i = 0; i < blinkCount; i++)
        {
            blackScreenImage.GetComponent<BlackScreenController>().StartFadeIn(blinkDuration/3f);
            yield return new WaitForSeconds(blackscreenDuration);
            if (i == 2)
            {
                // Show classroomWithTeacher
                classroomWithTeacher.SetActive(true);
                classroomWithTeacher.GetComponent<ActorController>().FadeToAlpha(1f, 0.5f);
            }

            blackScreenImage.GetComponent<BlackScreenController>().StartFadeOut(blinkDuration);
            yield return new WaitForSeconds(blackscreenDuration);
        }
        
        
        yield return new WaitUntil(() => isDialogueFinished); // Wait until the dialogue is finished

        // Wait for a short time
        yield return new WaitForSeconds(1f);

        // Play Dialogue 2
        bool isDialogueFinished2 = false;
        var dialogueAsset2 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName2);
        if (dialogueAsset2 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName2}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset2, () => { isDialogueFinished2 = true; });

        yield return new WaitUntil(() => isDialogueFinished2); // Wait until the dialogue is finished
        
        // Fade in black screen then show assignment
        bool isFadeInComplete = false;
        blackScreenImage.GetComponent<BlackScreenController>().StartFadeIn(0.5f, () =>
        {
            // Show assignment
            assignment.SetActive(true);
            // Make sure assignment's sprite is at the original transform position and scale
            assignment.transform.localPosition = new Vector3(0f, 0f, 0f);
            assignment.transform.localScale = new Vector3(1f, 1f, 1f);
            isFadeInComplete = true;
        });
        yield return new WaitUntil(() => isFadeInComplete);
        yield return new WaitForSeconds(0.5f);

        // Fade out black screen
        bool isFadeOutComplete = false;
        blackScreenImage.GetComponent<BlackScreenController>().StartFadeOut(0.5f, () => { isFadeOutComplete = true; });
        yield return new WaitUntil(() => isFadeOutComplete);

        // Wait for a short time
        yield return new WaitForSeconds(1f);

        // Play Dialogue 3
        bool isDialogueFinished3 = false;
        var dialogueAsset3 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName3);
        if (dialogueAsset3 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName3}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset3, () => { isDialogueFinished3 = true; });
        yield return new WaitUntil(() => isDialogueFinished3); // Wait until the dialogue is finished
        
        // Wait for a short time
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 4
        bool isDialogueFinished4 = false;
        var dialogueAsset4 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName4);
        if (dialogueAsset4 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName4}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset4, () => { isDialogueFinished4 = true; });
        yield return new WaitUntil(() => isDialogueFinished4); // Wait until the dialogue is finished

        // Wait for a short time
        yield return new WaitForSeconds(3f);

        // Zoom in to the title of the assignment
        bool isMoveComplete = false;
        bool isScaleComplete = false;
        Vector3 targetPosition = new Vector3(24f, -33f, 0f);
        Vector3 targetScale = new Vector3(5f, 5f, 5f);
        float duration = 1f;
        assignment.GetComponent<ActorController>()
            .MoveToPosition(targetPosition, duration, () => { isMoveComplete = true; }, zoomInAssignmentCurve);
        assignment.GetComponent<ActorController>()
            .ScaleTo(targetScale, duration, () => { isScaleComplete = true; }, zoomInAssignmentCurve);
        
        yield return new WaitUntil(() => isMoveComplete && isScaleComplete); // Wait until the movement and scaling are complete
        
        // Wait for a short time
        yield return new WaitForSeconds(1f);
        
        // Play Dialogue 5
        bool isDialogueFinished5 = false;
        var dialogueAsset5 = DialogueLoader.LoadFromResources("Dialogue/" + dialogueFileName5);
        if (dialogueAsset5 == null)
        {
            Debug.LogError($"Failed to load dialogue: {dialogueFileName5}");
            yield break;
        }

        dialogueManager.PlayDialogue(dialogueAsset5, () => { isDialogueFinished5 = true; });
        yield return new WaitUntil(() => isDialogueFinished5); // Wait until the dialogue is finished
        
        // Wait for a short time
        yield return new WaitForSeconds(1f);
        
        strikethrough.SetActive(true);
        // Set strikethrough's position to Vector3(-13.3900003f,-3.08999991f,0f);
        // Set strikethrough's scale to Vector3(0.2,5,1)
        // Set strikethrough's rotation to Vector3(0,0,11)
        targetPosition = new Vector3(-13.3900003f,-3.08999991f,0f);
        targetScale = new Vector3(0.2f,5f,1f);
        Vector3 targetRotation = new Vector3(0f, 0f,11f);
        strikethrough.transform.position = targetPosition;
        strikethrough.transform.localScale = targetScale;
        strikethrough.transform.localRotation = Quaternion.Euler(targetRotation);
        
        // Enlarge the strikethrough: Move it to Vector3(-13.3900003,-3.08999991,0)
        // scale it to Vector3(7,5,1)
        bool isStrikethroughScaleComplete = false;
        targetScale = new Vector3(8f,5f,1f);
        duration = 0.2f;
        strikethrough.GetComponent<ActorController>()
            .ScaleTo(targetScale, duration, () => { isStrikethroughScaleComplete = true; }, strikethroughCurve);
        yield return new WaitUntil(((() => isStrikethroughScaleComplete)));
        
        // Instant black screen after a very short time
        yield return new WaitForSeconds(0.075f);
        gameTitle.SetActive(true);
    }
}