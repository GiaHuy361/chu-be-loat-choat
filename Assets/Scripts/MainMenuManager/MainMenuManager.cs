using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene vào game")]
    public string gameplayScene = "Demo_Terrain";

    [Header("UI Panels")]
    public GameObject menuPanel; // Panel chính chứa các nút PLAY, SETTINGS, QUIT
    public GameObject settingsPanel; // Đã đổi từ guidePanel thành settingsPanel

    void Start()
    {
        // Mở khóa và hiện chuột để người chơi bấm menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        // Đảm bảo panel cài đặt bị ẩn khi mới mở game
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Gắn hàm này vào sự kiện OnClick() của nút PLAY
    public void StartGame()
    {
        // Khóa chuột và ẩn chuột đi để chuẩn bị vào game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Application.CanStreamedLevelBeLoaded(gameplayScene))
        {
            // Thay vì tự load scene ngay lập tức, gọi AudioManager để nó chạy Coroutine Fade nhạc rồi tự chuyển cảnh
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.GoToGameplay(gameplayScene);
            }
            else
            {
                // Fallback trong trường hợp bạn lỡ xóa mất AudioManager lúc test
                SceneManager.LoadScene(gameplayScene);
            }
        }
        else
        {
            Debug.LogError($"MainMenu: Scene '{gameplayScene}' chưa add vào Build Settings!");
        }
    }

    // Gắn hàm này vào sự kiện OnClick() của nút SETTINGS
    public void OpenSettings()
    {
        // Gọi âm thanh click thông qua Singleton
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        if (settingsPanel != null)
        {
            menuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
    }

    // Gắn hàm này vào sự kiện OnClick() của nút BACK/APPLY trong bảng Settings
    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            menuPanel.SetActive(true);
        }
    }

    public void ApplySettingsButton()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
            AudioManager.Instance.ApplySettings(); // Lệnh này sẽ lưu mức âm lượng vào PlayerPrefs
            Debug.Log("Đã lưu cài đặt âm thanh!");
        }

        // Tùy chọn: Nếu bạn muốn bấm Apply xong mà bảng Settings vẫn mở thì XÓA 4 dòng dưới đi.
        // Còn nếu muốn bấm Apply xong tự động văng ra Menu luôn thì giữ lại:
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            menuPanel.SetActive(true);
        }
    }

    // Gắn hàm này vào sự kiện OnClick() của nút QUIT
    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        Debug.Log("Quit Game (chỉ hoạt động khi Build)");
        Application.Quit();
    }
}