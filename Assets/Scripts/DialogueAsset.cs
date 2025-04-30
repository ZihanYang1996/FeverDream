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

        // 如果未来加语言，只需要加字段：public string ja; public string fr; 等等
    }
}