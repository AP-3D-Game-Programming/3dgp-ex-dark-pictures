using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] GameManager gameManager;
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Camera cam;
    public float mouseSensitivity = 100f;
    public float cameraCrouchOffset = -0.8f;
    public float cameraTransitionSpeed = 8f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float normalHeight = 2f;
    public float crouchTransitionSpeed = 8f;

    [Header("Visual")]
    public Transform beanModel;
    public float beanCrouchOffset = -0.5f;
    public float beanTransitionSpeed = 10f;

    [Header("Collision")]
    public LayerMask obstacleMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRot = 0f;
    private bool isCrouching = false;
    private bool wantsToCrouch = false;
    private bool forcedCrouch = false;
    private Vector3 camDefaultLocalPos;
    private float currentHeight;
    private float targetHeight;

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
    public AudioClip heartbeatSound; // vak voor hartslaggeluid
    public Transform entity; // vak voor entity
    public float maxHeartbeatDistance = 20f; // Maximale afstand waar hartslag start
    private AudioSource heartbeatAudioSource;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina; // Starten met volle stamina

        // Setup AudioSource voor ademhalingsgeluid
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D geluid

        // Setup AudioSource voor hartslag
        heartbeatAudioSource = gameObject.AddComponent<AudioSource>();
        heartbeatAudioSource.playOnAwake = false;
        heartbeatAudioSource.spatialBlend = 0f; // 2D geluid
        heartbeatAudioSource.loop = false;

        normalHeight = controller.height;
        currentHeight = normalHeight;
        targetHeight = normalHeight;
        camDefaultLocalPos = cam.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
		if (gameManager.isGameOver)
			return;
        HandleLook();
        HandleCrouch();
        HandleMovement();
        HandleHeartbeat();
    }

    /// <summary>
    /// Handles the player's camera and character rotation based on mouse input.
    /// </summary>
    /// <remarks>
    /// This method reads the current mouse movement to adjust the camera's vertical rotation  and
    /// the character's horizontal rotation.
    /// </remarks>
    void HandleLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        xRot -= mouseDelta.y;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }


	/// <summary>
	/// Handles the crouching behavior of the player, including transitioning between crouching and standing states.
	/// </summary>
	/// <remarks>
	/// Crouching makes your character shorter and slower.
	/// You can crouch by pressing the 'C' key or the left Control key.
	/// </remarks>
	void HandleCrouch()
	{
		var kb = Keyboard.current;
		if (kb == null) return;

		wantsToCrouch = kb.cKey.isPressed || kb.leftCtrlKey.isPressed;

		bool canStand = !Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, normalHeight - 0.5f);

		if (!canStand && !forcedCrouch) forcedCrouch = true;
		else if (canStand && !wantsToCrouch) forcedCrouch = false;

		bool shouldCrouch = wantsToCrouch || forcedCrouch;
		isCrouching = shouldCrouch;
		targetHeight = shouldCrouch ? crouchHeight : normalHeight;

		currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);

		controller.height = currentHeight;
		controller.center = new Vector3(0, currentHeight * 0.5f, 0);

		float crouchPercent = Mathf.InverseLerp(normalHeight, crouchHeight, currentHeight);

		Vector3 crouchCamPos = camDefaultLocalPos + new Vector3(0, cameraCrouchOffset, 0);
		cam.transform.localPosition = Vector3.Lerp(camDefaultLocalPos, crouchCamPos, crouchPercent);

		if (beanModel != null)
		{
			Vector3 crouchBeanPos = new Vector3(0, beanCrouchOffset, 0);
			beanModel.localPosition = Vector3.Lerp(Vector3.zero, crouchBeanPos, crouchPercent);
		}
	}

    /// <summary>
    /// Handles player movement, including walking, sprinting, crouching, jumping, and stamina management.
    /// </summary>
    /// <remarks>
    /// This method processes keyboard input to determine movement direction and speed, applies
    /// gravity, and updates the player's position accordingly. Movement is constrained by the player's grounded state,
    /// crouching status, and gravity effects.
    /// 
    /// Stamina is consumed while sprinting and automatically regenerates when not sprinting.
    /// When stamina is depleted, the player is forced to walk speed until stamina fully recharges.
    /// A short delay occurs before stamina regeneration begins after stopping a sprint.
    /// </remarks>
    void HandleMovement()
	{
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;

        input = input.normalized;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // Handle stamina and sprinting
        // Stamina drains while sprinting and regenerates automatically after a delay
        bool wantsToSprint = kb.leftShiftKey.isPressed && input.magnitude > 0;
        bool isSprinting = false;

        if (wantsToSprint && currentStamina > 0 && canSprint && !isCrouching)
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

        // If crouching -> Crouch Speed
        // Else if Shift held AND stamina available -> Sprint Speed
        // Else -> Walk Speed
        float speed = isCrouching ? crouchSpeed :
                      isSprinting ? sprintSpeed :
                      moveSpeed;

        controller.Move(move * speed * Time.deltaTime);

        // JUMP (Only if grounded)
        if (kb.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        // Roept de breathing sound handler aan
        HandleBreathingSound();
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
    /// The heartbeat loops continuously when the player is within range of the entity
    /// and stops automatically when the player moves out of range.
    /// </remarks>
    void HandleHeartbeat()
    {
        if (heartbeatSound == null || heartbeatAudioSource == null || entity == null) return;

        // Bereken afstand tot entity
        float distance = Vector3.Distance(transform.position, entity.position);

        // Als we binnen max afstand zijn - start loop
        if (distance <= maxHeartbeatDistance)
        {
            if (!heartbeatAudioSource.isPlaying)
            {
                heartbeatAudioSource.clip = heartbeatSound;
                heartbeatAudioSource.loop = true;
                heartbeatAudioSource.Play();
            }
        }
        // Buiten bereik - stop loop
        else
        {
            if (heartbeatAudioSource.isPlaying)
            {
                heartbeatAudioSource.Stop();
                heartbeatAudioSource.loop = false;
            }
        }
    }

}