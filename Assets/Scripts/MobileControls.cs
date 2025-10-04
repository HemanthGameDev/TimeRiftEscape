using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MobileControls : MonoBehaviour
{
    [Header("Mobile Control Settings")]
    public bool enableMobileControls = true;
    public bool showOnlyOnMobile = false; // Changed to false for editor testing
    
    [Header("Button References")]
    public Button leftButton;
    public Button rightButton;
    public Button jumpButton;
    
    [Header("Button Visual Settings")]
    public float pressedAlpha = 0.6f;
    public float normalAlpha = 0.7f;
    public float buttonScaleEffect = 0.95f;
    public float animationDuration = 0.1f;
    
    [Header("Audio Settings")]
    public AudioClip buttonPressSound;
    public AudioClip buttonReleaseSound;
    [Range(0f, 1f)]
    public float buttonSoundVolume = 0.5f;
    
    // Private variables
    private PlayerController playerController;
    private AudioSource audioSource;
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isJumpPressed = false;
    
    // Visual feedback
    private Image leftButtonImage;
    private Image rightButtonImage;
    private Image jumpButtonImage;
    
    void Start()
    {
        SetupMobileControls();
        SetupAudioSource();
        CheckPlatformVisibility();
    }
    
    private void SetupMobileControls()
    {
        // Find PlayerController in the scene
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("MobileControls: PlayerController not found in scene!");
            return;
        }
        else
        {
            Debug.Log("MobileControls: Successfully found PlayerController!");
        }

        // Auto-assign buttons if they're not assigned
        if (leftButton == null || rightButton == null || jumpButton == null)
        {
            AutoAssignButtons();
        }
        
        // Setup button images for visual feedback
        if (leftButton != null)
        {
            leftButtonImage = leftButton.GetComponent<Image>();
            SetupButtonEvents(leftButton, OnLeftButtonDown, OnLeftButtonUp);
            SetButtonLabel(leftButton, "←");
            Debug.Log("MobileControls: Left button configured");
        }
        else
        {
            Debug.LogWarning("MobileControls: Left button not assigned!");
        }
        
        if (rightButton != null)
        {
            rightButtonImage = rightButton.GetComponent<Image>();
            SetupButtonEvents(rightButton, OnRightButtonDown, OnRightButtonUp);
            SetButtonLabel(rightButton, "→");
            Debug.Log("MobileControls: Right button configured");
        }
        else
        {
            Debug.LogWarning("MobileControls: Right button not assigned!");
        }
        
        if (jumpButton != null)
        {
            jumpButtonImage = jumpButton.GetComponent<Image>();
            SetupButtonEvents(jumpButton, OnJumpButtonDown, OnJumpButtonUp);
            SetButtonLabel(jumpButton, "↑");
            Debug.Log("MobileControls: Jump button configured");
        }
        else
        {
            Debug.LogWarning("MobileControls: Jump button not assigned!");
        }
    }

    private void AutoAssignButtons()
    {
        Debug.Log("MobileControls: Auto-assigning buttons...");
        
        // Find all buttons that are children of this GameObject
        Button[] buttons = GetComponentsInChildren<Button>();
        
        if (buttons.Length >= 3)
        {
            // Assign based on their position or names
            foreach (Button button in buttons)
            {
                string buttonName = button.gameObject.name;
                Vector3 position = button.transform.localPosition;
                
                if (buttonName == "Button" && position.x < -600) // Left button (far left)
                {
                    leftButton = button;
                    Debug.Log($"MobileControls: Auto-assigned LEFT button: {buttonName} at {position}");
                }
                else if (buttonName == "Button (1)" && position.x < 0 && position.x > -600) // Right button (middle-left)
                {
                    rightButton = button;
                    Debug.Log($"MobileControls: Auto-assigned RIGHT button: {buttonName} at {position}");
                }
                else if (buttonName == "Button (2)" && position.x > 600) // Jump button (far right)
                {
                    jumpButton = button;
                    Debug.Log($"MobileControls: Auto-assigned JUMP button: {buttonName} at {position}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"MobileControls: Only found {buttons.Length} buttons, need 3!");
        }
    }
    
    private void SetButtonLabel(Button button, string label)
    {
        if (button == null) return;
        
        // Find the text component in the button
        TMPro.TextMeshProUGUI textComponent = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = label;
            textComponent.fontSize = 48;
            textComponent.fontStyle = TMPro.FontStyles.Bold;
            Debug.Log($"MobileControls: Set button label to '{label}'");
        }
    }

    private void SetupButtonEvents(Button button, System.Action onDown, System.Action onUp)
    {
        // Add EventTrigger component for press and release events
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        // Pointer Down Event
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { onDown?.Invoke(); });
        trigger.triggers.Add(pointerDown);
        
        // Pointer Up Event
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { onUp?.Invoke(); });
        trigger.triggers.Add(pointerUp);
        
        // Pointer Exit Event (in case finger slides off button)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { onUp?.Invoke(); });
        trigger.triggers.Add(pointerExit);
    }
    
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = buttonSoundVolume;
        audioSource.spatialBlend = 0f; // 2D sound
    }
    
    private void CheckPlatformVisibility()
    {
        if (!enableMobileControls)
        {
            gameObject.SetActive(false);
            return;
        }
        
        if (showOnlyOnMobile)
        {
            // Show mobile controls only on mobile platforms
            bool isMobilePlatform = Application.isMobilePlatform || 
                                  Application.platform == RuntimePlatform.Android || 
                                  Application.platform == RuntimePlatform.IPhonePlayer;
            
            gameObject.SetActive(isMobilePlatform);
        }
        else
        {
            // Always show when showOnlyOnMobile is false (good for editor testing)
            gameObject.SetActive(true);
        }
    }
    
    // Left Button Events
    private void OnLeftButtonDown()
    {
        if (!enableMobileControls || playerController == null) return;
        
        Debug.Log("MobileControls: Left button pressed!");
        isLeftPressed = true;
        PlayButtonSound(buttonPressSound);
        StartCoroutine(ButtonPressAnimation(leftButtonImage, leftButton));
        
        // Set continuous left movement
        SetMobileInput(-1f, 0f);
    }
    
    private void OnLeftButtonUp()
    {
        if (!enableMobileControls) return;
        
        Debug.Log("MobileControls: Left button released!");
        isLeftPressed = false;
        PlayButtonSound(buttonReleaseSound);
        StartCoroutine(ButtonReleaseAnimation(leftButtonImage, leftButton));
        
        // Stop movement if no other movement button is pressed
        if (!isRightPressed)
        {
            SetMobileInput(0f, 0f);
        }
    }
    
    // Right Button Events
    private void OnRightButtonDown()
    {
        if (!enableMobileControls || playerController == null) return;
        
        Debug.Log("MobileControls: Right button pressed!");
        isRightPressed = true;
        PlayButtonSound(buttonPressSound);
        StartCoroutine(ButtonPressAnimation(rightButtonImage, rightButton));
        
        // Set continuous right movement
        SetMobileInput(1f, 0f);
    }
    
    private void OnRightButtonUp()
    {
        if (!enableMobileControls) return;
        
        Debug.Log("MobileControls: Right button released!");
        isRightPressed = false;
        PlayButtonSound(buttonReleaseSound);
        StartCoroutine(ButtonReleaseAnimation(rightButtonImage, rightButton));
        
        // Stop movement if no other movement button is pressed
        if (!isLeftPressed)
        {
            SetMobileInput(0f, 0f);
        }
    }
    
    // Jump Button Events
    private void OnJumpButtonDown()
    {
        if (!enableMobileControls || playerController == null) return;
        
        Debug.Log("MobileControls: Jump button pressed!");
        isJumpPressed = true;
        PlayButtonSound(buttonPressSound);
        StartCoroutine(ButtonPressAnimation(jumpButtonImage, jumpButton));
        
        // Trigger jump immediately
        TriggerMobileJump();
    }
    
    private void OnJumpButtonUp()
    {
        if (!enableMobileControls) return;
        
        Debug.Log("MobileControls: Jump button released!");
        isJumpPressed = false;
        PlayButtonSound(buttonReleaseSound);
        StartCoroutine(ButtonReleaseAnimation(jumpButtonImage, jumpButton));
    }
    
    // Public methods to interface with PlayerController
    public void SetMobileInput(float horizontal, float vertical)
    {
        // Send input to PlayerController directly
        if (playerController != null)
        {
            playerController.SetMobileMovementInput(new Vector2(horizontal, vertical));
            Debug.Log($"MobileControls: Set mobile input: {horizontal}, {vertical}");
        }
    }
    
    public void TriggerMobileJump()
    {
        if (playerController != null)
        {
            // Call the PlayerController's TriggerMobileJump method directly
            playerController.TriggerMobileJump();
            Debug.Log("MobileControls: Triggered jump via PlayerController!");
        }
    }
    
    // Visual Feedback Animations
    private IEnumerator ButtonPressAnimation(Image buttonImage, Button button)
    {
        if (buttonImage == null || button == null) yield break;
        
        Vector3 originalScale = button.transform.localScale;
        Color originalColor = buttonImage.color;
        
        // Scale down and fade
        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animationDuration;
            
            button.transform.localScale = Vector3.Lerp(originalScale, originalScale * buttonScaleEffect, progress);
            buttonImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 
                                        Mathf.Lerp(normalAlpha, pressedAlpha, progress));
            yield return null;
        }
    }
    
    private IEnumerator ButtonReleaseAnimation(Image buttonImage, Button button)
    {
        if (buttonImage == null || button == null) yield break;
        
        Vector3 originalScale = button.transform.localScale;
        Color originalColor = buttonImage.color;
        
        // Scale up and restore alpha
        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animationDuration;
            
            button.transform.localScale = Vector3.Lerp(originalScale, Vector3.one, progress);
            buttonImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 
                                        Mathf.Lerp(pressedAlpha, normalAlpha, progress));
            yield return null;
        }
        
        // Ensure we end at original values
        button.transform.localScale = Vector3.one;
        buttonImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, normalAlpha);
    }
    
    // Audio
    private void PlayButtonSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(clip, buttonSoundVolume);
        }
    }
    
    // Public methods for runtime control
    public void EnableMobileControls(bool enable)
    {
        enableMobileControls = enable;
        if (!enable)
        {
            // Stop all current inputs
            isLeftPressed = false;
            isRightPressed = false;
            isJumpPressed = false;
            SetMobileInput(0f, 0f);
        }
    }
    
    public void SetButtonSoundVolume(float volume)
    {
        buttonSoundVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = buttonSoundVolume;
        }
    }
    
    void OnDisable()
    {
        // Clean up when disabled
        isLeftPressed = false;
        isRightPressed = false;
        isJumpPressed = false;
        
        if (playerController != null)
        {
            SetMobileInput(0f, 0f);
        }
    }
}