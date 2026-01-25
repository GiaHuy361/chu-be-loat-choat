using UnityEngine;
using TMPro;

/// <summary>
/// NPC riêng cho Stealth Mission - KHÔNG dùng NPCMissionGiver
/// Đơn giản, chuyên nghiệp, tách biệt hoàn toàn
/// </summary>
public class StealthTrainer : MonoBehaviour
{
    [Header("Mission Dialogues")]
    [TextArea(2, 4)]
    public string missionBriefing = "Bạn cần lấy mật thư ở cuối đường mà không bị huấn luyện viên phát hiện. Hãy sử dụng kỹ năng lén lút của mình.";
    
    [TextArea(2, 4)]
    public string missionCompleteDialogue = "Xuất sắc! Bạn đã hoàn thành nhiệm vụ lén lút. Đây là phần thưởng của bạn.";
    
    [TextArea(2, 4)]
    public string missionActiveDialogue = "Hãy hoàn thành nhiệm vụ trước đã. Lấy mật thư và quay về.";

    [Header("Interaction Settings")]
    [Tooltip("Khoảng cách để tương tác")]
    public float interactDistance = 2.5f;
    
    [Tooltip("Phím tương tác")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Visual Feedback")]
    [Tooltip("Hiển thị icon khi player gần")]
    public GameObject interactionIcon;
    
    [Tooltip("Outline material khi có thể interact")]
    public Material highlightMaterial;

    [Header("References")]
    [Tooltip("Mission Manager reference")]
    public StealthMissionManager missionManager;

    private Transform player;
    private MissionManager dialogueManager; // Để hiển thị dialogue
    private bool playerInRange = false;
    private Renderer npcRenderer;
    private Material originalMaterial;

    void Start()
    {
        Debug.Log($"[StealthTrainer] {gameObject.name} initializing...");
        
        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[StealthTrainer] Player found: {player.name}");
        }
        else
        {
            Debug.LogError($"[StealthTrainer] ❌ PLAYER NOT FOUND! Check Player tag!");
        }

        // Tìm dialogue manager (MissionManager cũ)
        dialogueManager = Object.FindFirstObjectByType<MissionManager>();
        if (dialogueManager != null)
        {
            Debug.Log($"[StealthTrainer] MissionManager found for dialogues");
        }
        else
        {
            Debug.LogWarning($"[StealthTrainer] MissionManager not found - dialogues will be logged only");
        }

        // Tìm mission manager nếu chưa assign
        if (missionManager == null)
        {
            missionManager = StealthMissionManager.Instance;
            if (missionManager != null)
            {
                Debug.Log($"[StealthTrainer] Found StealthMissionManager via Instance");
            }
        }
        else
        {
            Debug.Log($"[StealthTrainer] StealthMissionManager assigned in Inspector");
        }

        if (missionManager == null)
        {
            Debug.LogError($"[StealthTrainer] ❌ NO StealthMissionManager! NPC won't work!");
        }

        // Setup visual
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
            Debug.Log($"[StealthTrainer] Interaction icon setup");
        }

        npcRenderer = GetComponentInChildren<Renderer>();
        if (npcRenderer != null && highlightMaterial == null)
        {
            originalMaterial = npcRenderer.material;
        }
        
        Debug.Log($"[StealthTrainer] {gameObject.name} ready! Interact distance: {interactDistance}m");
    }

    void Update()
    {
        if (player == null) return;

        // Check distance
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactDistance;

        // Update visual feedback
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            UpdateVisualFeedback(inRange);
            
            if (inRange)
            {
                Debug.Log($"[StealthTrainer] Player entered interact range ({distance:F1}m). Press E to interact.");
            }
            else
            {
                Debug.Log($"[StealthTrainer] Player left interact range");
            }
        }

        // Handle interaction
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[StealthTrainer] ⌨️ Player pressed {interactKey} - Processing interaction...");
            OnInteract();
        }
    }

    void UpdateVisualFeedback(bool show)
    {
        // Show/hide interaction icon
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(show);
        }

        // Highlight NPC
        if (npcRenderer != null && highlightMaterial != null)
        {
            npcRenderer.material = show ? highlightMaterial : originalMaterial;
        }
    }

    void OnInteract()
    {
        Debug.Log($"[StealthTrainer] === INTERACTION START ===");
        
        if (missionManager == null)
        {
            Debug.LogError("[StealthTrainer] ❌ StealthMissionManager is NULL! Cannot process interaction!");
            Debug.LogError("[StealthTrainer] Make sure StealthMissionManager exists in scene and is assigned!");
            return;
        }

        var state = missionManager.GetCurrentState();
        Debug.Log($"[StealthTrainer] Current mission state: {state}");

        switch (state)
        {
            case StealthMissionManager.MissionState.Inactive:
                Debug.Log($"[StealthTrainer] → State: Inactive → Starting mission...");
                StartMission();
                break;

            case StealthMissionManager.MissionState.HasDocument:
                Debug.Log($"[StealthTrainer] → State: HasDocument → Completing mission...");
                CompleteMission();
                break;

            case StealthMissionManager.MissionState.Active:
                Debug.Log($"[StealthTrainer] → State: Active → Reminding player...");
                ShowDialogue(missionActiveDialogue, "Nhiệm vụ đang thực hiện");
                break;

            case StealthMissionManager.MissionState.Completed:
                Debug.Log($"[StealthTrainer] → State: Completed → Already done!");
                ShowDialogue("Bạn đã hoàn thành nhiệm vụ này rồi!", "Nhiệm vụ hoàn thành");
                break;
        }
        
        Debug.Log($"[StealthTrainer] === INTERACTION END ===\n");
    }

    void StartMission()
    {
        Debug.Log("[StealthTrainer] ▶️ STARTING STEALTH MISSION");
        
        // Show briefing dialogue
        ShowDialogue(missionBriefing, "Nhiệm vụ lén lút");
        
        // Start mission (guards/document sẽ spawn)
        Debug.Log("[StealthTrainer] Calling missionManager.StartMission()...");
        missionManager.StartMission();
        Debug.Log("[StealthTrainer] ✅ Mission started! Guards and document should now be active.");
    }

    void CompleteMission()
    {
        Debug.Log("[StealthTrainer] 🏆 COMPLETING STEALTH MISSION");
        
        // Show completion dialogue
        ShowDialogue(missionCompleteDialogue, "Nhiệm vụ hoàn thành!");
        
        // Complete mission (guards/document sẽ despawn)
        Debug.Log("[StealthTrainer] Calling missionManager.CompleteMission()...");
        missionManager.CompleteMission();
        Debug.Log("[StealthTrainer] ✅ Mission completed! Guards and document should now be hidden.");
        
        // TODO: Give reward
        GiveReward();
    }

    void ShowDialogue(string dialogue, string title)
    {
        if (dialogueManager != null)
        {
            dialogueManager.OpenDialogue(dialogue, title);
        }
        else
        {
            Debug.Log($"[StealthTrainer] {title}: {dialogue}");
        }
    }

    void GiveReward()
    {
        // TODO: Implement reward system
        Debug.Log("[StealthTrainer] Reward given! (TODO: Implement)");
        
        // Examples:
        // - playerInventory.AddItem("Stealth Badge");
        // - playerStats.AddXP(100);
        // - playerCurrency.AddCoins(50);
    }

    // Gizmo để visualize interact range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
