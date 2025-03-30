using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float lifetime = 3f; // Time before fireball disappears (Adjustable in Inspector)

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy after traveling a certain range
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null && player.IsShieldActive())
            {
                Debug.Log("Shield destroyed by fireball!");
                player.DeactivateShield();
            }
            else
            {
                Debug.Log("Fireball hit! Playing death animation...");
                player.TriggerDeath(); // Play death animation before disabling
            }
        }
        else if (!other.CompareTag("Fireball")) // Ignore other fireballs
        {
            Debug.Log("Fireball hit an object. Destroying.");
            Destroy(gameObject); // Fireball disappears on impact
        }
    }
}
