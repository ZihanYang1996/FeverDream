using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UICurtain : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Image curtainImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float blackScreenPauseDuration = 0f;

    /// <summary>
    /// 淡入（从透明变黑）+ 可选停留黑屏一段时间
    /// </summary>
    public IEnumerator FadeIn(Action onComplete = null)
    {
        if (curtainImage == null)
        {
            Debug.LogWarning("Curtain Image not assigned.");
            yield break;
        }

        curtainImage.gameObject.SetActive(true);
        Color c = curtainImage.color;
        c.a = 0f;
        curtainImage.color = c;

        for (float t = 0f; t < fadeInDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            curtainImage.color = c;
            yield return null;
        }
        c.a = 1f;
        curtainImage.color = c;

        if (blackScreenPauseDuration > 0f)
            yield return new WaitForSeconds(blackScreenPauseDuration);

        onComplete?.Invoke();
    }

    /// <summary>
    /// 淡出（从黑变透明）
    /// </summary>
    public IEnumerator FadeOut(Action onComplete = null)
    {
        if (curtainImage == null)
        {
            Debug.LogWarning("Curtain Image not assigned.");
            yield break;
        }

        Color c = curtainImage.color;
        c.a = 1f;

        for (float t = 0f; t < fadeOutDuration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            curtainImage.color = c;
            yield return null;
        }
        c.a = 0f;
        curtainImage.color = c;
        curtainImage.gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}