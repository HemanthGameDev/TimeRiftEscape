using UnityEngine;

public class BoulderTrap : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasFallen = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Keeps boulder stationary until triggered
    }

    public void DropBoulder()
    {
        if (!hasFallen)
        {
            hasFallen = true;
            Debug.Log("Boulder is falling!");
            rb.bodyType = RigidbodyType2D.Dynamic; // Enables physics
            rb.gravityScale = 1; // Enables falling
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();

            if (player != null && player.IsShieldActive())
            {
                player.DeactivateShield();
                Debug.Log("Shield absorbed the boulder impact!");
            }
            else
            {
                Debug.Log("Boulder crushed the player! Playing death animation...");
                player.TriggerDeath(); // Play death animation before disabling
            }
        }
        else if (collision.collider.CompareTag("Ground"))
        {
            Debug.Log("Boulder hit the ground. Disappearing.");
            Destroy(gameObject, 1f);
        }
    }
}
