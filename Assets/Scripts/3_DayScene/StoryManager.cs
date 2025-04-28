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
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject pressEnterText;

    [Header("Story Content")]
    [SerializeField] private StoryStep[] storySteps;

    [SerializeField] private float frameRate = 10f; // 帧动画播放速度
    [SerializeField] private float delayBetweenBackgroundAndDialogue = 1.0f; // 背景和对话之间的间隔时间

    private int storyIndex = 0;
    private int dialogueIndex = 0;

    private TypingEffect typingEffect;
    private BlinkingText blinkingText;

    private Coroutine frameAnimationCoroutine;

    private void Start()
    {
        typingEffect = dialogueText.GetComponent<TypingEffect>();
        blinkingText = pressEnterText.GetComponent<BlinkingText>();

        dialoguePanel.SetActive(false); // 一开始隐藏对话框
        pressEnterText.SetActive(false);
        
        videoPlayer.loopPointReached += OnVideoFinished;

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

    private IEnumerator WaitAndShowDialogue()
    {
        yield return new WaitForSeconds(delayBetweenBackgroundAndDialogue); // 这里可以调整延迟时间，比如1秒、2秒
        ShowDialogue();
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
        else if (step.backgroundType == BackgroundType.Unchanged)
        {
            // Keep the current background, so do nothing except for showing the dialogue
            StartCoroutine(WaitAndShowDialogue());
        }

        dialogueIndex = 0;
    }

    private void ShowDialogue()
    {
        dialoguePanel.SetActive(true); //  显示对话框

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
        // Loop through the frames, and stop at the last frame
        for (int frame = 0; frame < frames.Length; frame++)
        {
            backgroundImage.sprite = frames[frame];
            yield return new WaitForSeconds(1f / frameRate);
        }
        // Wait for a short time before showing the dialogue
        StartCoroutine(WaitAndShowDialogue());
    }
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        vp.Pause(); // 确保播放完后停在最后一帧
        // 以后这里可以选择是否要显示最后一帧的图片
        // backgroundImage.sprite = ...; // 设置最后一帧的图片
        
        // Wait for a short time before showing the dialogue
        StartCoroutine(WaitAndShowDialogue());
    }
}