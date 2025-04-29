using UnityEngine;
using UnityEngine.Video;

public enum BackgroundType
{
    Image,
    Video,
    Unchanged
}

[CreateAssetMenu(fileName = "New StoryStep", menuName = "Story/StoryStep")]
public class StoryStep : ScriptableObject
{
    public BackgroundType backgroundType;

    public Sprite[] animationFrames;   // 支持单图/帧动画
    public VideoClip backgroundVideo;  // 支持背景视频

    public string dialogueFileName;    // 对应 Resources/Dialogue/下的 JSON 文件名（不包括.json）

    // 未来扩展（可以留空）
    // public AudioClip voiceClip;
    // public Sprite characterPortrait;
    // public string characterName;
}