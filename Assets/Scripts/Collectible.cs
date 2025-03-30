using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Shield, SpeedBoost, Coin }
    public CollectibleType type;
    public float duration = 5f; // Default duration for power-ups

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyEffect(player);
            }
            Destroy(gameObject); // Remove collectible after effect is applied
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
                
                break;
        }
    }

    void CollectCoin()
    {
        GameManager.Instance.AddCoins(1);
    }
}
