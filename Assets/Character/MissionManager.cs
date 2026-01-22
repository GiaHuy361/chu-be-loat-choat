using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    // Mission States
    public enum MissionState
    {
        NotStarted,
        Briefing,
        Active,
        HasDocument,
        Failed,
        Completed
    }

    [Header("Mission State")]
    public MissionState currentState = MissionState.NotStarted;

    [Header("Dialogue UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI hintText;

    [Header("Stealth Mission UI Panels")]
    public GameObject tutorialPanel;
    public GameObject failurePanel;
    public GameObject successPanel;

    [Header("Keys")]
    public KeyCode interactKey = KeyCode.E;

    private bool isDialogueOpen;
    private GameObject player;
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    private void Awake()
    {
        // Singleton guard (prevents duplicates)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Find and store player reference
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStartPosition = player.transform.position;
            playerStartRotation = player.transform.rotation;
        }

        // Hide all UI panels initially
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);
        if (successPanel != null) successPanel.SetActive(false);

        // Clear text fields
        if (dialogueText != null) dialogueText.text = "";
        if (hintText != null) hintText.text = "";
        if (missionText != null) missionText.text = "";
    }

    void Update()
    {
        if (!isDialogueOpen) return;
        if (Input.GetKeyDown(interactKey)) CloseDialogue();
    }

    // Original dialogue system (for general NPC interactions)
    public void OpenDialogue(string dialogueLine, string missionLine)
    {
        if (dialoguePanel == null) return;

        isDialogueOpen = true;
        dialoguePanel.SetActive(true);

        if (dialogueText != null) dialogueText.text = $"\"{dialogueLine}\"";
        if (missionText != null) missionText.text = $"NEW MISSION:\n{missionLine}";
        if (hintText != null) hintText.text = $"(Press {interactKey} to close)";
    }

    public void CloseDialogue()
    {
        isDialogueOpen = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
        if (hintText != null) hintText.text = "";
    }

    // Stealth Mission Methods
    public void StartStealthMission()
    {
        if (currentState != MissionState.NotStarted && currentState != MissionState.Failed)
            return;

        currentState = MissionState.Active;
        Debug.Log("Stealth Mission Started!");

        // Show tutorial panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            // Auto-hide tutorial after 5 seconds (optional)
            Invoke(nameof(HideTutorial), 5f);
        }
    }

    void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    public void OnPlayerDetected()
    {
        if (currentState != MissionState.Active && currentState != MissionState.HasDocument)
            return;

        currentState = MissionState.Failed;
        Debug.Log("Player detected! Mission Failed!");

        // Show failure panel
        if (failurePanel != null)
        {
            failurePanel.SetActive(true);
        }

        // Pause game (optional)
        Time.timeScale = 0f;
    }

    public void OnDocumentCollected()
    {
        if (currentState != MissionState.Active)
            return;

        currentState = MissionState.HasDocument;
        Debug.Log("Secret document collected! Return to NPC.");
    }

    public void CompleteMission()
    {
        if (currentState != MissionState.HasDocument)
            return;

        currentState = MissionState.Completed;
        Debug.Log("Mission Completed!");

        // Show success panel
        if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
    }

    public void RetryMission()
    {
        // Reset mission state
        currentState = MissionState.NotStarted;

        // Hide failure panel
        if (failurePanel != null)
            failurePanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;

        // Reload scene to reset everything
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        // Resume time before loading
        Time.timeScale = 1f;
        
        // Load main menu scene (change "MainMenu" to your actual menu scene name)
        // SceneManager.LoadScene("MainMenu");
        Debug.Log("Quit to menu (implement scene loading)");
    }

    // Helper methods for other scripts
    public bool IsMissionActive()
    {
        return currentState == MissionState.Active || currentState == MissionState.HasDocument;
    }

    public bool HasDocument()
    {
        return currentState == MissionState.HasDocument || currentState == MissionState.Completed;
    }

    public MissionState GetCurrentState()
    {
        return currentState;
    }
}
