using NUnit.Framework;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameObject key;
    [SerializeField] Inventory inventory;
    // --- Variables you can set in the Inspector ---
    public float openAngle = -90f; // How much the door will rotate (negative for opposite direction)
    public float openSpeed = 2f;   // How fast the door opens/closes
    public bool isOpen = false;    // Tracks whether the door is open or closed

    // --- Internal variables ---
    private Quaternion closedRotation; // The door's rotation when closed
    private Quaternion openRotation;   // The door's rotation when open

    private Transform parentTransform;

    void Start()
    {
        parentTransform = transform.parent;
        // Store the door's initial rotation as closed rotation
        closedRotation = parentTransform.rotation;

        // Calculate open rotation by adding openAngle to the current rotation
        // Only rotates around Y-axis (up/down)
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        // Smoothly rotate the door towards the target rotation every frame
        if (isOpen)
        {
            parentTransform.rotation = Quaternion.Lerp(parentTransform.rotation, openRotation, Time.deltaTime * openSpeed);
        }
        else
        {
            parentTransform.rotation = Quaternion.Lerp(parentTransform.rotation, closedRotation, Time.deltaTime * openSpeed);
        }
    }

    // Call this function to toggle door open/close
    public void ToggleDoor()
    {
       if (inventory.items.Find(item => item == key) == null)
            return; 
        isOpen = !isOpen; // Switch between true/false
    }
}