using UnityEngine;

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

	private bool hasTriggered = false;
	private float spawnTime;

	// Zodra dit object AAN gaat (door de trigger), starten we de klok
	void OnEnable()
	{
		spawnTime = Time.time;
	}

	public void TriggerIntroScare()
	{
		if (Time.time < spawnTime + 0.5f) return;

		if (hasTriggered) return;

		Debug.Log("MONSTER WEGGEFLITST!");

		// 1. Geluid en Effecten
		if (audioSource != null && screamSound != null) audioSource.PlayOneShot(screamSound);
		if (vanishEffect != null) vanishEffect.Play();
		if (visualModel != null) visualModel.SetActive(false);

		// 2. Speler weer loslaten
		if (playerScript != null) playerScript.enabled = true;

		// 3. Tekst "Press LMB" weer UIT zetten
		if (flashPromptUI != null) flashPromptUI.SetActive(false);

		// 4. Start de echte game
		if (gameManager != null) gameManager.StartGame();
		if (realMonster != null) realMonster.SetActive(true);

		Destroy(gameObject, 2f);
	}
}