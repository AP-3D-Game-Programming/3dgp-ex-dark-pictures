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
    public Transform doorTransform; // DEUR GameObject
    public float openAngle = -90f;
    public float openSpeed = 2f;

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
    private bool isOpen = false;
    private bool isFocused = false;
    private AudioSource audioSource;

    // Door rotation
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Transform doorParent;

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

        // Setup door rotation
        if (doorTransform != null)
        {
            // Check of er een parent is, zo niet gebruik het object zelf
            if (doorTransform.parent != null)
            {
                doorParent = doorTransform.parent;
            }
            else
            {
                doorParent = doorTransform; // Geen parent - roteer object zelf
            }

            closedRotation = doorParent.rotation;
            openRotation = Quaternion.Euler(doorParent.eulerAngles + new Vector3(0, openAngle, 0));
        }

        UpdateDisplay();
    }

    void Update()
    {
        // Handle door rotation
        if (doorParent != null)
        {
            if (isOpen)
            {
                doorParent.rotation = Quaternion.Lerp(doorParent.rotation, openRotation, Time.deltaTime * openSpeed);
            }
            else
            {
                doorParent.rotation = Quaternion.Lerp(doorParent.rotation, closedRotation, Time.deltaTime * openSpeed);
            }
        }

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
            isOpen = true;

            if (statusText != null)
            {
                statusText.text = "Access Granted";
                statusText.color = correctColor;
            }

            PlaySound(correctSound);

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

        if (doorTransform != null)
        {
            Gizmos.color = isUnlocked ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, doorTransform.position);
        }
    }
}
