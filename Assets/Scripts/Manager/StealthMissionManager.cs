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

    [Header("Kịch bản thoại")]
    [TextArea] public string msg_StartPhase1 = "Kim Đồng: Suỵt! Anh/chị ơi, có một mật thư giấu trong hang đá. Đi theo la bàn để tìm nhé!";
    [TextArea] public string msg_PickLetter1 = "Kim Đồng: Tuyệt quá! Có thư rồi. Giờ mình mang về Làng an toàn nhé.";
    [TextArea] public string msg_StartPhase2 = "Kim Đồng: Đêm xuống rồi... Bọn địch canh gác kỹ lắm. Tìm giúp em mật thư thật nhé.";
    [TextArea] public string msg_PickLetter2 = "Kim Đồng: Lấy được mật thư rồi! Mình rút lui ngay thôi!";
    [TextArea] public string msg_StartPhase3 = "Kim Đồng: Xong nhiệm vụ 2 rồi! Bây giờ mình đi đón cán bộ nhé anh/chị!";
    [TextArea] public string mission3WinCongratsMessage = "Kim Đồng: Hoan hô! Chúng ta đưa được cán bộ đến nơi an toàn rồi!";
    public float mission3WinCongratsDuration = 3f;

    [TextArea] public string mission3_StartEscort_1 = "Kim Đồng: Bác đi sát theo cháu nhé. Còn anh/chị thì đi trước cảnh giới!";
    [TextArea] public string mission3_StartEscort_2 = "Cán bộ: Cảm ơn đồng chí. Ta đi thôi!";

    private bool isTransitioning = false;
    private bool isFailed = false;

    void Awake() { if (Instance == null) Instance = this; else { Destroy(gameObject); return; } }

    void Start()
    {
        if (playerTransform == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) playerTransform = p.transform; }
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(true);
        if (outpostSearchAreas != null) foreach (Transform area in outpostSearchAreas) if (area != null) area.gameObject.SetActive(false);
        if (mission3_Group != null) mission3_Group.SetActive(false);

        StartPhase1();
        LockCursor();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayLevel1();
    }

    void Update()
    {
        if (currentPhase == MissionPhase.Completed || playerTransform == null) return;
        if (currentPhase == MissionPhase.Night_Outpost && currentState == MissionState.FindingLetter)
            currentObjective = isInsideSearchArea ? null : GetClosestActiveSearchArea();
        if (currentPhase == MissionPhase.Mission3_Escort)
            currentObjective = !mission3_EscortActive ? mission3_EscortNPC : mission3_Goal;

        if (UIManager.Instance != null && currentObjective != null)
        {
            float dist = Vector3.Distance(playerTransform.position, currentObjective.position);
            UIManager.Instance.UpdateDistance(dist);
        }
    }

    // Hàm hỗ trợ phát thoại an toàn (Tự động tắt nhạc nền nếu có voice)
    private void PlayDialogueWithMusicControl(string message, float duration, AudioClip voice)
    {
        if (UIManager.Instance != null)
        {
            // Nếu có clip nói, ra lệnh AudioManager giảm nhạc
            if (voice != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.StartDucking();
                // Chúng ta để UIManager xử lý việc StopDucking sau khi clip chạy xong 
                // (Đã cài đặt trong Coroutine PlayVoiceAndMuteBGM ở UIManager)
            }
            UIManager.Instance.ShowSystemDialogue(message, duration, false, voice);
        }
    }

    Transform GetClosestActiveSearchArea()
    {
        Transform closest = null; float minDistance = float.MaxValue;
        if (outpostSearchAreas == null) return null;
        foreach (Transform area in outpostSearchAreas) { if (area != null && area.gameObject.activeInHierarchy) { float dist = Vector3.Distance(playerTransform.position, area.position); if (dist < minDistance) { minDistance = dist; closest = area; } } }
        return closest;
    }

    void StartPhase1()
    {
        isFailed = false; isTransitioning = false; currentPhase = MissionPhase.Day_Cave; currentState = MissionState.FindingLetter; currentObjective = letter1_Cave;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Mật Thư Hang Đá", "Lần theo La bàn để tìm mật thư.");
        }
        PlayDialogueWithMusicControl(msg_StartPhase1, 5f, v_StartPhase1);
    }

    public void OnLetterPickedUp()
    {
        if (isFailed || isTransitioning) return;
        currentState = MissionState.Delivering; currentObjective = deliveryLocation;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Giao Thư Mật", "Mang tài liệu về Làng an toàn");
        }
        AudioClip clipToPlay = (currentPhase == MissionPhase.Day_Cave) ? v_PickLetter1 : v_PickLetter2;
        string msgToPlay = (currentPhase == MissionPhase.Day_Cave) ? msg_PickLetter1 : msg_PickLetter2;
        PlayDialogueWithMusicControl(msgToPlay, 5f, clipToPlay);
    }

    public void OnDelivered()
    {
        if (isFailed || isTransitioning || currentState != MissionState.Delivering) return;
        if (currentPhase == MissionPhase.Day_Cave) StartCoroutine(TransitionToPhase2());
        else if (currentPhase == MissionPhase.Night_Outpost) StartCoroutine(TransitionToMission3());
    }

    public void FailMission(string reason)
    {
        if (isFailed || currentPhase == MissionPhase.Completed) return;
        isFailed = true; isTransitioning = true; currentObjective = null;
        if (UIManager.Instance != null) { UIManager.Instance.UpdateDistance(-1); UIManager.Instance.ShowLoseUI(reason); }
    }

    public void RetryCurrentScene() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void BackToHub() { SceneManager.LoadScene(string.IsNullOrEmpty(hubSceneName) ? SceneManager.GetActiveScene().name : hubSceneName); }

    public void Mission3_StartEscort() { Mission3_StartEscort_Internal(false); }

    void Mission3_StartEscort_Internal(bool silent)
    {
        if (currentPhase != MissionPhase.Mission3_Escort || isFailed) return;
        mission3_EscortActive = true; currentObjective = mission3_Goal;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Hộ tống cán bộ", "Dẫn cán bộ tới điểm an toàn.");
        }
        PlayDialogueWithMusicControl(mission3_StartEscort_1, 4f, v_m3_StartEscort1);
        StartCoroutine(Mission3StartEscortSecondLine());
    }

    IEnumerator Mission3StartEscortSecondLine()
    {
        yield return new WaitForSeconds(4.2f);
        PlayDialogueWithMusicControl(mission3_StartEscort_2, 4f, v_m3_StartEscort2);
    }

    IEnumerator TransitionToPhase2()
    {
        isTransitioning = true;
        if (UIManager.Instance != null) yield return UIManager.Instance.Fade(1f, 1.2f);
        currentPhase = MissionPhase.Night_Outpost; currentState = MissionState.FindingLetter;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLevel2();
        TeleportPlayer(outpostSpawnPoint);
        ApplyNightEnvironment();
        if (outpostSearchAreas != null) foreach (Transform area in outpostSearchAreas) if (area != null) area.gameObject.SetActive(true);
        if (UIManager.Instance != null) yield return UIManager.Instance.Fade(0f, 0.8f);
        PlayDialogueWithMusicControl(msg_StartPhase2, 5f, v_StartPhase2);
        isTransitioning = false;
    }

    IEnumerator TransitionToMission3()
    {
        isTransitioning = true;
        if (UIManager.Instance != null) yield return UIManager.Instance.Fade(1f, 0.8f);
        currentPhase = MissionPhase.Mission3_Escort;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLevel3();
        if (mission3_Group != null) mission3_Group.SetActive(true);
        TeleportPlayer(mission3_StartPoint); ApplyMission3Night();
        if (UIManager.Instance != null) yield return UIManager.Instance.Fade(0f, 0.6f);
        isTransitioning = false;
        if (mission3_AutoStartEscortOnEnter) Mission3_StartEscort_Internal(true);
    }

    public void Mission3_Win() { if (!isTransitioning) StartCoroutine(Mission3WinSequence()); }

    IEnumerator Mission3WinSequence()
    {
        PlayDialogueWithMusicControl(mission3WinCongratsMessage, 3f, v_EndGame);
        yield return new WaitForSeconds(3f);
        yield return StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        isTransitioning = true; currentPhase = MissionPhase.Completed;
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null) AudioManager.Instance.bgmSource.Stop();
        if (UIManager.Instance != null) { UIManager.Instance.ShowWinUI(); }
        UnlockCursor();
        yield return null;
    }

    void TeleportPlayer(Transform target) { if (playerTransform == null || target == null) return; CharacterController cc = playerTransform.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; playerTransform.position = target.position; playerTransform.rotation = target.rotation; if (cc != null) cc.enabled = true; }
    void ApplyNightEnvironment() { if (sunLight != null) { sunLight.color = nightColor; sunLight.intensity = nightIntensity; } if (nightSkybox != null) RenderSettings.skybox = nightSkybox; DynamicGI.UpdateEnvironment(); }
    void ApplyMission3Night() { if (sunLight != null) { sunLight.color = mission3_NightColor; sunLight.intensity = mission3_NightIntensity; } Material sb = mission3_NightSkybox != null ? mission3_NightSkybox : nightSkybox; if (sb != null) RenderSettings.skybox = sb; DynamicGI.UpdateEnvironment(); }
    public void LockCursor() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    public void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}