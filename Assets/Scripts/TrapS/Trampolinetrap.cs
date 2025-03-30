using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float bounceForce = 10f; // Adjustable bounce power

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log("Player bounced on trampoline!");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset Y velocity
                rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse); // Apply bounce force
            }
        }
    }
}
