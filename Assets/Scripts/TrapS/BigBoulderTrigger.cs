using UnityEngine;

public class BoulderTrigger : MonoBehaviour
{
    public BoulderTrap boulderTrap;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player stepped under boulder! Trigger activated.");
            boulderTrap.DropBoulder(); // Calls function in BoulderTrap script
            Destroy(gameObject); // Remove the trigger after activation
        }
    }
}
