using System.Collections;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    private bool facingRight = true;
    [SerializeField] private float defaultMoveDuration = 1f;

    private Coroutine currentMoveCoroutine;
    private Coroutine currentScaleCoroutine;
    private Coroutine currentRotateCoroutine;

    /// <summary>
    /// 移动到目标世界坐标位置。
    /// </summary>
    public void MoveToPosition(Vector3 targetPosition, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null, bool usePathOffset = false, bool useLocalSpace = false, float settleDuration = -1f)
    {
        if (currentMoveCoroutine != null)
            StopCoroutine(currentMoveCoroutine);

        currentMoveCoroutine = StartCoroutine(MoveRoutine(targetPosition, duration < 0 ? defaultMoveDuration : duration, onComplete, curve, usePathOffset, useLocalSpace, settleDuration));
    }

    /// <summary>
    /// 相对当前位置移动一定的偏移。
    /// </summary>
    public void MoveByDelta(Vector3 delta, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null, bool usePathOffset = false, bool useLocalSpace = false, float settleDuration = -1f)
    {
        Vector3 basePosition = useLocalSpace ? transform.localPosition : transform.position;
        Vector3 targetPosition = basePosition + delta;
        MoveToPosition(targetPosition, duration, onComplete, curve, usePathOffset, useLocalSpace, settleDuration);
    }

    /// <summary>
    /// 缩放到目标大小（局部缩放）。
    /// </summary>
    public void ScaleTo(Vector3 targetScale, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null)
    {
        if (currentScaleCoroutine != null)
            StopCoroutine(currentScaleCoroutine);

        currentScaleCoroutine = StartCoroutine(ScaleRoutine(targetScale, duration < 0 ? defaultMoveDuration : duration, onComplete, curve));
    }

    /// <summary>
    /// 原地旋转到目标角度（世界旋转）。
    /// </summary>
    public void RotateTo(float targetAngle, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null, Vector3 axis = default, bool useLocalSpace = false)
    {
        if (currentRotateCoroutine != null)
            StopCoroutine(currentRotateCoroutine);

        if (axis == default)
            axis = Vector3.forward; // 默认绕 Z 轴旋转

        Quaternion startRotation = useLocalSpace ? transform.localRotation : transform.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(targetAngle, axis);
        currentRotateCoroutine = StartCoroutine(RotateRoutine(startRotation, endRotation, duration < 0 ? defaultMoveDuration : duration, onComplete, curve, useLocalSpace));
    }

    /// <summary>
    /// 相对当前角度旋转一定的角度。
    /// </summary>
    public void RotateByDelta(float deltaAngle, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null, Vector3 axis = default, bool useLocalSpace = false)
    {
        if (axis == default)
            axis = Vector3.forward;

        Quaternion startRotation = useLocalSpace ? transform.localRotation : transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.AngleAxis(deltaAngle, axis);
        if (currentRotateCoroutine != null)
            StopCoroutine(currentRotateCoroutine);

        currentRotateCoroutine = StartCoroutine(RotateRoutine(startRotation, endRotation, duration < 0 ? defaultMoveDuration : duration, onComplete, curve, useLocalSpace));
    }

    private IEnumerator RotateRoutine(Quaternion startRotation, Quaternion endRotation, float duration, System.Action onComplete, AnimationCurve curve = null, bool useLocalSpace = false)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            Quaternion q = Quaternion.Slerp(startRotation, endRotation, curvedT);
            if (useLocalSpace)
                transform.localRotation = q;
            else
                transform.rotation = q;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (useLocalSpace)
            transform.localRotation = endRotation;
        else
            transform.rotation = endRotation;
        currentRotateCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition, float duration, System.Action onComplete = null, AnimationCurve curve = null, bool usePathOffset = false, bool useLocalSpace = false, float settleDuration = -1f)
    {
        // 如果有 motion，就停止 motion
        var motion = GetComponent<IActorMotion>();
        if (motion != null)
        {
            motion.StopMotion();
        }

        IPathOffsetProvider pathOffsetProvider = usePathOffset ? GetComponent<IPathOffsetProvider>() : null;

        Vector3 startPosition = useLocalSpace ? transform.localPosition : transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            Vector3 basePos = Vector3.Lerp(startPosition, targetPosition, curvedT);
            Vector3 offset = pathOffsetProvider != null ? pathOffsetProvider.GetOffset(curvedT) : Vector3.zero;
            if (useLocalSpace)
                transform.localPosition = basePos + offset;
            else
                transform.position = basePos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalOffset = pathOffsetProvider != null ? pathOffsetProvider.GetOffset(1f) : Vector3.zero;
        if (useLocalSpace)
            transform.localPosition = targetPosition + finalOffset;
        else
            transform.position = targetPosition + finalOffset;

        // Settle motion back to exact targetPosition
        float settleTime = (usePathOffset && settleDuration > 0f) ? settleDuration : 0f;
        if (settleTime > 0f)
        {
            float settleElapsed = 0f;
            Vector3 settleStart = useLocalSpace ? transform.localPosition : transform.position;
            while (settleElapsed < settleTime)
            {
                float t = settleElapsed / settleTime;
                Vector3 settlePos = Vector3.Lerp(settleStart, targetPosition, t);
                if (useLocalSpace)
                    transform.localPosition = settlePos;
                else
                    transform.position = settlePos;

                settleElapsed += Time.deltaTime;
                yield return null;
            }

            if (useLocalSpace)
                transform.localPosition = targetPosition;
            else
                transform.position = targetPosition;
        }

        // 如果有 motion，就恢复 motion
        if (motion != null)
        {
            Vector3 pos = useLocalSpace ? transform.localPosition : transform.position;
            motion.StartMotion(pos);
        }

        currentMoveCoroutine = null;  // Maybe will be moved before the settling !!!!
        onComplete?.Invoke();
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, float duration, System.Action onComplete, AnimationCurve curve = null)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            transform.localScale = Vector3.Lerp(startScale, targetScale, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        currentScaleCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator FadeToAlphaRoutine(float targetAlpha, float duration, System.Action onComplete = null, AnimationCurve curve = null)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Cache original colors for all materials with _Color
        System.Collections.Generic.List<(Material mat, Color originalColor)> materialColors = new System.Collections.Generic.List<(Material, Color)>();
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                materialColors.Add((r.material, r.material.color));
            }
        }

        // Cache original colors for all SpriteRenderers
        System.Collections.Generic.List<(SpriteRenderer sr, Color originalColor)> spriteColors = new System.Collections.Generic.List<(SpriteRenderer, Color)>();
        foreach (var sr in spriteRenderers)
        {
            spriteColors.Add((sr, sr.color));
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            // Lerp alpha from original to target
            foreach (var (mat, original) in materialColors)
            {
                Color c = original;
                c.a = Mathf.Lerp(original.a, targetAlpha, curvedT);
                mat.color = c;
            }
            foreach (var (sr, original) in spriteColors)
            {
                Color c = original;
                c.a = Mathf.Lerp(original.a, targetAlpha, curvedT);
                sr.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Set final alpha
        foreach (var (mat, original) in materialColors)
        {
            Color c = original;
            c.a = targetAlpha;
            mat.color = c;
        }
        foreach (var (sr, original) in spriteColors)
        {
            Color c = original;
            c.a = targetAlpha;
            sr.color = c;
        }
        onComplete?.Invoke();
    }

    public void SetAlphaInstantly(float targetAlpha)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                c.a = targetAlpha;
                r.material.color = c;
            }
        }
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = targetAlpha;
            sr.color = c;
        }
    }

    public void FadeToAlpha(float targetAlpha, float duration, System.Action onComplete = null, AnimationCurve curve = null)
    {
        StartCoroutine(FadeToAlphaRoutine(targetAlpha, duration, onComplete, curve));
    }
    
    /// <summary>
    /// 水平翻转角色。
    /// </summary>
    public void FlipActor()
    {
        facingRight = !facingRight;

        Vector3 euler = transform.localEulerAngles;
        euler.y = facingRight ? 0f : 180f;
        transform.localEulerAngles = euler;
    }
    
    /// <summary>
    /// 立即将角色传送到指定位置。
    /// </summary>
    public void TeleportToPosition(Vector3 targetPosition, bool useLocalSpace = false)
    {
        if (useLocalSpace)
            transform.localPosition = targetPosition;
        else
            transform.position = targetPosition;

        // 传送后，若有 motion 组件，则重新开始 motion
        var motion = GetComponent<IActorMotion>();
        if (motion != null)
        {
            Vector3 pos = useLocalSpace ? transform.localPosition : transform.position;
            motion.StartMotion(pos);
        }
    }
}