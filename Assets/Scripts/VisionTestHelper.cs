using UnityEngine;

/// <summary>
/// Simple runtime test cho guard vision
/// Attach vào bất kỳ GameObject nào
/// </summary>
public class VisionTestHelper : MonoBehaviour
{
    void Update()
    {
        // Nhấn V để test vision
        if (Input.GetKeyDown(KeyCode.V))
        {
            TestAllGuards();
        }
    }

    void TestAllGuards()
    {
        var guards = Object.FindObjectsByType<GuardAI>(FindObjectsSortMode.None);
        var player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("❌ PLAYER NOT FOUND! Check Player tag!");
            return;
        }

        Debug.Log($"=== VISION TEST (Press V) ===");
        Debug.Log($"Player: {player.name} at {player.transform.position}");
        Debug.Log($"Guards found: {guards.Length}\n");

        foreach (var guard in guards)
        {
            Vector3 dirToPlayer = player.transform.position - guard.transform.position;
            float distance = dirToPlayer.magnitude;
            float angle = Vector3.Angle(guard.transform.forward, dirToPlayer);

            Debug.Log($"--- {guard.name} ---");
            Debug.Log($"  Position: {guard.transform.position}");
            Debug.Log($"  Distance to player: {distance:F1}m");
            Debug.Log($"  Angle to player: {angle:F1}°");
            Debug.Log($"  Guard forward: {guard.transform.forward}");
            Debug.Log($"  Active: {guard.gameObject.activeInHierarchy}");
            Debug.Log($"  Enabled: {guard.enabled}");

            // Check có thấy không
            bool inRange = distance <= 10f; // Default vision range
            bool inAngle = angle <= 45f; // Half of 90° cone
            
            if (inRange && inAngle)
            {
                Debug.Log($"  ✅ SHOULD SEE PLAYER!");
            }
            else
            {
                if (!inRange) Debug.Log($"  ❌ TOO FAR (max 10m)");
                if (!inAngle) Debug.Log($"  ❌ OUT OF ANGLE (max 45°)");
            }
            Debug.Log("");
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 50, 300, 20), "Press V to test guard vision");
    }
}
