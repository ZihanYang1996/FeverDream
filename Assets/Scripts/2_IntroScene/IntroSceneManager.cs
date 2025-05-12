using System.Collections;
using UnityEngine;
using TMPro;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private TypingEffect typingEffect;
    [SerializeField] private GameObject pressEnterText;
    [SerializeField] private UICurtain uiCurtain;

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
            StartCoroutine(GoToNextSceneCoroutine());
        }
    }

    private void OnTypingFinished()
    {
        canProceed = true;
        pressEnterText.SetActive(true);
    }
    
    private IEnumerator GoToNextSceneCoroutine()
    {
        bool isFadeInComplete = false;
        yield return uiCurtain.FadeIn((() => { isFadeInComplete = true; }));
        yield return new WaitUntil(() => isFadeInComplete);
        GameManager.Instance.GoToNextScene(SceneTransitionConditions.Default);
    }
}