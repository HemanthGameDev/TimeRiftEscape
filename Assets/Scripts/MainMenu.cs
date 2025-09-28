using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    public GameObject instructionsPanel; // Assign in Inspector (ONLY in Level 1 Scene)
    
    [Header("Button References")]
    public Button startButton;
    public Button quitButton;
    
    [Header("Audio Settings")]
    public AudioClip buttonClickSound;
    [Range(0f, 1f)]
    public float buttonSoundVolume = 1f;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.2f;
    public float scaleAmount = 0.85f;
    public float bounceAmount = 1.1f;
    public float rotationAmount = 5f;
    public float delayBeforeAction = 0.4f;

    private AudioSource buttonAudioSource;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Level 1") // Only for Level 1
        {
            if (PlayerPrefs.GetInt("PausedGame", 0) == 1)
            {
                if (instructionsPanel != null) instructionsPanel.SetActive(false);
                PlayerPrefs.SetInt("PausedGame", 0);
            }
            else
            {
                Time.timeScale = 0; // Pause game for instructions
                if (instructionsPanel != null) instructionsPanel.SetActive(true);
            }
        }
        else
        {
            SetupButtonAnimations();
            SetupButtonAudio();
            EnsureGlobalMusicIsPlaying();
        }
    }

    private void EnsureGlobalMusicIsPlaying()
    {
        // Make sure global music is playing when in main menu
        if (GlobalMusicManager.Instance != null)
        {
            if (!GlobalMusicManager.Instance.IsPlaying())
            {
                GlobalMusicManager.Instance.PlayMusic();
            }
        }
    }

    private void SetupButtonAudio()
    {
        // Create a dedicated AudioSource for button sounds
        buttonAudioSource = gameObject.GetComponent<AudioSource>();
        if (buttonAudioSource == null)
        {
            buttonAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure the AudioSource for UI sounds
        buttonAudioSource.playOnAwake = false;
        buttonAudioSource.loop = false;
        buttonAudioSource.volume = buttonSoundVolume;
        buttonAudioSource.spatialBlend = 0f; // 2D sound
        buttonAudioSource.priority = 1; // High priority for UI sounds
    }

    private void SetupButtonAnimations()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => StartCoroutine(AnimateButtonClick(startButton, StartGameDelayed)));
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => StartCoroutine(AnimateButtonClick(quitButton, QuitGameDelayed)));
        }
    }

    private IEnumerator AnimateButtonClick(Button button, System.Action onComplete)
    {
        // Play click sound immediately
        PlayButtonSound(buttonClickSound);
        
        // Store original transform values
        Vector3 originalScale = button.transform.localScale;
        Vector3 originalRotation = button.transform.localEulerAngles;
        Vector3 targetScale = originalScale * scaleAmount;
        Vector3 bounceScale = originalScale * bounceAmount;
        
        // Cute animation sequence
        
        // Phase 1: Quick squish down with slight rotation
        float timer = 0f;
        float phase1Duration = animationDuration * 0.3f;
        while (timer < phase1Duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / phase1Duration;
            
            // Smooth scale down
            button.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            
            // Add cute wiggle rotation
            float rotationWiggle = Mathf.Sin(progress * Mathf.PI * 2) * rotationAmount;
            button.transform.localEulerAngles = originalRotation + new Vector3(0, 0, rotationWiggle);
            
            yield return null;
        }
        
        // Phase 2: Bounce back bigger than original (overshoot)
        timer = 0f;
        float phase2Duration = animationDuration * 0.4f;
        while (timer < phase2Duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / phase2Duration;
            
            // Bouncy scale up with overshoot
            float bounceProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);
            button.transform.localScale = Vector3.Lerp(targetScale, bounceScale, bounceProgress);
            
            // Reduce rotation wiggle
            float rotationWiggle = Mathf.Sin(progress * Mathf.PI) * rotationAmount * (1 - progress);
            button.transform.localEulerAngles = originalRotation + new Vector3(0, 0, rotationWiggle);
            
            yield return null;
        }
        
        // Phase 3: Settle back to normal with elastic effect
        timer = 0f;
        float phase3Duration = animationDuration * 0.3f;
        while (timer < phase3Duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / phase3Duration;
            
            // Elastic settle effect
            float elasticProgress = 1 - Mathf.Pow(2, -10 * progress) * Mathf.Cos((progress * 10 - 0.75f) * (2 * Mathf.PI) / 3);
            button.transform.localScale = Vector3.Lerp(bounceScale, originalScale, elasticProgress);
            
            // Return rotation to normal
            button.transform.localEulerAngles = Vector3.Lerp(button.transform.localEulerAngles, originalRotation, progress);
            
            yield return null;
        }
        
        // Ensure we end at original values
        button.transform.localScale = originalScale;
        button.transform.localEulerAngles = originalRotation;
        
        // Wait a bit before executing action
        yield return new WaitForSecondsRealtime(delayBeforeAction);
        
        // Execute the button action
        onComplete?.Invoke();
    }

    private void PlayButtonSound(AudioClip clip)
    {
        if (clip != null)
        {
            // Use AudioSource.PlayClipAtPoint for independent sound playback
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, buttonSoundVolume);
            
            // Alternative method using the dedicated AudioSource
            if (buttonAudioSource != null)
            {
                buttonAudioSource.pitch = Random.Range(0.95f, 1.05f); // Slight pitch variation
                buttonAudioSource.PlayOneShot(clip, buttonSoundVolume);
            }
        }
        else
        {
            Debug.Log("Button click sound clip is missing! Please assign audio clip in the Inspector.");
        }
    }

    public void OnButtonHover()
    {
        // This method is kept for backward compatibility but no longer plays sound
        // Only click sounds will play now
    }

    private void StartGameDelayed()
    {
        PlayerPrefs.SetInt("PausedGame", 0);
        PlayerPrefs.SetString("LastLevel", "Level 1"); // Start from Level 1
        PlayerPrefs.SetFloat("PlayerX", 0f); // Reset Player Position
        PlayerPrefs.SetFloat("PlayerY", 0f);
        SceneManager.LoadScene("Level 1");
    }

    private void QuitGameDelayed()
    {
        Application.Quit();
        Debug.Log("Game Quit!");
    }

    public void StartGame()
    {
        StartGameDelayed();
    }

    public void ResumeGame()
    {
        if (PlayerPrefs.GetInt("PausedGame", 0) == 1)
        {
            string lastLevel = PlayerPrefs.GetString("LastLevel", "Level 1"); // Default to Level 1 if no saved level
            SceneManager.LoadScene(lastLevel);
        }
    }

    public void QuitGame()
    {
        QuitGameDelayed();
    }

    public void LoadGameScene()
    {
        PlayerPrefs.SetInt("PausedGame", 1); // Set flag to resume
        SceneManager.LoadScene(PlayerPrefs.GetString("LastLevel", "Level 1")); // Load Last Played Level
    }
}
