using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Tangram/StageData")]
public class StageData : ScriptableObject
{
    public Sprite outlineSprite;    // 玩家看到的轮廓
    public Texture2D maskTexture;   // 预先渲染的黑底白形状蒙版
    
    [Range(0f, 1f)]
    public float iouThreshold = 0.9f;
}
