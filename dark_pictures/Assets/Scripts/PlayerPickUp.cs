using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEditor;
using UnityEngine;

public class PlayerPickUp : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    float playerPickUpDistance = 3f;
    [SerializeField] Transform playerCameraTrans;
    [SerializeField] LayerMask layerMask;
    List<string> inventoryTags = new List<string>()
    {
        "Key",
        "Battery",
    };
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool isLookingAtUseableObject = Physics.Raycast(playerCameraTrans.position, playerCameraTrans.forward, out RaycastHit hit, playerPickUpDistance, layerMask);
            if (isLookingAtUseableObject)
            {
                var obj = hit.transform.gameObject;
                if (obj.CompareTag("Door"))
                {
                    obj.GetComponent<Door>().ToggleDoor();
                }
                else if (inventoryTags.Any(i => obj.tag == $"{i}Parent" || obj.tag == i))
                {
                    inventory.AddObject(obj.transform);
                }
            }
        }
    }
}
