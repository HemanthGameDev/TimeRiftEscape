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
                LevelManager.Instance.StartTimer();
            }
            else if (triggerType == TriggerType.End)
            {
                LevelManager.Instance.StopTimer();
            }
        }
    }
}
