using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject instructionsPanel; // Assign in Inspector (ONLY in Level 1 Scene)

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
    }
    public void StartGame()
    {
        PlayerPrefs.SetInt("PausedGame", 0);
        PlayerPrefs.SetString("LastLevel", "Level 1"); // Start from Level 1
        PlayerPrefs.SetFloat("PlayerX", 0f); // Reset Player Position
        PlayerPrefs.SetFloat("PlayerY", 0f);
        SceneManager.LoadScene("Level 1");
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
        Application.Quit();
        Debug.Log("Game Quit!");
    }

    public void LoadGameScene()
    {
        PlayerPrefs.SetInt("PausedGame", 1); // Set flag to resume
        SceneManager.LoadScene(PlayerPrefs.GetString("LastLevel", "Level 1")); // Load Last Played Level
    }
}
