using UnityEngine;

public class GuardDetectNPC : MonoBehaviour
{
    [TextArea] public string failReason = "Bị phát hiện! Nhiệm vụ hộ tống thất bại.";

    private void OnTriggerEnter(Collider other)
    {
        if (StealthMissionManager.Instance == null) return;
        if (StealthMissionManager.Instance.currentPhase != StealthMissionManager.MissionPhase.Mission3_Escort) return;
        if (!StealthMissionManager.Instance.mission3_EscortActive) return;

        // FAIL nếu Player hoặc NPC hộ tống bị phát hiện
        if (other.CompareTag("Player") || other.CompareTag("EscortNPC"))
        {
            StealthMissionManager.Instance.FailMission(failReason);
        }
    }
}