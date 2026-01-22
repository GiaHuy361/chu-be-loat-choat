using UnityEngine;

/// <summary>
/// Debug helper cho GuardAI
/// Right-click component → "Check Guard Status"
/// </summary>
public class GuardDebugHelper : MonoBehaviour
{
    [ContextMenu("Check Guard Status")]
    public void CheckStatus()
    {
        var guards = Object.FindObjectsByType<GuardAI>(FindObjectsSortMode.None);
        
        Debug.Log("=== GUARD DEBUG CHECK ===");
        Debug.Log($"Total guards found: {guards.Length}");
        
        foreach (var guard in guards)
        {
            Debug.Log($"\n--- {guard.gameObject.name} ---");
            Debug.Log($"  GameObject Active: {guard.gameObject.activeInHierarchy}");
            Debug.Log($"  Component Enabled: {guard.enabled}");
            
            // Check waypoints using reflection
            var waypointsField = typeof(GuardAI).GetField("waypoints", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (waypointsField != null)
            {
                var waypoints = waypointsField.GetValue(guard) as Transform[];
                if (waypoints != null && waypoints.Length > 0)
                {
                    Debug.Log($"  Waypoints: {waypoints.Length} assigned");
                }
                else
                {
                    Debug.LogWarning($"  Waypoints: NONE! Guard won't patrol.");
                }
            }
        }
        
        Debug.Log("=== END DEBUG CHECK ===");
    }

    [ContextMenu("Force Enable All Guards")]
    public void ForceEnableAllGuards()
    {
        var guards = Object.FindObjectsByType<GuardAI>(FindObjectsSortMode.None);
        
        foreach (var guard in guards)
        {
            guard.EnableGuard();
        }
        
        Debug.Log($"[GuardDebugHelper] Enabled {guards.Length} guards");
    }
}
