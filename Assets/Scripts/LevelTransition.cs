using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private bool moveToNextLevel = true; // Check this in Inspector for moving forward, uncheck for moving backward

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Ensure only the player triggers transition
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex; // Get current scene index
            int targetSceneIndex = moveToNextLevel ? currentSceneIndex + 1 : currentSceneIndex - 1; // Decide next scene

            if (targetSceneIndex >= 0 && targetSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(targetSceneIndex); // Load the target scene
            }
            else
            {
                Debug.LogWarning("Target scene index is out of bounds. Check Build Settings.");
            }
        }
    }
}
