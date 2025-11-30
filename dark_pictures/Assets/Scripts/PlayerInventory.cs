using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // List to store all items player has picked up
    private List<string> items = new List<string>();
    
    // Add an item to inventory
    public void AddItem(string itemType)
    {
        if (!items.Contains(itemType))
        {
            items.Add(itemType);
            Debug.Log("Added to inventory: " + itemType);
        }
        else
        {
            Debug.Log("Already have: " + itemType);
        }
    }
    
    // Check if player has a specific item
    public bool HasItem(string itemType)
    {
        return items.Contains(itemType);
    }
    
    // Optional: See what's in inventory (for debugging)
    public void ShowInventory()
    {
        Debug.Log("Inventory contains " + items.Count + " items:");
        foreach (string item in items)
        {
            Debug.Log("- " + item);
        }
    }
}
