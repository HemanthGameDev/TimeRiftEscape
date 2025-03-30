using UnityEngine;

public class FireworkParticle2D : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    public void Initialize(Vector2 dir, float spd, float life, Color color)
    {
        direction = dir;
        speed = spd;
        lifetime = life;

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>("circle"); // Ensure you have a "circle" sprite in Resources
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 10; // Renders above the background

        transform.localScale = Vector3.one * 0.3f; // Adjust particle size
        startColor = color;
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        lifetime -= Time.deltaTime;

        // Gradually fade out
        float fade = Mathf.Clamp01(lifetime / 1.5f);
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, fade);

        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
