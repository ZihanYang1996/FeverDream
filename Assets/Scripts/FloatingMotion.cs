using UnityEngine;

public class FloatingMotion : MonoBehaviour
{
    [Header("Motion Settings")]
    public Vector3 amplitude = new Vector3(0f, 0.5f, 0f);   // 漂浮的最大偏移
    public Vector3 frequency = new Vector3(0f, 1f, 0f);     // 浮动速度（频率）

    [Header("Randomization")]
    public int seed = 0;
    public bool useRandomOffset = false;

    private Vector3 initialPosition;
    private Vector3 randomPhaseOffset;

    void Start()
    {
        initialPosition = transform.localPosition;

        if (useRandomOffset)
        {
            System.Random rand = new System.Random(seed);
            randomPhaseOffset = new Vector3(
                (float)(rand.NextDouble() * 2 * Mathf.PI),
                (float)(rand.NextDouble() * 2 * Mathf.PI),
                (float)(rand.NextDouble() * 2 * Mathf.PI)
            );
        }
        else
        {
            randomPhaseOffset = Vector3.zero;
        }
    }

    void Update()
    {
        float time = Time.time;
        Vector3 offset = new Vector3(
            amplitude.x * Mathf.Sin(time * frequency.x + randomPhaseOffset.x),
            amplitude.y * Mathf.Sin(time * frequency.y + randomPhaseOffset.y),
            amplitude.z * Mathf.Sin(time * frequency.z + randomPhaseOffset.z)
        );

        transform.localPosition = initialPosition + offset;
    }
    
    public void SetInitialPosition(Vector3 newInitialLocalPos)
    {
        initialPosition = newInitialLocalPos;

        // 更新 randomPhaseOffset 使得当前 offset 为 0，从而无“跳跃”
        float time = Time.time;
        randomPhaseOffset = new Vector3(
            -time * frequency.x,
            -time * frequency.y,
            -time * frequency.z
        );
    }
}