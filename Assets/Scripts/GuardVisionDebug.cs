using UnityEngine;

/// <summary>
/// Visual debug helper cho GuardAI vision
/// Attach vào Guard để visualize vision cone trong game view
/// </summary>
public class GuardVisionDebug : MonoBehaviour
{
    public GuardAI guard;
    public bool showVisionCone = true;
    public bool showRaycast = true;
    public Color visionConeColor = new Color(1f, 0f, 0f, 0.3f);
    public Color detectedColor = Color.red;

    private Transform player;

    void Start()
    {
        if (guard == null)
        {
            guard = GetComponent<GuardAI>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void OnDrawGizmos()
    {
        if (!showVisionCone || guard == null) return;

        // Get vision settings via reflection
        var visionRangeField = typeof(GuardAI).GetField("visionRange", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var visionAngleField = typeof(GuardAI).GetField("visionAngle", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (visionRangeField == null || visionAngleField == null) return;

        float visionRange = (float)visionRangeField.GetValue(guard);
        float visionAngle = (float)visionAngleField.GetValue(guard);

        //Draw vision cone
        Gizmos.color = visionConeColor;
        
        Vector3 forward = transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;

        Gizmos.DrawLine(eyePos, eyePos + forward);
        Gizmos.DrawLine(eyePos, eyePos + rightBoundary);
        Gizmos.DrawLine(eyePos, eyePos + leftBoundary);

        // Draw arc
        int segments = 20;
        Vector3 previousPoint = eyePos + rightBoundary;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -visionAngle / 2f + (visionAngle * i / segments);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = eyePos + direction;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Draw raycast to player
        if (showRaycast && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(eyePos, player.position);
            Gizmos.DrawWireSphere(player.position, 0.5f);
        }
    }

    void Update()
    {
        // Runtime debug info
        if (Input.GetKeyDown(KeyCode.G))
        {
            ShowDebugInfo();
        }
    }

    void ShowDebugInfo()
    {
        if (player == null || guard == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        Debug.Log($"=== GUARD VISION DEBUG: {gameObject.name} ===");
        Debug.Log($"Distance to player: {distance:F2}m");
        Debug.Log($"Angle to player: {angle:F1}°");
        Debug.Log($"Player tag: {player.tag}");
        Debug.Log($"Guard forward: {transform.forward}");
        Debug.Log($"Direction to player: {directionToPlayer.normalized}");
    }
}
