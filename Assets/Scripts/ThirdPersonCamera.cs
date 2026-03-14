using UnityEngine;
using UnityEngine.SceneManagement;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Orbit Settings")]
    public float distance = 5f;
    public float height = 1.5f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.2f;
    public float collisionOffset = 0.1f;
    public float minDistance = 0.5f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;
    public float rotationSmooth = 12f;

    [Header("Pause UI")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public string menuScene = "MainMenu";

    float yaw;
    float pitch;

    Vector3 currentVelocity;
    Vector3 smoothPosition;

    bool isPaused = false;
    bool isAltHeld = false;

    void Start()
    {
        if (!target) return;

        LockCursor();

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

        smoothPosition = transform.position;

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (!isPaused)
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                isAltHeld = true;
                UnlockCursor();
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                isAltHeld = false;
                LockCursor();
            }
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        if (!isPaused && !isAltHeld)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 pivot = target.position + Vector3.up * height;
        Vector3 direction = targetRotation * -Vector3.forward;

        float finalDistance = distance;

        RaycastHit hit;

        if (Physics.SphereCast(pivot, collisionRadius, direction, out hit, distance, collisionLayers))
        {
            float distanceToWall = hit.distance - collisionOffset;
            finalDistance = Mathf.Max(distanceToWall, minDistance);
        }

        Vector3 desiredPosition = pivot + direction * finalDistance;

        smoothPosition = Vector3.SmoothDamp(
            smoothPosition,
            desiredPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        transform.position = smoothPosition;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;

            if (pausePanel)
                pausePanel.SetActive(true);

            UnlockCursor();
        }
        else
        {
            ResumeGame();
        }
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ NÚT BẤM UI (ĐÃ THÊM ÂM THANH)
    // ==========================================

    public void ResumeGame()
    {
        if (AudioManager.Instance != null && isPaused) AudioManager.Instance.PlayButtonClickSound();

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);

        if (!isAltHeld)
            LockCursor();
    }

    public void OpenSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        if (pausePanel) pausePanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();

        if (settingsPanel) settingsPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    // Thêm hàm Apply Settings để dùng cho bảng cài đặt âm lượng
    public void ApplySettingsButton()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
            AudioManager.Instance.ApplySettings();
        }

        if (settingsPanel) settingsPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();

        // Chạy hiệu ứng mờ nhạc và tải lại cảnh chơi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
            AudioManager.Instance.GoToGameplay(scene.name);
        }
        else
        {
            SceneManager.LoadScene(scene.name);
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        // Chạy hiệu ứng chuyển cảnh và đổi sang nhạc Menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.GoToMenu(menuScene);
        }
        else
        {
            SceneManager.LoadScene(menuScene);
        }
    }

    // ==========================================
    // QUẢN LÝ CHUỘT
    // ==========================================

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}