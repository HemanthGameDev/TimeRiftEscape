using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        public float parallaxSpeed;
        private float textureUnitSizeX;

        public void Initialize()
        {
            SpriteRenderer spriteRenderer = layerTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                textureUnitSizeX = spriteRenderer.bounds.size.x;
            }
        }

        public void CheckPosition(Transform cameraTransform)
        {
            float cameraPositionX = cameraTransform.position.x;
            float layerPositionX = layerTransform.position.x;

            if (Mathf.Abs(cameraPositionX - layerPositionX) >= textureUnitSizeX)
            {
                float offsetPositionX = (cameraPositionX - layerPositionX) % textureUnitSizeX;
                layerTransform.position = new Vector3(cameraPositionX + offsetPositionX, layerTransform.position.y, layerTransform.position.z);
            }
        }
    }

    public ParallaxLayer[] layers;
    private Transform cameraTransform;
    private Vector3 previousCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;

        foreach (ParallaxLayer layer in layers)
        {
            layer.Initialize();
        }
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                Vector3 newPosition = layer.layerTransform.position;
                newPosition.x += deltaMovement.x * layer.parallaxSpeed;
                layer.layerTransform.position = newPosition;

                // Check and reposition for infinite scrolling
                layer.CheckPosition(cameraTransform);
            }
        }

        previousCameraPosition = cameraTransform.position;
    }
}
