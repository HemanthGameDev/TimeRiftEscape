using UnityEngine; 
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 mobileInput; // Mobile input from touch controls

    private float jumpForce = 10f;
    private bool isShieldActive = false;
    private GameObject shieldObject;
    private float defaultSpeed = 5f;
    private float currentSpeed;
    private bool isFlipped = false;
    private bool isDead = false;
    private bool mobileJumpRequested = false; // Flag for mobile jump input

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;

    // Animation States
    private const string PLAYER_IDLE = "Player_Idle";
    private const string PLAYER_RUN = "Player_Run";
    private const string PLAYER_JUMP = "Player_Jump";
    private const string PLAYER_FALL = "Player_Fall";
    private const string PLAYER_DIE = "Player_Die";
    private string currentState;

    void Start()
    {
        currentSpeed = defaultSpeed;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); // Ensure Rigidbody is initialized
    }

    private void Update()
    {
        if (isDead) return;

        // Combine keyboard/gamepad input with mobile input
        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
        moveInput = keyboardInput + mobileInput;
        
        // Clamp to prevent over-acceleration when both inputs are active
        moveInput.x = Mathf.Clamp(moveInput.x, -1f, 1f);
        
        CheckGround();
        HandleAnimation();

        // Jump using Spacebar OR mobile jump request
        if (Input.GetKeyDown(KeyCode.Space) || mobileJumpRequested)
        {
            Jump();
            mobileJumpRequested = false; // Reset mobile jump flag
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);

        if (moveInput.x > 0 && isFlipped)
        {
            FlipPlayer();
        }
        else if (moveInput.x < 0 && !isFlipped)
        {
            FlipPlayer();
        }
    }

    private void HandleAnimation()
    {
        if (isDead) return;

        if (!isGrounded)
        {
            ChangeAnimationState(rb.linearVelocity.y > 0 ? PLAYER_JUMP : PLAYER_FALL);
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            ChangeAnimationState(PLAYER_RUN);
        }
        else
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
    }

    private void FlipPlayer()
    {
        isFlipped = !isFlipped;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);
        currentState = newState;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Jump()
    {
        if (isGrounded && !isDead)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            ChangeAnimationState(PLAYER_JUMP);
        }
    }

    public void ActivateShield()
    {
        if (!isShieldActive)
        {
            isShieldActive = true;

            if (shieldObject == null)
            {
                shieldObject = new GameObject("Shield");
                shieldObject.transform.SetParent(transform);
                shieldObject.transform.localPosition = Vector3.zero;
                SpriteRenderer sr = shieldObject.AddComponent<SpriteRenderer>();
                sr.sprite = Resources.Load<Sprite>("ShieldSprite");
                sr.sortingOrder = 1;
            }
            shieldObject.SetActive(true);
        }
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }

    public void DeactivateShield()
    {
        if (isShieldActive)
        {
            isShieldActive = false;
            if (shieldObject != null)
            {
                Destroy(shieldObject);
                shieldObject = null;
            }
        }
    }

    public void IncreaseSpeed(float duration)
    {
        currentSpeed = defaultSpeed * 3f;
        Invoke("ResetSpeed", duration);
    }

    void ResetSpeed()
    {
        currentSpeed = defaultSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Trap"))
        {
            Debug.Log($"Player hit a trap: {collision.gameObject.name}"); // Debug to check traps
            DieByTrap();
        }
    }
    // Handles death by trap (Respawns at checkpoint)
    private void DieByTrap()
    {
        if (isDead) return;

        isDead = true;
        ChangeAnimationState(PLAYER_DIE);
        rb.linearVelocity = Vector2.zero; // Fix typo (use `velocity` instead of `linearVelocity`)
        rb.bodyType = RigidbodyType2D.Static; // Stop movement completely

        Invoke(nameof(RespawnAfterTrap), 0.4f); // Allow death animation before respawning
    }
    private void RespawnAfterTrap()
    {
        isDead = false;
        rb.bodyType = RigidbodyType2D.Dynamic; // Re-enable physics
        gameObject.SetActive(true);
        LevelManager.Instance.PlayerDiedByTrap();
    }

    public void RespawnAt(Vector3 position)
    {
        transform.position = position;
        isDead = false;
        this.enabled = true;
        gameObject.SetActive(true);
        Debug.Log($"Player respawned at: {position}");
        //  Reset Rigidbody velocity
        rb.linearVelocity = Vector2.zero;

        //  Set animation to idle to ensure movement resumes
        ChangeAnimationState(PLAYER_IDLE);
    }


    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;
        ChangeAnimationState(PLAYER_DIE);
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
        Invoke(nameof(RespawnAfterTrap), 0.4f);
    }

    // Mobile input methods (called by MobileControls script)
    public void SetMobileMovementInput(Vector2 input)
    {
        mobileInput = input;
        if (input.x != 0)
        {
            Debug.Log($"PlayerController: Mobile input received: {input.x}");
        }
    }

    public void TriggerMobileJump()
    {
        Debug.Log("PlayerController: Mobile jump triggered!");
        mobileJumpRequested = true;
    }

    // Public method to check if mobile controls should be active
    public bool ShouldShowMobileControls()
    {
        return Application.isMobilePlatform || 
               Application.platform == RuntimePlatform.Android || 
               Application.platform == RuntimePlatform.IPhonePlayer;
    }
   

}
