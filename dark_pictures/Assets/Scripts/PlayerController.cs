using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        normalHeight = controller.height;
        currentHeight = normalHeight;
        targetHeight = normalHeight;
        camDefaultLocalPos = cam.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        HandleCrouch();
        HandleMovement();
    }

    void HandleLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        xRot -= mouseDelta.y;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    void HandleCrouch()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        wantsToCrouch = kb.cKey.isPressed;

        bool canStand = !Physics.Raycast(transform.position + Vector3.up * crouchHeight * 0.5f, Vector3.up, normalHeight - crouchHeight);

        if (!canStand && !forcedCrouch)
        {
            forcedCrouch = true;
        }

        else if (canStand && !wantsToCrouch)
        {
            forcedCrouch = false;
        }

        bool shouldCrouch = wantsToCrouch || forcedCrouch;

        targetHeight = shouldCrouch ? crouchHeight : normalHeight;

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed * 0.5f);
        isCrouching = shouldCrouch;

        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2f, 0);

        Vector3 targetCamPos = camDefaultLocalPos + (shouldCrouch ? new Vector3(0, cameraCrouchOffset, 0) : Vector3.zero);
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, targetCamPos, Time.deltaTime * cameraTransitionSpeed * 0.4f);

        if (beanModel != null)
        {
            Vector3 modelTargetPos = shouldCrouch ? new Vector3(0, beanCrouchOffset, 0) : Vector3.zero;
            beanModel.localPosition = Vector3.Lerp(beanModel.localPosition, modelTargetPos, Time.deltaTime * beanTransitionSpeed * 0.5f);
        }
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -0.5f;

        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.isPressed) input.y += 1;
        if (kb.sKey.isPressed) input.y -= 1;
        if (kb.dKey.isPressed) input.x += 1;
        if (kb.aKey.isPressed) input.x -= 1;

        input = input.normalized;

        Vector3 move = transform.right * input.x + transform.forward * input.y;

        float speed = isCrouching ? crouchSpeed :
                      kb.leftShiftKey.isPressed ? sprintSpeed :
                      moveSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (kb.spaceKey.wasPressedThisFrame && isGrounded && !isCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
