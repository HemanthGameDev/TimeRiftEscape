using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int currentScore = 0;
    private int highestScore = 0;

    public GameObject player; // Assign Player in Inspector
    public TextMeshProUGUI currentScoreText; // Assign in Inspector
    public TextMeshProUGUI highestScoreText; // Assign in Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Move this GameObject to root level if it's not already
            if (transform.parent != null)
            {
                transform.SetParent(null);
                Debug.Log("GameManager: Moved to root level for proper persistence");
            }
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        LoadScoreData(); // Load saved scores
    }

    private void Start()
    {
        // Auto-find references when scene loads
        FindSceneReferences();
        RestorePlayerPosition();
        UpdateScoreUI(); // Refresh UI at start
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // FIRST: Handle instructions cleanup regardless of scene
        HandleInstructionsCleanup();
        
        // Re-find references when a new scene loads
        FindSceneReferences();
        RestorePlayerPosition();
        
        // Check if this is MainMenu scene - refresh menu state
        if (scene.name == "MainMenu")
        {
            // If we have a MainMenu component, refresh its state
            MainMenu mainMenu = FindFirstObjectByType<MainMenu>();
            if (mainMenu != null)
            {
                // Wait a frame then refresh to ensure everything is initialized
                StartCoroutine(RefreshMainMenuDelayed(mainMenu));
            }
        }
        else
        {
            // For level scenes, restore timer and coin state when resuming
            if (PlayerPrefs.GetInt("PausedGame", 0) == 1)
            {
                // Use coroutine to ensure LevelManager is initialized first
                StartCoroutine(RestoreGameStateDelayed());
            }
        }
        
        // Check if this is a completely new game (starting from main menu with LastLevel cleared)
        string lastLevel = PlayerPrefs.GetString("LastLevel", "");
        bool isPausedGame = PlayerPrefs.GetInt("PausedGame", 0) == 1;
        
        if (scene.name == "Level 1" && lastLevel == "" && !isPausedGame)
        {
            // This is a brand new game starting from main menu
            StartNewGame();
            Debug.Log("GameManager: New game started from main menu - Score reset to 0");
        }
        else
        {
            // This is either:
            // - Level progression (keep score)
            // - Resumed game (keep score) 
            // - Scene reload (keep score)
            UpdateScoreUI();
            Debug.Log("GameManager: Continuing game - Score preserved: " + currentScore);
        }
        
        Debug.Log("GameManager: Scene loaded - " + scene.name + ", references updated");
    }
    
    private System.Collections.IEnumerator RefreshMainMenuDelayed(MainMenu mainMenu)
    {
        yield return null; // Wait one frame
        mainMenu.RefreshMenuState();
    }
    
    private System.Collections.IEnumerator RestoreGameStateDelayed()
    {
        // Wait just one frame for LevelManager to initialize
        yield return null;
        RestoreTimerState();
        RestoreCollectedCoinsState();
    }

    private void FindSceneReferences()
    {
        // Auto-find player if not assigned or if current reference is invalid
        if (player == null || player.gameObject == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("GameManager: Player found and assigned: " + player.name);
            }
            else
            {
                Debug.LogWarning("GameManager: Player not found in scene! Make sure the player GameObject has the 'Player' tag.");
            }
        }
        
        // Auto-find score UI elements if not assigned or invalid
        if (currentScoreText == null || currentScoreText.gameObject == null)
        {
            // Look for CurrentScore text in ScoreUpdate
            GameObject scoreObject = GameObject.Find("ScoreUpdate/CurrentScore");
            if (scoreObject != null)
            {
                currentScoreText = scoreObject.GetComponent<TextMeshProUGUI>();
                if (currentScoreText != null)
                {
                    Debug.Log("GameManager: Current score text found and assigned");
                }
            }
            
            // Fallback: try to find any TextMeshProUGUI with "Score" in the name
            if (currentScoreText == null)
            {
                TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                foreach (var text in allTexts)
                {
                    if (text.name.ToLower().Contains("currentscore") || text.name.ToLower().Contains("score"))
                    {
                        currentScoreText = text;
                        Debug.Log("GameManager: Current score text found via fallback: " + text.name);
                        break;
                    }
                }
            }
        }
        
        if (highestScoreText == null || highestScoreText.gameObject == null)
        {
            // Look for HighestScore text in ScoreUpdate
            GameObject highScoreObject = GameObject.Find("ScoreUpdate/HighestScore");
            if (highScoreObject != null)
            {
                highestScoreText = highScoreObject.GetComponent<TextMeshProUGUI>();
                if (highestScoreText != null)
                {
                    Debug.Log("GameManager: Highest score text found and assigned");
                }
            }
            
            // Fallback: try to find any TextMeshProUGUI with "High" in the name
            if (highestScoreText == null)
            {
                TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                foreach (var text in allTexts)
                {
                    if (text.name.ToLower().Contains("highest") || text.name.ToLower().Contains("high"))
                    {
                        highestScoreText = text;
                        Debug.Log("GameManager: Highest score text found via fallback: " + text.name);
                        break;
                    }
                }
            }
        }
    }

    public void AddCoins(int amount)
    {
        currentScore += amount;

        // Update highest score if current score exceeds it
        if (currentScore > highestScore)
        {
            highestScore = currentScore;
            PlayerPrefs.SetInt("HighestScore", highestScore);
            PlayerPrefs.Save(); // Force save in WebGL
            Debug.Log("GameManager: NEW HIGH SCORE! " + highestScore);
        }

        SaveScoreData();
        UpdateScoreUI();
        Debug.Log("GameManager: Score updated - Current: " + currentScore + ", High: " + highestScore);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetHighestScore()
    {
        return highestScore;
    }

    public void ResetCurrentScore()
    {
        currentScore = 0;
        SaveScoreData(); // Save the reset
        UpdateScoreUI();
        Debug.Log("GameManager: Current score reset to 0, High score preserved: " + highestScore);
    }

    public void GoToMainMenu()
    {
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            PlayerPrefs.SetInt("PausedGame", 1);
            Debug.Log("GameManager: Player position saved - X: " + player.transform.position.x + ", Y: " + player.transform.position.y);
        }
        
        // Save current level for resume
        string currentLevelName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastLevel", currentLevelName);
        Debug.Log("GameManager: LastLevel saved as: '" + currentLevelName + "'");
        
        // Save timer state if LevelManager exists
        if (LevelManager.Instance != null)
        {
            float currentTime = LevelManager.Instance.GetCurrentTime();
            bool timerRunning = LevelManager.Instance.IsTimerRunning();
            PlayerPrefs.SetFloat("SavedTimer", currentTime);
            PlayerPrefs.SetInt("TimerWasRunning", timerRunning ? 1 : 0);
            Debug.Log("GameManager: Timer state saved - Time: " + currentTime + ", Running: " + timerRunning);
        }
        
        // Save collected coins state
        SaveCollectedCoinsState();
        
        // Save current game state including score
        SaveScoreData();
        PlayerPrefs.Save(); // Force save in WebGL
        Debug.Log("GameManager: Going to main menu - Complete state saved, Score: " + currentScore);

        SceneManager.LoadScene("MainMenu");
    }
    
    private void SaveCollectedCoinsState()
    {
        // Find all existing (not collected) coins and save their positions
        string sceneName = SceneManager.GetActiveScene().name;
        Coin[] remainingCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        System.Text.StringBuilder existingCoins = new System.Text.StringBuilder();
        
        for (int i = 0; i < remainingCoins.Length; i++)
        {
            if (remainingCoins[i] != null && remainingCoins[i].gameObject.activeInHierarchy)
            {
                Vector3 pos = remainingCoins[i].transform.position;
                string coinId = pos.x.ToString("F2") + "," + pos.y.ToString("F2");
                if (i > 0) existingCoins.Append("|");
                existingCoins.Append(coinId);
            }
        }
        
        PlayerPrefs.SetString("ExistingCoins_" + sceneName, existingCoins.ToString());
        Debug.Log("GameManager: Coin state saved for " + sceneName + " - Remaining coins: " + remainingCoins.Length);
    }

    private void RestorePlayerPosition()
    {
        if (PlayerPrefs.GetInt("PausedGame", 0) == 1)
        {
            if (player != null && PlayerPrefs.HasKey("PlayerX") && PlayerPrefs.HasKey("PlayerY"))
            {
                float x = PlayerPrefs.GetFloat("PlayerX");
                float y = PlayerPrefs.GetFloat("PlayerY");
                player.transform.position = new Vector2(x, y);
                Debug.Log("GameManager: Player position restored - X: " + x + ", Y: " + y);
            }
        }
    }
    
    private void RestoreTimerState()
    {
        if (PlayerPrefs.GetInt("PausedGame", 0) == 1 && LevelManager.Instance != null)
        {
            float savedTime = PlayerPrefs.GetFloat("SavedTimer", 60f);
            bool wasRunning = PlayerPrefs.GetInt("TimerWasRunning", 0) == 1;
            
            Debug.Log("GameManager: Restoring timer - SavedTimer: " + savedTime + ", WasRunning: " + wasRunning);
            Debug.Log("GameManager: LevelManager currentTime before restore: " + LevelManager.Instance.GetCurrentTime());
            
            // First set the timer state with the restored values
            LevelManager.Instance.SetTimerState(savedTime, wasRunning);
            
            Debug.Log("GameManager: LevelManager currentTime after restore: " + LevelManager.Instance.GetCurrentTime());
            
            // Force update timer UI immediately
            TimerUI timerUI = FindFirstObjectByType<TimerUI>();
            if (timerUI != null)
            {
                timerUI.ForceUpdateDisplay();
                Debug.Log("GameManager: Timer UI force updated to show: " + Mathf.Ceil(savedTime));
            }
            
            Debug.Log("GameManager: Timer restoration COMPLETE - Time: " + savedTime + ", Running: " + wasRunning);
        }
        else
        {
            Debug.Log("GameManager: Timer restore skipped - PausedGame: " + PlayerPrefs.GetInt("PausedGame", 0) + ", LevelManager exists: " + (LevelManager.Instance != null));
        }
    }
    
    private void HandleInstructionsCleanup()
    {
        // Check if instructions have been shown - if so, destroy any instruction panels in the scene
        bool instructionsShown = PlayerPrefs.GetInt("InstructionsShown", 0) == 1;
        
        if (instructionsShown)
        {
            // Find and destroy all InstructionsManager GameObjects
            InstructionsManager[] instructionsManagers = FindObjectsByType<InstructionsManager>(FindObjectsSortMode.None);
            foreach (InstructionsManager manager in instructionsManagers)
            {
                if (manager != null)
                {
                    Debug.Log("GameManager: DESTROYING InstructionsManager GameObject - instructions already shown");
                    Destroy(manager.gameObject);
                }
            }
            
            // Also find any GameObject with "Instruction" in the name and destroy it
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.name.ToLower().Contains("instruction"))
                {
                    Debug.Log("GameManager: DESTROYING Instructions GameObject: " + obj.name);
                    Destroy(obj);
                }
            }
        }
    }
    
    private void RestoreCollectedCoinsState()
    {
        if (PlayerPrefs.GetInt("PausedGame", 0) == 1)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string existingCoinsData = PlayerPrefs.GetString("ExistingCoins_" + sceneName, "");
            
            if (!string.IsNullOrEmpty(existingCoinsData))
            {
                // Get all coins in the scene
                Coin[] allCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
                
                // Parse saved existing coins
                string[] existingCoinIds = existingCoinsData.Split('|');
                System.Collections.Generic.List<string> existingPositions = new System.Collections.Generic.List<string>(existingCoinIds);
                
                // Destroy coins that were collected (not in the existing list)
                int destroyedCount = 0;
                foreach (Coin coin in allCoins)
                {
                    if (coin != null)
                    {
                        Vector3 pos = coin.transform.position;
                        string coinId = pos.x.ToString("F2") + "," + pos.y.ToString("F2");
                        
                        if (!existingPositions.Contains(coinId))
                        {
                            // This coin was collected, remove it
                            Destroy(coin.gameObject);
                            destroyedCount++;
                        }
                    }
                }
                
                Debug.Log("GameManager: Coin state restored for " + sceneName + " - Removed " + destroyedCount + " collected coins");
            }
        }
    }

    public void SaveCurrentScoreForTransition()
    {
        // Save current score and mark as level progression (not paused)
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.SetString("LastLevel", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("PausedGame", 0); // Not paused, just progressing
        PlayerPrefs.Save();
        Debug.Log("GameManager: Score saved for level transition - Current: " + currentScore);
    }

    private void SaveScoreData()
    {
        PlayerPrefs.SetInt("HighestScore", highestScore);
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.Save(); // Ensure WebGL saves data
    }

    private void LoadScoreData()
    {
        // Always load highest score
        highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        
        // Always load current score (preserve it across level transitions)
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        
        Debug.Log("GameManager: Scores loaded - Current: " + currentScore + ", High: " + highestScore);
    }

    public void ForceNewGame()
    {
        // Force start a completely new game, clearing all paused state
        PlayerPrefs.SetInt("PausedGame", 0);
        PlayerPrefs.SetInt("CurrentScore", 0);
        PlayerPrefs.SetFloat("PlayerX", 0f);
        PlayerPrefs.SetFloat("PlayerY", 0f);
        PlayerPrefs.SetString("LastLevel", "Level 1");
        PlayerPrefs.Save();
        
        currentScore = 0;
        UpdateScoreUI();
        
        Debug.Log("GameManager: Forced new game - Current score: 0, High score preserved: " + highestScore);
    }

    public void StartNewGame()
    {
        // Clear paused game state
        PlayerPrefs.SetInt("PausedGame", 0);
        PlayerPrefs.SetInt("CurrentScore", 0); // Explicitly reset current score
        PlayerPrefs.Save();
        
        // Reset current score in memory
        currentScore = 0;
        
        // Keep highest score as is
        UpdateScoreUI();
        
        Debug.Log("GameManager: New game started - Current score: " + currentScore + ", High score: " + highestScore);
    }

    public void RefreshReferences()
    {
        FindSceneReferences();
        UpdateScoreUI();
        Debug.Log("GameManager: References manually refreshed");
    }

    private void UpdateScoreUI()
    {
        if (currentScoreText != null)
            currentScoreText.text = "Score: " + currentScore;

        if (highestScoreText != null)
            highestScoreText.text = "High Score: " + highestScore;
    }
}
