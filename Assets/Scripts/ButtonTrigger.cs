using System.Collections;
using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    public PlatformController platform; // Reference to the platform
    public float activationDelay = 2f; // Delay before platform moves
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            StartCoroutine(DelayedActivation());
        }
    }

    IEnumerator DelayedActivation()
    {
        Debug.Log("Button Pressed! Platform will activate in " + activationDelay + " seconds.");
        yield return new WaitForSeconds(activationDelay);
        platform.ActivatePlatform();
    }
}
