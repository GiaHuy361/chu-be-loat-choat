using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("UI References (Kéo các thành phần của Bảng Hội Thoại vào đây)")]
    public GameObject dialoguePanel;      // Cái khung lớn chứa tất cả
    public TextMeshProUGUI dialogueText;  // Lời thoại ông lão
    public TextMeshProUGUI missionText;   // Bảng nhiệm vụ (Nằm bên trái)
    public TextMeshProUGUI closeHintText; // Dòng chữ nhỏ "(Ấn E để đóng)" bên trong bảng

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool isDialogueOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Chỉ tắt cái Panel đi thôi, ĐỪNG xóa nội dung text bên trong
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueOpen) return;

        // Bấm E lần nữa để đóng hội thoại
        if (Input.GetKeyDown(interactKey))
        {
            CloseDialogue();
        }
    }

    public void OpenDialogue(string dialogueLine, string missionLine)
    {
        if (dialoguePanel == null) return;

        isDialogueOpen = true;
        dialoguePanel.SetActive(true); // Bật bảng lên

        // Cập nhật nội dung
        if (dialogueText != null) dialogueText.text = dialogueLine;
        if (missionText != null) missionText.text = $"NHIỆM VỤ MỚI:\n- {missionLine}";

        // Hiện gợi ý cách đóng
        if (closeHintText != null) closeHintText.text = $"(Ấn {interactKey} để đóng)";
    }

    public void CloseDialogue()
    {
        isDialogueOpen = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Trả lại quyền điều khiển cho nhân vật (nếu cần)
        // PlayerMovement.instance.canMove = true; 
    }
}