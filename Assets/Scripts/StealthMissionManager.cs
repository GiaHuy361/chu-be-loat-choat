using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý Stealth Mission - REFACTORED
/// Đơn giản hơn, không tutorial, guards/document dynamic
/// </summary>
public class StealthMissionManager : MonoBehaviour
{
    public static StealthMissionManager Instance { get; private set; }

    [Header("Mission State")]
    public MissionState currentState = MissionState.Inactive;

    [Header("Mission Objects")]
    [Tooltip("Tất cả guards trong mission")]
    public GuardAI[] guards;
    
    [Tooltip("Secret document")]
    public SecretDocument secretDocument;

    [Header("UI Panels (Optional)")]
    public GameObject failurePanel;
    public GameObject successPanel;

    [Header("Settings")]
    [Tooltip("Tự động restart sau khi fail (giây)")]
    public float autoRestartDelay = 3f;
    
    [Tooltip("Có cho phép retry không")]
    public bool allowRetry = true;

    // Mission tracking
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    public enum MissionState
    {
        Inactive,       // Mission chưa active - Guards/Document ẩn
        Active,         // Đang thực hiện - Guards/Document hiện
        HasDocument,    // Đã lấy document
        Completed       // Hoàn thành
    }

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("[StealthMissionManager] ========== INITIALIZING ==========");
        Debug.Log($"[StealthMissionManager] Initial state: {currentState}");
        
        // Lưu player start position
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerStartPosition = playerObj.transform.position;
            playerStartRotation = playerObj.transform.rotation;
            Debug.Log($"[StealthMissionManager] Player start position saved: {playerStartPosition}");
        }
        else
        {
            Debug.LogError("[StealthMissionManager] ❌ PLAYER NOT FOUND! Tag must be 'Player'");
        }

        // Hide UI panels
        HideAllPanels();
        Debug.Log("[StealthMissionManager] UI panels hidden");

        // IMPORTANT: Disable guards và hide document ban đầu
        Debug.Log("[StealthMissionManager] Disabling mission objects (guards/document)...");
        SetMissionObjectsActive(false);
        
        ValidateReferences();
        Debug.Log("[StealthMissionManager] ========== READY ==========\n");
    }

    void ValidateReferences()
    {
        Debug.Log("[StealthMissionManager] === Validating References ===");
        
        if (guards == null || guards.Length == 0)
        {
            Debug.LogWarning("[StealthMissionManager] ⚠️ NO GUARDS assigned! Mission won't have patrols.");
        }
        else
        {
            Debug.Log($"[StealthMissionManager] ✅ {guards.Length} guards assigned");
            for (int i = 0; i < guards.Length; i++)
            {
                if (guards[i] != null)
                {
                    Debug.Log($"[StealthMissionManager]   - Guard {i}: {guards[i].gameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"[StealthMissionManager]   - Guard {i}: NULL!");
                }
            }
        }
        
        if (secretDocument == null)
        {
            Debug.LogWarning("[StealthMissionManager] ⚠️ NO SECRET DOCUMENT assigned!");
        }
        else
        {
            Debug.Log($"[StealthMissionManager] ✅ Secret Document: {secretDocument.gameObject.name}");
        }
        
        if (failurePanel != null)
        {
            Debug.Log($"[StealthMissionManager] ✅ Failure Panel assigned");
        }
        
        if (successPanel != null)
        {
            Debug.Log($"[StealthMissionManager] ✅ Success Panel assigned");
        }
        
        Debug.Log("[StealthMissionManager] === Validation Complete ===\n");
    }

    #region Mission Flow

    /// <summary>
    /// Bắt đầu mission - Được gọi từ StealthTrainer
    /// </summary>
    public void StartMission()
    {
        Debug.Log($"[StealthMissionManager] ========== START MISSION CALLED ==========");
        Debug.Log($"[StealthMissionManager] Current state: {currentState}");
        
        if (currentState != MissionState.Inactive)
        {
            Debug.LogWarning($"[StealthMissionManager] ⚠️ Cannot start - mission already active! State: {currentState}");
            return;
        }

        Debug.Log("[StealthMissionManager] ✅ Starting mission...");
        currentState = MissionState.Active;
        Debug.Log($"[StealthMissionManager] State changed: Inactive → Active");
        
        // Enable guards và show document
        SetMissionObjectsActive(true);
        Debug.Log("[StealthMissionManager] Mission objects activated!");
        Debug.Log("[StealthMissionManager] ========== MISSION STARTED ==========\n");
    }

    /// <summary>
    /// Player nhặt được document
    /// </summary>
    public void OnDocumentPickedUp()
    {
        Debug.Log($"[StealthMissionManager] ========== DOCUMENT PICKED UP ==========");
        Debug.Log($"[StealthMissionManager] Current state: {currentState}");
        
        if (currentState != MissionState.Active)
        {
            Debug.LogWarning($"[StealthMissionManager] ⚠️ Cannot pickup - mission not active! State: {currentState}");
            return;
        }

        Debug.Log("[StealthMissionManager] ✅ Document acquired! Return to NPC to complete.");
        currentState = MissionState.HasDocument;
        Debug.Log($"[StealthMissionManager] State changed: Active → HasDocument");
        Debug.Log("[StealthMissionManager] ========================================\n");
    }

    /// <summary>
    /// Hoàn thành mission - Được gọi từ StealthTrainer
    /// </summary>
    public void CompleteMission()
    {
        Debug.Log($"[StealthMissionManager] ========== COMPLETE MISSION CALLED ==========");
        Debug.Log($"[StealthMissionManager] Current state: {currentState}");
        
        if (currentState != MissionState.HasDocument)
        {
            Debug.LogWarning($"[StealthMissionManager] ⚠️ Cannot complete - don't have document! State: {currentState}");
            return;
        }

        Debug.Log("[StealthMissionManager] 🏆 MISSION COMPLETED!");
        currentState = MissionState.Completed;
        Debug.Log($"[StealthMissionManager] State changed: HasDocument → Completed");
        
        // Disable guards và hide document
        SetMissionObjectsActive(false);
        Debug.Log("[StealthMissionManager] Mission objects deactivated!");
        
        ShowSuccessPanel();
        Debug.Log("[StealthMissionManager] ==========================================\n");
    }

    /// <summary>
    /// Guard phát hiện player - Mission fail
    /// </summary>
    public void OnPlayerDetectedByGuard(GuardAI guard)
    {
        Debug.Log($"[StealthMissionManager] ========== PLAYER DETECTED! ==========");
        Debug.Log($"[StealthMissionManager] Detected by: {guard.gameObject.name}");
        Debug.Log($"[StealthMissionManager] Current state: {currentState}");
        
        if (currentState != MissionState.Active && currentState != MissionState.HasDocument)
        {
            Debug.Log("[StealthMissionManager] Mission not active - ignoring detection");
            return;
        }

        Debug.Log($"[StealthMissionManager] ❌ MISSION FAILED!");
        
        ShowFailurePanel();
        
        // Auto restart sau delay
        if (allowRetry && autoRestartDelay > 0)
        {
            Debug.Log($"[StealthMissionManager] Auto-restart in {autoRestartDelay} seconds...");
            Invoke(nameof(RestartMission), autoRestartDelay);
        }
        
        Debug.Log("[StealthMissionManager] ====================================\n");
    }

    #endregion

    #region Mission Object Management

    /// <summary>
    /// Enable/Disable guards và document
    /// </summary>
    void SetMissionObjectsActive(bool active)
    {
        Debug.Log($"[StealthMissionManager] --- SetMissionObjectsActive({active}) ---");

        // Guards
        if (guards != null && guards.Length > 0)
        {
            Debug.Log($"[StealthMissionManager] Processing {guards.Length} guards...");
            int enabledCount = 0;
            
            foreach (var guard in guards)
            {
                if (guard != null)
                {
                    if (active)
                    {
                        guard.EnableGuard();
                        enabledCount++;
                    }
                    else
                    {
                        guard.DisableGuard();
                    }
                }
                else
                {
                    Debug.LogWarning("[StealthMissionManager] Found NULL guard in array!");
                }
            }
            
            if (active)
            {
                Debug.Log($"[StealthMissionManager] ✅ {enabledCount} guards ENABLED");
            }
            else
            {
                Debug.Log($"[StealthMissionManager] 🔒 All guards DISABLED");
            }
        }
        else
        {
            Debug.LogWarning("[StealthMissionManager] ⚠️ No guards to process!");
        }

        // Document
        if (secretDocument != null)
        {
            if (active)
            {
                secretDocument.Show();
                Debug.Log($"[StealthMissionManager] ✅ Document SHOWN: {secretDocument.gameObject.name}");
            }
            else
            {
                secretDocument.Hide();
                Debug.Log($"[StealthMissionManager] 🔒 Document HIDDEN");
            }
        }
        else
        {
            Debug.LogWarning("[StealthMissionManager] ⚠️ No document to process!");
        }
        
        Debug.Log($"[StealthMissionManager] --- SetMissionObjectsActive COMPLETE ---\n");
    }

    #endregion

    #region UI Management

    void HideAllPanels()
    {
        if (failurePanel != null) failurePanel.SetActive(false);
        if (successPanel != null) successPanel.SetActive(false);
    }

    void ShowFailurePanel()
    {
        HideAllPanels();
        if (failurePanel != null)
        {
            failurePanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void ShowSuccessPanel()
    {
        HideAllPanels();
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion

    #region Mission Control

    /// <summary>
    /// Restart mission
    /// </summary>
    public void RestartMission()
    {
        Debug.Log("[StealthMissionManager] Restarting Mission...");

        // Reset state
        currentState = MissionState.Active;

        // Reset player position
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                playerObj.transform.position = playerStartPosition;
                playerObj.transform.rotation = playerStartRotation;
                cc.enabled = true;
            }
            else
            {
                playerObj.transform.position = playerStartPosition;
                playerObj.transform.rotation = playerStartRotation;
            }
        }

        // Reset guards
        if (guards != null)
        {
            foreach (GuardAI guard in guards)
            {
                if (guard != null)
                {
                    guard.ResetGuard();
                }
            }
        }

        // Reset document
        if (secretDocument != null)
        {
            secretDocument.ResetDocument();
        }

        // Hide UI
        HideAllPanels();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public MissionState GetCurrentState()
    {
        return currentState;
    }

    #endregion

    #region Debug

    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Stealth Mission: {currentState}");
        }
    }

    #endregion
}
