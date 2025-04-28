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

    private void Start()
    {
        if (playOnStart)
        {
            storyText.text = "";
            StartCoroutine(TypeText());
        }
    }

    private IEnumerator TypeText()
    {
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
        storyText.text = fullText;
        isTyping = false;
        OnTypingComplete?.Invoke();
    }

    public void Play(string newText, Action onComplete)
    {
        StopAllCoroutines();
        storyText.text = "";
        fullText = newText;
        OnTypingComplete = onComplete;
        isTyping = true;
        StartCoroutine(TypeText());
    }
}