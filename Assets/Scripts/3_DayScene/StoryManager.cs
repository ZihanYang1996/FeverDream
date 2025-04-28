using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class StoryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;

    [SerializeField] private RawImage videoBackground;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject pressEnterText;

    [Header("Story Content")]
    [SerializeField] private StoryStep[] storySteps;

    [SerializeField] private float frameRate = 10f; // 帧动画播放速度

    private int storyIndex = 0;
    private int dialogueIndex = 0;

    private TypingEffect typingEffect;
    private BlinkingText blinkingText;

    private Coroutine frameAnimationCoroutine;

    private void Start()
    {
        typingEffect = dialogueText.GetComponent<TypingEffect>();
        blinkingText = pressEnterText.GetComponent<BlinkingText>();
        pressEnterText.SetActive(false);

        LoadCurrentStory();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (typingEffect.IsTyping())
            {
                typingEffect.ForceComplete();
            }
            else
            {
                NextDialogueOrStory();
            }
        }
    }

    private void LoadCurrentStory()
    {
        StoryStep step = storySteps[storyIndex];

        if (frameAnimationCoroutine != null)
            StopCoroutine(frameAnimationCoroutine);

        // 切换背景
        if (step.backgroundType == BackgroundType.Image)
        {
            backgroundImage.gameObject.SetActive(true);
            videoBackground.gameObject.SetActive(false);

            if (step.animationFrames != null && step.animationFrames.Length > 0)
            {
                frameAnimationCoroutine = StartCoroutine(PlayFrameAnimation(step.animationFrames));
            }
        }
        else if (step.backgroundType == BackgroundType.Video)
        {
            backgroundImage.gameObject.SetActive(false);
            videoBackground.gameObject.SetActive(true);

            videoPlayer.clip = step.backgroundVideo;
            videoPlayer.Play();
        }

        dialogueIndex = 0;
        ShowDialogue();
    }

    private void ShowDialogue()
    {
        StoryStep step = storySteps[storyIndex];

        typingEffect.Play(step.dialogues[dialogueIndex], () => { pressEnterText.SetActive(true); });
    }

    private void NextDialogueOrStory()
    {
        pressEnterText.SetActive(false);

        StoryStep step = storySteps[storyIndex];

        dialogueIndex++;

        if (dialogueIndex < step.dialogues.Length)
        {
            ShowDialogue();
        }
        else
        {
            storyIndex++;

            if (storyIndex < storySteps.Length)
            {
                LoadCurrentStory();
            }
            else
            {
                Debug.Log("Story Finished!");
                // TODO: 进入Puzzle或下一幕
            }
        }
    }

    private IEnumerator PlayFrameAnimation(Sprite[] frames)
    {
        int frame = 0;
        while (true)
        {
            backgroundImage.sprite = frames[frame];
            frame = (frame + 1) % frames.Length;
            yield return new WaitForSeconds(1f / frameRate);
        }
    }
}