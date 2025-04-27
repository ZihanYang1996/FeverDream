using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    [SerializeField] private float blinkSpeed = 2.0f; // Speed of the blinking effect
    [SerializeField] private float fadeInDuration = 1.0f; // Duration of the fade-in effect during the first time
    [SerializeField] private float minAlpha = 0.2f; // Min alpha value (default 0.2)
    [SerializeField] private float maxAlpha = 1.0f; // Max alpha value (default 1.0)
    private TextMeshProUGUI textMesh;
    private Color originalColor;
    private float timer;
    private float fadeInTimer;
    private bool isFadingIn = true;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        originalColor = textMesh.color;
        originalColor.a = 0f;
        textMesh.color = originalColor;
    }

    private void OnEnable()
    {
        timer = 0f; // Reset the timer when the object is enabled
        fadeInTimer = 0f;
        isFadingIn = true;
    }

    private void Update()
    {
        if (isFadingIn)
        {
            // Fade-in effect
            fadeInTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeInTimer / fadeInDuration); // 0~1
            Color newColor = originalColor;
            newColor.a = t; // Slowly increase alpha from 0 to 1
            textMesh.color = newColor;

            if (t >= 1f)
            {
                isFadingIn = false; // Fade-in complete, start blinking
                timer = Mathf.PI / 2f; // Reset the timer for blinking, which gives the peak of the sine wave.
            }
        }
        else
        {
            // Blinking effect using sine wave, starting with timer at PI/2, meaning alpha = 1
            float baseAlpha = (Mathf.Sin(timer) + 1f) / 2f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, baseAlpha); // Lerp between min and max alpha
            
            Color newColor = originalColor;
            newColor.a = alpha;
            textMesh.color = newColor;
            
            // Increment the timer based on the blink speed
            timer += Time.deltaTime * blinkSpeed;
        }
    }
}