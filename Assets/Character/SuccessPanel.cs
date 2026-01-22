using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Success Panel - "Nhiệm vụ hoàn thành!"
/// Hiển thị khi hoàn thành mission
/// </summary>
public class SuccessPanel : MonoBehaviour
{
    [Header("UI References")]
    public Button continueButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image backgroundImage;

    [Header("Content")]
    public string successTitle = "NHIỆM VỤ HOÀN THÀNH!";
    [TextArea]
    public string successMessage = "Xuất sắc! Bạn đã hoàn thành nhiệm vụ lén lút!\n\nBạn đã thành công lấy được mật thư và giao lại cho huấn luyện viên.";

    [Header("Settings")]
    public bool pauseGameWhenShown = true;
    public Color backgroundColor = new Color(0f, 0.2f, 0f, 0.8f);

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
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
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
            titleText.text = successTitle;
        }

        if (messageText != null)
        {
            messageText.text = successMessage;
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

    public void OnContinueClicked()
    {
        Debug.Log("[SuccessPanel] Continue button clicked");
        
        // Đóng panel
        gameObject.SetActive(false);

        // Lock cursor lại
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // TODO: Load next mission hoặc về menu
    }
}
