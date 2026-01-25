using UnityEngine;

/// <summary>
/// Guard AI hoàn chỉnh - Đã sửa lỗi Vision Detection
/// </summary>
public class GuardAI : MonoBehaviour
{
    [Header("Vision Settings")]
    [Tooltip("Tầm nhìn xa (mét)")]
    public float visionRange = 10f;

    [Tooltip("Góc nhìn ngang (độ)")]
    public float visionAngle = 90f;

    [Tooltip("Layer của vật cản (Tường, Cột...)")]
    public LayerMask obstacleLayer;

    [Tooltip("Layer của Player (Bắt buộc phải set đúng)")]
    public LayerMask playerLayer;

    [Tooltip("Độ cao mắt của Guard (tính từ chân lên)")]
    public float eyeHeight = 1.6f;

    [Tooltip("Độ cao mục tiêu trên người Player (để nhìn vào ngực thay vì chân)")]
    public float playerTargetOffset = 1.2f;

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waypointWaitTime = 1f;
    public float waypointReachedDistance = 0.5f;

    [Header("Detection Settings")]
    public float detectionDelay = 0.5f;

    // Internal state
    private bool isEnabled = false;
    private int currentWaypointIndex = 0;
    private float waypointWaitTimer = 0f;
    private bool isWaiting = false;
    private Transform player;
    private float detectionTimer = 0f;
    private bool playerDetected = false;

    // Starting position for reset
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        // 1. Lưu vị trí gốc để Reset
        startPosition = transform.position;
        startRotation = transform.rotation;

        // 2. Tìm Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError($"[GuardAI] {gameObject.name}: KHÔNG TÌM THẤY PLAYER! Hãy kiểm tra Tag 'Player'.");
        }

        // 3. Tự động kích hoạt nếu GameObject đang bật
        if (gameObject.activeInHierarchy)
        {
            isEnabled = true;
        }
    }

    void Update()
    {
        if (!isEnabled) return;

        // Kiểm tra tầm nhìn
        CheckForPlayer();

        // Nếu chưa phát hiện thì đi tuần
        if (!playerDetected)
        {
            Patrol();
        }
    }

    #region Vision Detection (ĐÃ SỬA LỖI)

    void CheckForPlayer()
    {
        if (player == null) return;

        // A. Tính toán vị trí đầu và cuối của tia nhìn
        Vector3 guardEyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerCenterPos = player.position + Vector3.up * playerTargetOffset; // Nhìn vào ngực Player

        Vector3 directionToPlayer = playerCenterPos - guardEyePos;
        float distanceToPlayer = directionToPlayer.magnitude;

        // B. Kiểm tra cơ bản (Khoảng cách & Góc)
        if (distanceToPlayer > visionRange)
        {
            ResetDetection();
            return;
        }

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle / 2f)
        {
            ResetDetection();
            return;
        }

        // C. Raycast kiểm tra vật cản (Phần quan trọng nhất)
        RaycastHit hit;

        // Gom layer lại: Obstacle + Player
        LayerMask combinedMask = obstacleLayer | playerLayer;

        // Dùng QueryTriggerInteraction.Ignore để nhìn xuyên qua các Trigger vô hình
        if (Physics.Raycast(guardEyePos, directionToPlayer.normalized, out hit, distanceToPlayer, combinedMask, QueryTriggerInteraction.Ignore))
        {
            // Nếu tia Ray bắn trúng Player
            if (hit.collider.CompareTag("Player"))
            {
                // Vẽ tia màu ĐỎ (Nhìn thấy)
                Debug.DrawLine(guardEyePos, hit.point, Color.red);
                OnPlayerInSight();
            }
            else
            {
                // Vẽ tia màu VÀNG (Bị chặn bởi tường/vật cản)
                // Debug.Log($"Guard bị chặn bởi: {hit.collider.name}"); // Bật dòng này nếu cần debug kỹ
                Debug.DrawLine(guardEyePos, hit.point, Color.yellow);
                ResetDetection();
            }
        }
        else
        {
            // Trường hợp hy hữu: Raycast không trúng gì cả trong tầm distance (có thể coi là thấy hoặc không tùy logic)
            // Thường thì nên reset để an toàn
            ResetDetection();
        }
    }

    void OnPlayerInSight()
    {
        detectionTimer += Time.deltaTime;

        // Nếu nhìn thấy đủ lâu (detectionDelay) thì báo động
        if (detectionTimer >= detectionDelay && !playerDetected)
        {
            playerDetected = true;
            OnPlayerDetected();
        }
    }

    void ResetDetection()
    {
        detectionTimer = 0f;
    }

    void OnPlayerDetected()
    {
        Debug.Log($"<color=red>[GuardAI] {gameObject.name} ĐÃ PHÁT HIỆN PLAYER!</color>");

        // Gọi sang Manager để báo thua
        if (StealthMissionManager.Instance != null)
        {
            StealthMissionManager.Instance.OnPlayerDetectedByGuard(this);
        }
    }

    #endregion

    #region Patrol System

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (isWaiting)
        {
            waypointWaitTimer -= Time.deltaTime;
            if (waypointWaitTimer <= 0f)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        Transform targetWP = waypoints[currentWaypointIndex];
        if (targetWP == null) return;

        // Chỉ di chuyển trên mặt phẳng ngang (bỏ qua trục Y để không bay lên trời)
        Vector3 targetPos = targetWP.position;
        targetPos.y = transform.position.y;

        Vector3 moveDir = (targetPos - transform.position).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }

        if (Vector3.Distance(transform.position, targetPos) <= waypointReachedDistance)
        {
            isWaiting = true;
            waypointWaitTimer = waypointWaitTime;
        }
    }

    #endregion

    #region Enable/Disable & Reset

    public void EnableGuard()
    {
        isEnabled = true;
        gameObject.SetActive(true);
        // Reset trạng thái khi enable lại
        playerDetected = false;
        detectionTimer = 0f;
    }

    public void DisableGuard()
    {
        isEnabled = false;
        gameObject.SetActive(false);
    }

    public void ResetGuard()
    {
        Debug.Log($"[GuardAI] Resetting {gameObject.name}");
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentWaypointIndex = 0;
        isWaiting = false;
        waypointWaitTimer = 0f;
        playerDetected = false;
        detectionTimer = 0f;

        // Đảm bảo bật lại guard nếu nó đang bị tắt
        EnableGuard();
    }

    #endregion

    #region Debug Gizmos

    void OnDrawGizmos()
    {
        if (!isEnabled && !Application.isPlaying) return; // Chỉ vẽ khi active hoặc trong edit mode

        // Vẽ hình nón tầm nhìn
        Gizmos.color = playerDetected ? Color.red : new Color(1f, 1f, 0f, 0.3f);
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Vector3 leftRay = Quaternion.AngleAxis(-visionAngle / 2, Vector3.up) * transform.forward * visionRange;
        Vector3 rightRay = Quaternion.AngleAxis(visionAngle / 2, Vector3.up) * transform.forward * visionRange;

        Gizmos.DrawRay(eyePos, leftRay);
        Gizmos.DrawRay(eyePos, rightRay);

        // Vẽ đường nối Waypoints
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            if (waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }

    #endregion
}