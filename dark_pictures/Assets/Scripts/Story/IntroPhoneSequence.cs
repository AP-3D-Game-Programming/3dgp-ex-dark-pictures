using UnityEngine;
using TMPro;
using System.Collections;

public class IntroPhoneSequence : MonoBehaviour
{
	[Header("UI References")]
	public GameObject pressTabUI; //in canvas
	public TextMeshProUGUI screenText;

	[Header("Player References")]
	public PlayerPhoneSystem phoneSystemScript;
	public PlayerController playerMovementScript;
	public Transform playerCamera;

	[Header("Flashlight Sequence Settings")]
	public CameraFlash flashlight;

	[Header("Sequence Settings")]
	public float lookDownAngle = 30f;
	public float textSpeed = 1.5f;
	public float readTime = 4.0f;

	[Header("Audio (Optioneel)")]
	public AudioSource phoneAudioSource;
	public AudioClip textSound;
	public AudioClip objectiveSound;

	private bool waitingForInput = true;
	private bool sequenceStarted = false;

	void Start()
	{
		// 1. Speler bevriezen
		if (playerMovementScript != null) playerMovementScript.enabled = false;
		
		if (flashlight != null) flashlight.enabled = false;

		// 2. Camera naar beneden
		if (playerCamera != null) playerCamera.localRotation = Quaternion.Euler(lookDownAngle, 0, 0);

		// 3. Telefoon scherm tekst leegmaken
		if (screenText != null) screenText.text = "";

		// 4. Zorg dat de "Press TAB" UI zichtbaar is
		if (pressTabUI != null) pressTabUI.SetActive(true);
	}

	void Update()
	{
		// Speler moet tab drukken om te starten
		if (waitingForInput)
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				StartTheShow();
			}
		}
	}

	void StartTheShow()
	{
		waitingForInput = false;
		sequenceStarted = true;

		// 1. Verberg de "Press Tab" UI
		if (pressTabUI != null) pressTabUI.SetActive(false);

		// 2. Dwing de telefoon omhoog (visueel)
		if (phoneSystemScript != null) phoneSystemScript.SetPhoneVisuals(true);

		// 3. Start de chat sequence
		StartCoroutine(PlayPhoneSequence());
	}

	IEnumerator PlayPhoneSequence()
	{
		yield return new WaitForSeconds(1f);

		AddText("User88: FAKE! 😂", false);
		yield return new WaitForSeconds(textSpeed);

		AddText("GX_Hunter: Photoshop skills 2/10", false);
		yield return new WaitForSeconds(textSpeed);

		AddText("Mom: Come home, please.", false);
		yield return new WaitForSeconds(textSpeed);

		AddText("> SYSTEM: BATTERY 15%", false);

		yield return new WaitForSeconds(2f);

		AddText("----------------", false);
		AddText("<color=yellow>NEW OBJECTIVE UPDATED</color>", true);
		yield return new WaitForSeconds(0.5f);

		AddText("- Enter the Facility", false);
		AddText("- Find Evidence (Photos: 0/10)", false);

		yield return new WaitForSeconds(readTime);

		PutPhoneAway();
		if (flashlight != null) flashlight.enabled = true;

	}

	void AddText(string txt, bool isObjective)
	{
		if (screenText != null) screenText.text += txt + "\n";

		if (phoneAudioSource != null)
		{
			if (isObjective && objectiveSound != null) phoneAudioSource.PlayOneShot(objectiveSound);
			else if (textSound != null) phoneAudioSource.PlayOneShot(textSound);
		}
	}

	void PutPhoneAway()
	{
		// 1. Speler mag weer bewegen
		if (playerMovementScript != null) playerMovementScript.enabled = true;

		// 2. Tutorial tekst start
		if (TutorialNarrativeManager.Instance != null) TutorialNarrativeManager.Instance.AdvanceStory(0);

		// 3. Telefoon systeem neemt het over (en unlockt)
		if (phoneSystemScript != null) phoneSystemScript.UnlockPhone();

		// 4. Dit script is klaar
		this.enabled = false;
	}

	void LateUpdate()
	{
		// Camera geforceerd houden zolang de sequence bezig is
		if (sequenceStarted && playerCamera != null)
		{
			playerCamera.localRotation = Quaternion.Euler(lookDownAngle, 0, 0);
		}
	}
}