using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("1. Left Mission Panel")]
    public GameObject leftMissionPanel;
    public TextMeshProUGUI txtMissionName;
    public TextMeshProUGUI txtMissionDetail;
    public TextMeshProUGUI txtDistance;

    [Header("2. Top Notification")]
    public GameObject topNotifPanel;
    public TextMeshProUGUI txtTopNotif;

    [Header("3. Bottom Dialogue (Cập nhật Voice & Skip)")]
    public GameObject bottomDialoguePanel;
    public TextMeshProUGUI txtBottomDialogue;
    [Tooltip("Tốc độ gõ chữ (giây/ký tự)")]
    public float typingSpeed = 0.03f;
    public AudioSource voiceSource; // Kéo AudioSource vào đây để phát giọng nói

    [Header("4. Screen Effects")]
    public Image blackScreen;
    public ParticleSystem winFirework;

    [Header("5. Interact Prompt")]
    public GameObject interactPanel;
    public TextMeshProUGUI txtInteractPrompt;

    [Header("6. NEW: Fail Panel")]
    public GameObject failPanel;
    public TextMeshProUGUI txtFailReason;

    [Header("7. NEW: End Panel")]
    public GameObject endPanel;
    public TextMeshProUGUI txtEndMessage;

    private bool isShowingResult = false;
    private Coroutine dialogueCoroutine;

    // Biến hỗ trợ nút Skip
    private bool isTypingDialogue = false;
    private string currentDialogueFullText = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (interactPanel != null) interactPanel.SetActive(false);
        if (topNotifPanel != null) topNotifPanel.SetActive(false);
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);

        if (failPanel != null) failPanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(false);

        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;
        }
    }

    public void UpdateMissionPanel(string missionName, string detail)
    {
        if (isShowingResult) return;
        if (txtMissionName != null) txtMissionName.text = "Nhiệm vụ: " + missionName;
        if (txtMissionDetail != null) txtMissionDetail.text = "- " + detail;
    }

    public void UpdateDistance(float distance)
    {
        if (isShowingResult) return;
        if (txtDistance == null) return;
        if (distance < 0) txtDistance.text = "";
        else txtDistance.text = Mathf.RoundToInt(distance) + "m";
    }

    // ===== ĐÃ SỬA: Thêm biến AudioClip voiceClip =====
    public void ShowSystemDialogue(string message, float duration = 3f, bool isInstant = false, AudioClip voiceClip = null)
    {
        if (isShowingResult) return;

        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }

        dialogueCoroutine = StartCoroutine(TempDialogueCoroutine(message, duration, isInstant, voiceClip));
    }

    private IEnumerator TempDialogueCoroutine(string message, float duration, bool isInstant, AudioClip voiceClip)
    {
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(true);
        if (txtBottomDialogue != null) txtBottomDialogue.text = "";

        currentDialogueFullText = message;
        isTypingDialogue = true;

        // Phát file âm thanh Voice nếu có
        if (voiceClip != null && voiceSource != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.Play();
        }

        string lowerMsg = message.ToLower();
        bool isUrgent = isInstant || lowerMsg.Contains("bị phát hiện") || lowerMsg.Contains("mau chạy đi") || lowerMsg.Contains("thất bại");

        if (isUrgent)
        {
            if (txtBottomDialogue != null) txtBottomDialogue.text = message;
            isTypingDialogue = false;
        }
        else
        {
            for (int i = 0; i < message.Length; i++)
            {
                if (!isTypingDialogue) break; // Nếu người chơi bấm Skip thì vòng lặp dừng lại
                if (txtBottomDialogue != null) txtBottomDialogue.text += message[i];
                yield return new WaitForSeconds(typingSpeed);
            }
            isTypingDialogue = false;
            if (txtBottomDialogue != null) txtBottomDialogue.text = message; // Đảm bảo hiện full chữ
        }

        yield return new WaitForSeconds(duration);

        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
    }

    // ===== HÀM MỚI CHO NÚT SKIP =====
    public void OnClickSkipDialogue()
    {
        if (isTypingDialogue)
        {
            // Nếu chữ đang chạy -> Cho hiện full dòng ngay lập tức
            isTypingDialogue = false;
            if (txtBottomDialogue != null) txtBottomDialogue.text = currentDialogueFullText;
        }
        else
        {
            // Nếu chữ đã hiện xong nhưng bảng chưa tắt -> Tắt luôn
            if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
            if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
            if (voiceSource != null) voiceSource.Stop();
        }
    }

    public void ShowTopNotification(string message, float duration = 3f)
    {
        if (isShowingResult) return;
        StopCoroutine(nameof(TempTopNotifCoroutine));
        StartCoroutine(TempTopNotifCoroutine(message, duration));
    }

    private IEnumerator TempTopNotifCoroutine(string message, float duration)
    {
        if (topNotifPanel != null) topNotifPanel.SetActive(true);
        if (txtTopNotif != null) txtTopNotif.text = message;
        yield return new WaitForSeconds(duration);
        if (topNotifPanel != null) topNotifPanel.SetActive(false);
    }

    public void ShowWinUI()
    { /* Giữ nguyên như cũ */
        if (isShowingResult) return; StopAllCoroutines();
        if (topNotifPanel != null) topNotifPanel.SetActive(true);
        if (txtTopNotif != null) txtTopNotif.text = "<color=yellow>MISSION COMPLETE</color>";
        if (winFirework != null) winFirework.Play();
    }
    public void ShowLoseUI(string loseMessage)
    { /* Giữ nguyên như cũ */
        if (isShowingResult) return; isShowingResult = true; StopAllCoroutines();
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
        if (interactPanel != null) interactPanel.SetActive(false);
        if (leftMissionPanel != null) leftMissionPanel.SetActive(false);
        if (topNotifPanel != null) topNotifPanel.SetActive(true);
        if (txtTopNotif != null) txtTopNotif.text = "<color=red>MISSION FAILED</color>";
        if (txtFailReason != null) txtFailReason.text = loseMessage;
        StartCoroutine(FadeToBlackThenShowFail());
    }
    private IEnumerator FadeToBlackThenShowFail() { yield return Fade(1f, 0.6f); if (failPanel != null) failPanel.SetActive(true); if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.UnlockCursor(); }
    public void ShowEndUI(string endMessage)
    { /* Giữ nguyên như cũ */
        if (isShowingResult) return; isShowingResult = true; StopAllCoroutines();
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
        if (interactPanel != null) interactPanel.SetActive(false);
        if (leftMissionPanel != null) leftMissionPanel.SetActive(false);
        if (txtEndMessage != null) txtEndMessage.text = endMessage;
        StartCoroutine(FadeToBlackThenShowEnd());
    }
    private IEnumerator FadeToBlackThenShowEnd() { yield return Fade(1f, 1.0f); if (endPanel != null) endPanel.SetActive(true); if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.UnlockCursor(); }
    public IEnumerator Fade(float targetAlpha, float duration)
    { /* Giữ nguyên như cũ */
        if (blackScreen == null) yield break; float startAlpha = blackScreen.color.a; float t = 0f;
        while (t < duration) { t += Time.deltaTime; float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration); Color c = blackScreen.color; c.a = a; blackScreen.color = c; yield return null; }
    }
    public void HideInteractPrompt() { if (interactPanel != null && interactPanel.activeSelf) interactPanel.SetActive(false); }
    public void ShowInteractPrompt(string promptMessage) { if (isShowingResult) return; if (interactPanel != null && !interactPanel.activeSelf) interactPanel.SetActive(true); if (txtInteractPrompt != null) txtInteractPrompt.text = promptMessage; }
    public void OnClickRetry() { if (failPanel != null) failPanel.SetActive(false); if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.RetryCurrentScene(); }
    public void OnClickBackToHub() { if (failPanel != null) failPanel.SetActive(false); if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.BackToHub(); }
    public void OnClickReplayDemo() { if (endPanel != null) endPanel.SetActive(false); if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.RetryCurrentScene(); }
    public void OnClickExitToHub() { if (endPanel != null) endPanel.SetActive(false); if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.BackToHub(); }
}