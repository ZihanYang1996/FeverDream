using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEntranceEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private int pulseCount = 2;

    private Button button;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = GameManager.Instance.defaultStartPuzzelButtonPosition;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f; // Start fully transparent
        button.interactable = false;
    }

    public void PlayEntranceAnimation()
    {
        StartCoroutine(AnimateEntrance());
    }

    private IEnumerator AnimateEntrance()
    {
        // Fade in
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeInDuration);
            yield return null;
        }

        // Pulse
        for (int i = 0; i < pulseCount; i++)
        {
            // Enlarge
            timer = 0f;
            while (timer < pulseDuration)
            {
                timer += Time.deltaTime;
                float t = timer / pulseDuration;
                float scale = Mathf.Lerp(1f, pulseScale, Mathf.SmoothStep(0f, 1f, t));
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            // Shrink
            timer = 0f;
            while (timer < pulseDuration)
            {
                timer += Time.deltaTime;
                float t = timer / pulseDuration;
                float scale = Mathf.Lerp(pulseScale, 1f, Mathf.SmoothStep(0f, 1f, t));
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }

        // Enable interaction after animation
        button.interactable = true;
    }
}