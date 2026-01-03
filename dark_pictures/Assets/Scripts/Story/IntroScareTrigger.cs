using UnityEngine;
using System.Collections;

public class IntroScareTrigger : MonoBehaviour
{
	[Header("References")]
	public PlayerController playerScript;
	public Camera playerCamera;
	public GameObject flashPromptUI;
	public GameObject tutorialEntity;
    public MainDoorController mainDoor; // Reference to the main door

	[Header("Target")]
	public Transform scareTarget; // tutorialEntity

	[Header("Settings")]
	public float turnSpeed = 5f;

	private bool hasTriggered = false;

    void Start()
    {
        if (mainDoor == null)
        {
            mainDoor = FindObjectOfType<MainDoorController>();
        }
    }

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered) return;

		if (other.CompareTag("Player"))
		{
			hasTriggered = true;
			Debug.Log("JUMPSCARE START!");

            // Lock the main door when the scare starts
            if (mainDoor != null)
            {
                mainDoor.SetLocked(true);
            }
            else
            {
                Debug.LogWarning("IntroScareTrigger: MainDoorController reference is missing!");
            }

			// 1. Speler input uit
			if (playerScript != null) playerScript.enabled = false;

			// 2. Activeer monster
			if (tutorialEntity != null) tutorialEntity.SetActive(true);

			// 3. Toon tekst
			if (flashPromptUI != null) flashPromptUI.SetActive(true);

            // Update phone objective
            if (TutorialNarrativeManager.Instance != null)
            {
                TutorialNarrativeManager.Instance.AdvanceStory(7);
            }

			// 4. Forceer de draai
			if (playerScript != null && scareTarget != null)
			{
				StartCoroutine(SmoothLookAt());
			}
		}
	}
    // ... existing SmoothLookAt code ...
    IEnumerator SmoothLookAt()
	{
		yield return null; // Wacht 1 frame

		// 1. Bereken de richting naar het monster
		Vector3 directionToTarget = scareTarget.position - playerCamera.transform.position;

		Vector3 flatDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);
		Quaternion targetBodyRotation = Quaternion.LookRotation(flatDirection);

		Quaternion targetCameraRotation = Quaternion.LookRotation(directionToTarget);

		float duration = 0.5f;
		float elapsed = 0f;

		// Startposities opslaan
		Quaternion startBodyRot = playerScript.transform.rotation;
		Quaternion startCamRot = playerCamera.transform.rotation;

		while (elapsed < duration)
		{
			float t = elapsed / duration * turnSpeed;

			// Draai het lichaam (Links/Rechts)
			playerScript.transform.rotation = Quaternion.Slerp(startBodyRot, targetBodyRotation, t);
			playerCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCameraRotation, t);

			elapsed += Time.deltaTime;
			yield return null;
		}

		playerScript.transform.rotation = targetBodyRotation;
		playerCamera.transform.LookAt(scareTarget);

		if (playerScript != null)
		{

		}
	}
}