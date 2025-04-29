using UnityEngine;

public class TutorialSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToDayScene()
    {
        GameManager.Instance.hasSeenTutorial = true;
        GameManager.Instance.justFinishedTutorial = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("DayScene");
    }
}
