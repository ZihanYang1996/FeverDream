using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlackScreenController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float targetAlpha = 1f; // Default target for FadeIn
    [SerializeField] private bool autoFadeInOnStart = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (autoFadeInOnStart)
        {
            StartCoroutine(FadeToAlpha(targetAlpha, fadeDuration));
        }
    }

    private IEnumerator FadeIn(System.Action onComplete = null)
    {
        yield return FadeToAlpha(1f, fadeDuration, onComplete);
    }

    private IEnumerator FadeOut(System.Action onComplete = null)
    {
        yield return FadeToAlpha(0f, fadeDuration, onComplete);
    }

    private IEnumerator FadeToAlpha(float alpha, float duration, System.Action onComplete = null)
    {
        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, alpha, t);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = alpha;
        spriteRenderer.color = color;
        onComplete?.Invoke();
    }

    // Optional: Call externally with custom values
    public void StartFadeIn(System.Action onComplete = null)
    {
        StartCoroutine(FadeIn(onComplete));
    }

    public void StartFadeOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeOut(onComplete));
    }
}