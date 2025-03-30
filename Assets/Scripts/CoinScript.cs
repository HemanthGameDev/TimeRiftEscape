using UnityEngine;

public class Coin : MonoBehaviour
{
    private Animator animator;
    private static readonly int CoinAnimation = Animator.StringToHash("Coin_Animation");
    private bool collected = false; // Prevent multiple triggers

    private void Start()
    {
        animator = GetComponent<Animator>();
        PlayCoinAnimation();
    }

    public void PlayCoinAnimation()
    {
        if (animator != null)
        {
            animator.Play(CoinAnimation);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return; // Prevent multiple triggers

        if (other.CompareTag("Player"))
        {
            collected = true; // Mark as collected

            // Disable Collider to prevent further triggers
            GetComponent<Collider2D>().enabled = false;

            // Add coin to GameManager
            GameManager.Instance.AddCoins(1);

            // Play animation before collecting the coin
            PlayCoinAnimation();

            // Destroy the coin after animation plays
            Destroy(gameObject, 0.3f);
        }
    }
}
