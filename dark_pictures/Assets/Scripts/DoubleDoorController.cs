// 12/12/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class MainDoorController : MonoBehaviour
{
    public Transform leftDoor; // Reference to the left door (Cube_10)
    public Transform rightDoor; // Reference to the right door (Cube_14)
    public MeshCollider leftDoorCollider; // Collider for the left door
    public MeshCollider rightDoorCollider; // Collider for the right door
    public float openAngle = 90f; // The angle to rotate the doors to open them
    public float rotationSpeed = 2f; // Speed of rotation
    private bool isOpen = false; // Tracks if the doors are open or closed

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen; // Toggle the door state
            UpdateColliders(); // Update the colliders based on the door state
        }

        RotateDoors();
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
