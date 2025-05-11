using UnityEngine;

public class ButterflyMotion : MonoBehaviour, IActorMotion
{
    [Header("Motion Settings")]
    public float moveRadius = 2f;          // 飞行范围半径
    public float moveSpeed = 1.5f;         // 移动速度
    public float hoverTime = 0.5f;         // 每次停顿时间

    [Header("Randomization")]
    public int seed = 0;
    public bool useRandomSeed = true;

    private Vector3 initialPosition;
    private Vector3 targetOffset;
    private float lastChangeTime;
    private float nextHoverTime;

    private System.Random rand;
    private bool _isActive = true;

    void Start()
    {
        initialPosition = transform.localPosition;

        rand = useRandomSeed ? new System.Random() : new System.Random(seed);

        ChooseNewTarget();
        lastChangeTime = Time.time;
        _isActive = true;
    }

    void Update()
    {
        if (!_isActive) return;

        Vector3 target = initialPosition + targetOffset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * moveSpeed);

        if (Time.time - lastChangeTime > nextHoverTime)
        {
            ChooseNewTarget();
            lastChangeTime = Time.time;
        }
    }

    void ChooseNewTarget()
    {
        targetOffset = new Vector3(
            (float)(rand.NextDouble() * 2 - 1) * moveRadius,
            (float)(rand.NextDouble() * 2 - 1) * moveRadius,
            (float)(rand.NextDouble() * 2 - 1) * moveRadius
        );
        nextHoverTime = hoverTime + (float)rand.NextDouble(); // 加点随机性
    }

    public void SetInitialPosition(Vector3 newInitialLocalPos)
    {
        initialPosition = newInitialLocalPos;
        transform.localPosition = newInitialLocalPos;
        ChooseNewTarget();
        lastChangeTime = Time.time;
    }

    public void StartMotion(Vector3? newInitialLocalPos = null)
    {
        _isActive = true;
        if (newInitialLocalPos.HasValue)
            SetInitialPosition(newInitialLocalPos.Value);
        else
            SetInitialPosition(transform.localPosition);
    }

    public void StopMotion()
    {
        _isActive = false;
    }
}