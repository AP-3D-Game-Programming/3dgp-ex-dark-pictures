using UnityEngine;
using TMPro;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [SerializeField] string requiredItemType = "key";
    [SerializeField] Transform doorTransform;
    [SerializeField] Vector3 openOffset = new Vector3(0, 3f, 0);
    [SerializeField] float openSpeed = 2f;

    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] float infoTime = 2f;

    bool isOpen, playerNearby;
    Vector3 closedPos, targetPos;
    Transform player;

    void Start()
    {
        doorTransform ??= transform;
        closedPos = doorTransform.position;
        targetPos = closedPos + openOffset;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!playerNearby || !Input.GetKeyDown(KeyCode.E)) return;

        var inv = player.GetComponent<PlayerInventory>();
        if (inv?.HasItem(requiredItemType) ?? false)
        {
            isOpen = true;
            ShowText("Door unlocked!");
        }
        else ShowText("Door is locked!");
    }

    void LateUpdate()
    {
        if (isOpen) doorTransform.position = Vector3.Lerp(doorTransform.position, targetPos, Time.deltaTime * openSpeed);
    }

    void OnTriggerEnter(Collider col) => playerNearby |= col.CompareTag("Player");
    void OnTriggerExit(Collider col) => playerNearby &= !col.CompareTag("Player");

    void ShowText(string msg)
    {
        if (!infoText) return;
        StopAllCoroutines();
        StartCoroutine(TextRoutine(msg));
    }

    IEnumerator TextRoutine(string msg)
    {
        infoText.text = msg;
        infoText.gameObject.SetActive(true);
        yield return new WaitForSeconds(infoTime);
        infoText.gameObject.SetActive(false);
    }
}
