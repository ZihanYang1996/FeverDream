using UnityEngine;
using TMPro;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private TypingEffect typingEffect;
    [SerializeField] private GameObject pressEnterText;

    private bool canProceed = false;

    private void Start()
    {
        pressEnterText.SetActive(false);

        // Watch for the typing effect to finish
        typingEffect.OnTypingComplete += OnTypingFinished;
    }

    private void Update()
    {
        if (canProceed && Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.Instance.GoToNextScene(SceneTransitionConditions.Default);
        }
    }

    private void OnTypingFinished()
    {
        canProceed = true;
        pressEnterText.SetActive(true);
    }
}