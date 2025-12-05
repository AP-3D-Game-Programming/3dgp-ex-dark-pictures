using UnityEngine;

public class CameraFlash : MonoBehaviour
{
	[Header("Settings")]
	public Light flashLight;
	public float flashIntensity = 10f;
	public float fadeSpeed = 10f;
	public float flashCooldown = 5f;

	[Header("Stun Settings")]
	public float stunDuration = 3f;
	public float flashRange = 15f;
	public float flashAngle = 60f;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip flashSound;

	private float nextFlashTime = 0f;

	[SerializeField] GameManager gameManager;

	void Start()
	{
		if (flashLight != null) flashLight.intensity = 0f;
	}

	void Update()
	{
		if (gameManager != null && gameManager.isGameOver) return;

		if (Input.GetMouseButtonDown(0))
		{
			if (flashLight == null) return;
			if (Time.time >= nextFlashTime)
			{
				TriggerFlash();
				nextFlashTime = Time.time + flashCooldown;
			}
		}

		if (flashLight.intensity > 0)
		{
			flashLight.intensity -= fadeSpeed * Time.deltaTime;
			if (flashLight.intensity < 0) flashLight.intensity = 0;
		}
	}

	void TriggerFlash()
	{
		flashLight.intensity = flashIntensity;
		if (audioSource != null && flashSound != null) audioSource.PlayOneShot(flashSound);

		DetectEnemies();
	}

	void DetectEnemies()
	{
		Collider[] hits = Physics.OverlapSphere(transform.position, flashRange);

		foreach (Collider hit in hits)
		{
			EntityAI realEnemy = hit.GetComponentInParent<EntityAI>();
			IntroEntity introEnemy = hit.GetComponentInParent<IntroEntity>();

			if (realEnemy == null && introEnemy == null) continue;

			Transform targetTransform = hit.transform;
			Vector3 directionToEnemy = (targetTransform.position - transform.position).normalized;
			float angle = Vector3.Angle(transform.forward, directionToEnemy);

			if (angle < flashAngle)
			{
				Vector3 startPos = transform.position + (transform.forward * 0.5f);
				Vector3 endPos = targetTransform.position;

				RaycastHit lineHit;

				// Negeer triggers (deuren etc)
				if (Physics.Linecast(startPos, endPos, out lineHit, -1, QueryTriggerInteraction.Ignore))
				{
					if (lineHit.transform.root == transform.root)
					{
						Debug.Log("<color=red>GEBLOKKEERD DOOR MEZELF:</color> Probeer stap 2 hieronder!");
						continue;
					}

					EntityAI hitReal = lineHit.transform.GetComponentInParent<EntityAI>();
					if (realEnemy != null && hitReal == realEnemy)
					{
						Debug.Log("Stunned Real Entity (Via " + lineHit.transform.name + ")");
						realEnemy.StunEntity(stunDuration);
						continue;
					}

					IntroEntity hitIntro = lineHit.transform.GetComponentInParent<IntroEntity>();
					if (introEnemy != null && hitIntro == introEnemy)
					{
						Debug.Log("Triggered Intro Sequence (Via " + lineHit.transform.name + ")");
						introEnemy.TriggerIntroScare();
						continue;
					}

					Debug.Log("<color=red>GEBLOKKEERD DOOR:</color> " + lineHit.transform.name);
				}
			}
		}
	}
}