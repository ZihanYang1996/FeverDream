using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Tangram/StageData")]
public class StageData : ScriptableObject
{
    public string id; // 唯一标识符，用于在 GameManager 中查找和记录完成状态

    public Sprite outlineSprite;    // 玩家看到的轮廓（用于拼图参考）
    public Texture2D maskTexture;   // 黑底白形状蒙版，用于 IoU 判定
    public Sprite solutionSprite;   // 玩家完成后显示的完整彩色答案图像
    
    [Range(0f, 1f)]
    public float iouThreshold = 0.9f;
}
