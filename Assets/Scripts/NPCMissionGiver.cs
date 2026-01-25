using UnityEngine;

public class NPCMissionGiver : MonoBehaviour
{
    [Header("Stealth Mission Dialogues")]
    [TextArea] public string briefingDialogue = "Luom oi! I need you to sneak into the training area and retrieve a secret document. Be careful! The guards are patrolling and will catch you if you're spotted!";
    [TextArea] public string briefingMission = "Sneak past the guards and collect the secret document.";
    
    [TextArea] public string inProgressDialogue = "You haven't completed the mission yet! Go collect the document and don't get caught!";
    
    [TextArea] public string hasDocumentDialogue = "You got the document! Well done, Luom!";
    
    [TextArea] public string completedDialogue = "Fantastic work! You are a true stealth master!";

    [Header("Settings")]
    public float interactDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Visual Feedback")]
    public GameObject interactionPrompt; // Optional UI prompt like "Press E to talk"

    private Transform player;
    private MissionManager missionManager;
    private bool playerInRange = false;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Unity 6: use FindFirstObjectByType instead of FindObjectOfType
        missionManager = Object.FindFirstObjectByType<MissionManager>();

        // Hide interaction prompt initially
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (player == null || missionManager == null) return;

        // Check distance to player
        float dist = Vector3.Distance(transform.position, player.position);
        playerInRange = dist <= interactDistance;

        // Show/hide interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(playerInRange);

        // Handle interaction
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        if (missionManager == null) return;

        MissionManager.MissionState currentState = missionManager.GetCurrentState();

        switch (currentState)
        {
            case MissionManager.MissionState.NotStarted:
                // Give mission briefing and start mission
                missionManager.OpenDialogue(briefingDialogue, briefingMission);
                // Start mission after dialogue closes
                Invoke(nameof(StartMissionDelayed), 0.5f);
                break;

            case MissionManager.MissionState.Active:
                // Player hasn't collected document yet
                missionManager.OpenDialogue(inProgressDialogue, "Find and collect the secret document!");
                break;

            case MissionManager.MissionState.HasDocument:
                // Player has document - complete mission!
                missionManager.OpenDialogue(hasDocumentDialogue, "Mission Complete!");
                missionManager.CompleteMission();
                break;

            case MissionManager.MissionState.Completed:
                // Mission already completed
                missionManager.OpenDialogue(completedDialogue, "You already completed this mission!");
                break;

            case MissionManager.MissionState.Failed:
                // Mission failed (shouldn't happen as scene reloads)
                missionManager.OpenDialogue("You were caught! Try again.", "");
                break;
        }
    }

    void StartMissionDelayed()
    {
        if (missionManager != null)
        {
            missionManager.StartStealthMission();
        }
    }

    // Draw interaction range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
