using UnityEngine;

public class InstructionsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsPanel; // Assign in Inspector
    public GameObject backButton; // Assign in Inspector
    public GameObject pauseButton; // Assign in Inspector
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip continueButtonSound;
    [SerializeField] [Range(0f, 1f)] private float uiSoundVolume = 0.7f;
    
    // Independent UI AudioSource
    private static AudioSource independentInstructionsAudioSource;

    private void Awake()
    {
        // IMMEDIATE CHECK: If instructions have been shown, destroy this GameObject before anything else happens
        bool instructionsShown = PlayerPrefs.GetInt("InstructionsShown", 0) == 1;
        
        if (instructionsShown)
        {
            Debug.Log("InstructionsManager: Instructions already shown - DESTROYING in Awake()");
            Destroy(this.gameObject);
            return;
        }
        
        Debug.Log("InstructionsManager: First time - allowing instructions to show");
    }

    private void Start()
    {
        SetupIndependentAudioSource();
        
        // Check if instructions have already been shown
        bool instructionsShown = PlayerPrefs.GetInt("InstructionsShown", 0) == 1;
        
        if (instructionsShown)
        {
            // Instructions already shown - immediately destroy this GameObject
            Debug.Log("InstructionsManager: Instructions already shown - DESTROYING panel immediately");
            Destroy(this.gameObject);
            return; // Exit early to prevent any further execution
        }
        
        // First time showing instructions
        Time.timeScale = 0; // Pause game when instructions show
        instructionsPanel.SetActive(true); // Show Instructions first
        
        // **NEVER HIDE PAUSE & BACK BUTTONS - LET PAUSEMANAGER HANDLE THEM**
        // Removed the code that hides buttons - PauseManager will ensure they stay visible
        Debug.Log("InstructionsManager: Started - First time showing instructions");
    }
    
    private static void SetupIndependentAudioSource()
    {
        // Create a persistent AudioSource for instructions sounds
        if (independentInstructionsAudioSource == null)
        {
            GameObject audioObject = new GameObject("InstructionsAudioSource");
            DontDestroyOnLoad(audioObject);
            independentInstructionsAudioSource = audioObject.AddComponent<AudioSource>();
            independentInstructionsAudioSource.playOnAwake = false;
            independentInstructionsAudioSource.spatialBlend = 0f; // 2D audio for UI
            independentInstructionsAudioSource.loop = false;
        }
    }
    
    private void PlayUISound(AudioClip clip)
    {
        if (clip != null)
        {
            SetupIndependentAudioSource();
            
            if (independentInstructionsAudioSource != null)
            {
                independentInstructionsAudioSource.volume = uiSoundVolume;
                independentInstructionsAudioSource.pitch = Random.Range(0.95f, 1.05f);
                independentInstructionsAudioSource.PlayOneShot(clip);
            }
            else
            {
                // Fallback
                Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(clip, playPosition, uiSoundVolume);
            }
        }
    }

    public void ContinueGame()
    {
        PlayUISound(continueButtonSound);
        
        // Mark that instructions have been shown so they don't appear again
        PlayerPrefs.SetInt("InstructionsShown", 1);
        PlayerPrefs.Save(); // Force save to ensure persistence
        
        Time.timeScale = 1; // Unpause game
        
        // Destroy the entire GameObject (including this script and the panel)
        Debug.Log("InstructionsManager: Instructions dismissed - PERMANENTLY marked as shown, DESTROYING entire GameObject");
        Destroy(this.gameObject); // This destroys the entire instructions GameObject
    }
    
    // Public method for UI button to call
    public void OnContinueButtonClicked()
    {
        ContinueGame();
    }
    
    // Optional method to permanently disable instructions (call this if you never want them again)
    public static void PermanentlyDisableInstructions()
    {
        PlayerPrefs.SetInt("InstructionsShown", 1);
        PlayerPrefs.Save();
        Debug.Log("InstructionsManager: Instructions permanently disabled");
    }
    
    // Optional method to re-enable instructions for testing
    public static void ResetInstructionsFlag()
    {
        PlayerPrefs.SetInt("InstructionsShown", 0);
        PlayerPrefs.Save();
        Debug.Log("InstructionsManager: Instructions flag reset - will show again");
    }
}
