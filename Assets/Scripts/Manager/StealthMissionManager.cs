using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StealthMissionManager : MonoBehaviour
{
    public static StealthMissionManager Instance;

    public enum MissionPhase { Day_Cave, Night_Outpost, Mission3_Escort, Completed }
    public enum MissionState { FindingLetter, Delivering }

    [Header("Trạng thái hiện tại")]
    public MissionPhase currentPhase = MissionPhase.Day_Cave;
    public MissionState currentState = MissionState.FindingLetter;
    public string hubSceneName = "Demo_Terrain";

    [Header("Cấu hình Mục Tiêu & Vị trí")]
    public Transform playerTransform;
    public Transform letter1_Cave;
    public Transform[] outpostSearchAreas;
    public Transform deliveryLocation;
    public Transform outpostSpawnPoint;

    [Header("Môi trường Ngày/Đêm")]
    public Light sunLight;
    public Material nightSkybox;
    public Color nightColor = new Color(0.1f, 0.15f, 0.3f);
    public float nightIntensity = 0.2f;

    [Header("NV3 - Night Override")]
    public bool mission3_UseNightOverride = true;
    public Material mission3_NightSkybox;
    public Color mission3_NightColor = new Color(0.05f, 0.08f, 0.18f);
    public float mission3_NightIntensity = 0.12f;

    [Header("Dữ liệu La bàn")]
    public Transform currentObjective;
    public bool isInsideSearchArea = false;

    [Header("NHIỆM VỤ 3 - Escort")]
    public GameObject mission3_Group;
    public Transform mission3_EscortNPC;
    public Transform mission3_Goal;
    public Transform mission3_StartPoint;
    [HideInInspector] public bool mission3_EscortActive = false;
    public bool mission3_AutoStartEscortOnEnter = true;
    public float mission3_ReminderInterval = 18f;

    // ================= GIỌNG NÓI CỦA KIM ĐỒNG (VOICE CLIPS) =================
    [Header("Voice Clips - Kim Đồng")]
    public AudioClip v_StartPhase1;
    public AudioClip v_PickLetter1;
    public AudioClip v_PickLetter2;
    public AudioClip v_StartPhase2;
    public AudioClip v_StartPhase3;

    public AudioClip v_m3_Brief1;
    public AudioClip v_m3_Brief2;
    public AudioClip v_m3_Approach;
    public AudioClip v_m3_StartEscort1;
    public AudioClip v_m3_StartEscort2;

    public AudioClip v_m3_Reminder1;
    public AudioClip v_m3_Reminder2;
    public AudioClip v_m3_Reminder3;

    public AudioClip v_EndGame;

    // ================= LỜI THOẠI MỚI (PHÁ VỠ BỨC TƯỜNG THỨ 4) =================
    [Header("Kịch bản thoại")]
    [TextArea] public string msg_StartPhase1 = "Kim Đồng: Suỵt! Anh/chị ơi, có một mật thư giấu trong hang đá. Đi theo la bàn để tìm nhé, nhớ cẩn thận lính gác!";
    [TextArea] public string msg_PickLetter1 = "Kim Đồng: Tuyệt quá! Có thư rồi. Giờ anh/chị giúp em luồn lách qua hàng rào địch mang về Làng an toàn nhé.";
    [TextArea] public string msg_StartPhase2 = "Kim Đồng: Đêm xuống rồi... Bọn địch canh gác kỹ lắm. Anh/chị ráng tìm giúp em mật thư thật ở đồn địch nha.";
    [TextArea] public string msg_PickLetter2 = "Kim Đồng: Lấy được mật thư rồi! Mình rút lui ngay thôi kẻo chúng phát hiện!";
    [TextArea] public string msg_StartPhase3 = "Kim Đồng: Xong nhiệm vụ 2 rồi! Bây giờ là lúc quan trọng nhất, mình đi đón cán bộ nhé anh/chị!";

    [TextArea] public string mission3_Brief_1 = "Kim Đồng: Anh/chị ơi, nhiệm vụ này quan trọng lắm. Mình phải đưa cán bộ vượt qua đồn địch trong đêm.";
    [TextArea] public string mission3_Brief_2 = "Kim Đồng: Tuyệt đối không được để lính gác nhìn thấy. Đi thấp người xuống nhé!";
    [TextArea] public string mission3_ApproachNPC_1 = "Kim Đồng: Cán bộ đang nấp ở đằng kia kìa, anh/chị tiến lại gần đi.";
    [TextArea] public string mission3_StartEscort_1 = "Kim Đồng: Bác đi sát theo cháu nhé. Còn anh/chị thì đi trước cảnh giới giúp em!";
    [TextArea] public string mission3_StartEscort_2 = "Cán bộ: Cảm ơn đồng chí. Ta đi thôi!";

    [TextArea] public string mission3_Reminder_1 = "Kim Đồng: Chờ bác ấy một chút anh/chị ơi, đừng đi nhanh quá bác theo không kịp.";
    [TextArea] public string mission3_Reminder_2 = "Kim Đồng: Cẩn thận đèn pha! Đi sát vào vách đá che khuất nhé.";
    [TextArea] public string mission3_Reminder_3 = "Kim Đồng: Bình tĩnh thôi anh/chị... chậm mà chắc.";

    [TextArea] public string mission3WinCongratsMessage = "Kim Đồng: Hoan hô! Chúng ta đưa được cán bộ đến nơi an toàn rồi anh/chị ơi!";
    public float mission3WinCongratsDuration = 3f;

    // ================= INTERNAL FLAGS =================
    private bool isTransitioning = false;
    private bool isFailed = false;
    private Coroutine mission3DialogueRoutine;
    private Coroutine mission3ReminderRoutine;

    // ================= UNITY =================
    void Awake() { if (Instance == null) Instance = this; else { Destroy(gameObject); return; } }

    void Start()
    {
        if (playerTransform == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) playerTransform = p.transform; }
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(true);
        if (outpostSearchAreas != null) { foreach (Transform area in outpostSearchAreas) { if (area != null) area.gameObject.SetActive(false); } }
        if (mission3_Group != null) mission3_Group.SetActive(false);

        StartPhase1();
        LockCursor();
    }

    void Update()
    {
        if (currentPhase == MissionPhase.Completed || playerTransform == null) return;

        if (currentPhase == MissionPhase.Night_Outpost && currentState == MissionState.FindingLetter)
            currentObjective = isInsideSearchArea ? null : GetClosestActiveSearchArea();

        if (currentPhase == MissionPhase.Mission3_Escort)
        {
            if (!mission3_EscortActive) currentObjective = mission3_EscortNPC;
            else currentObjective = mission3_Goal;
        }

        if (UIManager.Instance != null)
        {
            if (currentObjective != null) { float dist = Vector3.Distance(playerTransform.position, currentObjective.position); UIManager.Instance.UpdateDistance(dist); }
            else { UIManager.Instance.UpdateDistance(-1); }
        }
    }

    // ================= HELPERS =================
    Transform GetClosestActiveSearchArea()
    {
        Transform closest = null; float minDistance = float.MaxValue;
        if (outpostSearchAreas == null) return null;
        foreach (Transform area in outpostSearchAreas) { if (area != null && area.gameObject.activeInHierarchy) { float dist = Vector3.Distance(playerTransform.position, area.position); if (dist < minDistance) { minDistance = dist; closest = area; } } }
        return closest;
    }

    void TeleportPlayer(Transform targetPoint) { if (playerTransform == null || targetPoint == null) return; CharacterController cc = playerTransform.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; playerTransform.position = targetPoint.position; playerTransform.rotation = targetPoint.rotation; if (cc != null) cc.enabled = true; }
    void StopMission3Coroutines() { if (mission3DialogueRoutine != null) { StopCoroutine(mission3DialogueRoutine); mission3DialogueRoutine = null; } if (mission3ReminderRoutine != null) { StopCoroutine(mission3ReminderRoutine); mission3ReminderRoutine = null; } }

    // ================= PHASE 1 =================
    void StartPhase1()
    {
        isFailed = false; isTransitioning = false; currentPhase = MissionPhase.Day_Cave; currentState = MissionState.FindingLetter; currentObjective = letter1_Cave;
        StopMission3Coroutines();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Mật Thư Hang Đá", "Lần theo La bàn để tìm mật thư trong hang.");
            UIManager.Instance.ShowSystemDialogue(msg_StartPhase1, 5f, false, v_StartPhase1);
        }
    }

    public void OnLetterPickedUp()
    {
        if (isFailed || isTransitioning) return;
        currentState = MissionState.Delivering; isInsideSearchArea = false; currentObjective = deliveryLocation;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowTopNotification("Đã nhặt: Tài Liệu Mật");
            UIManager.Instance.UpdateMissionPanel("Giao Thư Mật", "Rút lui và giao tài liệu đến Làng an toàn");
            if (currentPhase == MissionPhase.Day_Cave) UIManager.Instance.ShowSystemDialogue(msg_PickLetter1, 5f, false, v_PickLetter1);
            else UIManager.Instance.ShowSystemDialogue(msg_PickLetter2, 4f, false, v_PickLetter2);
        }
    }

    public void OnDelivered()
    {
        if (isFailed || isTransitioning) return;
        if (currentState != MissionState.Delivering) return;
        if (currentPhase == MissionPhase.Day_Cave) StartCoroutine(TransitionToPhase2());
        else if (currentPhase == MissionPhase.Night_Outpost) StartCoroutine(TransitionToMission3());
    }

    // ================= FAIL =================
    public void FailMission(string reason) { if (isFailed || currentPhase == MissionPhase.Completed) return; isFailed = true; isTransitioning = true; currentObjective = null; StopMission3Coroutines(); if (UIManager.Instance != null) { UIManager.Instance.UpdateDistance(-1); UIManager.Instance.HideInteractPrompt(); UIManager.Instance.ShowLoseUI(reason); } }
    public void RetryCurrentScene() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void BackToHub() { if (string.IsNullOrEmpty(hubSceneName)) SceneManager.LoadScene(SceneManager.GetActiveScene().name); else SceneManager.LoadScene(hubSceneName); }

    // ================= END GAME =================
    IEnumerator EndGameSequence()
    {
        isTransitioning = true; currentPhase = MissionPhase.Completed; currentObjective = null; StopMission3Coroutines();
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue("Kim Đồng: Xong rồi! Cảm ơn anh/chị đã giúp em hoàn thành nhiệm vụ nhé!", 4f, false, v_EndGame);
        yield return new WaitForSeconds(3.0f);
        if (UIManager.Instance != null) { UIManager.Instance.ShowWinUI(); string endText = "Sáng 15/2/1943\nKim Đồng hy sinh khi mới 14 tuổi\nđể bảo vệ cuộc họp của cán bộ Việt Minh."; UIManager.Instance.ShowEndUI(endText); }
        UnlockCursor();
    }

    // ================= TRANSITION PHASE 1 -> 2 =================
    IEnumerator TransitionToPhase2()
    {
        isTransitioning = true;
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue("Hệ thống: Hoàn thành xuất sắc Nhiệm vụ 1! Đang chờ đêm xuống...", 3f);
        if (UIManager.Instance != null) yield return UIManager.Instance.Fade(1f, 1.2f);
        currentPhase = MissionPhase.Night_Outpost; currentState = MissionState.FindingLetter; isInsideSearchArea = false; TeleportPlayer(outpostSpawnPoint);
        if (outpostSearchAreas != null) { foreach (Transform area in outpostSearchAreas) { if (area != null) area.gameObject.SetActive(true); } }
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(false);
        ApplyNightEnvironment(); currentObjective = GetClosestActiveSearchArea();
        if (UIManager.Instance != null) { UIManager.Instance.UpdateMissionPanel("Trinh sát đồn địch", "Tìm kiếm mật thư thật trong các khu vực tình nghi."); UIManager.Instance.ShowTopNotification("Đêm xuống... hãy lén lút hơn!", 3f); yield return UIManager.Instance.Fade(0f, 0.8f); UIManager.Instance.ShowSystemDialogue(msg_StartPhase2, 5f, false, v_StartPhase2); }
        isTransitioning = false; LockCursor();
    }

    // ================= TRANSITION PHASE 2 -> 3 =================
    IEnumerator TransitionToMission3()
    {
        isTransitioning = true;
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(msg_StartPhase3, 3.5f, false, v_StartPhase3);
        if (UIManager.Instance != null) yield return UIManager.Instance.Fade(1f, 0.8f);
        if (outpostSearchAreas != null) { foreach (Transform area in outpostSearchAreas) { if (area != null) area.gameObject.SetActive(false); } }
        currentPhase = MissionPhase.Mission3_Escort; currentState = MissionState.FindingLetter; isInsideSearchArea = false;
        if (mission3_Group != null) mission3_Group.SetActive(true);
        TeleportPlayer(mission3_StartPoint); ApplyMission3Night();
        mission3_EscortActive = false; currentObjective = (mission3_EscortNPC != null) ? mission3_EscortNPC : mission3_Goal;
        if (UIManager.Instance != null) { UIManager.Instance.UpdateMissionPanel("Hộ tống cán bộ", "Tiếp cận cán bộ để bắt đầu dẫn đường tới điểm an toàn."); UIManager.Instance.ShowTopNotification("Nhiệm vụ 3: Hộ tống", 2f); yield return UIManager.Instance.Fade(0f, 0.6f); }
        isTransitioning = false; LockCursor(); StopMission3Coroutines();

        if (mission3_AutoStartEscortOnEnter) Mission3_StartEscort_Internal(true);
        else mission3DialogueRoutine = StartCoroutine(Mission3DialogueFlow());
    }

    IEnumerator Mission3DialogueFlow()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_Brief_1, 4f, false, v_m3_Brief1);
        yield return new WaitForSeconds(4.5f);
        if (currentPhase != MissionPhase.Mission3_Escort || isFailed) yield break;
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_Brief_2, 4f, false, v_m3_Brief2);
        yield return new WaitForSeconds(4.5f);

        while (currentPhase == MissionPhase.Mission3_Escort && !isFailed && !mission3_EscortActive)
        {
            if (UIManager.Instance != null) { UIManager.Instance.ShowTopNotification("👉 Tiếp cận cán bộ", 2f); UIManager.Instance.ShowSystemDialogue(mission3_ApproachNPC_1, 3.5f, false, v_m3_Approach); }
            yield return new WaitForSeconds(10f);
        }
    }

    IEnumerator Mission3ReminderLoop()
    {
        float t = 0f; int idx = 0;
        while (currentPhase == MissionPhase.Mission3_Escort && !isFailed && mission3_EscortActive)
        {
            t += Time.deltaTime;
            if (t >= mission3_ReminderInterval)
            {
                t = 0f;
                if (UIManager.Instance != null)
                {
                    if (idx % 3 == 0) UIManager.Instance.ShowSystemDialogue(mission3_Reminder_1, 3.2f, false, v_m3_Reminder1);
                    else if (idx % 3 == 1) UIManager.Instance.ShowSystemDialogue(mission3_Reminder_2, 3.2f, false, v_m3_Reminder2);
                    else UIManager.Instance.ShowSystemDialogue(mission3_Reminder_3, 3.2f, false, v_m3_Reminder3);
                    idx++;
                }
            }
            yield return null;
        }
    }

    public void Mission3_StartEscort() { Mission3_StartEscort_Internal(false); }

    void Mission3_StartEscort_Internal(bool silent)
    {
        if (currentPhase != MissionPhase.Mission3_Escort) return;
        if (isFailed || isTransitioning) return;
        mission3_EscortActive = true; currentObjective = mission3_Goal;
        if (mission3DialogueRoutine != null) { StopCoroutine(mission3DialogueRoutine); mission3DialogueRoutine = null; }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Hộ tống cán bộ", "Dẫn cán bộ tới điểm an toàn. Tránh vùng phát hiện!");
            UIManager.Instance.ShowTopNotification("✅ Đã bắt đầu hộ tống!", 2f);
            UIManager.Instance.ShowSystemDialogue(mission3_StartEscort_1, 4f, false, v_m3_StartEscort1);
        }
        StartCoroutine(Mission3StartEscortSecondLine());
        if (mission3ReminderRoutine != null) StopCoroutine(mission3ReminderRoutine);
        mission3ReminderRoutine = StartCoroutine(Mission3ReminderLoop());
    }

    IEnumerator Mission3StartEscortSecondLine()
    {
        yield return new WaitForSeconds(4.2f);
        if (currentPhase != MissionPhase.Mission3_Escort || isFailed || !mission3_EscortActive) yield break;
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_StartEscort_2, 4f, false, v_m3_StartEscort2);
    }

    public void Mission3_Win() { if (currentPhase != MissionPhase.Mission3_Escort) return; if (isFailed || isTransitioning) return; StartCoroutine(Mission3WinSequence()); }

    IEnumerator Mission3WinSequence()
    {
        isTransitioning = true; StopMission3Coroutines();
        if (UIManager.Instance != null) { UIManager.Instance.ShowSystemDialogue(mission3WinCongratsMessage, mission3WinCongratsDuration); UIManager.Instance.ShowTopNotification("🎉 HOÀN THÀNH NHIỆM VỤ 3!", 2f); }
        yield return new WaitForSeconds(mission3WinCongratsDuration);
        yield return StartCoroutine(EndGameSequence());
    }

    void ApplyNightEnvironment() { if (sunLight != null) { sunLight.color = nightColor; sunLight.intensity = nightIntensity; } if (nightSkybox != null) { RenderSettings.skybox = nightSkybox; DynamicGI.UpdateEnvironment(); } }
    void ApplyMission3Night() { if (!mission3_UseNightOverride) { ApplyNightEnvironment(); return; } if (sunLight != null) { sunLight.color = mission3_NightColor; sunLight.intensity = mission3_NightIntensity; } Material sb = mission3_NightSkybox != null ? mission3_NightSkybox : nightSkybox; if (sb != null) { RenderSettings.skybox = sb; DynamicGI.UpdateEnvironment(); } }
    public void LockCursor() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    public void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}