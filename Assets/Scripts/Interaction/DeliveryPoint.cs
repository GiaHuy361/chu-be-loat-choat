using UnityEngine;

public class DeliveryPoint : MonoBehaviour, IInteractable
{
    // Cờ khóa tạm thời để tránh bấm E liên tục khi đang chuyển cảnh
    private bool isProcessing = false;

    public string GetInteractPrompt()
    {
        // Trả về chuỗi rỗng để PlayerInteraction tự động ẩn Panel
        if (StealthMissionManager.Instance == null || isProcessing) return "";

        // Nếu game đã xong -> Ẩn vĩnh viễn
        if (StealthMissionManager.Instance.currentPhase == StealthMissionManager.MissionPhase.Completed)
            return "";

        if (StealthMissionManager.Instance.currentState == StealthMissionManager.MissionState.Delivering)
        {
            return "Giao Thư [E]";
        }
        else
        {
            return "<color=red>Chưa có mật thư!</color>";
        }
    }

    public void OnInteract()
    {
        if (isProcessing) return; // Đang chuyển cảnh thì cấm bấm

        if (StealthMissionManager.Instance != null &&
            StealthMissionManager.Instance.currentState == StealthMissionManager.MissionState.Delivering)
        {
            isProcessing = true; // Bật khóa
            StealthMissionManager.Instance.OnDelivered();

            // --- PHÁT ÂM THANH GIAO THƯ Ở ĐÂY ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayItemPickup();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractPrompt();
            }

            // Mở khóa lại sau 3 giây (để sẵn sàng cho màn đêm)
            Invoke("ResetProcessing", 3f);
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowTopNotification("Bạn cần tìm mật thư trước!");
        }
    }

    void ResetProcessing()
    {
        isProcessing = false;
    }
}