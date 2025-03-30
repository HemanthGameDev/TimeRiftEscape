using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && player.IsShieldActive())
            {
                Debug.Log("Trap hit! Shield destroyed, player survives.");
                player.DeactivateShield(); // Destroy only the shield
            }
            else
            {
                Debug.Log("Trap activated! Player is dead.");
                player.TriggerDeath(); // Call a new function in PlayerController
            }
        }
    }

}
