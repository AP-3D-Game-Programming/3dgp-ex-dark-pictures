using UnityEngine;
using TMPro;
using System.Collections;

public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string itemName = "Key";
    [SerializeField] private string itemType = "key";
    [SerializeField] private float pickupRange = 3f;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI pickupText;
    [SerializeField] private float textDisplayTime = 2f;
    
    private Transform player;
    private bool isPlayerNearby = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerNearby = distance <= pickupRange;
        }

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        
        if (inventory != null)
        {
            inventory.AddItem(itemType);
            Debug.Log(itemName + " picked up!");
            
            // Show UI text and destroy afterwards
            if (pickupText != null)
            {
                StartCoroutine(ShowPickupMessageAndDestroy());
            }
            else
            {
                // If no UI text assigned, just destroy immediately
                Destroy(gameObject);
            }
        }
    }

    IEnumerator ShowPickupMessageAndDestroy()
    {
        pickupText.text = itemName + " Picked Up!";
        pickupText.gameObject.SetActive(true);
        yield return new WaitForSeconds(textDisplayTime);
        pickupText.gameObject.SetActive(false);
        
        // Now destroy the key object
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
