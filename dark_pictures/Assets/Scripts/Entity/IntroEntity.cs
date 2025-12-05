using UnityEngine;
using System.Collections; // Nodig voor Coroutines

public class IntroEntity : MonoBehaviour
{
	[Header("Setup")]
	public GameObject realMonster;
	public GameManager gameManager;
	public PlayerController playerScript;

	[Header("UI References")]
	public GameObject flashPromptUI;

	[Header("Visuals & Audio")]
	public GameObject visualModel;
	public AudioSource audioSource;
	public AudioClip screamSound;
	public ParticleSystem vanishEffect;

	[Header("Settings")]
	public float echoFadeSpeed = 4f;

	private bool hasTriggered = false;
	private float spawnTime;

	void OnEnable()
	{
		spawnTime = Time.time;

		if (audioSource != null && screamSound != null)
		{
			audioSource.clip = screamSound;
			audioSource.Play();
		}
	}

	public void TriggerIntroScare()
	{
		if (Time.time < spawnTime + 0.5f) return;

		if (hasTriggered) return;
		hasTriggered = true;

		Debug.Log("MONSTER WEGGEFLITST!");

		if (visualModel != null) visualModel.SetActive(false);
		if (vanishEffect != null) vanishEffect.Play();

		// 2. Audio "Echo Stop" Effect
		if (audioSource != null)
		{
			//  een echo.
			StartCoroutine(FadeOutAudio());
		}

		if (playerScript != null) playerScript.enabled = true;
		if (flashPromptUI != null) flashPromptUI.SetActive(false);

		if (gameManager != null) gameManager.StartGame();
		if (realMonster != null) realMonster.SetActive(true);

		Destroy(gameObject, 3f);
	}

	IEnumerator FadeOutAudio()
	{
		float startVolume = audioSource.volume;

		while (audioSource.volume > 0)
		{
			audioSource.volume -= startVolume * echoFadeSpeed * Time.deltaTime;
			yield return null;
		}

		audioSource.Stop();
		audioSource.volume = startVolume; // Reset volume voor de zekerheid
	}
}