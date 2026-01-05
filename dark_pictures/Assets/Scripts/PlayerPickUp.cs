using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using TMPro;

public class PlayerPickUp : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    float playerPickUpDistance = 3f;
    [SerializeField] Transform playerCameraTrans;
    [SerializeField] LayerMask layerMask;

    [Header("Interaction UI")]
    [SerializeField] GameObject interactionUI;
    [SerializeField] TextMeshProUGUI interactionText;
    [SerializeField] AudioClip equipClip;
    private AudioSource audioSource;

    List<string> inventoryTags = new List<string>()
    {
        "Key",
        "Battery",
    };
    void Start()
    {
        audioSource = transform.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInteraction();
    }

    void HandleInteraction()
    {
        // Debug: Draw the ray in the Scene view to verify distance and direction
        Debug.DrawRay(playerCameraTrans.position, playerCameraTrans.forward * playerPickUpDistance, Color.red);

        // Perform raycast every frame to detect objects
        bool isLookingAtUseableObject = Physics.Raycast(playerCameraTrans.position, playerCameraTrans.forward, out RaycastHit hit, playerPickUpDistance, layerMask);
        
        if (isLookingAtUseableObject)
        {
            var obj = hit.transform.gameObject;
            
            // Check for specific controllers first (in case they are also tagged "Door")
            SingleDoorController singleDoor = obj.GetComponentInParent<SingleDoorController>();
            DoubleDoorController doubleDoor = obj.GetComponentInParent<DoubleDoorController>();

            // 1. Check for SingleDoorController
            if (singleDoor != null)
            {
                if (singleDoor.isKeypadControlled)
                {
                    ShowPrompt("Locked (Use Keypad)");
                }
                else if (singleDoor.requiresKey)
                {
                    ShowPrompt("Locked (Requires Key)");
                }
                else
                {
                    ShowPrompt("Press [E] to Open");
                }
            }
            // 2. Check for DoubleDoorController
            else if (doubleDoor != null)
            {
                if (doubleDoor.isLocked)
                {
                    ShowPrompt("Locked");
                }
                else
                {
                    ShowPrompt("Press [E] to Open");
                }
            }
            // 3. Check for generic Door (Tag: Door)
            else if (obj.CompareTag("Door"))
            {
                ShowPrompt("Press [E] to Open");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    var doorScript = obj.GetComponent<Door>();
                    if (doorScript != null) doorScript.ToggleDoor();
                }
            }
            // 4. Check for Inventory Items
            else if (inventoryTags.Any(i => obj.tag == $"{i}Parent" || obj.tag == i))
            {
                // Clean up tag name for display (e.g. "KeyParent" -> "Key")
                string itemName = obj.tag.Replace("Parent", "");
                ShowPrompt($"Press [E] to Pick Up {itemName}");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    inventory.AddObject(obj.transform);
                    audioSource.PlayOneShot(equipClip);
                    HidePrompt(); // Hide immediately as object will be disabled/moved
                }
            }
            // 5. Generic Interactable (Optional: Add more checks here later)
            else
            {
                HidePrompt();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    void ShowPrompt(string message)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            if (interactionText != null)
                interactionText.text = message;
        }
    }

    void HidePrompt()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
}
