using UnityEngine;

public class TitleSceneManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Press Enter to proceed
        {
            GameManager.Instance.GoToNextScene(SceneTransitionConditions.Default);
        }
    }
}