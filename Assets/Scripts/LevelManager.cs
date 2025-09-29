using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private float levelTime = 60f; // Adjustable in Inspector
    private float currentTime;
    private bool isTimerRunning = false;
    private bool timerStateRestored = false; // Flag to prevent resets after restoration

    private Transform lastCheckpoint; // Stores the last checkpoint position
    private PlayerController player; // Reference to the player
    private Vector3 startPosition; // Stores the player's initial position

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;  // Game Over UI
    [SerializeField] private Button restartButton;   // Restart button

    [Header("Audio Settings")]
    [Tooltip("Sound played when player dies from trap/hazard - triggers in PlayerDiedByTrap()")]
    [SerializeField] private AudioClip playerDeathSound;     // Sound when player dies from trap/hazard
    [Tooltip("Sound played when timer runs out and game over occurs - triggers in HandleGameOver()")]
    [SerializeField] private AudioClip gameOverSound;       // Sound when timer runs out (game over)
    [Tooltip("Sound played when player restarts level after game over - triggers in RestartLevel()")]
    [SerializeField] private AudioClip restartSound;        // Sound when restarting level
    [Tooltip("Volume level for all death/game over/restart sounds")]
    [SerializeField] [Range(0f, 1f)] private float audioVolume = 1f; // Volume for death/game over sounds
    
    private AudioSource audioSource; // AudioSource for playing sounds
    private bool isGameOver = false; // Prevent multiple game over triggers

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Setup AudioSource for sound effects
        SetupAudioSource();
        
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerController>();
                startPosition = player.transform.position; // Set start position
            }
        }

        gameOverUI.SetActive(false); // Hide UI at the start
        restartButton.onClick.AddListener(RestartLevel); // Add listener to restart button
        
        // Start timer based on game state
        InitializeTimer();
    }
    
    private void InitializeTimer()
    {
        // Check if this is a resumed game with saved timer state
        if (PlayerPrefs.GetInt("PausedGame", 0) == 1)
        {
            // Don't initialize timer - wait for GameManager to restore state
            Debug.Log("LevelManager: Waiting for timer state restoration from saved game");
            // Pre-load the saved time to prevent showing 0
            currentTime = PlayerPrefs.GetFloat("SavedTimer", levelTime);
            isTimerRunning = false; // Will be set by restore method
            // DON'T reset timerStateRestored flag - let restoration set it
        }
        else
        {
            // New game or level progression - start fresh timer
            timerStateRestored = false; // Reset flag for new games
            StartTimer();
            Debug.Log("LevelManager: Fresh timer started for new game/level");
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                HandleGameOver(); // Timer expired
            }
        }
    }

    public void StartTimer()
    {
        if (!isTimerRunning) // Prevent multiple starts
        {
            isTimerRunning = true;
            
            // CRITICAL: Don't reset currentTime if it was already restored from save data
            if (timerStateRestored)
            {
                Debug.Log("LevelManager: Timer started with RESTORED time: " + currentTime + " (state was restored, not resetting to " + levelTime + ")");
            }
            else if (PlayerPrefs.GetInt("PausedGame", 0) != 1)
            {
                // Only reset for truly new games
                currentTime = levelTime;
                Debug.Log("LevelManager: Timer started fresh with time: " + levelTime);
            }
            else
            {
                // This is a resumed game but state hasn't been restored yet - keep existing currentTime
                Debug.Log("LevelManager: Timer started (resumed) with pre-loaded time: " + currentTime + " (waiting for restoration)");
            }
        }
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
    
    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }
    
    public void SetTimerState(float time, bool running)
    {
        currentTime = time;
        isTimerRunning = running;
        timerStateRestored = true; // Mark that state has been restored
        
        // Force update the timer UI immediately
        if (running)
        {
            Debug.Log("LevelManager: Timer state RESTORED and RUNNING - Time: " + time + ", Running: " + running + ", Timer display should show: " + Mathf.Ceil(time));
        }
        else
        {
            Debug.Log("LevelManager: Timer state RESTORED but PAUSED - Time: " + time + ", Running: " + running + ", Timer display should show: " + Mathf.Ceil(time));
        }
    }
    
    public void ResumeTimer()
    {
        if (!isTimerRunning)
        {
            isTimerRunning = true;
            Debug.Log("LevelManager: Timer resumed, current time: " + currentTime);
        }
    }
    
    public void ResetTimerForNewGame()
    {
        timerStateRestored = false;
        currentTime = levelTime;
        isTimerRunning = false;
        Debug.Log("LevelManager: Timer reset for new game - Time: " + levelTime);
    }

    // Called when the player dies by a trap
    public void PlayerDiedByTrap()
    {
        Vector3 respawnPosition = lastCheckpoint != null ? lastCheckpoint.position : startPosition;
        Debug.Log($"Player died! Respawning at {respawnPosition}");

        // Play player death sound
        PlaySound(playerDeathSound, "Player Death");

        player.RespawnAt(respawnPosition);

        player.gameObject.SetActive(true);
        player.enabled = true;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Stop momentum after respawning
    }


    // Called when the timer reaches zero
    public void HandleGameOver()
    {
        if (isGameOver) return; // Prevent multiple game over calls
        isGameOver = true;

        StopTimer();
        Debug.Log("Game Over! Timer expired - Playing game over sound.");

        // Play game over sound
        PlaySound(gameOverSound, "Game Over");

        player.TriggerDeath(); // Play death animation
        Invoke(nameof(ShowGameOverUI), 1.5f); // Wait for animation before showing UI
    }

    private void ShowGameOverUI()
    {
        gameOverUI.SetActive(true); // Show Game Over UI
        player.gameObject.SetActive(false); // Hide player
        Time.timeScale = 0;  // **Pause the game**
    }

    private void RestartLevel()
    {
        gameOverUI.SetActive(false);  // Hide UI
        Time.timeScale = 1;  // **Resume game speed**

        // Play restart sound
        PlaySound(restartSound, "Restart");

        player.RespawnAt(startPosition);
        player.gameObject.SetActive(true); // Ensure player is active
        player.enabled = true; // Re-enable player script

        ResetTimer(); // Reset the timer
        StartTimer(); // Restart timer

        isGameOver = false; // Reset game-over state
    }

    public void TimerExpired()
    {
        StopTimer();
        Debug.Log("Game Over! Timer expired - Respawning at STARTING checkpoint.");

        // ALWAYS respawn at starting position when timer runs out
        player.RespawnAt(startPosition);
        ResetTimer();
        
        Debug.Log("LevelManager: Player respawned at starting checkpoint due to timer expiration");
    }

    public void LoadNextLevel()
    {
        Debug.Log("Loading Next Level...");
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
    }

    public void ResetTimer()
    {
        currentTime = levelTime;
    }
    
    private void SetupAudioSource()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource for sound effects
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = audioVolume;
        
        Debug.Log("LevelManager: AudioSource setup complete");
    }
    
    private void PlaySound(AudioClip clip, string soundType = "")
    {
        if (clip != null && audioSource != null)
        {
            audioSource.volume = audioVolume;
            audioSource.PlayOneShot(clip);
            Debug.Log($"LevelManager: Playing {soundType} sound");
        }
        else if (clip == null)
        {
            Debug.LogWarning($"LevelManager: {soundType} sound clip is not assigned!");
        }
        else if (audioSource == null)
        {
            Debug.LogError("LevelManager: AudioSource is null! Cannot play sound.");
        }
    }
    
    // Public methods for testing sounds (optional)
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestPlayerDeathSound()
    {
        PlaySound(playerDeathSound, "Player Death (Test)");
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestGameOverSound()
    {
        PlaySound(gameOverSound, "Game Over (Test)");
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestRestartSound()
    {
        PlaySound(restartSound, "Restart (Test)");
    }
}