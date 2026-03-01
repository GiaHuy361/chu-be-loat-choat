using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
    public static PlayerRoot Instance;

    private void Awake()
    {
        // Singleton Pattern: Đảm bảo chỉ có 1 Player tồn tại
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Hủy nhân vật mới nếu đã có nhân vật cũ
            return;
        }

        Instance = this;

        // [ĐÃ SỬA] Xóa/Comment dòng này lại!
        // Giờ đây khi Load Scene, nhân vật cũ sẽ chết đi, 
        // và PlayerSpawner sẽ lo việc đẻ ra một nhân vật mới tinh ở cửa hang.
        // DontDestroyOnLoad(gameObject); 
    }
}