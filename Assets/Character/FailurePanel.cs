using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Failure Panel - "Thôi rồi lượm ơi!"
/// Hiển thị khi bị guard phát hiện
/// </summary>
public class FailurePanel : MonoBehaviour
{
    [Header("UI References")]
    public Button retryButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image backgroundImage;

    [Header("Content")]
    public string failureTitle = "THÔI RỒI LƯỢM ƠI!";
    [TextArea]
    public string failureMessage = "Bạn đã bị huấn luyện viên phát hiện!\n\nHãy cẩn thận hơn lần sau.";

    [Header("Settings")]
    public bool pauseGameWhenShown = true;
    public Color backgroundColor = new Color(0.1f, 0f, 0f, 0.8f);

    [Header("Animation")]
    public bool useAnimation = true;
    public float fadeInDuration = 0.3f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && useAnimation)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        // Setup button
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        UpdateContent();
    }

    void OnEnable()
    {
        UpdateContent();
        
        // Pause game
        if (pauseGameWhenShown)
        {
            Time.timeScale = 0f;
        }

        // Fade in animation
        if (useAnimation && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        // Resume game
        if (pauseGameWhenShown)
        {
            Time.timeScale = 1f;
        }
    }

    void UpdateContent()
    {
        if (titleText != null)
        {
            titleText.text = failureTitle;
        }

        if (messageText != null)
        {
            messageText.text = failureMessage;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }

    System.Collections.IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void OnRetryClicked()
    {
        Debug.Log("[FailurePanel] Retry button clicked");
        
        // Đóng panel
        gameObject.SetActive(false);

        // Restart mission
        var missionManager = StealthMissionManager.Instance;
        if (missionManager != null)
        {
            missionManager.RestartMission();
        }

        // Lock cursor lại
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
