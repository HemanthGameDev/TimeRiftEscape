using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    public enum TriggerType { Start, End }
    public TriggerType triggerType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Timer Triggered: {triggerType}"); // Debug log

            if (triggerType == TriggerType.Start)
            {
                // Only start timer if it's not already running (prevents overriding restored state)
                if (LevelManager.Instance != null && !LevelManager.Instance.IsTimerRunning())
                {
                    LevelManager.Instance.StartTimer();
                    Debug.Log("TimerTrigger: Started timer");
                }
                else
                {
                    Debug.Log("TimerTrigger: Timer already running, not starting again");
                }
            }
            else if (triggerType == TriggerType.End)
            {
                LevelManager.Instance.StopTimer();
            }
        }
    }
}
