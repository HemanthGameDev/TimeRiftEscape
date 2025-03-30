using UnityEngine;

public class SwingingBlade : MonoBehaviour
{
    public float swingSpeed = 2f; // Adjust in Inspector
    public float swingAngle = 45f; // Max swing angle

    private float startRotation;

    private void Start()
    {
        startRotation = transform.rotation.eulerAngles.z;
    }

    private void Update()
    {
        float angle = startRotation + Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.IsShieldActive())
            {
                Debug.Log("Shield destroyed by swinging blade!");
                player.DeactivateShield();
            }
            else
            {
                Debug.Log("Player hit by swinging blade! Player is dead.");
                player.TriggerDeath();
            }
        }
    }
}
