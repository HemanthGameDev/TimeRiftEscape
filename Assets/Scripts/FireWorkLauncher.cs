using UnityEngine;

public class FireworkLauncher2D : MonoBehaviour
{
    public int particlesPerFirework = 15;
    public float explosionForce = 3.5f;
    public float particleLifetime = 1.5f;
    public float launchInterval = 1.2f;
    public Color[] fireworkColors;

    void Start()
    {
        InvokeRepeating(nameof(LaunchFirework), 1f, launchInterval);
    }

    void LaunchFirework()
    {
        Vector2 startPosition = new Vector2(Random.Range(-3f, 3f), Random.Range(-1f, 3f));
        Color fireworkColor = fireworkColors[Random.Range(0, fireworkColors.Length)];

        for (int i = 0; i < particlesPerFirework; i++)
        {
            float angle = i * (360f / particlesPerFirework);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject particle = new GameObject("FireworkParticle2D");
            particle.transform.position = startPosition;
            FireworkParticle2D fireworkParticle = particle.AddComponent<FireworkParticle2D>();
            fireworkParticle.Initialize(direction, explosionForce, particleLifetime, fireworkColor);
        }
    }
}
