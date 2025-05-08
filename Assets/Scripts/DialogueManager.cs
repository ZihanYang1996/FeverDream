using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DialogueSystem
{
    [Serializable]
    public class StyleSettings
    {
        public FontStyles fontStyle = FontStyles.Normal;
        public Color fontColor = Color.white;
        public float fontSize = 36f;
    }

    public enum Language
    {
        ZH,
        EN
    }

    public class DialogueManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject dialoguePanel;

        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject pressEnterText;
        [SerializeField] private float delayBetweenSpeakerAndDialogueText = 0.3f;


        private StyleSettings defaultStyle;
        private StyleSettings thoughtStyle;
        private StyleSettings narratorStyle;

        private TypingEffect typingEffect;
        private BlinkingText blinkingText;

        private List<DialogueLine> lines;
        private int index = 0;
        private Action onComplete;
        private Language currentLanguage = Language.ZH;

        private void Awake()
        {
            typingEffect = dialogueText.GetComponent<TypingEffect>();
            blinkingText = pressEnterText.GetComponent<BlinkingText>();
            dialoguePanel.SetActive(false); // 一开始隐藏对话框
            pressEnterText.SetActive(false);
            
            // Get the font style settings from the GameManager
            defaultStyle = GameManager.Instance.defaultStyle;
            thoughtStyle = GameManager.Instance.thoughtStyle;
            narratorStyle = GameManager.Instance.narratorStyle;
        }

        private void Update()
        {
            if (!dialoguePanel.activeSelf || lines == null) return;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (typingEffect.IsTyping())
                {
                    typingEffect.ForceComplete();
                }
                else
                {
                    index++;
                    if (index < lines.Count)
                    {
                        ShowLine();
                        pressEnterText.SetActive(false);
                    }
                    else
                    {
                        dialoguePanel.SetActive(false);
                        pressEnterText.SetActive(false);
                        onComplete?.Invoke();
                    }
                }
            }
        }

        public void PlayDialogue(DialogueAsset asset, Language language, Action onComplete = null)
        {
            if (asset == null || asset.lines == null || asset.lines.Count == 0)
            {
                Debug.LogWarning("[DialogueManager] Invalid dialogue asset.");
                return;
            }

            this.lines = asset.lines;
            this.index = 0;
            this.onComplete = onComplete;
            this.currentLanguage = language;

            dialoguePanel.SetActive(true);
            pressEnterText.SetActive(false);
            ShowLine();
        }

        private void ShowLine()
        {
            DialogueLine line = lines[index];
            string speakerPrefix =
                (string.IsNullOrEmpty(line.speaker) || line.speaker == "旁白") ? "" : line.speaker + "：";
            string text = currentLanguage == Language.ZH ? line.zh : line.en;

            StyleSettings currentStyle;

            if (line.isThought)
            {
                currentStyle = thoughtStyle;
            }
            else if (line.speaker == "旁白")
            {
                currentStyle = narratorStyle;
            }
            else
            {
                currentStyle = defaultStyle;
            }

            dialogueText.fontStyle = currentStyle.fontStyle;
            dialogueText.color = currentStyle.fontColor;
            dialogueText.fontSize = currentStyle.fontSize;

            dialogueText.text = speakerPrefix; // 立即显示说话人
            typingEffect.Play(text, () => { pressEnterText.SetActive(true); }, append: true, speakerPrefix,
                delayBeforeTyping: delayBetweenSpeakerAndDialogueText);
        }
    }
}