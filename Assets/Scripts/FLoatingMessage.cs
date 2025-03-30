using UnityEngine;
using UnityEngine.UI;

public class FloatingMessage : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatHeight = 10f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatHeight, 0);
    }
}
