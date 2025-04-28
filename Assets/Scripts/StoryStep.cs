using UnityEngine;
using UnityEngine.Video;

public enum BackgroundType
{
    Image,
    Video
}

[CreateAssetMenu(fileName = "New StoryStep", menuName = "Story/StoryStep")]
public class StoryStep : ScriptableObject
{
    public BackgroundType backgroundType;

    public Sprite[] animationFrames;   // 支持单图/帧动画
    public VideoClip backgroundVideo;  // 支持背景视频

    [TextArea(3, 10)]
    public string[] dialogues;          // 支持多句对话

    // 未来扩展（可以留空）
    // public AudioClip voiceClip;
    // public Sprite characterPortrait;
    // public string characterName;
}