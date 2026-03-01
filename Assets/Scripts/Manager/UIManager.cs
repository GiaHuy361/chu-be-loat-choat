using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("1. Left Mission Panel")]
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

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (interactPanel != null) interactPanel.SetActive(false);
        if (topNotifPanel != null) topNotifPanel.SetActive(false);
        if (bottomDialoguePanel != null) bottomDialoguePanel.SetActive(false);
    }

    public void UpdateMissionPanel(string missionName, string detail)
    {
        if (txtMissionName != null) txtMissionName.text = "Nhiệm vụ: " + missionName;
        if (txtMissionDetail != null) txtMissionDetail.text = "- " + detail;
    }

    public void UpdateDistance(float distance)
    {
        if (txtDistance == null) return;
        if (distance < 0) txtDistance.text = "";
        else txtDistance.text = "Khoảng cách: " + Mathf.RoundToInt(distance) + "m";
    }

    public void ShowSystemDialogue(string message, float duration = 3f)
    {
        StopCoroutine("TempDialogueCoroutine");
        StartCoroutine(TempDialogueCoroutine(message, duration));
    }

    private IEnumerator TempDialogueCoroutine(string message, float duration)
    {
        bottomDialoguePanel.SetActive(true);
        txtBottomDialogue.text = message;
        yield return new WaitForSeconds(duration);
        bottomDialoguePanel.SetActive(false);
    }

    public void ShowTopNotification(string message, float duration = 3f)
    {
        StartCoroutine(TempTopNotifCoroutine(message, duration));
    }

    private IEnumerator TempTopNotifCoroutine(string message, float duration)
    {
        topNotifPanel.SetActive(true);
        txtTopNotif.text = message;
        yield return new WaitForSeconds(duration);
        topNotifPanel.SetActive(false);
    }

    public void ShowWinUI()
    {
        StopAllCoroutines();
        topNotifPanel.SetActive(true);
        txtTopNotif.text = "<color=yellow>MISSION COMPLETE</color>";

        if (winFirework != null) winFirework.Play();
    }

    public void ShowLoseUI(string loseMessage)
    {
        StopAllCoroutines();
        topNotifPanel.SetActive(true);
        txtTopNotif.text = "<color=red>MISSION FAILED</color>";

        bottomDialoguePanel.SetActive(true);
        txtBottomDialogue.text = loseMessage;

        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack()
    {
        float timer = 0f;
        while (timer < 2f)
        {
            timer += Time.deltaTime;
            Color c = blackScreen.color;
            c.a = Mathf.Lerp(0, 1, timer / 2f);
            blackScreen.color = c;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        StealthMissionManager.Instance.RestartLevel();
    }

    public void ShowInteractPrompt(string promptMessage)
    {
        if (interactPanel != null && !interactPanel.activeSelf) interactPanel.SetActive(true);
        if (txtInteractPrompt != null) txtInteractPrompt.text = promptMessage;
    }

    public void HideInteractPrompt()
    {
        if (interactPanel != null && interactPanel.activeSelf) interactPanel.SetActive(false);
    }
}