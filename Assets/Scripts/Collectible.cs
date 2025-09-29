using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Shield, SpeedBoost, Coin }
    
    [Header("Collectible Settings")]
    public CollectibleType type;
    public float duration = 5f; // Default duration for power-ups
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip shieldCollectSound;
    [SerializeField] private AudioClip speedBoostCollectSound;
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;
    
    // Independent AudioSource for all collectibles
    private static AudioSource independentCollectibleAudioSource;

    private void Start()
    {
        SetupIndependentAudioSource();
    }
    
    private static void SetupIndependentAudioSource()
    {
        // Create a persistent AudioSource for collectibles
        if (independentCollectibleAudioSource == null)
        {
            GameObject audioObject = new GameObject("CollectibleAudioSource");
            DontDestroyOnLoad(audioObject);
            independentCollectibleAudioSource = audioObject.AddComponent<AudioSource>();
            independentCollectibleAudioSource.playOnAwake = false;
            independentCollectibleAudioSource.spatialBlend = 0f; // 2D audio
            independentCollectibleAudioSource.loop = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if this GameObject already has a dedicated Coin script
            // If it does, let the Coin script handle the collection instead
            if (type == CollectibleType.Coin && GetComponent<Coin>() != null)
            {
                Debug.Log("Collectible: Coin type detected but dedicated Coin script found - skipping to prevent double collection");
                return; // Exit early to prevent double collection
            }
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                PlayCollectSound();
                ApplyEffect(player);
            }
            Destroy(gameObject); // Remove collectible after effect is applied
        }
    }
    
    private void PlayCollectSound()
    {
        AudioClip soundToPlay = null;
        
        switch (type)
        {
            case CollectibleType.Shield:
                soundToPlay = shieldCollectSound;
                break;
            case CollectibleType.SpeedBoost:
                soundToPlay = speedBoostCollectSound;
                break;
            case CollectibleType.Coin:
                soundToPlay = coinCollectSound;
                break;
        }
        
        if (soundToPlay != null)
        {
            // Ensure independent AudioSource exists
            SetupIndependentAudioSource();
            
            if (independentCollectibleAudioSource != null)
            {
                independentCollectibleAudioSource.volume = soundVolume;
                independentCollectibleAudioSource.pitch = Random.Range(0.95f, 1.05f);
                independentCollectibleAudioSource.PlayOneShot(soundToPlay);
            }
            else
            {
                // Fallback: Play at camera position
                Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                AudioSource.PlayClipAtPoint(soundToPlay, playPosition, soundVolume);
            }
        }
    }

    void ApplyEffect(PlayerController player)
    {
        switch (type)
        {
            case CollectibleType.Shield:
                player.ActivateShield();
                break;

            case CollectibleType.SpeedBoost:
                player.IncreaseSpeed(duration);
                break;

            case CollectibleType.Coin:
                CollectCoin();
                break;
        }
    }

    void CollectCoin()
    {
        GameManager.Instance.AddCoins(1);
    }
}
