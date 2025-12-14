using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
     [Header("Atmosphere Loops")]
    public AudioClip[] atmosphereClips;      // Atmosphere files hier
    public float atmosphereVolume = 0.2f;     // Laag volume voor background

    [Header("Timing")]
    public float minPlayDuration = 20f;       // Minimale speeltijd van een atmosphere
    public float maxPlayDuration = 60f;       // Maximale speeltijd
    public float minSilenceDuration = 5f;     // Minimale stilte tussen atmospheres
    public float maxSilenceDuration = 20f;    // Maximale stilte

    private AudioSource atmosphereSource;
    private float nextActionTime = 0f;
    private bool isPlaying = false;

    void Start()
    {
        atmosphereSource = gameObject.AddComponent<AudioSource>();
        atmosphereSource.playOnAwake = false;
        atmosphereSource.loop = true;  // Loopt terwijl actief
        atmosphereSource.spatialBlend = 0f; // 2D background
        atmosphereSource.volume = atmosphereVolume;

        // Start met stilte
        PlanNextAction(true);
    }

    void Update()
    {
        if (Time.time >= nextActionTime)
        {
            if (isPlaying)
            {
                // Stop huidige atmosphere en plan stilte
                StopAtmosphere();
            }
            else
            {
                // Start nieuwe atmosphere
                PlayRandomAtmosphere();
            }
        }
    }

    void PlayRandomAtmosphere()
    {
        if (atmosphereClips == null || atmosphereClips.Length == 0) return;

        // Kies random atmosphere
        AudioClip clip = atmosphereClips[Random.Range(0, atmosphereClips.Length)];
        if (clip == null) return;

        atmosphereSource.clip = clip;
        atmosphereSource.Play();
        isPlaying = true;

        // Plan wanneer te stoppen (random tussen min en max)
        PlanNextAction(false);
    }

    void StopAtmosphere()
    {
        atmosphereSource.Stop();
        isPlaying = false;

        // Plan wanneer volgende atmosphere te starten
        PlanNextAction(true);
    }

    void PlanNextAction(bool isSilence)
    {
        float duration;

        if (isSilence)
        {
            // Random stilte duur
            duration = Random.Range(minSilenceDuration, maxSilenceDuration);
        }
        else
        {
            // Random speelduur
            duration = Random.Range(minPlayDuration, maxPlayDuration);
        }

        nextActionTime = Time.time + duration;
    }
}
