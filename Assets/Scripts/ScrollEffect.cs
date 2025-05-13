using UnityEngine;
using System.Collections;

public class ScrollEffect : MonoBehaviour
{
    public Material targetMaterial; // Only tested to work with Unlit/Transparent shader
    public Vector2 scrollSpeed = new Vector2(0.5f, 0f); // 水平滚动

    [SerializeField] private bool scrollOnStart = true;
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private float fadeDuration = 1.0f;

    private Vector2 currentOffset = Vector2.zero;
    private bool scrolling = false;

    void Start()
    {
        if (scrollOnStart)
            StartScroll(fadeInOnStart);
    }

    public void StartScroll(bool triggerFadeIn = false, float? customFadeDuration = null)
    {
        scrolling = true;
        currentOffset = Vector2.zero;

        if (triggerFadeIn)
        {
            if (customFadeDuration.HasValue)
            {
                float originalFadeDuration = fadeDuration;
                fadeDuration = customFadeDuration.Value;
                StartCoroutine(FadeIn());
                fadeDuration = originalFadeDuration;
            }
            else
            {
                StartCoroutine(FadeIn());
            }
        }
    }

    public void StopScroll(bool triggerFadeOut = false, float? customFadeDuration = null)
    {
        scrolling = false;

        if (triggerFadeOut)
        {
            if (customFadeDuration.HasValue)
            {
                float originalFadeDuration = fadeDuration;
                fadeDuration = customFadeDuration.Value;
                StartCoroutine(FadeOut());
                fadeDuration = originalFadeDuration;
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }
    }

    void Update()
    {
        if (!scrolling) return;

        currentOffset += scrollSpeed * Time.deltaTime;
        targetMaterial.mainTextureOffset = currentOffset;
    }

    private IEnumerator FadeIn()
    {
        if (targetMaterial == null) yield break;

        Color color = targetMaterial.color;
        color.a = 0f;
        targetMaterial.color = color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            Debug.Log("Fading in");
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = alpha;
            targetMaterial.color = color;
            yield return null;
        }

        color.a = 1f;
        targetMaterial.color = color;
    }

    private IEnumerator FadeOut()
    {
        if (targetMaterial == null) yield break;

        Color color = targetMaterial.color;
        color.a = 1f;
        targetMaterial.color = color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            color.a = alpha;
            targetMaterial.color = color;
            yield return null;
        }

        color.a = 0f;
        targetMaterial.color = color;
    }
}