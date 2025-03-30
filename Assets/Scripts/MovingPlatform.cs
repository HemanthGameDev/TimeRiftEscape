using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public Transform startPoint, endPoint;
    public float moveSpeed = 2f;
    private bool isMoving = false;
    private Vector3 lastPlatformPosition;

    private void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Start or End point is not assigned in " + gameObject.name);
            enabled = false;
        }
        lastPlatformPosition = transform.position;
    }

    public void ActivatePlatform()
    {
        if (!isMoving)
        {
            StartCoroutine(MovePlatform());
        }
    }

    IEnumerator MovePlatform()
    {
        isMoving = true;

        while (true) // Infinite loop to keep moving without delay
        {
            yield return StartCoroutine(MoveToPosition(endPoint.position));
            yield return StartCoroutine(MoveToPosition(startPoint.position));
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float timeOut = 10f;
        float timer = 0f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;

            timer += Time.deltaTime;
            if (timer > timeOut)
            {
                Debug.LogError("MoveToPosition timed out! Stopping movement.");
                transform.position = targetPosition;
                break;
            }
        }
    }

    // Carry the Player on Platform
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform); // Make player a child of platform
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null); // Remove player from platform
        }
    }
    private void LateUpdate()
    {
        Vector3 platformVelocity = (transform.position - lastPlatformPosition) / Time.deltaTime;
        lastPlatformPosition = transform.position;

        // Apply platform velocity to the player
        ApplyPlatformVelocityToPlayer(platformVelocity);
    }
    private void ApplyPlatformVelocityToPlayer(Vector3 velocity)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                Rigidbody2D playerRb = col.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity += new Vector2(velocity.x, 0);
                }
            }
        }
    }
}
