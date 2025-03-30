using UnityEngine;

public class AcidPool : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.IsShieldActive())
            {
                Debug.Log("Shield destroyed by acid pool!");
                player.DeactivateShield();
            }
            else
            {
                Debug.Log("Player stepped into acid! Player is dead.");
                player.TriggerDeath();
            }
        }
    }
}
