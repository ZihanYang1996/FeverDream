using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "IntroScene"; // Name of the next scene to load

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Press Enter to proceed
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}