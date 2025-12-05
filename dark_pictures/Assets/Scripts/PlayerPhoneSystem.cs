using UnityEngine;
using TMPro;

public class PlayerPhoneSystem : MonoBehaviour
{
	[Header("References")]
	public Transform phoneModel;
	public TextMeshProUGUI objectiveText;  // Sleep hier je ObjectiveText (in GameplayContent) in

	[Header("Screen Groups")]
	public GameObject introContentGroup;   // Sleep 'IntroContent' hierin
	public GameObject gameplayContentGroup;// Sleep 'GameplayContent' hierin

	[Header("Settings")]
	public bool startHidden = false; // LAAT DIT UIT STAAN!
	public float animationSpeed = 5f;

	[Header("Positions")]
	public Vector3 activePosition;
	public Vector3 hiddenPosition;

	// Public status voor het Intro Script
	public bool isPhoneUp = false;
	private bool canUsePhone = false;

	void Start()
	{
		// Zet intro aan, game uit
		if (introContentGroup != null) introContentGroup.SetActive(true);
		if (gameplayContentGroup != null) gameplayContentGroup.SetActive(false);

		// Pak automatisch de positie die jij in de Scene hebt ingesteld
		if (phoneModel != null)
		{
			activePosition = phoneModel.localPosition;
		}

		// Maar begin omlaag, want we wachten op TAB
		isPhoneUp = false;
		if (phoneModel != null) phoneModel.localPosition = hiddenPosition;
	}

	void Update()
	{
		// Alleen als we volledige controle hebben (na de intro) werkt TAB
		if (canUsePhone)
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				isPhoneUp = !isPhoneUp;
			}
		}

		HandleMovement();
	}

	// Hiermee dwingt de intro de telefoon omhoog
	public void SetPhoneVisuals(bool up)
	{
		isPhoneUp = up;
	}

	public void UnlockPhone()
	{
		canUsePhone = true;

		// Wissel content: Intro weg, Objectives aan
		if (introContentGroup != null) introContentGroup.SetActive(false);
		if (gameplayContentGroup != null) gameplayContentGroup.SetActive(true);

		if (phoneModel != null) phoneModel.gameObject.SetActive(true);

		// Update tekst direct
		if (TutorialNarrativeManager.Instance != null && objectiveText != null)
		{
			objectiveText.text = TutorialNarrativeManager.Instance.GetCurrentObjective();
		}

		isPhoneUp = false;
	}

	void HandleMovement()
	{
		if (phoneModel == null) return;
		Vector3 targetPos = isPhoneUp ? activePosition : hiddenPosition;
		phoneModel.localPosition = Vector3.Lerp(phoneModel.localPosition, targetPos, Time.deltaTime * animationSpeed);
	}
}