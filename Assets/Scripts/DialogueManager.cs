using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DialogueSystem
{
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
            string speakerPrefix = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker + "：";
            string text = currentLanguage == Language.ZH ? line.zh : line.en;

            // Set the text color and style based on if the line is a thought
            if (line.isThought)
            {
                dialogueText.fontStyle = FontStyles.Italic;
                dialogueText.color = new Color(1f, 1f, 1f, 0.6f);
            }
            else
            {
                dialogueText.fontStyle = FontStyles.Normal;
                dialogueText.color = Color.white;
            }
            
            dialogueText.text = speakerPrefix;  // 立即显示说话人
            typingEffect.Play(text, () => { pressEnterText.SetActive(true); }, append: true, delayBeforeTyping: delayBetweenSpeakerAndDialogueText);
        }
    }
}