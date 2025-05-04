using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class TypingEffect : MonoBehaviour
{
    [Header("Typing Machine")]
    [SerializeField] private bool playOnStart = true;

    [SerializeField] private TextMeshProUGUI storyText;

    [TextArea(3, 10)]
    [SerializeField] private string fullText;

    [SerializeField] private float typingSpeed = 0.05f; // Typing speed in seconds

    public Action OnTypingComplete; // Event to notify when typing is complete

    private bool isTyping = true;

    private string prefixText = ""; // 例如 "我："

    private void Start()
    {
        if (playOnStart)
        {
            storyText.text = "";
            StartCoroutine(TypeText(0f)); // Start typing immediately
        }
    }

    private IEnumerator TypeText(float delay)
    {
        yield return new WaitForSeconds(delay); // 先等一段时间
        storyText.text = prefixText;
        foreach (char c in fullText)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(typingSpeed * 3);

        isTyping = false;
        OnTypingComplete?.Invoke(); // Invoke the event when typing is complete
    }

    public bool IsTyping()
    {
        return isTyping;
    }

    public void ForceComplete()
    {
        StopAllCoroutines();
        storyText.text = prefixText + fullText;
        isTyping = false;
        OnTypingComplete?.Invoke();
    }

    public void Play(string newText, Action onComplete, bool append = false, string speakerPrefix = "", float delayBeforeTyping = 0f)
    {
        StopAllCoroutines();
        fullText = newText;
        prefixText = speakerPrefix;
        OnTypingComplete = onComplete;
        isTyping = true;

        if (!append)
            storyText.text = "";

        StartCoroutine(TypeText(delayBeforeTyping));
    }
}