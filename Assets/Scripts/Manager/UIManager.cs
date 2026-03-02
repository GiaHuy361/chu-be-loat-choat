using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("1. Left Mission Panel")]
    public GameObject leftMissionPanel; // (optional) kéo Left_MissionPanel để hide khi fail/end
    public TextMeshProUGUI txtMissionName;
    public TextMeshProUGUI txtMissionDetail;
    public TextMeshProUGUI txtDistance;

    [Header("2. Top Notification")]
    public GameObject topNotifPanel;
    public TextMeshProUGUI txtTopNotif;

    [Header("3. Bottom Dialogue")]
    public GameObject bottomDialoguePanel;
    public TextMeshProUGUI txtBottomDialogue;

    [Header("4. Screen Effects")]
    public Image blackScreen;
    public ParticleSystem winFirework;

    [Header("5. Interact Prompt")]
    public GameObject interactPanel;
    public TextMeshProUGUI txtInteractPrompt;

    [Header("6. NEW: Fail Panel")]
    public GameObject failPanel;            // Panel chứa nút Retry/Hub
    public TextMeshProUGUI txtFailReason;   // Text lý do fail (optional)

    [Header("7. NEW: End Panel")]
    public GameObject endPanel;             // Panel chứa nút Replay/Hub
    public TextMeshProUGUI txtEndMessage;   // Text kết thúc (optional)

    private bool isShowingResult = false;

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
        else txtDistance.text = "Khoảng cách: " + Mathf.RoundToInt(distance) + "m";
    }

    public void ShowSystemDialogue(string message, float duration = 3f)
    {
        if (isShowingResult) return;

        StopCoroutine(nameof(TempDialogueCoroutine));
        StartCoroutine(TempDialogueCoroutine(message, duration));
    }

    private IEnumerator TempDialogueCoroutine(string message, float duration)
    {
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(true);
        if (txtBottomDialogue != null) txtBottomDialogue.text = message;

        yield return new WaitForSeconds(duration);

        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
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
    {
        if (isShowingResult) return;

        StopAllCoroutines();
        if (topNotifPanel != null) topNotifPanel.SetActive(true);
        if (txtTopNotif != null) txtTopNotif.text = "<color=yellow>MISSION COMPLETE</color>";
        if (winFirework != null) winFirework.Play();
    }

    // ===== FAIL FLOW (FIX: KHÔNG TRÙNG TEXT) =====
    public void ShowLoseUI(string loseMessage)
    {
        if (isShowingResult) return;
        isShowingResult = true;

        StopAllCoroutines();

        // Ẩn UI phụ để khỏi rối (KHÔNG XOÁ, CHỈ TẮT)
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false); // <- FIX TRÙNG
        if (interactPanel != null) interactPanel.SetActive(false);
        if (leftMissionPanel != null) leftMissionPanel.SetActive(false); // optional

        // Top header (giữ nếu bạn thích)
        if (topNotifPanel != null) topNotifPanel.SetActive(true);
        if (txtTopNotif != null) txtTopNotif.text = "<color=red>MISSION FAILED</color>";

        // Chỉ hiện 1 chỗ: FailPanel text
        if (txtFailReason != null)
        {
            // Bạn muốn đổi text thì sửa tại đây (hoặc set trực tiếp trong Inspector)
            txtFailReason.text = loseMessage;
        }

        StartCoroutine(FadeToBlackThenShowFail());
    }

    private IEnumerator FadeToBlackThenShowFail()
    {
        yield return Fade(1f, 0.6f);

        if (failPanel != null) failPanel.SetActive(true);

        if (StealthMissionManager.Instance != null)
            StealthMissionManager.Instance.UnlockCursor();
    }

    // ===== END FLOW =====
    public void ShowEndUI(string endMessage)
    {
        if (isShowingResult) return;
        isShowingResult = true;

        StopAllCoroutines();

        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
        if (interactPanel != null) interactPanel.SetActive(false);
        if (leftMissionPanel != null) leftMissionPanel.SetActive(false);

        if (txtEndMessage != null) txtEndMessage.text = endMessage;

        StartCoroutine(FadeToBlackThenShowEnd());
    }

    private IEnumerator FadeToBlackThenShowEnd()
    {
        yield return Fade(1f, 1.0f);

        if (endPanel != null) endPanel.SetActive(true);

        if (StealthMissionManager.Instance != null)
            StealthMissionManager.Instance.UnlockCursor();
    }

    public IEnumerator Fade(float targetAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        float startAlpha = blackScreen.color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            Color c = blackScreen.color;
            c.a = a;
            blackScreen.color = c;
            yield return null;
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPanel != null && interactPanel.activeSelf) interactPanel.SetActive(false);
    }

    public void ShowInteractPrompt(string promptMessage)
    {
        if (isShowingResult) return;

        if (interactPanel != null && !interactPanel.activeSelf) interactPanel.SetActive(true);
        if (txtInteractPrompt != null) txtInteractPrompt.text = promptMessage;
    }

    // ===== Buttons =====
    public void OnClickRetry()
    {
        if (failPanel != null) failPanel.SetActive(false);
        if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.RetryCurrentScene();
    }

    public void OnClickBackToHub()
    {
        if (failPanel != null) failPanel.SetActive(false);
        if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.BackToHub();
    }

    public void OnClickReplayDemo()
    {
        if (endPanel != null) endPanel.SetActive(false);
        if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.RetryCurrentScene();
    }

    public void OnClickExitToHub()
    {
        if (endPanel != null) endPanel.SetActive(false);
        if (StealthMissionManager.Instance != null) StealthMissionManager.Instance.BackToHub();
    }
}