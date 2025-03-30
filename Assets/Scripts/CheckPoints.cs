using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.Instance.SetCheckpoint(transform);
            Debug.Log("CheckPoint Activated");
        }
    }
}
