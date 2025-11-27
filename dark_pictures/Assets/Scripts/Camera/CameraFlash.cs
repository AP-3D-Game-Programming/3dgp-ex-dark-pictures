using UnityEngine;

public class CameraFlash : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("Drag the Light component here")]
	public Light flashLight;

	[Tooltip("How bright the flash is at its peak")]
	public float flashIntensity = 10f;

	[Tooltip("How fast the light fades to black")]
	public float fadeSpeed = 10f;

	[Tooltip("How long the cooldown between flashes is (in seconds)")]
	public float flashCooldown = 5f;

	[Header("Audio (Optional)")]
	public AudioSource audioSource;
	public AudioClip flashSound;

	private float nextFlashTime = 0f;

	void Start()
	{
		if (flashLight != null)
		{
			flashLight.intensity = 0f;
		}
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

		if (audioSource != null && flashSound != null)
		{
			audioSource.PlayOneShot(flashSound);
		}
	}
}