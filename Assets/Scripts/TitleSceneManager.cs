using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "IntroScene"; // 这里填下一个Scene的名字

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // 按回车键
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}