using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;  

public class UploadScreen : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI statusText;
    public Image progressBarFill;

    [Header("Settings")]
    public float uploadDuration = 5f;  // Hoelang upload duurt in seconden
    public float interactionRange = 3f; // Afstand waarbinnen je kan interacteren

    [Header("Player Reference")]
    public Transform player;

    [Header("Door Reference")]
    public MainDoorController mainDoor; // Reference to the main door to open

    [Header("Audio")]
    public AudioClip doneSound;  // Geluid bij upload complete
    public float doneVolume = 0.8f;

    private enum UploadState
    {
        Idle,           // Wachten op player
        ReadyToUpload,  // Player is dichtbij
        Uploading,      // Bezig met uploaden
        Done            // Klaar
    }

    private UploadState currentState = UploadState.Idle;
    private float uploadProgress = 0f;
    private AudioSource audioSource;

    void Start()
    {
        if (player == null)
        {
            // Probeer player automatisch te vinden
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Setup AudioSource voor done sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = doneVolume;

        UpdateUI();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check states
        switch (currentState)
        {
            case UploadState.Idle:
            case UploadState.Done:
                // Check of player dichtbij komt
                if (distanceToPlayer <= interactionRange && currentState != UploadState.Done)
                {
                    currentState = UploadState.ReadyToUpload;
                    UpdateUI();
                }
                break;

            case UploadState.ReadyToUpload:
                // Check of player te ver weg gaat
                if (distanceToPlayer > interactionRange)
                {
                    currentState = UploadState.Idle;
                    UpdateUI();
                }
                // Check voor E toets
                else if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartUpload();
                }
                break;

            case UploadState.Uploading:
                // Update progress
                uploadProgress += Time.deltaTime / uploadDuration;
                progressBarFill.fillAmount = uploadProgress;

                // Check of klaar
                if (uploadProgress >= 1f)
                {
                    currentState = UploadState.Done;
                    UpdateUI();
                }
                break;
        }
    }

    void StartUpload()
    {
        currentState = UploadState.Uploading;
        uploadProgress = 0f;
        UpdateUI();
    }

    void UpdateUI()
    {
        switch (currentState)
        {
            case UploadState.Idle:
                statusText.text = "";
                progressBarFill.fillAmount = 0f;
                break;

            case UploadState.ReadyToUpload:
                statusText.text = "Press E to upload files";
                progressBarFill.fillAmount = 0f;
                break;

            case UploadState.Uploading:
                statusText.text = "Uploading...";
                break;

            case UploadState.Done:
                statusText.text = "Done!";
                progressBarFill.fillAmount = 1f;

                // Speel done sound af
                if (doneSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(doneSound, doneVolume);
                }

                // Open the main door
                if (mainDoor != null)
                {
                    mainDoor.ForceOpen();
                }
                break;
        }
    }

    //Reset functie voor later
    public void ResetUpload()
    {
        currentState = UploadState.Idle;
        uploadProgress = 0f;
        UpdateUI();
    }
}
