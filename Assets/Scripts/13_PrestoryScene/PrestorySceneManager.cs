using UnityEngine;

public class PrestorySceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StoryManager storyManager;

    [Header("Story Parts")]
    [SerializeField] private StoryStep[] storySteps;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        storyManager.Play(storySteps, () =>
        {
            GameManager.Instance.GoToNextScene(SceneTransitionConditions.Default);
        });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
