using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
	[Header("Welk verhaal stukje is dit?")]
	public int storyID; // 1 = Crouch, 2 = Jump, etc.

	[Header("Eenmalig?")]
	public bool triggerOnce = true;
	private bool hasTriggered = false;

	private void OnTriggerEnter(Collider other)
	{
		if (triggerOnce && hasTriggered) return;

		// Check of het de speler is (Zorg dat je Player de tag "Player" heeft!)
		if (other.CompareTag("Player"))
		{
			TutorialNarrativeManager.Instance.AdvanceStory(storyID);
			hasTriggered = true;

			// Optioneel: Vernietig de trigger na gebruik om geheugen te sparen
			// Destroy(gameObject); 
		}
	}
}