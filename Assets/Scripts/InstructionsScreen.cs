using UnityEngine;

public class InstructionsManager : MonoBehaviour
{
    public GameObject instructionsPanel; // Assign in Inspector
    public GameObject backButton; // Assign in Inspector
    public GameObject pauseButton; // Assign in Inspector

    private void Start()
    {
        Time.timeScale = 0; // Pause game when instructions show
        instructionsPanel.SetActive(true); // Show Instructions first
                                           // Ensure Back & Pause buttons are hidden initially
        if (backButton != null) backButton.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(false);
    }

    public void ContinueGame()
    {
        Time.timeScale = 1; // Unpause game
        instructionsPanel.SetActive(false); // Hide instructions
                                            // **Enable Back & Pause buttons**
        if (backButton != null) backButton.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(true);
    }
}
