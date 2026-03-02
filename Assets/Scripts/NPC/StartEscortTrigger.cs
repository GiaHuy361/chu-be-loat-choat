using UnityEngine;

public class StartEscortTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (StealthMissionManager.Instance != null)
            StealthMissionManager.Instance.Mission3_StartEscort();

        gameObject.SetActive(false);
    }
}