using UnityEngine;

public class SearchAreaTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && StealthMissionManager.Instance.currentState == StealthMissionManager.MissionState.FindingLetter)
        {
            StealthMissionManager.Instance.isInsideSearchArea = true;
            UIManager.Instance.ShowTopNotification("Bạn đã vào Khu Vực Tình Nghi");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && StealthMissionManager.Instance.currentState == StealthMissionManager.MissionState.FindingLetter)
        {
            StealthMissionManager.Instance.isInsideSearchArea = false;
        }
    }
}