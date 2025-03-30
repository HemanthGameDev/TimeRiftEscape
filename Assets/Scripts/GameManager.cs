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
        RestorePlayerPosition();
        UpdateScoreUI(); // Refresh UI at start
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
        }

        SaveScoreData();
        UpdateScoreUI();
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
        UpdateScoreUI();
    }

    public void GoToMainMenu()
    {
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            PlayerPrefs.SetInt("PausedGame", 1);
            PlayerPrefs.Save(); // Force save in WebGL
        }

        SceneManager.LoadScene("MainMenu");
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
            }
        }
    }

    private void SaveScoreData()
    {
        PlayerPrefs.SetInt("HighestScore", highestScore);
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.Save(); // Ensure WebGL saves data
    }

    private void LoadScoreData()
    {
        highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0); // Restore previous score
    }

    private void UpdateScoreUI()
    {
        if (currentScoreText != null)
            currentScoreText.text = "Score: " + currentScore;

        if (highestScoreText != null)
            highestScoreText.text = "High Score: " + highestScore;
    }
}
