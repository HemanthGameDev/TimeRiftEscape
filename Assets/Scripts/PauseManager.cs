using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel; // Assign in Inspector
    private bool isPaused = false;
    private GameObject player; // Reference to the player

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Ensure player is found

        // **Restore Player Position on Resume**
        if (PlayerPrefs.GetInt("PausedGame", 0) == 1 && player != null)
        {
            float savedX = PlayerPrefs.GetFloat("PlayerX", player.transform.position.x);
            float savedY = PlayerPrefs.GetFloat("PlayerY", player.transform.position.y);
            player.transform.position = new Vector2(savedX, savedY);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0;
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenuPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            // Save Current Level
            PlayerPrefs.SetString("LastLevel", SceneManager.GetActiveScene().name);
            PlayerPrefs.SetInt("PausedGame", 1); // Mark game as paused
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
