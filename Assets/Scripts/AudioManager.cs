using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private float fadeDuration = 0.5f;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    public List<SceneMusic> sceneMusicList = new List<SceneMusic>();
    private Dictionary<string, AudioClip> sceneMusicDict = new Dictionary<string, AudioClip>();

    private AudioSource audioSource;
    private AudioSource sfxSource;
    private string currentScene = "";
    private AudioClip pendingClip = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;

            foreach (var pair in sceneMusicList)
            {
                if (pair.sceneName != null && pair.bgmClip != null)
                {
                    sceneMusicDict[pair.sceneName] = pair.bgmClip;
                }
            }

            SceneManager.sceneLoaded += OnSceneLoaded;

            PlayBGMForScene(SceneManager.GetActiveScene().name); // 播放初始场景音乐
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    void PlayBGMForScene(string sceneName)
    {
        if (currentScene == sceneName) return;
        currentScene = sceneName;

        if (sceneMusicDict.TryGetValue(sceneName, out AudioClip newClip))
        {
            // 如果是不同音乐才切换（重复音乐就不做操作）
            if (audioSource.clip != newClip)
            {
                StopAllCoroutines();
                StartCoroutine(SwitchMusicCoroutine(newClip, fadeDuration));
            }
        }
        else
        {
            Debug.LogWarning($"No BGM assigned for scene: {sceneName}");
            audioSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }

    public void PlayBGM()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    private IEnumerator SwitchMusicCoroutine(AudioClip newClip, float fadeDuration)
    {
        // 淡出
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();

        // 切换音乐
        audioSource.clip = newClip;
        audioSource.Play();

        // 淡入
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = startVolume;
    }
    
    // Removed FadeOutCurrentAndPrepareNew. Use FadeOutCurrentBGM and PrepareNextBGM instead.

    public void FadeOutCurrentBGM(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutOnlyCoroutine(duration));
    }

    public void PrepareNextBGM(AudioClip newClip)
    {
        pendingClip = newClip;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
    }

    public void PlaySFXInstant(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("No SFX clip provided.");
            return;
        }

        sfxSource.clip = clip;
        sfxSource.volume = volume;
        sfxSource.Play();
    }
    
    private IEnumerator FadeOutAndPrepareCoroutine(AudioClip newClip, float fadeDuration)
    {
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.clip = null;

        pendingClip = newClip;
    }

    private IEnumerator FadeOutOnlyCoroutine(float fadeDuration)
    {
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.clip = null;
    }

    public void PlayPendingBGM()
    {
        if (pendingClip != null)
        {
            audioSource.clip = pendingClip;
            audioSource.volume = 1f;
            audioSource.Play();
            pendingClip = null;
        }
        else
        {
            Debug.LogWarning("No pending BGM to play.");
        }
    }
}
