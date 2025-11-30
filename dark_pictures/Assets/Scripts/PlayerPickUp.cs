using UnityEngine;

public class PlayerPickUp : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    float playerPickUpDistance = 3f;
    [SerializeField] Transform playerCameraTrans;
    [SerializeField] LayerMask layerMask;
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
                else if (obj.CompareTag("Key"))  
                {
                    inventory.AddObject(obj.transform);
                }
            } 
        }
    }
}
