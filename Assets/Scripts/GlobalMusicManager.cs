using UnityEngine;

public class GlobalMusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioClip backgroundMusic;
    public float musicVolume = 1f;
    public bool playOnStart = true;
    
    private AudioSource audioSource;
    private static GlobalMusicManager instance;
    
    public static GlobalMusicManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // Singleton pattern - ensure only one music manager exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupAudioSource();
    }

    private void Start()
    {
        if (playOnStart && backgroundMusic != null)
        {
            PlayMusic();
        }
    }

    private void SetupAudioSource()
    {
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;
        audioSource.spatialBlend = 0f; // 2D audio
        audioSource.priority = 0; // High priority
    }

    public void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }

    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }

    public void FadeIn(float duration = 2f)
    {
        if (audioSource != null)
        {
            audioSource.volume = 0f;
            PlayMusic();
            StartCoroutine(FadeInCoroutine(duration));
        }
    }

    public void FadeOut(float duration = 2f)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    private System.Collections.IEnumerator FadeInCoroutine(float duration)
    {
        float currentTime = 0f;
        float startVolume = 0f;
        
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, musicVolume, currentTime / duration);
            yield return null;
        }
        
        audioSource.volume = musicVolume;
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float currentTime = 0f;
        float startVolume = audioSource.volume;
        
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        StopMusic();
        audioSource.volume = musicVolume; // Reset for next play
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    public void ChangeMusic(AudioClip newClip, bool fadeTransition = true)
    {
        if (newClip == backgroundMusic) return;
        
        backgroundMusic = newClip;
        audioSource.clip = newClip;
        
        if (fadeTransition)
        {
            FadeOut(1f);
            Invoke(nameof(PlayMusicAfterFade), 1.1f);
        }
        else
        {
            StopMusic();
            PlayMusic();
        }
    }

    private void PlayMusicAfterFade()
    {
        FadeIn(1f);
    }
}