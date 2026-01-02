using UnityEngine;
using System.Linq;

public class SingleDoorController : MonoBehaviour
{
	[Header("Door Settings")]
	public Transform door;
	public MeshCollider doorCollider;
	public float openAngle = 90f;
	public float rotationSpeed = 2f;
	public float interactionDistance = 3f;

	[Header("Key Settings")]
	public bool requiresKey = false; // Check this box in Inspector if door is locked
	public string keyTagName = "Key"; // The tag name of the key required (match this with your Inventory tags)

	private Transform player;
	private Inventory playerInventory; // Reference to the inventory script
	private bool isOpen = false;

	void Start()
	{
		// 1. Find the player
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

		if (playerObj != null)
		{
			player = playerObj.transform;
			
            // FIX: Use GetComponentInChildren because the Inventory script is on a child GameObject, not the root Player
			playerInventory = playerObj.GetComponentInChildren<Inventory>();

            if (playerInventory == null)
            {
                Debug.LogError("Inventory script not found on Player object or its children! Door keys will not work.");
            }
		}
		else
		{
			Debug.LogError("Player not found! Make sure your Player object has the tag 'Player'.");
		}
	}

	void Update()
	{
		// Check for input and distance
		if (Input.GetKeyDown(KeyCode.E) && IsPlayerInRange())
		{
			TryInteractWithDoor();
		}

		RotateDoor();
	}

	void TryInteractWithDoor()
	{
		// If the door is already open, we can close it without a key
		// OR if the door doesn't require a key, we just toggle it.
		if (isOpen || !requiresKey)
		{
			ToggleDoor();
			return;
		}

		// If we represent here, the door is Closed AND Requires a Key.
		// We need to check the inventory.
		if (playerInventory != null)
		{
			// Use the .Contains method you already wrote in your Inventory script
			if (playerInventory.Contains(keyTagName))
			{
				Debug.Log($"Key '{keyTagName}' found! Opening door.");
				ToggleDoor();
			}
			else
			{
				Debug.Log($"Door is locked. You need a '{keyTagName}'.");
                
                // DEBUGGING: Print what keys we actually have to the console
                if (playerInventory.items.Count > 0)
                {
                    string keys = string.Join(", ", playerInventory.items.Keys);
                    Debug.Log($"Current Inventory Tags: {keys}");
                }
                else
                {
                    Debug.Log("Inventory is empty.");
                }
			}
		}
        else
        {
            Debug.LogError("Cannot check for key: Inventory component is missing from Player.");
        }
	}

	void ToggleDoor()
	{
		isOpen = !isOpen;
		UpdateCollider();
	}

	bool IsPlayerInRange()
	{
		if (player == null) return false;
		float distance = Vector3.Distance(player.position, transform.position);
		return distance <= interactionDistance;
	}

	void RotateDoor()
	{
		float targetAngle = isOpen ? openAngle : 0f;
		Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
		door.localRotation = Quaternion.Lerp(door.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
	}

	void UpdateCollider()
	{
        if (doorCollider != null)
		    doorCollider.isTrigger = isOpen;
	}
}