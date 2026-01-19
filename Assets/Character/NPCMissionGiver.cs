using UnityEngine;

public class NPCMissionGiver : MonoBehaviour
{
    [Header("NPC Text (English to avoid font issues)")]
    [TextArea] public string dialogueLine = "Please bring this item to the base.";
    [TextArea] public string missionLine = "Deliver the package to the base.";

    [Header("Settings")]
    public float interactDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    private Transform player;
    private MissionManager missionManager;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Unity 6: use FindFirstObjectByType instead of FindObjectOfType
        missionManager = Object.FindFirstObjectByType<MissionManager>();
    }

    void Update()
    {
        if (player == null || missionManager == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= interactDistance && Input.GetKeyDown(interactKey))
        {
            missionManager.OpenDialogue(dialogueLine, missionLine);
        }
    }
}
