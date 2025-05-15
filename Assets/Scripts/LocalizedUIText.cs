using DialogueSystem;
using UnityEngine;
using TMPro;

public class LocalizedUIText : MonoBehaviour
{
    public string textChinese;
    public string textEnglish;

    private TextMeshProUGUI uiText;

    void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();

        switch (GameManager.Instance.currentLanguage)
        {
            case Language.ZH:
                uiText.text = textChinese;
                break;
            case Language.EN:
                uiText.text = textEnglish;
                break;
        }
    }
}