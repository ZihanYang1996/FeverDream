using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private TypingEffect typingEffect;
    [SerializeField] private GameObject pressEnterText;
    [SerializeField] private string nextSceneName = "GameScene";

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
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnTypingFinished()
    {
        canProceed = true;
        pressEnterText.SetActive(true);
    }
}