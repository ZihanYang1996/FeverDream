using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

        [SerializeField] private Image portraitImage;
        private const string PortraitRootPath = "Portraits";

        private StyleSettings defaultStyle;
        private StyleSettings thoughtStyle;
        private StyleSettings narratorStyle;

        private Dictionary<(DialogueRole, bool), StyleSettings> styleMap;

        private TypingEffect typingEffect;
        private BlinkingText blinkingText;

        private List<DialogueLine> lines;
        private int index = 0;
        private Action onComplete;

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

            styleMap = new Dictionary<(DialogueRole, bool), StyleSettings>
            {
                { (DialogueRole.Character, false), defaultStyle },
                { (DialogueRole.Character, true), thoughtStyle },
                { (DialogueRole.Narrator, false), narratorStyle },
                { (DialogueRole.Narrator, true), narratorStyle },
                { (DialogueRole.SoundEffect, false), narratorStyle }
            };
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

        public void PlayDialogue(DialogueAsset asset, Action onComplete = null)
        {
            if (asset == null || asset.lines == null || asset.lines.Count == 0)
            {
                Debug.LogWarning("[DialogueManager] Invalid dialogue asset.");
                return;
            }

            this.lines = asset.lines;
            this.index = 0;
            this.onComplete = onComplete;

            dialoguePanel.SetActive(true);
            pressEnterText.SetActive(false);
            ShowLine();
        }


        private void ShowLine()
        {
            DialogueLine line = lines[index];
            string text = GameManager.Instance.currentLanguage == Language.ZH ? line.zh : line.en;
            // Assume JSON already includes parentheses if needed
            // if (line.isThought) text = $"（{text}）";

            // No longer display speaker name explicitly in text
            string speakerPrefix = "";

            // Load portrait
            if (line.role == DialogueRole.Character && !string.IsNullOrEmpty(line.speaker))
            {
                string status = string.IsNullOrEmpty(line.portraitStatus) ? "Normal" : line.portraitStatus;
                string path = $"{PortraitRootPath}/{line.speaker}/{status}";
                Sprite sprite = Resources.Load<Sprite>(path);

                if (sprite != null)
                {
                    portraitImage.sprite = sprite;
                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] Portrait not found: {path}");
                    portraitImage.gameObject.SetActive(false);
                }
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }

            // Apply style
            if (!styleMap.TryGetValue((line.role, line.isThought), out var currentStyle))
            {
                currentStyle = defaultStyle;
            }

            dialogueText.fontStyle = currentStyle.fontStyle;
            dialogueText.color = currentStyle.fontColor;
            dialogueText.fontSize = currentStyle.fontSize;

            dialogueText.text = speakerPrefix;
            typingEffect.Play(text, () => { pressEnterText.SetActive(true); }, append: true, speakerPrefix,
                delayBeforeTyping: delayBetweenSpeakerAndDialogueText);
        }
    }
}