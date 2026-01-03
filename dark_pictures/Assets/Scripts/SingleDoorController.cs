using UnityEngine;
using System.Linq;

public class SingleDoorController : MonoBehaviour
{
	[Header("Door Settings")]
	public Transform door;
	public MeshCollider doorCollider;
	public float openAngle = 90f;
	public float rotationSpeed = 90f; // Degrees per second
	public float interactionDistance = 3f;

	[Header("Key Settings")]
	public bool requiresKey = false; 
	public string keyTagName = "Key"; 

    [Header("Access Settings")]
    public bool isKeypadControlled = false;

    [Header("Audio")]
    public AudioClip lockedSound;
    public float lockedVolume = 1f;
    private AudioSource audioSource;

	private Transform player;
	private Inventory playerInventory;
	private bool isOpen = false;

	void Start()
	{
		// 1. Find the player
		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

		if (playerObj != null)
		{
			player = playerObj.transform;
			
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

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Make it 3D sound
	}

	void Update()
	{
		// Check for input and distance
		if (!isKeypadControlled && Input.GetKeyDown(KeyCode.E) && IsPlayerInRange())
		{
			TryInteractWithDoor();
		}

		RotateDoor();
	}

    // Called by PlayerPickUp
    public void Interact()
    {
        if (!isKeypadControlled)
        {
            TryInteractWithDoor();
        }
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
		// check the inventory.
		if (playerInventory != null)
		{
			if (playerInventory.Contains(keyTagName))
			{
				Debug.Log($"Key '{keyTagName}' found! Opening door.");
				
				// 1. Remove the key from inventory
				playerInventory.UseItem(keyTagName);
				
				// 2. Unlock the door permanently so we don't need a second key if we close it
				requiresKey = false; 
				
				ToggleDoor();
			}
			else
			{
				Debug.Log($"Door is locked. You need a '{keyTagName}'.");
                
                // Play locked sound
                if (lockedSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(lockedSound, lockedVolume);
                }
                
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

	public void ToggleDoor()
	{
		isOpen = !isOpen;
		UpdateCollider();
	}

    public void SetDoorState(bool open)
    {
        isOpen = open;
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
		// Use RotateTowards to actually reach the target angle
		door.localRotation = Quaternion.RotateTowards(door.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
	}

	void UpdateCollider()
	{
        if (doorCollider != null)
		    doorCollider.isTrigger = isOpen;
	}

	void PlayLockedSound()
	{
		if (audioSource != null && lockedSound != null)
		{
			audioSource.PlayOneShot(lockedSound, lockedVolume);
		}
	}
}