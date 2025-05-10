using System.Collections.Generic;
using UnityEngine;

public class TreeCutDetector : MonoBehaviour
{
    private List<TreeCuttable> allTrees;
    
    bool isCutting = false;

    void Start()
    {
        // 找到所有树（也可以优化成提前 assign）
        TreeCuttable[] trees = Object.FindObjectsByType<TreeCuttable>(FindObjectsSortMode.None);
        allTrees = new List<TreeCuttable>(trees);
    }

    void Update()
    {
        if (!isCutting) return;
        float swordX = transform.position.x;

        foreach (var tree in allTrees)
        {
            float treeX = tree.transform.position.x;

            // 如果树在剑的左侧并且未被砍断
            if (!tree.hasBeenCut && swordX >= treeX)
            {
                tree.Cut();
            }
        }
    }
    
    public void StartCutting()
    {
        isCutting = true;
    }
}
