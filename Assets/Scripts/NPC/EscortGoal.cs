using UnityEngine;

public class EscortGoal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // WIN khi NPC hộ tống vào goal
        if (!other.CompareTag("EscortNPC")) return;

        if (StealthMissionManager.Instance != null &&
            StealthMissionManager.Instance.currentPhase == StealthMissionManager.MissionPhase.Mission3_Escort &&
            StealthMissionManager.Instance.mission3_EscortActive)
        {
            StealthMissionManager.Instance.Mission3_Win();
        }
    }
}