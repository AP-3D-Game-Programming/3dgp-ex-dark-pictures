using NUnit.Framework;
using Unity.VisualScripting;
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

	[SerializeField] BatteryLife batteryLife;

	[SerializeField] FlashIndicator flashIndicator;
	[SerializeField] float batteryDecreasingAmount = 25f;
	[SerializeField] AudioClip smallFlashSound;

	[SerializeField] private float smallFlashlightIntensity = 0.5f;
	[SerializeField] private float smallFlashlightPowerConsumption = 2f;
    [SerializeField] AudioClip errorClip;

	private bool isSmallFlashlightOn = false;

	private float lightIntensityToStop = 0f;

	void Start()
	{
		if (flashLight != null) flashLight.intensity = 0f;
	}

	void Update()
	{
		if (gameManager != null && gameManager.isGameOver) return;

		if (Input.GetMouseButtonDown(0))
		{
			// Prevent flashing if the cursor is unlocked (e.g. interacting with Keypad)
			if (Cursor.lockState != CursorLockMode.Locked) return;

			if (flashLight == null) return;
			if (Time.time >= nextFlashTime)
			{
				TriggerFlash();
				nextFlashTime = Time.time + flashCooldown;
			}
		}

		if (flashLight.intensity > lightIntensityToStop)
		{
			flashLight.intensity -= fadeSpeed * Time.deltaTime;
			if (flashLight.intensity < lightIntensityToStop) flashLight.intensity = lightIntensityToStop;
		}

		if (Input.GetKeyDown(KeyCode.F) && batteryLife.GetBatteryLife() > -.5f)
		{
			if (isSmallFlashlightOn)
				TriggerSmallFlash(false);
			else TriggerSmallFlash(true);
		}
		else if (isSmallFlashlightOn && batteryLife.GetBatteryLife() <= -.5f)
		{
			TriggerSmallFlash(false);
			flashIndicator.SetColor(ObjectColor.RED);
		}

	}

	private void TriggerSmallFlash(bool On)
	{
		if (On)
		{
			isSmallFlashlightOn = true;

			lightIntensityToStop = smallFlashlightIntensity;
			TriggerFlash(smallFlashlightIntensity, smallFlashSound);

			batteryLife.isDecreasingOverTime = true;
			StartCoroutine(batteryLife.DecreaseBatteryOverTime(1f, smallFlashlightPowerConsumption));
		}
		else
		{
			isSmallFlashlightOn = false;

			lightIntensityToStop = 0;
			audioSource.PlayOneShot(smallFlashSound);

			batteryLife.isDecreasingOverTime = false;
		}

	}



	void TriggerFlash(float flashPower, AudioClip clip, bool isBigFlash = false)
	{
		if (isBigFlash && batteryLife != null && flashIndicator != null)
		{
			if (!(batteryLife.GetBatteryLife() <= batteryDecreasingAmount))
			{
				batteryLife.DecreaseBattery(batteryDecreasingAmount);
				flashIndicator.SetCooldown(flashCooldown);
			}
			else
			{
				audioSource.PlayOneShot(errorClip);
				return;
			}
		}

		flashLight.intensity = flashPower;
		if (audioSource != null && flashSound != null) audioSource.PlayOneShot(clip);

		DetectEnemies();
	}
	void TriggerFlash() => TriggerFlash(flashIntensity, flashSound, true);
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

						// *** ADDED THIS LINE ***
						// Notify the manager that we stunned the entity
						if (ObjectivesManager.Instance != null)
						{
							ObjectivesManager.Instance.CompleteFlashObjective();
						}
						// ***********************

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