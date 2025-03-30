using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText; // Reference to UI Text

    private void Update()
    {
        if (LevelManager.Instance != null) // Ensure LevelManager exists
        {
            float timeRemaining = LevelManager.Instance.GetCurrentTime();
            UpdateTimerDisplay(timeRemaining);
        }
    }

    private void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);  // Convert seconds to minutes
        int seconds = Mathf.FloorToInt(time % 60);  // Get remaining seconds
        timerText.text = $"Time Left: {minutes:00}:{seconds:00}"; // Format as MM:SS
    }
}
