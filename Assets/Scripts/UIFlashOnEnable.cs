using UnityEngine;
using UnityEngine.UI;

public class UIFlashOnEnable : MonoBehaviour
{
    [SerializeField] private float duration = 1.0f;       // 闪烁总时长
    [SerializeField] private float flashFrequency = 5.0f; // 闪烁频率（每秒几次）

    private Image image;
    private float elapsedTime;
    private bool isFlashing = false;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("UIFlashOnEnable requires an Image component.");
        }
    }

    private void OnEnable()
    {
        if (image != null)
        {
            elapsedTime = 0f;
            isFlashing = true;
            StartCoroutine(FlashCoroutine());
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        Color originalColor = image.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Abs(Mathf.Sin(elapsedTime * Mathf.PI * flashFrequency)); // 在0和1之间闪烁
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 恢复为不透明
        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        isFlashing = false;
    }
}