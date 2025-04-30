using UnityEngine;

namespace DialogueSystem
{
    public static class DialogueLoader
    {
        public static DialogueAsset LoadFromResources(string fileName)
        {
            // Resources.Load 不需要加 .json 后缀，只要传入相对路径
            TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
            if (jsonFile == null)
            {
                Debug.LogError($"[DialogueLoader] Failed to load: Resources/{fileName}.json");
                return null;
            }

            DialogueAsset dialogue = JsonUtility.FromJson<DialogueAsset>(jsonFile.text);
            return dialogue;
        }
    }
}