using System.Collections;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    [SerializeField] private float defaultMoveDuration = 1f;

    private Coroutine currentMoveCoroutine;
    private Coroutine currentScaleCoroutine;

    /// <summary>
    /// 移动到目标世界坐标位置。
    /// </summary>
    public void MoveToPosition(Vector3 targetPosition, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null, bool usePathOffset = false)
    {
        if (currentMoveCoroutine != null)
            StopCoroutine(currentMoveCoroutine);

        currentMoveCoroutine = StartCoroutine(MoveRoutine(targetPosition, duration < 0 ? defaultMoveDuration : duration, onComplete, curve, usePathOffset));
    }

    /// <summary>
    /// 相对当前位置移动一定的偏移。
    /// </summary>
    public void MoveByDelta(Vector3 delta, float duration = -1f, System.Action onComplete = null, AnimationCurve curve = null, bool usePathOffset = false)
    {
        Vector3 targetPosition = transform.position + delta;
        MoveToPosition(targetPosition, duration, onComplete, curve, usePathOffset);
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

    private IEnumerator MoveRoutine(Vector3 targetPosition, float duration, System.Action onComplete = null, AnimationCurve curve = null, bool usePathOffset = false)
    {
        // 如果有 motion，就停止 motion
        var motion = GetComponent<IActorMotion>();
        if (motion != null)
        {
            motion.StopMotion();
        }

        IPathOffsetProvider pathOffsetProvider = usePathOffset ? GetComponent<IPathOffsetProvider>() : null;
        
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curvedT = curve != null ? curve.Evaluate(t) : t;
            Vector3 basePos = Vector3.Lerp(startPosition, targetPosition, curvedT);
            Vector3 offset = pathOffsetProvider != null ? pathOffsetProvider.GetOffset(curvedT) : Vector3.zero;
            transform.position = basePos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        
        // 如果有 motion，就恢复 motion
        if (motion != null)
        {
            motion.StartMotion(transform.localPosition);
        }
        
        currentMoveCoroutine = null;
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
}