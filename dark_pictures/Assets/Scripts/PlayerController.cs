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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        defaultHeight = controller.height;

        if(playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (gameManager != null && gameManager.isGameOver) return;
        if (!playerCanMove) return;

        HandleLook();
        HandleMovement();
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
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
        }
        Vector2 input = new Vector2(x, y).normalized;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        bool wantsSprint = Keyboard.current.leftShiftKey.isPressed;
        bool wantsCrouch = Keyboard.current.cKey.isPressed;
        
        isCrouching = wantsCrouch;

        currentSpeed = isCrouching ? crouchSpeed : (wantsSprint ? sprintSpeed : moveSpeed);
        
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
        
        // 6. Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}