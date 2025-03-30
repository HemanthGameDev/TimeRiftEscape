using UnityEngine;
using System.Collections;

public class FireballTrap : MonoBehaviour
{
    public float speed = 3f; // Speed of fireball
    public float fireRate = 2f; // Time between fireball shots
    public Transform firePoint; // Spawn point for fireball
    public GameObject fireballPrefab; // Fireball prefab

    private void Start()
    {
        StartCoroutine(FireballRoutine());
    }

    private IEnumerator FireballRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireRate);
            ShootFireball();
        }
    }

    private void ShootFireball()
    {
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        // Set velocity to move UPWARDS
        fireball.GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * speed;

        Debug.Log("Fireball Launched Upward!");
    }
}
