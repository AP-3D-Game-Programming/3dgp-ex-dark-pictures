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

	void Start()
	{
		if (flashLight != null) flashLight.intensity = 0f;
	}

	void Update()
	{
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

		DetectAndStunEnemies();
	}

	void DetectAndStunEnemies()
	{
		Collider[] hits = Physics.OverlapSphere(transform.position, flashRange);

		foreach (Collider hit in hits)
		{
			EntityAI enemy = hit.GetComponentInParent<EntityAI>();

			if (enemy != null)
			{
				Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
				float angle = Vector3.Angle(transform.forward, directionToEnemy);

				if (angle < flashAngle)
				{
					Vector3 startPos = transform.position + (transform.forward * 0.5f);

					Vector3 endPos = enemy.transform.position + Vector3.up;

					RaycastHit lineHit;

					if (Physics.Linecast(startPos, endPos, out lineHit))
					{
						// Check if we hit the enemy OR a child of the enemy
						if (lineHit.transform == enemy.transform || lineHit.transform.IsChildOf(enemy.transform))
						{
							Debug.Log("<color=green>STUN SUCCESS:</color> " + enemy.name);
							enemy.StunEntity(stunDuration);
						}
						else
						{
							Debug.Log("<color=red>BLOCKED BY:</color> " + lineHit.transform.name);
						}
					}
				}
			}
		}
	}
}