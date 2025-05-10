using UnityEngine;

public class WalkingPathOffset : MonoBehaviour, IPathOffsetProvider
{
    [Header("Walking Motion Settings")]
    public float amplitude = 0.1f;         // 每一步的垂直起伏幅度
    public float frequency = 2f;           // 步伐频率（越大“脚步”越快）
    public float horizontalSway = 0.05f;   // 左右轻微晃动

    public Vector3 GetOffset(float normalizedTime)
    {
        // 垂直步伐感
        float vertical = Mathf.Sin(normalizedTime * Mathf.PI * frequency) * amplitude;

        // 左右轻晃：加入一个周期更快的cos波（可选）
        float sway = Mathf.Cos(normalizedTime * Mathf.PI * frequency * 2f) * horizontalSway;

        return new Vector3(sway, vertical, 0f);  // 仅作用于 X（左右）和 Y（上下）
    }
}