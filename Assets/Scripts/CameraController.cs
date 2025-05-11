using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Follow Settings")]
    [SerializeField] private Transform followTarget;
    public float followDamping = 0.1f;
    [SerializeField] private bool enableFollow = false;
    [SerializeField] private Vector3 followAxis = new Vector3(1, 1, 0); // 1 = follow, 0 = ignore axis

    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Coroutine moveCoroutine;
    private Coroutine zoomCoroutine;
    private Coroutine shakeCoroutine;

    private Vector3 shakeOffset;
    private Vector3 velocity;

    void LateUpdate()
    {
        if (enableFollow && followTarget != null)
        {
            Vector3 targetPos = followTarget.position;

            Vector3 maskedTarget = new Vector3(
                followAxis.x == 1 ? targetPos.x : transform.position.x,
                followAxis.y == 1 ? targetPos.y : transform.position.y,
                transform.position.z
            );
            Vector3 smoothPos = Vector3.SmoothDamp(transform.position, maskedTarget, ref velocity, followDamping);
            smoothPos += shakeOffset;

            if (useBounds)
            {
                smoothPos.x = Mathf.Clamp(smoothPos.x, minBounds.x, maxBounds.x);
                smoothPos.y = Mathf.Clamp(smoothPos.y, minBounds.y, maxBounds.y);
            }

            transform.position = smoothPos;
        }
    }

    public void FollowTarget(Transform target, float damping = 0.1f, Vector3? axis = null)
    {
        followTarget = target;
        followDamping = damping;
        enableFollow = true;
        followAxis = axis ?? new Vector3(1, 1, 0);
    }

    public void StopFollowing()
    {
        enableFollow = false;
        followTarget = null;
    }

    public void MoveTo(Vector3 targetPosition, float duration, AnimationCurve curve = null, Action onComplete = null)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(targetPosition, duration, curve, onComplete));
    }

    public void MoveToActor(Transform targetActor, float duration, AnimationCurve curve = null, Action onComplete = null)
    {
        Vector3 targetPosition = targetActor.position;
        targetPosition.z = transform.position.z; // 保持相机 Z 不变
        MoveTo(targetPosition, duration, curve, onComplete);
    }

    public void MoveToAndFollowActor(Transform targetActor, float duration, AnimationCurve curve = null, Action onComplete = null, Vector3? axis = null)
    {
        Vector3 targetPosition = targetActor.position;
        targetPosition.z = transform.position.z; // 保持 Z 不变

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(targetPosition, duration, curve, () =>
        {
            SnapTo(targetPosition); // 确保位置对齐，避免抖动
            FollowTarget(targetActor, followDamping, axis); // 使用指定轴
            onComplete?.Invoke();
        }));
    }

    public void MoveBy(Vector3 delta, float duration, AnimationCurve curve = null, Action onComplete = null)
    {
        MoveTo(transform.position + delta, duration, curve, onComplete);
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration, AnimationCurve curve, Action onComplete)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            transform.position = Vector3.Lerp(startPos, targetPos, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        onComplete?.Invoke();
    }

    public void ZoomTo(float targetSize, float duration, AnimationCurve curve = null, Action onComplete = null)
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomRoutine(targetSize, duration, curve, onComplete));
    }

    public void ZoomBy(float deltaSize, float duration, AnimationCurve curve = null, Action onComplete = null)
    {
        ZoomTo(targetCamera.orthographicSize + deltaSize, duration, curve, onComplete);
    }
    
    public void ZoomInstantlyTo(float targetSize)
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        targetCamera.orthographicSize = targetSize;
    }

    private IEnumerator ZoomRoutine(float targetSize, float duration, AnimationCurve curve, Action onComplete)
    {
        float startSize = targetCamera.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            targetCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetCamera.orthographicSize = targetSize;
        onComplete?.Invoke();
    }

    public void Shake(float intensity, float duration, float frequency = 25f)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, duration, frequency));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration, float frequency)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Mathf.PerlinNoise(Time.time * frequency, 0f) * 2 - 1;
            float y = Mathf.PerlinNoise(0f, Time.time * frequency) * 2 - 1;
            shakeOffset = new Vector3(x, y, 0f) * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }

    public void SnapTo(Vector3 position)
    {
        Vector3 newPos = new Vector3(position.x, position.y, transform.position.z);
        transform.position = newPos;
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        useBounds = true;
        minBounds = min;
        maxBounds = max;
    }

    public void ClearBounds()
    {
        useBounds = false;
    }

    public void SetFollowAxis(Vector3 axis)
    {
        followAxis = axis;
    }
}
