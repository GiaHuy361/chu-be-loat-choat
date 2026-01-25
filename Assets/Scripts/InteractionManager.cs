using UnityEngine;
using TMPro; // Dùng TextMeshPro

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Kéo UI vào ?ây")]
    public GameObject messagePanel; // Cái khung ch?a ch?
    public TextMeshProUGUI messageText; // Dòng ch?

    private void Awake()
    {
        // T?o Singleton ?? g?i t? b?t k? ?âu
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideMessage(); // ?n ngay khi game b?t ??u
    }

    public void ShowMessage(string content)
    {
        messageText.text = content;
        messagePanel.SetActive(true);
    }

    public void HideMessage()
    {
        messagePanel.SetActive(false);
    }
}