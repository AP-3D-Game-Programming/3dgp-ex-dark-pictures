using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Keypad : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI[] digitDisplays; // 4 cijfer displays
    public TextMeshProUGUI statusText;
    public GameObject keypadUI; // UI panel

    [Header("Door Settings")]
    public SingleDoorController doorController;

    [Header("Keypad Settings")]
    public string correctCode = "1234";
    public float interactionRange = 3f;
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Player")]
    public Transform player;
    public PlayerController playerController;

    [Header("Audio")]
    public AudioClip buttonPressSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public float audioVolume = 0.5f;

    private string enteredCode = "";
    private bool isUnlocked = false;
    private bool isFocused = false;
    private AudioSource audioSource;

    void Start()
    {
        // Auto-find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = audioVolume;

        // Validate door controller reference
        if (doorController == null)
        {
            Debug.LogWarning("Keypad: No SingleDoorController assigned! Door will not open.");
        }

        UpdateDisplay();
    }

    void Update()
    {
        // Handle keypad focus
        if (player == null || isUnlocked) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactionRange;

        // E toets om te focussen/unfocussen
        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleFocus();
        }
        else if (!inRange && isFocused)
        {
            Unfocus();
        }
    }

    void ToggleFocus()
    {
        if (isFocused)
        {
            Unfocus();
        }
        else
        {
            Focus();
        }
    }

    void Focus()
    {
        isFocused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
            playerController.playerCanMove = false;
    }

    void Unfocus()
    {
        isFocused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.playerCanMove = true;
    }

    public void PressNumber(int number)
    {
        if (!isFocused || isUnlocked || enteredCode.Length >= 4) return;

        enteredCode += number.ToString();
        PlaySound(buttonPressSound);
        UpdateDisplay();

        if (enteredCode.Length == 4)
        {
            Invoke("CheckCode", 0.5f);
        }
    }

    public void ClearCode()
    {
        if (!isFocused) return;

        enteredCode = "";
        if (statusText != null)
        {
            statusText.text = "Enter Code";
            statusText.color = normalColor;
        }
        PlaySound(buttonPressSound);
        UpdateDisplay();
    }

    void CheckCode()
    {
        if (enteredCode == correctCode)
        {
            // CORRECT!
            isUnlocked = true;

            if (statusText != null)
            {
                statusText.text = "Access Granted";
                statusText.color = correctColor;
            }

            PlaySound(correctSound);

            // Open the door using SingleDoorController
            if (doorController != null)
            {
                Debug.Log("Keypad: Opening door via SingleDoorController");
                doorController.SetDoorState(true);
            }
            else
            {
                Debug.LogError("Keypad: doorController is null! Cannot open door.");
            }

            Invoke("Unfocus", 2f);
        }
        else
        {
            // WRONG!
            if (statusText != null)
            {
                statusText.text = "Access Denied";
                statusText.color = wrongColor;
            }

            PlaySound(wrongSound);

            Invoke("ClearCode", 1.5f);
        }
    }

    void UpdateDisplay()
    {
        if (digitDisplays == null) return;

        for (int i = 0; i < digitDisplays.Length; i++)
        {
            if (digitDisplays[i] != null)
            {
                if (i < enteredCode.Length)
                    digitDisplays[i].text = enteredCode[i].ToString();
                else
                    digitDisplays[i].text = "_";
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, audioVolume);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw line to door controller if assigned
        if (doorController != null)
        {
            Gizmos.color = isUnlocked ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, doorController.transform.position);
        }
    }
}
