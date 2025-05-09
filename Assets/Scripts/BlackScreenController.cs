using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class BlackScreenController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Fade Settings")]
    [SerializeField] private float sceneTransitionFadeDuration = 1f;

    [SerializeField] private float blackScreenStayDuration = 2f; // The duration to stay black

    [SerializeField] private float targetAlpha = 1f; // Default target for FadeIn

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
    }

    private IEnumerator FadeIn(float duration, System.Action onComplete = null)
    {
        yield return FadeToAlpha(1f, duration, onComplete);
    }

    private IEnumerator FadeOut(float duration, System.Action onComplete = null)
    {
        yield return FadeToAlpha(0f, duration, onComplete);
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
    public void StartFadeIn(float duration, System.Action onComplete = null)
    {
        StartCoroutine(FadeIn(duration, onComplete));
    }

    public void StartFadeOut(float duration, System.Action onComplete = null)
    {
        StartCoroutine(FadeOut(duration, onComplete));
    }

    public void SetAlphaInstantly(float alpha)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        color.a = Mathf.Clamp01(alpha);
        spriteRenderer.color = color;
    }

    private IEnumerator SceneStartFadeOutCoroutine(System.Action onComplete = null)
    {
        yield return new WaitForSeconds(blackScreenStayDuration);
        StartCoroutine(FadeOut(sceneTransitionFadeDuration, onComplete));
    }

    private IEnumerator SceneEndFadeInCoroutine(System.Action onComplete = null)
    {
        bool isFadeInComplete = false;
        StartCoroutine(FadeIn(sceneTransitionFadeDuration, (() => { isFadeInComplete = true; })));
        yield return new WaitUntil(() => isFadeInComplete);
        yield return new WaitForSeconds(blackScreenStayDuration);
        onComplete?.Invoke();
    }

    public void SceneStartFadeOut(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        SetAlphaInstantly(1.0f);
        StartCoroutine(SceneStartFadeOutCoroutine(onComplete));
    }

    public void SceneEndFadeIn(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        SetAlphaInstantly(0.0f);
        StartCoroutine(SceneEndFadeInCoroutine(onComplete));
    }
}