using System.Collections;
using DialogueSystem;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject textZH;
    [SerializeField] private GameObject textEN;
    [SerializeField] private GameObject pressEnterText;
    [SerializeField] private UICurtain uiCurtain;

    private bool canProceed = false;

    private void Start()
    {
        pressEnterText.SetActive(false);
        if (GameManager.Instance.currentLanguage == Language.ZH)
        {
            textZH.SetActive(true);
            textZH.GetComponent<TypingEffect>().OnTypingComplete += OnTypingFinished;
            textEN.SetActive(false);
        }
        else
        {
            textZH.SetActive(false);
            textEN.SetActive(true);
            textEN.GetComponent<TypingEffect>().OnTypingComplete += OnTypingFinished;
        }

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