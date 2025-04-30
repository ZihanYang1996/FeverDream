using UnityEngine;

public class TutorialSceneManager : MonoBehaviour
{
    public GameObject showPuzzleButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (showPuzzleButton != null)
        {
            showPuzzleButton.SetActive(false); // hide initially
        }
        StartPuzzle();
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

    public void StartPuzzle()
    {
        if (showPuzzleButton != null)
        {
            showPuzzleButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning("ShowPuzzleButton is not assigned in the inspector.");
        }
    }
}
