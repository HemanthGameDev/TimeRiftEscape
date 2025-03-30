using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Assign the player here
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f); // Adjust as needed
    [SerializeField] private float smoothSpeed = 5f; // Smooth transition speed
    [SerializeField] private bool useLateUpdate = true; // Ensures compatibility with parallax scripts

    private void FixedUpdate()
    {
        if (!useLateUpdate)
        {
            FollowTarget();
        }
    }

    private void LateUpdate()
    {
        if (useLateUpdate)
        {
            FollowTarget();
        }
    }

    private void FollowTarget()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
