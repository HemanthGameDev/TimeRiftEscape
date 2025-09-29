using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Animation")]
    private Animator animator;
    private static readonly int CoinAnimation = Animator.StringToHash("Coin_Animation");
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField] private bool use3DAudio = false;
    
    // Static AudioSource for all coins - completely independent
    private static AudioSource independentAudioSource;
    private bool collected = false; // Prevent multiple triggers

    private void Start()
    {
        animator = GetComponent<Animator>();
        SetupIndependentAudioSource();
        PlayCoinAnimation();
    }
    
    private static void SetupIndependentAudioSource()
    {
        // Create a persistent AudioSource that survives coin destruction
        if (independentAudioSource == null)
        {
            GameObject audioObject = new GameObject("CoinAudioSource");
            DontDestroyOnLoad(audioObject);
            independentAudioSource = audioObject.AddComponent<AudioSource>();
            independentAudioSource.playOnAwake = false;
            independentAudioSource.spatialBlend = 0f; // Always 2D for independence
            independentAudioSource.loop = false;
        }
    }

    public void PlayCoinAnimation()
    {
        if (animator != null)
        {
            animator.Play(CoinAnimation);
        }
    }
    
    private void PlayCoinSound()
    {
        if (coinCollectSound != null)
        {
            // Ensure independent AudioSource exists
            SetupIndependentAudioSource();
            
            if (independentAudioSource != null)
            {
                // Configure for this sound
                independentAudioSource.volume = soundVolume;
                independentAudioSource.pitch = Random.Range(0.9f, 1.1f);
                independentAudioSource.PlayOneShot(coinCollectSound);
            }
            else
            {
                // Ultimate fallback - play at camera position
                Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(coinCollectSound, playPosition, soundVolume);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return; // Prevent multiple triggers

        if (other.CompareTag("Player"))
        {
            collected = true; // Mark as collected

            // Disable Collider to prevent further triggers (including Collectible script)
            GetComponent<Collider2D>().enabled = false;
            
            // Disable Collectible component if it exists to prevent interference
            Collectible collectibleComponent = GetComponent<Collectible>();
            if (collectibleComponent != null)
            {
                collectibleComponent.enabled = false;
                Debug.Log("Coin: Disabled Collectible component to prevent double collection");
            }

            // Play sound immediately - independent of this object
            PlayCoinSound();

            // Add coin to GameManager
            GameManager.Instance.AddCoins(1);

            // Play animation before collecting the coin
            PlayCoinAnimation();

            // Destroy the coin after animation plays
            Destroy(gameObject, 0.3f);
        }
    }
}
