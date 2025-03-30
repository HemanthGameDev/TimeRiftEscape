using UnityEngine;

public class MovingTrap : MonoBehaviour
{
    public float speed = 2f; // Adjustable speed in Inspector
    public Transform pointA;
    public Transform pointB;

    private Vector3 target;
    private bool isMoving = true;

    private void Start()
    {
        target = pointB.position; // Start moving towards point B
    }

    private void Update()
    {
        if (!isMoving) return;

        // Move towards the target
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // If reached target, switch direction
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
            Debug.Log("Trap changing direction.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.IsShieldActive())
            {
                Debug.Log("Shield destroyed by moving trap!");
                player.DeactivateShield();
            }
            else
            {
                Debug.Log("Moving Trap hit! Playing death animation...");
                player.TriggerDeath(); // Play death animation before disabling
            }
        }
        else
        {
            Debug.Log("Trap hit an object. Stopping movement.");
            isMoving = false; // Stops the trap on collision
        }
    }
}
