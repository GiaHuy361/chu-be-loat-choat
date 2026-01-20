using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI hintText;

    [Header("Keys")]
    public KeyCode interactKey = KeyCode.E;

    private bool isDialogueOpen;

    private void Awake()
    {
        // Singleton guard (prevents duplicates)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Only if you really need persistence across scenes
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
        if (hintText != null) hintText.text = "";
        if (missionText != null) missionText.text = "";
    }

    void Update()
    {
        if (!isDialogueOpen) return;
        if (Input.GetKeyDown(interactKey)) CloseDialogue();
    }

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
}
