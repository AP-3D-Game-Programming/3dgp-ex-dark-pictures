using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
	[Header("Welk verhaal stukje is dit?")]
	public int storyID; // 1 = Crouch, 2 = Jump, etc. Zie TutorialNarrativeManager

	[Header("Eenmalig?")]
	public bool triggerOnce = true;
	private bool hasTriggered = false;

	private void OnTriggerEnter(Collider other)
	{
		if (triggerOnce && hasTriggered) return;

		if (other.CompareTag("Player"))
		{
			TutorialNarrativeManager.Instance.AdvanceStory(storyID);
			hasTriggered = true;
		}
	}
}