using UnityEngine;

public class SecretDocument : MonoBehaviour, IInteractable
{
    [Header("Cài đặt Tài Liệu")]
    [Tooltip("Đánh dấu tích nếu đây là bức thư THẬT. Bỏ tick nếu là thư GIẢ.")]
    public bool isCorrectLetter = true;

    public string GetInteractPrompt()
    {
        return "Kiểm tra Tài Liệu [E]"; // Đổi chữ một chút cho hợp hoàn cảnh lục lọi
    }

    public void OnInteract()
    {
        // Tắt chữ E trên màn hình ngay lập tức
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideInteractPrompt();
        }

        // --- PHÁT ÂM THANH NHẶT ĐỒ Ở ĐÂY ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayItemPickup();
        }

        // KIỂM TRA XEM ĐÂY LÀ THƯ THẬT HAY GIẢ
        if (isCorrectLetter)
        {
            // --- NẾU LÀ THƯ THẬT ---
            if (StealthMissionManager.Instance != null)
            {
                StealthMissionManager.Instance.OnLetterPickedUp();
            }
        }
        else
        {
            // --- NẾU LÀ THƯ GIẢ ---
            if (UIManager.Instance != null)
            {
                // Tạo vài câu thoại ngẫu nhiên cho chân thực
                string[] fakeMessages = {
                    "Kim Đồng: 'Chỉ là giấy tờ sổ sách bình thường, không phải mật thư...'",
                    "Kim Đồng: 'Thư gửi về gia đình của lính địch. Phải tìm chỗ khác thôi.'",
                    "Kim Đồng: 'Không có thông tin tình báo gì ở đây cả.'"
                };
                string randomMsg = fakeMessages[Random.Range(0, fakeMessages.Length)];

                // Hiện lên bảng Bottom_DialoguePanel trong 3 giây
                UIManager.Instance.ShowSystemDialogue(randomMsg, 3f);
            }
        }

        // Dù là thật hay giả thì kiểm tra xong cũng cất đi (ẩn object) để không bị bấm nhầm lại
        gameObject.SetActive(false);
    }
}