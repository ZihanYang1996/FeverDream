using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scene Flow/Scene Transition Rule")]
public class SceneTransitionRule : ScriptableObject
{
    public string currentScene;
    public List<Transition> transitions;
}

[System.Serializable]
public class Transition
{
    public string condition;        // e.g. "NormalSolved", "Timeout", "SecretSolved"
    public string nextSceneName;    // e.g. "SceneB"
}

public static class SceneTransitionConditions
{
    public const string Default = "__DEFAULT__";
}