using System.Collections.Generic;
using UnityEngine;

public class SceneFlowController : MonoBehaviour
{
    [SerializeField] private List<SceneTransitionRule> transitionRules;

    // Optionally auto-load from Resources (if you prefer not to assign manually)
    private static Dictionary<string, SceneTransitionRule> ruleMap;

    private void Awake()
    {
        ruleMap = new Dictionary<string, SceneTransitionRule>();
        foreach (var rule in transitionRules)
        {
            if (rule != null && !string.IsNullOrEmpty(rule.currentScene))
            {
                ruleMap[rule.currentScene] = rule;
            }
        }
    }

    /// <summary>
    /// 根据当前 Scene 和条件，解析出下一个场景名
    /// </summary>
    public string ResolveNextScene(string currentScene, string condition)
    {
        // `rule` declared here is available in the entire method scope, not just within this `if` block
        if (!ruleMap.TryGetValue(currentScene, out var rule))
        {
            Debug.LogWarning($"[SceneFlow] No rule defined for scene: {currentScene}");
            return "TitleScene"; // fallback
        }

        foreach (var t in rule.transitions)
        {
            if (t.condition == condition)
            {
                return t.nextSceneName;
            }
        }

        Debug.LogWarning($"[SceneFlow] No transition match for ({currentScene}, {condition})");
        return "TitleScene"; // fallback
    }
}