using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Status")]
    public bool playerCanMove = true; 

    [Header("Setup")]
    public Camera playerCamera;
    public GameManager gameManager;
    public Transform beanModel;

    [Header("Settings")]
    public float mouseSensitivity = 15f;
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f;

    [Header("Model Adjustment")]
    public float modelYOffset = 1.0f; 

    private float xRotation = 0f;
    private CharacterController controller;
    private float currentSpeed;
    private Vector3 velocity;
    private float defaultHeight;
    private bool isCrouching = false;

	[Header("Stamina")]
	public float maxStamina = 100f;
	public float staminaDrainRate = 20f; // Hoeveel stamina word gebruikt per seconden
	public float staminaRegenRate = 15f; // Hoeveel stamina genereert per seconden
	public float staminaRegenDelay = 1f; // De tijd tot stamina terug begint te regenereren na sprinten
	public float minStaminaForSprint = 30f; // Minimale stamina om te kunnen sprinten
	public float minStaminaForBreathing = 30f; // Wanneer de audio voor zwaar ademen begint
	private float currentStamina;
	private float timeSinceLastSprint = 0f;
	private bool canSprint = true;

	[Header("Audio")]
	public AudioClip outOfBreathSound; // Vak voor geluidsbestand
	private AudioSource audioSource;

    [Header("Heartbeat")]
    public AudioClip heartbeatSound; // Vak voor hartslaggeluid
    public Transform entity; // Vak voor entity
    public float maxHeartbeatDistance = 20f; // Maximale afstand waar hartslag start
    public float minHeartbeatVolume = 0.1f; // Minimum volume op max afstand
    public float maxHeartbeatVolume = 1.0f; // Maximum volume op dichtste afstand
    private AudioSource heartbeatAudioSource;

	void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

		currentStamina = maxStamina;

		// Setup AudioSource voor ademhalingsgeluid
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
		audioSource.playOnAwake = false;
		audioSource.spatialBlend = 0f;

        // Setup AudioSource voor hartslag
        heartbeatAudioSource = gameObject.AddComponent<AudioSource>();
        heartbeatAudioSource.playOnAwake = false;
        heartbeatAudioSource.spatialBlend = 0f; // 2D geluid
        heartbeatAudioSource.loop = false;

		defaultHeight = controller.height;

        if(playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (gameManager != null && gameManager.isGameOver) return;
        if (!playerCanMove) return;

        HandleLook();
        HandleMovement();
        HandleBreathingSound();
        HandleHeartbeat();
    }

    public void ForceCameraPitch(float angle)
    {
        xRotation = angle;
        if(playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    void HandleLook()
    {
        if (Mouse.current == null) return;
        
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Keyboard.current == null) return;

        float x = 0f;
        float y = 0f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;

        Vector2 input = new Vector2(x, y).normalized;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        bool wantsSprint = Keyboard.current.leftShiftKey.isPressed && input.magnitude > 0;
        bool wantsCrouch = Keyboard.current.cKey.isPressed;
        
        isCrouching = wantsCrouch;

        // Handle stamina and sprinting
        bool isSprinting = false;

        if (wantsSprint && currentStamina > 0 && canSprint && !isCrouching)
        {
            // Draining stamina while sprinting
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            timeSinceLastSprint = 0f;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false; // Prevent sprinting until stamina fully recharges
            }
        }
        else
        {
            // Regenerate stamina after delay
            timeSinceLastSprint += Time.deltaTime;

            if (timeSinceLastSprint >= staminaRegenDelay && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;

                // Sprinten toegestaan vanaf minStaminaForSprint
                if (currentStamina >= minStaminaForSprint)
                {
                    canSprint = true;
                }

                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
        }

        currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : moveSpeed);
        
        float targetControllerHeight = isCrouching ? defaultHeight / 2 : defaultHeight;
        controller.height = Mathf.Lerp(controller.height, targetControllerHeight, Time.deltaTime * 10f);

        if(beanModel != null)
        {
            float crouchOffset = isCrouching ? -0.5f : 0f;
            float targetY = crouchOffset + modelYOffset;
            
            beanModel.localPosition = Vector3.Lerp(beanModel.localPosition, new Vector3(0, targetY, 0), Time.deltaTime * 10f);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handles the out of breath sound effect based on current stamina levels.
    /// </summary>
    /// <remarks>
    /// The breathing sound loops continuously when stamina drops below the minimum threshold
    /// and stops automatically once stamina regenerates above that threshold.
    /// </remarks>
    void HandleBreathingSound()
    {
        if (outOfBreathSound == null || audioSource == null) return;

        // Start looping breathing sound als stamina onder drempel komt
        if (currentStamina < minStaminaForBreathing && !audioSource.isPlaying)
        {
            audioSource.clip = outOfBreathSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        // Stop breathing sound als stamina boven drempel komt
        else if (currentStamina >= minStaminaForBreathing && audioSource.isPlaying && audioSource.clip == outOfBreathSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    /// <summary>
    /// Handles the heartbeat sound effect based on proximity to entity.
    /// </summary>
    /// <remarks>
    /// The heartbeat loops continuously when the player is within range of the entity.
    /// Volume is adjusted based on distance - louder when closer, quieter when further away.
    /// </remarks>
    void HandleHeartbeat()
    {
        if (heartbeatSound == null || heartbeatAudioSource == null || entity == null) return;

        // Bereken afstand tot entity
        float distance = Vector3.Distance(transform.position, entity.position);
        bool isInRange = distance <= maxHeartbeatDistance;

        // Als we binnen max afstand zijn - start/update loop
        if (isInRange)
        {
            // Bereken volume based op afstand (dichter = luider)
            // Inverse lerp: 0 bij maxDistance, 1 bij distance 0
            float normalizedDistance = Mathf.Clamp01(distance / maxHeartbeatDistance);
            float targetVolume = Mathf.Lerp(maxHeartbeatVolume, minHeartbeatVolume, normalizedDistance);
            heartbeatAudioSource.volume = targetVolume;

            if (!heartbeatAudioSource.isPlaying)
            {
                heartbeatAudioSource.clip = heartbeatSound;
                heartbeatAudioSource.loop = true;
                heartbeatAudioSource.Play();
            }
        }
        // Buiten bereik - stop audio
        else
        {
            if (heartbeatAudioSource.isPlaying)
            {
                heartbeatAudioSource.Stop();
                heartbeatAudioSource.loop = false;
            }
        }
    }

    /// <summary>
    /// Draws gizmos in the Scene view to visualize the heartbeat detection range.
    /// </summary>
    void OnDrawGizmos()
    {
        // Check of entity is toegewezen (werkt ook in edit mode)
        if (entity == null) return;

        float distance = Vector3.Distance(transform.position, entity.position);
        bool isInRange = distance <= maxHeartbeatDistance;

        // Teken lijn tussen player en entity
        Gizmos.color = isInRange ? Color.red : Color.gray;
        Gizmos.DrawLine(transform.position, entity.position);

        // Teken sphere rond entity om max afstand te laten zien
        Gizmos.color = isInRange ? new Color(1f, 0f, 0f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
        Gizmos.DrawWireSphere(entity.position, maxHeartbeatDistance);

        // Teken kleine sphere op entity positie
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(entity.position, 0.5f);

        // Teken kleine sphere op player positie
        Gizmos.color = isInRange ? Color.red : Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}