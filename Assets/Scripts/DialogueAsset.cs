using System;
using System.Collections.Generic;

namespace DialogueSystem
{
    [Serializable]
    public class DialogueAsset
    {
        public string dialogueId;
        public List<DialogueLine> lines;
    }

    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string zh;
        public string en;
        public bool isThought;
        public string portraitStatus; // 表示立绘的状态，例如 "happy", "angry"
        public DialogueRole role;     // 表示当前行的角色类型，如 Character, Narrator, SoundEffect

        // 如果未来加语言，只需要加字段：public string ja; public string fr; 等等
    }

    public enum DialogueRole
    {
        Character,
        Narrator,
        SoundEffect
    }
}