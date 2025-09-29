using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public Button pauseButton; // Drag the /Canvas/Pause button here
    public Button backButton; // Drag the /Canvas/Back button here
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip pauseButtonSound;
    [SerializeField] private AudioClip backButtonSound;
    [SerializeField] private AudioClip resumeButtonSound;
    [SerializeField] [Range(0f, 1f)] private float uiSoundVolume = 0.7f;
    
    // Independent UI AudioSource
    private static AudioSource independentUIAudioSource;
    private bool isPaused = false;
    private GameObject player; // Reference to the player

    void Start()
    {
        SetupIndependentAudioSource();
        SetupButtons();
        
        // Auto-find player if not assigned or invalid
        if (player == null || (player != null && player.gameObject == null))
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("PauseManager: Player found and assigned: " + player.name);
            }
            else
            {
                Debug.LogWarning("PauseManager: Player not found! Make sure the player GameObject has the 'Player' tag.");
            }
        }
        
        // **ENSURE BUTTONS ARE VISIBLE BASED ON SCENE**
        EnsureButtonsVisibilityBasedOnScene();
        
        // Subscribe to scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from scene change events to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-setup buttons for the new scene (fresh button connections)
        SetupButtons();
        
        // Handle button visibility when a new scene loads
        EnsureButtonsVisibilityBasedOnScene();
        
        // Re-find player in the new scene if needed or invalid
        if (player == null || (player != null && player.gameObject == null))
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("PauseManager: Player found in new scene: " + player.name);
            }
        }
        
        Debug.Log("PauseManager: Scene loaded - " + scene.name + ", Buttons re-setup and visibility updated");
    }
    
    void Update()
    {
        // **CONTINUOUSLY ENSURE BUTTONS STAY VISIBLE BASED ON SCENE**
        EnsureButtonsVisibilityBasedOnScene();
        
        // ESC key support (only in level scenes)
        if (ShouldShowButtonsInCurrentScene() && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    private void EnsureButtonsVisibilityBasedOnScene()
    {
        // Only show buttons in level scenes (Level 1 to Level 6)
        if (!ShouldShowButtonsInCurrentScene())
        {
            HideButtonsInNonLevelScenes();
            return;
        }
        
        // Ensure buttons are visible in level scenes
        EnsureButtonsAreVisible();
    }
    
    private bool ShouldShowButtonsInCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Check if current scene is a level scene (Level 1 to Level 6)
        return currentSceneName == "Level 1" || 
               currentSceneName == "Level 2" || 
               currentSceneName == "Level 3" || 
               currentSceneName == "Level 4" || 
               currentSceneName == "Level 5" || 
               currentSceneName == "Level 6";
    }
    
    private void HideButtonsInNonLevelScenes()
    {
        if (pauseButton != null && pauseButton.gameObject.activeSelf)
        {
            pauseButton.gameObject.SetActive(false);
            Debug.Log("PauseManager: Pause button hidden in non-level scene: " + SceneManager.GetActiveScene().name);
        }
        
        if (backButton != null && backButton.gameObject.activeSelf)
        {
            backButton.gameObject.SetActive(false);
            Debug.Log("PauseManager: Back button hidden in non-level scene: " + SceneManager.GetActiveScene().name);
        }
    }
    
    private void EnsureButtonsAreVisible()
    {
        // Force pause button to be visible and active
        if (pauseButton != null)
        {
            if (!pauseButton.gameObject.activeSelf)
            {
                pauseButton.gameObject.SetActive(true);
                Debug.Log("PauseManager: Pause button activated for level scene");
            }
            if (!pauseButton.interactable)
            {
                pauseButton.interactable = true;
            }
        }
        
        // Force back button to be visible and active
        if (backButton != null)
        {
            if (!backButton.gameObject.activeSelf)
            {
                backButton.gameObject.SetActive(true);
                Debug.Log("PauseManager: Back button activated for level scene");
            }
            if (!backButton.interactable)
            {
                backButton.interactable = true;
            }
        }
    }
    
    private static void SetupIndependentAudioSource()
    {
        // Create a persistent AudioSource for UI sounds
        if (independentUIAudioSource == null)
        {
            GameObject audioObject = new GameObject("UIAudioSource");
            DontDestroyOnLoad(audioObject);
            independentUIAudioSource = audioObject.AddComponent<AudioSource>();
            independentUIAudioSource.playOnAwake = false;
            independentUIAudioSource.spatialBlend = 0f; // 2D audio for UI
            independentUIAudioSource.loop = false;
        }
    }
    
    private void SetupButtons()
    {
        // Auto-find buttons if not assigned or if they became null
        if (pauseButton == null)
        {
            GameObject pauseObj = GameObject.Find("Canvas/Pause");
            if (pauseObj != null)
            {
                pauseButton = pauseObj.GetComponent<Button>();
                Debug.Log("PauseManager: Found pause button in scene");
            }
        }
        
        if (backButton == null)
        {
            GameObject backObj = GameObject.Find("Canvas/Back");
            if (backObj != null)
            {
                backButton = backObj.GetComponent<Button>();
                Debug.Log("PauseManager: Found back button in scene");
            }
        }
        
        // Set up button listeners - always refresh to ensure proper connection
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            Debug.Log("PauseManager: Pause button connected and ready!");
        }
        else
        {
            Debug.LogWarning("PauseManager: Pause button not found! Please assign it in the inspector.");
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonClicked);
            Debug.Log("PauseManager: Back button connected and ready!");
        }
        else
        {
            Debug.LogWarning("PauseManager: Back button not found! Please assign it in the inspector.");
        }
    }
    
    private void PlayUISound(AudioClip clip)
    {
        if (clip != null)
        {
            SetupIndependentAudioSource();
            
            if (independentUIAudioSource != null)
            {
                independentUIAudioSource.volume = uiSoundVolume;
                independentUIAudioSource.pitch = Random.Range(0.95f, 1.05f);
                independentUIAudioSource.PlayOneShot(clip);
            }
            else
            {
                // Fallback
                Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(clip, playPosition, uiSoundVolume);
            }
        }
    }

    public void PauseGame()
    {
        PlayUISound(pauseButtonSound);
        isPaused = true;
        Time.timeScale = 0;
        
        Debug.Log("PauseManager: Game paused");
    }

    public void ResumeGame()
    {
        PlayUISound(resumeButtonSound);
        isPaused = false;
        Time.timeScale = 1;
        
        Debug.Log("PauseManager: Game resumed");
    }

    public void GoToMainMenu()
    {
        PlayUISound(backButtonSound);
        Time.timeScale = 1; // Reset time scale before scene change
        
        // Use GameManager's comprehensive save system instead of manual save
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
            Debug.Log("PauseManager: Using GameManager's comprehensive save system");
        }
        else
        {
            // Fallback if GameManager not available
            Debug.LogWarning("PauseManager: GameManager not found, using fallback");
            SceneManager.LoadScene("MainMenu");
        }
    }
    
    // Button event methods
    public void OnPauseButtonClicked()
    {
        Debug.Log("PauseManager: Pause button clicked!");
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void OnBackButtonClicked()
    {
        Debug.Log("PauseManager: Back button clicked!");
        GoToMainMenu();
    }
    
    // Legacy support methods
    public void TogglePause()
    {
        OnPauseButtonClicked();
    }
}
