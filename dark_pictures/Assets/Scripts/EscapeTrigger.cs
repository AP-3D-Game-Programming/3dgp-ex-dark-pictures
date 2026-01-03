using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EscapeTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winScreenUI; 

    [Header("Player Reference")]
    public PlayerController playerController;

    [Header("Audio")]
    public AudioClip winSound;
    private AudioSource audioSource;

    private bool hasEscaped = false;
    private bool canEscape = false; // Trigger is inactive by default

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Ensure UI is hidden at start
        if (winScreenUI != null) winScreenUI.SetActive(false);
    }

    // Call this method to activate the escape trigger (e.g. after upload)
    public void EnableEscape()
    {
        canEscape = true;
        Debug.Log("Escape Trigger Activated! Player can now leave.");
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Debug to confirm physics is working
        Debug.Log($"[EscapeTrigger] Hit by object: {other.name} with Tag: {other.tag}");

        if (hasEscaped) return;

        // 2. Check if we are allowed to escape yet
        if (!canEscape) 
        {
            Debug.LogWarning("[EscapeTrigger] Player hit trigger, but 'canEscape' is FALSE. (Did the upload finish? Is the reference in UploadScreen set?)");
            return;
        }

        // 3. Check tag
        if (other.CompareTag("Player"))
        {
            Escape();
        }
        else
        {
            Debug.LogWarning($"[EscapeTrigger] Object entered but tag was '{other.tag}' (Expected 'Player')");
        }
    }

    void Escape()
    {
        hasEscaped = true;
        Debug.Log("YOU ESCAPED!");

        // 1. Show Win Screen
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(true);
        }
        else
        {
            Debug.LogError("Win Screen UI reference is missing in Inspector!");
        }

        // 2. Play Sound
        if (winSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        // 3. Stop Player Movement & Unlock Cursor
        if (playerController != null)
        {
            playerController.playerCanMove = false;
            
            // Unlock cursor so player can click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Call this from a "Quit" button
    public void QuitGame()
    {
        Application.Quit();
    }

    // Call this from a "Restart" button
    [ContextMenu("Restart Level")]
    public void RestartLevel()
    {
        Debug.Log("Restarting Level...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
