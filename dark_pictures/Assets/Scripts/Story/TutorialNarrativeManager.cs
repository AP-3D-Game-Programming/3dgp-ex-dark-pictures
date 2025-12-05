using UnityEngine;
using TMPro;

public class TutorialNarrativeManager : MonoBehaviour
{
	public static TutorialNarrativeManager Instance;

	[Header("UI References")]
	public TextMeshProUGUI instructionText;

	[Header("Audio")]
	public AudioSource notificationSound;
	public AudioClip pingSound;

	[Header("Door")]
	public Door doorObject;

	private void Awake()
	{
		if (Instance == null) Instance = this;
	}

	private void Start()
	{
		// Begin leeg, de IntroPhoneSequence roept straks AdvanceStory(0) aan
		instructionText.text = "";
	}

	public void AdvanceStory(int triggerID)
	{
		// Speel geluidje als er nieuwe tekst komt
		if (notificationSound != null && pingSound != null)
			notificationSound.PlayOneShot(pingSound);

		switch (triggerID)
		{
			case 0: // Wordt aangeroepen door IntroPhoneSequence
				instructionText.text = "Walk to the abandoned building";
				break;

			case 1: // Trigger bij het hek
				instructionText.text = "Press [C] to Crouch under the fence";
				break;

			case 2: // Bij het obstakel
				instructionText.text = "Press [Space] to Jump over debris";
				break;

			case 3: // Bij de deur
				instructionText.text = "It's locked. Look for a key nearby.";
				break;

			case 4: // Binnen			niet gebruikt
				instructionText.text = "It's dark... Use [RMB] to Flash.";
				break;
			case 5: //sluit de deur achter speler
				doorObject.isOpen = false; 
				instructionText.text = "Find another way out.";
				break;
			case 99: // Gebruik dit om tekst weg te halen
				instructionText.text = "";
				break;
		}
	}
	public string GetCurrentObjective()
	{
		return instructionText.text;
	}
}