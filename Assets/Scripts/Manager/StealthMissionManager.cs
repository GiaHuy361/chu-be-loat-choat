using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StealthMissionManager : MonoBehaviour
{
    public static StealthMissionManager Instance;

    // ================= ENUM =================
    public enum MissionPhase { Day_Cave, Night_Outpost, Mission3_Escort, Completed }
    public enum MissionState { FindingLetter, Delivering }

    [Header("Trạng thái hiện tại")]
    public MissionPhase currentPhase = MissionPhase.Day_Cave;
    public MissionState currentState = MissionState.FindingLetter;

    [Header("Scene Names")]
    public string hubSceneName = "Demo_Terrain";

    // ================= OBJECTIVES / PLAYER =================
    [Header("Cấu hình Mục Tiêu & Vị trí")]
    public Transform playerTransform;
    public Transform letter1_Cave;

    [Tooltip("Kéo toàn bộ các vùng Search Area chứa thư (Thật & Giả) ở đồn địch vào đây")]
    public Transform[] outpostSearchAreas;

    public Transform deliveryLocation;
    public Transform outpostSpawnPoint;

    // ================= ENV NIGHT (NV2) =================
    [Header("Môi trường Ngày/Đêm (NV2)")]
    public Light sunLight;
    public Material nightSkybox;
    public Color nightColor = new Color(0.1f, 0.15f, 0.3f);
    public float nightIntensity = 0.2f;

    // ================= ENV NIGHT (NV3 OVERRIDE) =================
    [Header("NV3 - Night Override (xịn hơn)")]
    [Tooltip("Bật để NV3 có màu đêm riêng (đậm hơn NV2). Tắt thì NV3 dùng y hệt NV2.")]
    public bool mission3_UseNightOverride = true;
    public Material mission3_NightSkybox;
    public Color mission3_NightColor = new Color(0.05f, 0.08f, 0.18f);
    public float mission3_NightIntensity = 0.12f;

    // ================= COMPASS DATA =================
    [Header("Dữ liệu La bàn (QuestCompass đọc cái này)")]
    public Transform currentObjective;
    public bool isInsideSearchArea = false;

    // ================= MISSION 3 =================
    [Header("NHIỆM VỤ 3 - Escort (kéo trong Inspector)")]
    public GameObject mission3_Group;          // Mission3_Group
    public Transform mission3_EscortNPC;       // M3_EscortNPC
    public Transform mission3_Goal;            // M3_Goal
    public Transform mission3_StartPoint;      // M3_StartEscortTrigger (điểm teleport/điểm bắt đầu)

    [HideInInspector] public bool mission3_EscortActive = false;

    [Header("NV3 - AUTO START (mới)")]
    [Tooltip("Bật: Vừa vào NV3 là auto bắt đầu hộ tống ngay (khỏi chạm trigger/E).")]
    public bool mission3_AutoStartEscortOnEnter = true;

    [Header("NV3 - Lời thoại (nhiều hơn)")]
    [Tooltip("Thời gian giữa các câu nhắc khi đang hộ tống")]
    public float mission3_ReminderInterval = 18f;

    [TextArea]
    public string mission3_Brief_1 =
        "Hệ thống: Nhiệm vụ 3 bắt đầu. Đồng chí phải hộ tống cán bộ vượt qua khu vực nguy hiểm trong đêm.";
    [TextArea]
    public string mission3_Brief_2 =
        "Hệ thống: Tuyệt đối tránh vùng phát hiện của lính gác. Nếu bị lộ, nhiệm vụ sẽ thất bại.";
    [TextArea]
    public string mission3_Brief_3 =
        "Kim Đồng: Rõ! Tôi sẽ dẫn đường an toàn.";

    [TextArea]
    public string mission3_ApproachNPC_1 =
        "Hệ thống: Tiếp cận cán bộ để bắt đầu hộ tống.";
    [TextArea]
    public string mission3_ApproachNPC_2 =
        "Kim Đồng: Đồng chí theo sát tôi, đi thấp người… đừng để họ phát hiện.";

    [TextArea]
    public string mission3_StartEscort_1 =
        "Cán bộ: Cảm ơn đồng chí. Ta phải rút ngay, địch tuần tra rất gắt!";
    [TextArea]
    public string mission3_StartEscort_2 =
        "Kim Đồng: Theo tôi. Nếu nguy hiểm, dừng lại, chờ tín hiệu rồi đi tiếp.";

    [TextArea]
    public string mission3_Reminder_1 =
        "Hệ thống: Giữ khoảng cách hợp lý để cán bộ không bị tụt lại.";
    [TextArea]
    public string mission3_Reminder_2 =
        "Hệ thống: Tránh ánh sáng và vùng phát hiện. Ưu tiên đi sát địa hình che khuất.";
    [TextArea]
    public string mission3_Reminder_3 =
        "Kim Đồng: Bình tĩnh… chậm mà chắc.";

    [Header("NV3 - Chúc mừng khi hoàn thành")]
    [TextArea]
    public string mission3WinCongratsMessage =
        "Chúc mừng đồng chí! 🎉\nBạn đã hộ tống cán bộ an toàn và hoàn thành Nhiệm vụ 3!";
    public float mission3WinCongratsDuration = 2.5f;

    // ================= INTERNAL FLAGS =================
    private bool isTransitioning = false;
    private bool isFailed = false;

    private Coroutine mission3DialogueRoutine;
    private Coroutine mission3ReminderRoutine;

    // ================= UNITY =================
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // auto find player
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        // NV1 letter on
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(true);

        // NV2 areas off
        if (outpostSearchAreas != null)
        {
            foreach (Transform area in outpostSearchAreas)
            {
                if (area != null) area.gameObject.SetActive(false);
            }
        }

        // NV3 group off
        if (mission3_Group != null) mission3_Group.SetActive(false);

        StartPhase1();
        LockCursor();
    }

    void Update()
    {
        if (currentPhase == MissionPhase.Completed || playerTransform == null) return;

        if (currentPhase == MissionPhase.Night_Outpost && currentState == MissionState.FindingLetter)
        {
            currentObjective = isInsideSearchArea ? null : GetClosestActiveSearchArea();
        }

        if (currentPhase == MissionPhase.Mission3_Escort)
        {
            if (!mission3_EscortActive)
                currentObjective = mission3_EscortNPC;
            else
                currentObjective = mission3_Goal;
        }

        if (UIManager.Instance != null)
        {
            if (currentObjective != null)
            {
                float dist = Vector3.Distance(playerTransform.position, currentObjective.position);
                UIManager.Instance.UpdateDistance(dist);
            }
            else
            {
                UIManager.Instance.UpdateDistance(-1);
            }
        }
    }

    // ================= HELPERS =================
    Transform GetClosestActiveSearchArea()
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        if (outpostSearchAreas == null) return null;

        foreach (Transform area in outpostSearchAreas)
        {
            if (area != null && area.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(playerTransform.position, area.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = area;
                }
            }
        }
        return closest;
    }

    void TeleportPlayer(Transform targetPoint)
    {
        if (playerTransform == null || targetPoint == null) return;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerTransform.position = targetPoint.position;
        playerTransform.rotation = targetPoint.rotation;

        if (cc != null) cc.enabled = true;
    }

    void StopMission3Coroutines()
    {
        if (mission3DialogueRoutine != null) { StopCoroutine(mission3DialogueRoutine); mission3DialogueRoutine = null; }
        if (mission3ReminderRoutine != null) { StopCoroutine(mission3ReminderRoutine); mission3ReminderRoutine = null; }
    }

    // ================= PHASE 1 =================
    void StartPhase1()
    {
        isFailed = false;
        isTransitioning = false;

        currentPhase = MissionPhase.Day_Cave;
        currentState = MissionState.FindingLetter;
        currentObjective = letter1_Cave;

        StopMission3Coroutines();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Mật Thư Hang Đá", "Lần theo La bàn để tìm mật thư trong hang.");
            UIManager.Instance.ShowSystemDialogue(
                "Hệ thống: Đồng chí! Có một tài liệu mật được giấu trong hang đá. Hãy cẩn thận lính gác và lấy nó!",
                4f
            );
        }
    }

    // NV1 / NV2: nhặt thư thật
    public void OnLetterPickedUp()
    {
        if (isFailed || isTransitioning) return;

        currentState = MissionState.Delivering;
        isInsideSearchArea = false;
        currentObjective = deliveryLocation;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowTopNotification("Đã nhặt: Tài Liệu Mật");
            UIManager.Instance.UpdateMissionPanel("Giao Thư Mật", "Rút lui và giao tài liệu đến Làng an toàn");

            if (currentPhase == MissionPhase.Day_Cave)
                UIManager.Instance.ShowSystemDialogue(
                    "Hệ thống: Tốt lắm! Giờ hãy luồn lách qua hàng rào địch và mang nó về Điểm Tập Kết.",
                    4f
                );
            else
                UIManager.Instance.ShowSystemDialogue(
                    "Hệ thống: Có được mật thư đồn địch rồi! Rút lui ngay trước khi bị phát hiện!",
                    4f
                );
        }
    }

    // NV1 / NV2: giao thư
    public void OnDelivered()
    {
        if (isFailed || isTransitioning) return;
        if (currentState != MissionState.Delivering) return;

        if (currentPhase == MissionPhase.Day_Cave)
            StartCoroutine(TransitionToPhase2());
        else if (currentPhase == MissionPhase.Night_Outpost)
            StartCoroutine(TransitionToMission3());
    }

    // ================= FAIL =================
    public void FailMission(string reason)
    {
        if (isFailed || currentPhase == MissionPhase.Completed) return;

        isFailed = true;
        isTransitioning = true;
        currentObjective = null;

        StopMission3Coroutines();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDistance(-1);
            UIManager.Instance.HideInteractPrompt();
            UIManager.Instance.ShowLoseUI(reason);
        }
    }

    public void RetryCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToHub()
    {
        if (string.IsNullOrEmpty(hubSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(hubSceneName);
    }

    // ================= END GAME =================
    IEnumerator EndGameSequence()
    {
        isTransitioning = true;
        currentPhase = MissionPhase.Completed;
        currentObjective = null;

        StopMission3Coroutines();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDistance(-1);
            UIManager.Instance.ShowSystemDialogue("Kim Đồng: Hoàn thành nhiệm vụ... rút lui an toàn!", 3f);
        }

        yield return new WaitForSeconds(1.0f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinUI();

            string endText =
                "Sáng 15/2/1943\n" +
                "Kim Đồng hy sinh khi mới 14 tuổi\n" +
                "để bảo vệ cuộc họp của cán bộ Việt Minh.";

            UIManager.Instance.ShowEndUI(endText);
        }

        UnlockCursor();
    }

    // ================= TRANSITION PHASE 1 -> 2 =================
    IEnumerator TransitionToPhase2()
    {
        isTransitioning = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowSystemDialogue("Hệ thống: Hoàn thành xuất sắc Nhiệm vụ 1! Đang chờ đêm xuống...", 3f);

        if (UIManager.Instance != null)
            yield return UIManager.Instance.Fade(1f, 1.2f);

        currentPhase = MissionPhase.Night_Outpost;
        currentState = MissionState.FindingLetter;
        isInsideSearchArea = false;

        TeleportPlayer(outpostSpawnPoint);

        if (outpostSearchAreas != null)
        {
            foreach (Transform area in outpostSearchAreas)
            {
                if (area != null) area.gameObject.SetActive(true);
            }
        }

        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(false);

        ApplyNightEnvironment();

        currentObjective = GetClosestActiveSearchArea();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Trinh sát đồn địch", "Tìm kiếm mật thư thật trong các khu vực tình nghi.");
            UIManager.Instance.ShowTopNotification("Đêm xuống... hãy lén lút hơn!", 3f);
            yield return UIManager.Instance.Fade(0f, 0.8f);
        }

        isTransitioning = false;
        LockCursor();
    }

    // ================= TRANSITION PHASE 2 -> 3 (AUTO START) =================
    IEnumerator TransitionToMission3()
    {
        isTransitioning = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowSystemDialogue("Hệ thống: Hoàn thành Nhiệm vụ 2! Bắt đầu Nhiệm vụ 3...", 2.5f);

        if (UIManager.Instance != null)
            yield return UIManager.Instance.Fade(1f, 0.8f);

        if (outpostSearchAreas != null)
        {
            foreach (Transform area in outpostSearchAreas)
            {
                if (area != null) area.gameObject.SetActive(false);
            }
        }

        currentPhase = MissionPhase.Mission3_Escort;
        currentState = MissionState.FindingLetter;
        isInsideSearchArea = false;

        if (mission3_Group != null) mission3_Group.SetActive(true);

        TeleportPlayer(mission3_StartPoint);

        ApplyMission3Night();

        // objective ban đầu
        mission3_EscortActive = false;
        currentObjective = (mission3_EscortNPC != null) ? mission3_EscortNPC : mission3_Goal;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Hộ tống cán bộ", "Tiếp cận cán bộ để bắt đầu dẫn đường tới điểm an toàn.");
            UIManager.Instance.ShowTopNotification("Nhiệm vụ 3: Hộ tống", 2f);
            yield return UIManager.Instance.Fade(0f, 0.6f);
        }

        isTransitioning = false;
        LockCursor();

        StopMission3Coroutines();

        // ✅ AUTO START ESCORT NGAY KHI VÀO NV3
        if (mission3_AutoStartEscortOnEnter)
        {
            Mission3_StartEscort_Internal(true);   // silent start
        }
        else
        {
            // nếu bạn tắt auto start, vẫn chạy briefing bình thường
            mission3DialogueRoutine = StartCoroutine(Mission3DialogueFlow());
        }
    }

    IEnumerator Mission3DialogueFlow()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_Brief_1, 4f);
        yield return new WaitForSeconds(3.8f);
        if (currentPhase != MissionPhase.Mission3_Escort || isFailed) yield break;

        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_Brief_2, 4f);
        yield return new WaitForSeconds(3.8f);
        if (currentPhase != MissionPhase.Mission3_Escort || isFailed) yield break;

        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_Brief_3, 3.5f);
        yield return new WaitForSeconds(3.2f);

        while (currentPhase == MissionPhase.Mission3_Escort && !isFailed && !mission3_EscortActive)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTopNotification("👉 Tiếp cận cán bộ", 2f);
                UIManager.Instance.ShowSystemDialogue(mission3_ApproachNPC_1, 3.5f);
            }

            yield return new WaitForSeconds(10f);

            if (currentPhase != MissionPhase.Mission3_Escort || isFailed || mission3_EscortActive) break;

            if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_ApproachNPC_2, 4f);
            yield return new WaitForSeconds(10f);
        }
    }

    IEnumerator Mission3ReminderLoop()
    {
        float t = 0f;
        int idx = 0;

        while (currentPhase == MissionPhase.Mission3_Escort && !isFailed && mission3_EscortActive)
        {
            t += Time.deltaTime;
            if (t >= mission3_ReminderInterval)
            {
                t = 0f;

                if (UIManager.Instance != null)
                {
                    if (idx % 3 == 0) UIManager.Instance.ShowSystemDialogue(mission3_Reminder_1, 3.2f);
                    else if (idx % 3 == 1) UIManager.Instance.ShowSystemDialogue(mission3_Reminder_2, 3.2f);
                    else UIManager.Instance.ShowSystemDialogue(mission3_Reminder_3, 3.2f);
                    idx++;
                }
            }
            yield return null;
        }
    }

    // ===== NV3: Start Escort public (nếu trigger/E gọi) =====
    public void Mission3_StartEscort()
    {
        Mission3_StartEscort_Internal(false);
    }

    // ✅ internal: cho phép auto start (silent) hoặc start thường
    void Mission3_StartEscort_Internal(bool silent)
    {
        if (currentPhase != MissionPhase.Mission3_Escort) return;
        if (isFailed || isTransitioning) return;

        mission3_EscortActive = true;
        currentObjective = mission3_Goal;

        // stop nhắc tiếp cận npc nếu đang chạy
        if (mission3DialogueRoutine != null)
        {
            StopCoroutine(mission3DialogueRoutine);
            mission3DialogueRoutine = null;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Hộ tống cán bộ", "Dẫn cán bộ tới điểm an toàn. Tránh vùng phát hiện!");
            UIManager.Instance.ShowTopNotification("✅ Đã bắt đầu hộ tống!", 2f);

            // AUTO start vẫn nên có thoại cho “đã”
            UIManager.Instance.ShowSystemDialogue(mission3_StartEscort_1, 4f);
        }

        StartCoroutine(Mission3StartEscortSecondLine());

        if (mission3ReminderRoutine != null) StopCoroutine(mission3ReminderRoutine);
        mission3ReminderRoutine = StartCoroutine(Mission3ReminderLoop());
    }

    IEnumerator Mission3StartEscortSecondLine()
    {
        yield return new WaitForSeconds(3.6f);
        if (currentPhase != MissionPhase.Mission3_Escort || isFailed || !mission3_EscortActive) yield break;
        if (UIManager.Instance != null) UIManager.Instance.ShowSystemDialogue(mission3_StartEscort_2, 4f);
    }

    // ===== NV3: WIN =====
    public void Mission3_Win()
    {
        if (currentPhase != MissionPhase.Mission3_Escort) return;
        if (isFailed || isTransitioning) return;

        StartCoroutine(Mission3WinSequence());
    }

    IEnumerator Mission3WinSequence()
    {
        isTransitioning = true;

        StopMission3Coroutines();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowSystemDialogue(mission3WinCongratsMessage, mission3WinCongratsDuration);
            UIManager.Instance.ShowTopNotification("🎉 HOÀN THÀNH NHIỆM VỤ 3!", 2f);
        }

        yield return new WaitForSeconds(mission3WinCongratsDuration);

        yield return StartCoroutine(EndGameSequence());
    }

    // ================= ENV =================
    void ApplyNightEnvironment()
    {
        if (sunLight != null)
        {
            sunLight.color = nightColor;
            sunLight.intensity = nightIntensity;
        }

        if (nightSkybox != null)
        {
            RenderSettings.skybox = nightSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    void ApplyMission3Night()
    {
        if (!mission3_UseNightOverride)
        {
            ApplyNightEnvironment();
            return;
        }

        if (sunLight != null)
        {
            sunLight.color = mission3_NightColor;
            sunLight.intensity = mission3_NightIntensity;
        }

        Material sb = mission3_NightSkybox != null ? mission3_NightSkybox : nightSkybox;
        if (sb != null)
        {
            RenderSettings.skybox = sb;
            DynamicGI.UpdateEnvironment();
        }
    }

    // ================= CURSOR =================
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}