using System;
using UnityEditor;
using UnityEngine;

public class DoubleDoorController : MonoBehaviour
{
    public Transform leftDoor; // Reference to the left door (Cube_10)
    public Transform rightDoor; // Reference to the right door (Cube_14)
    public MeshCollider leftDoorCollider; // Collider for the left door
    public MeshCollider rightDoorCollider; // Collider for the right door
    public float openAngle = 90f; // The angle to rotate the doors to open them
    public float rotationSpeed = 2f; // Speed of rotation
    public float interactionDistance = 3f; // Maximum distance to interact with the door
    
    [Header("Main Door Settings")]
    public bool isMainDoor = false; // Toggle if this is the main entrance
    public bool isLocked = false;   // If true, door cannot be opened by player

    private Transform player; // Reference to the player
    private bool isOpen = false; // Tracks if the doors are open or closed

    void Start()
    {
        // Find the player by tag - make sure your player has the "Player" tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // If locked, do not allow interaction
        if (!isLocked && Input.GetKeyDown(KeyCode.E) && IsPlayerInRange())
        {
            isOpen = !isOpen; // Toggle the door state
            UpdateColliders(); // Update the colliders based on the door state
        }

        RotateDoors();
    }

    // Called by PlayerPickUp
    public void Interact()
    {
        // If locked, do not allow interaction
        if (!isLocked)
        {
            isOpen = !isOpen; // Toggle the door state
            UpdateColliders(); // Update the colliders based on the door state
        }
    }

    // Helper to lock/unlock the door from other scripts
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        // If we lock the door, force it closed immediately
        if (isLocked)
        {
            if (isOpen)
            {
                isOpen = false;
                UpdateColliders();
            }
        }
    }

    // Helper to force open the door (e.g. after upload)
    public void ForceOpen()
    {
        isLocked = false; // Unlock it
        isOpen = true;    // Open it
        UpdateColliders();
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;
        // Check distance from player to this door's position
        float distance = Vector3.Distance(player.position, transform.position);
        return distance <= interactionDistance;
    }

    void RotateDoors()
    {
        float leftTargetAngle = isOpen ? -openAngle : 0f; // Left door rotates to the left
        float rightTargetAngle = isOpen ? openAngle : 0f; // Right door rotates to the right

        Quaternion leftTargetRotation = Quaternion.Euler(0, leftTargetAngle, 0);
        Quaternion rightTargetRotation = Quaternion.Euler(0, rightTargetAngle, 0);

        leftDoor.localRotation = Quaternion.Lerp(leftDoor.localRotation, leftTargetRotation, Time.deltaTime * rotationSpeed);
        rightDoor.localRotation = Quaternion.Lerp(rightDoor.localRotation, rightTargetRotation, Time.deltaTime * rotationSpeed);
    }

    void UpdateColliders()
    {
        // Enable or disable colliders based on door state
        leftDoorCollider.isTrigger = isOpen;
        rightDoorCollider.isTrigger = isOpen;
    }
}
