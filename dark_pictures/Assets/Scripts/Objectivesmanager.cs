using UnityEngine;
using TMPro;

public class ObjectivesManager : MonoBehaviour
{
	public static ObjectivesManager Instance;

	[Header("UI Reference")]
	public TextMeshProUGUI objectiveText;

	[Header("Objectives State")]
	public bool hasFlashedEntity = false;

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		UpdateObjectiveText("Objective: Find the Entity and Flash it with your Camera!");
	}

	public void UpdateObjectiveText(string newText)
	{
		if (objectiveText != null)
			objectiveText.text = newText;
	}

	public void CompleteFlashObjective()
	{
		if (hasFlashedEntity) return;

		hasFlashedEntity = true;

		if (TutorialNarrativeManager.Instance != null)
		{
			TutorialNarrativeManager.Instance.AdvanceStory(6);
		}

		Debug.Log("Objective Updated: Entity Flashed!");
	}
}