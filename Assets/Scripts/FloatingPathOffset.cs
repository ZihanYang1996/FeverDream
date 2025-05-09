using UnityEngine;

public class FloatingPathOffset : MonoBehaviour, IPathOffsetProvider
{
    [Header("Floating Settings")]
    public Vector3 amplitude = new Vector3(0f, 0.5f, 0f);   // 漂浮的最大偏移
    public Vector3 frequency = new Vector3(0f, 1f, 0f);     // 浮动频率（次数/秒）

    [Header("Randomization")]
    public int seed = 0;
    public bool useRandomOffset = false;

    private Vector3 phaseOffset;

    private void Awake()
    {
        if (useRandomOffset)
        {
            System.Random rand = new System.Random(seed);
            phaseOffset = new Vector3(
                (float)(rand.NextDouble() * 2 * Mathf.PI),
                (float)(rand.NextDouble() * 2 * Mathf.PI),
                (float)(rand.NextDouble() * 2 * Mathf.PI)
            );
        }
        else
        {
            phaseOffset = Vector3.zero;
        }
    }

    public Vector3 GetOffset(float time)
    {
        return new Vector3(
            amplitude.x * Mathf.Sin(time * frequency.x + phaseOffset.x),
            amplitude.y * Mathf.Sin(time * frequency.y + phaseOffset.y),
            amplitude.z * Mathf.Sin(time * frequency.z + phaseOffset.z)
        );
    }
}