using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Button References")]
    public Button startButton;
    public Button resumeButton; // Add resume button reference
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
        ValidateSaveData(); // Clean up any corrupted save data
        SetupButtonAnimations();
        SetupButtonAudio();
        UpdateResumeButtonState(); // Check if resume should be available
        EnsureGlobalMusicIsPlaying();
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

    private void ValidateSaveData()
    {
        // Check if there's corrupted save data and clean it up
        bool hasPausedGame = PlayerPrefs.GetInt("PausedGame", 0) == 1;
        string lastLevel = PlayerPrefs.GetString("LastLevel", "");
        
        if (hasPausedGame)
        {
            // If PausedGame is set but LastLevel is invalid, clear the paused state
            bool isValidLevel = !string.IsNullOrEmpty(lastLevel) && 
                               (lastLevel == "Level 1" || lastLevel == "Level 2" || lastLevel == "Level 3" ||
                                lastLevel == "Level 4" || lastLevel == "Level 5" || lastLevel == "Level 6");
            
            if (!isValidLevel)
            {
                Debug.LogWarning("MainMenu: Found corrupted save data - PausedGame=1 but LastLevel='" + lastLevel + "'. Clearing save data.");
                PlayerPrefs.SetInt("PausedGame", 0);
                PlayerPrefs.DeleteKey("LastLevel");
                PlayerPrefs.DeleteKey("SavedTimer");
                PlayerPrefs.DeleteKey("TimerWasRunning");
                PlayerPrefs.DeleteKey("PlayerX");
                PlayerPrefs.DeleteKey("PlayerY");
                PlayerPrefs.Save();
                Debug.Log("MainMenu: Corrupted save data cleared. Resume button will be disabled.");
            }
            else
            {
                Debug.Log("MainMenu: Valid save data found - PausedGame=1, LastLevel='" + lastLevel + "'");
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
            startButton.onClick.AddListener(() => StartCoroutine(AnimateButtonClick(startButton, StartNewGame)));
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(() => StartCoroutine(AnimateButtonClick(resumeButton, ResumeGame)));
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

    private void UpdateResumeButtonState()
    {
        if (resumeButton != null)
        {
            bool hasPausedGame = PlayerPrefs.GetInt("PausedGame", 0) == 1;
            string lastLevel = PlayerPrefs.GetString("LastLevel", "");
            
            // Resume should only be enabled if game is paused AND a valid level is saved
            bool isValidLevel = !string.IsNullOrEmpty(lastLevel) && 
                               (lastLevel == "Level 1" || lastLevel == "Level 2" || lastLevel == "Level 3" ||
                                lastLevel == "Level 4" || lastLevel == "Level 5" || lastLevel == "Level 6");
            
            bool canResume = hasPausedGame && isValidLevel;
            
            // Enable/disable the resume button based on both conditions
            resumeButton.interactable = canResume;
            
            // Change visual appearance when disabled (gray out)
            var colors = resumeButton.colors;
            if (canResume)
            {
                colors.normalColor = Color.white; // Normal color when available
                colors.disabledColor = Color.white;
            }
            else
            {
                colors.normalColor = Color.gray; // Gray when not available
                colors.disabledColor = Color.gray;
            }
            resumeButton.colors = colors;
            
            Debug.Log("MainMenu: Resume button " + (canResume ? "enabled" : "disabled") + 
                     " - PausedGame: " + hasPausedGame + ", LastLevel: '" + lastLevel + "', Valid: " + isValidLevel);
        }
    }

    public void StartNewGame()
    {
        // ALWAYS start a completely new game from Level 1 with score 0
        Debug.Log("MainMenu: Starting NEW GAME - Level 1, Score 0");
        
        // Clear all saved game state
        PlayerPrefs.SetInt("PausedGame", 0);
        PlayerPrefs.SetString("LastLevel", ""); // Clear to indicate new game
        PlayerPrefs.SetFloat("PlayerX", 0f); // Reset Player Position
        PlayerPrefs.SetFloat("PlayerY", 0f);
        PlayerPrefs.SetInt("CurrentScore", 0); // Reset score to 0
        
        // CHOICE: Uncomment the line below if you want instructions to show for EVERY new game
        // PlayerPrefs.SetInt("InstructionsShown", 0);
        
        // Clear timer state
        PlayerPrefs.DeleteKey("SavedTimer");
        PlayerPrefs.DeleteKey("TimerWasRunning");
        
        // Clear coin collection state for all levels
        for (int i = 1; i <= 6; i++)
        {
            PlayerPrefs.DeleteKey("ExistingCoins_Level " + i);
        }
        
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level 1");
    }

    private void StartGameDelayed()
    {
        // This method is now deprecated - use StartNewGame instead
        StartNewGame();
    }

    private void QuitGameDelayed()
    {
        Application.Quit();
        Debug.Log("Game Quit!");
    }

    public void StartGame()
    {
        StartNewGame(); // Always start new game
    }

    public void ResumeGame()
    {
        // First, validate that we can actually resume
        bool hasPausedGame = PlayerPrefs.GetInt("PausedGame", 0) == 1;
        string lastLevel = PlayerPrefs.GetString("LastLevel", "Level 1");
        
        if (!hasPausedGame)
        {
            Debug.LogWarning("MainMenu: Cannot resume - no paused game found!");
            return;
        }
        
        // Validate the lastLevel before attempting to load
        if (string.IsNullOrEmpty(lastLevel) || lastLevel.Trim() == "")
        {
            Debug.LogError("MainMenu: LastLevel is empty! Clearing corrupted save data and aborting resume.");
            ValidateSaveData(); // This will clear the corrupted data
            UpdateResumeButtonState(); // Update button state to reflect cleared data
            return;
        }
        
        // Double-check that the scene name is valid before loading
        if (lastLevel != "Level 1" && lastLevel != "Level 2" && lastLevel != "Level 3" && 
            lastLevel != "Level 4" && lastLevel != "Level 5" && lastLevel != "Level 6")
        {
            Debug.LogError("MainMenu: Invalid level name '" + lastLevel + "'. Clearing corrupted save data and aborting resume.");
            ValidateSaveData(); // This will clear the corrupted data
            UpdateResumeButtonState(); // Update button state to reflect cleared data
            return;
        }
        
        int savedScore = PlayerPrefs.GetInt("CurrentScore", 0);
        Debug.Log("MainMenu: RESUMING GAME - Level: '" + lastLevel + "', Score: " + savedScore);
        Debug.Log("MainMenu: PlayerPrefs values - PausedGame: " + PlayerPrefs.GetInt("PausedGame", 0) + 
                 ", LastLevel: '" + PlayerPrefs.GetString("LastLevel", "NOT_FOUND") + "'");
        
        // All checks passed - safe to load the scene
        SceneManager.LoadScene(lastLevel);
    }

    public void QuitGame()
    {
        QuitGameDelayed();
    }

    public void RefreshMenuState()
    {
        // Call this method when returning to main menu to update button states
        UpdateResumeButtonState();
        Debug.Log("MainMenu: Menu state refreshed");
    }

    public void LoadGameScene()
    {
        PlayerPrefs.SetInt("PausedGame", 1); // Set flag to resume
        SceneManager.LoadScene(PlayerPrefs.GetString("LastLevel", "Level 1")); // Load Last Played Level
    }
}
