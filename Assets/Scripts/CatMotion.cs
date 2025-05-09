using UnityEngine;

public class CatMotion : MonoBehaviour, IActorMotion
{
    [Header("Motion Settings")]
    public Vector3 swayAmplitude = new Vector3(0.05f, 0.02f, 0f); // 偏移范围
    public Vector3 swayFrequency = new Vector3(1f, 1.5f, 0f);    // 频率

    [Header("Randomization")]
    public int seed = 0;
    public bool useRandomOffset = true;

    private Vector3 initialPosition;
    private Vector3 randomPhaseOffset;
    private bool _isActive = true;

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
        if (!_isActive) return;

        float time = Time.time;
        Vector3 offset = new Vector3(
            swayAmplitude.x * Mathf.Sin(time * swayFrequency.x + randomPhaseOffset.x),
            swayAmplitude.y * Mathf.Sin(time * swayFrequency.y + randomPhaseOffset.y),
            swayAmplitude.z * Mathf.Sin(time * swayFrequency.z + randomPhaseOffset.z)
        );

        transform.localPosition = initialPosition + offset;
    }

    public void SetInitialPosition(Vector3 newInitialLocalPos)
    {
        initialPosition = newInitialLocalPos;

        float time = Time.time;
        randomPhaseOffset = new Vector3(
            -time * swayFrequency.x,
            -time * swayFrequency.y,
            -time * swayFrequency.z
        );
    }

    public void StartMotion(Vector3? newInitialLocalPos = null)
    {
        _isActive = true;
        if (newInitialLocalPos.HasValue)
        {
            SetInitialPosition(newInitialLocalPos.Value);
        }
        else
        {
            SetInitialPosition(transform.localPosition);
        }
    }

    public void StopMotion()
    {
        _isActive = false;
    }
}