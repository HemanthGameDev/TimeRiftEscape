using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private float levelTime = 60f; // Adjustable in Inspector
    private float currentTime;
    private bool isTimerRunning = false;

    private Transform lastCheckpoint; // Stores the last checkpoint position
    private PlayerController player; // Reference to the player
    private Vector3 startPosition; // Stores the player's initial position

    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverUI;  // Game Over UI
    [SerializeField] private Button restartButton;   // Restart button

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
            currentTime = levelTime;
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

    // Called when the player dies by a trap
    public void PlayerDiedByTrap()
    {
        Vector3 respawnPosition = lastCheckpoint != null ? lastCheckpoint.position : startPosition;
        Debug.Log($"Player died! Respawning at {respawnPosition}");

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
        Debug.Log("Game Over! Playing death animation.");

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
        Debug.Log("Game Over! Respawning at last checkpoint.");

        if (lastCheckpoint != null)
        {
            player.RespawnAt(lastCheckpoint.position);
        }
        else
        {
            Debug.LogWarning("No checkpoint found! Respawning at start position.");
            player.RespawnAt(startPosition);
        }
        ResetTimer();
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
}