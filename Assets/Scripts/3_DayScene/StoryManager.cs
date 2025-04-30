using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using DialogueSystem;

public class StoryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RawImage videoBackground;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Story Settings")]
    [SerializeField] private float frameRate = 10f; // 帧动画播放速度
    [SerializeField] private float delayBetweenBackgroundAndDialogue = 1.0f; // 背景和对话之间的间隔时间

    private StoryStep[] storySteps;
    private int storyIndex = 0;

    private Coroutine frameAnimationCoroutine;

    private Action onStoryFinished; // 外部播放完成回调


    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    public void Play(StoryStep[] steps, Action onComplete = null)
    {
        StopAllCoroutines(); // 清理旧协程
        storySteps = steps;
        storyIndex = 0;
        onStoryFinished = onComplete;

        LoadCurrentStory();
    }

    private void LoadCurrentStory()
    {
        if (storySteps == null || storySteps.Length == 0)
        {
            Debug.LogWarning("No story steps to play!");
            onStoryFinished?.Invoke();
            return;
        }

        StoryStep step = storySteps[storyIndex];

        if (frameAnimationCoroutine != null)
        {
            StopCoroutine(frameAnimationCoroutine);
            frameAnimationCoroutine = null;
        }

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
    }

    // No longer used; logic handled in WaitAndShowDialogue completion callback

    private IEnumerator WaitAndShowDialogue()
    {
        yield return new WaitForSeconds(delayBetweenBackgroundAndDialogue); // 这里可以调整延迟时间，比如1秒、2秒
        if (!string.IsNullOrEmpty(storySteps[storyIndex].dialogueFileName))
        {
            var dialogueAsset = DialogueLoader.LoadFromResources("Dialogue/" + storySteps[storyIndex].dialogueFileName);
            if (dialogueAsset == null)
            {
                Debug.LogError($"Failed to load dialogue: {storySteps[storyIndex].dialogueFileName}");
                yield break;
            }

            dialogueManager.PlayDialogue(dialogueAsset, Language.ZH, () =>
            {
                storyIndex++;
                if (storyIndex < storySteps.Length)
                {
                    LoadCurrentStory();
                }
                else
                {
                    onStoryFinished?.Invoke();
                }
            });
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
                onStoryFinished?.Invoke();
            }
        }
    }

 private IEnumerator PlayFrameAnimation(Sprite[] frames)
{
    if (frames == null || frames.Length == 0)
    {
        Debug.LogWarning("No frames provided for frame animation.");
        yield return StartCoroutine(WaitAndShowDialogue());
        yield break;
    }

    if (frames.Length == 1)
    {
        backgroundImage.sprite = frames[0];
        yield return StartCoroutine(WaitAndShowDialogue());
        yield break;
    }

    for (int frame = 0; frame < frames.Length; frame++)
    {
        backgroundImage.sprite = frames[frame];
        yield return new WaitForSeconds(1f / frameRate);
    }

    yield return StartCoroutine(WaitAndShowDialogue());
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