using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StealthMissionManager : MonoBehaviour
{
    public static StealthMissionManager Instance;

    public enum MissionPhase { Day_Cave, Night_Outpost, Completed }
    public enum MissionState { FindingLetter, Delivering }

    [Header("Trạng thái hiện tại")]
    public MissionPhase currentPhase = MissionPhase.Day_Cave;
    public MissionState currentState = MissionState.FindingLetter;

    [Header("Scene Names")]
    public string hubSceneName = "Demo_Terrain"; // đổi đúng tên scene hub của bạn

    [Header("Cấu hình Mục Tiêu & Vị trí")]
    public Transform playerTransform;
    public Transform letter1_Cave;

    [Tooltip("Kéo toàn bộ các vùng Search Area chứa thư (Thật & Giả) ở đồn địch vào đây")]
    public Transform[] outpostSearchAreas;

    public Transform deliveryLocation;
    public Transform outpostSpawnPoint;

    [Header("Môi trường Ngày/Đêm")]
    public Light sunLight;
    public Material nightSkybox;
    public Color nightColor = new Color(0.1f, 0.15f, 0.3f);
    public float nightIntensity = 0.2f;

    [Header("Dữ liệu La bàn")]
    public Transform currentObjective;
    public bool isInsideSearchArea = false;

    private bool isTransitioning = false;
    private bool isFailed = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(true);

        foreach (Transform area in outpostSearchAreas)
        {
            if (area != null) area.gameObject.SetActive(false);
        }

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

    Transform GetClosestActiveSearchArea()
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

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

    // ================= PHASE =================
    void StartPhase1()
    {
        isFailed = false;
        isTransitioning = false;

        currentPhase = MissionPhase.Day_Cave;
        currentState = MissionState.FindingLetter;
        currentObjective = letter1_Cave;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Mật Thư Hang Đá", "Lần theo La bàn để tìm mật thư trong hang.");
            UIManager.Instance.ShowSystemDialogue("Hệ thống: Đồng chí! Có một tài liệu mật được giấu trong hang đá. Hãy cẩn thận lính gác và lấy nó!", 4f);
        }
    }

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
                UIManager.Instance.ShowSystemDialogue("Hệ thống: Tốt lắm! Giờ hãy luồn lách qua hàng rào địch và mang nó về Điểm Tập Kết.", 4f);
            else
                UIManager.Instance.ShowSystemDialogue("Hệ thống: Có được mật thư đồn địch rồi! Rút lui ngay trước khi bị phát hiện!", 4f);
        }
    }

    public void OnDelivered()
    {
        if (isFailed || isTransitioning) return;
        if (currentState != MissionState.Delivering) return;

        if (currentPhase == MissionPhase.Day_Cave)
            StartCoroutine(TransitionToPhase2());
        else if (currentPhase == MissionPhase.Night_Outpost)
            StartCoroutine(EndGameSequence());
    }

    // ================= FAIL =================
    public void FailMission(string reason)
    {
        if (isFailed || currentPhase == MissionPhase.Completed) return;

        isFailed = true;
        isTransitioning = true;
        currentObjective = null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDistance(-1);
            UIManager.Instance.HideInteractPrompt();

            // Bạn muốn đổi text chỉ cần đổi reason truyền vào đây
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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateDistance(-1);
            UIManager.Instance.ShowSystemDialogue("Kim Đồng: Thì ra đây là kế hoạch của chúng...", 4f);
        }

        yield return new WaitForSeconds(1.5f);

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

        // fade to black
        if (UIManager.Instance != null)
            yield return UIManager.Instance.Fade(1f, 1.2f);

        // switch
        currentPhase = MissionPhase.Night_Outpost;
        currentState = MissionState.FindingLetter;
        isInsideSearchArea = false;

        // teleport
        if (playerTransform != null && outpostSpawnPoint != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerTransform.position = outpostSpawnPoint.position;
            playerTransform.rotation = outpostSpawnPoint.rotation;

            if (cc != null) cc.enabled = true;
        }

        // enable areas
        foreach (Transform area in outpostSearchAreas)
        {
            if (area != null) area.gameObject.SetActive(true);
        }

        // disable cave letter
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(false);

        // apply night
        ApplyNightEnvironment();

        // set objective
        currentObjective = GetClosestActiveSearchArea();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMissionPanel("Trinh sát đồn địch", "Tìm kiếm mật thư thật trong các khu vực tình nghi.");
            UIManager.Instance.ShowTopNotification("Đêm xuống... hãy lén lút hơn!", 3f);

            // fade in
            yield return UIManager.Instance.Fade(0f, 0.8f);
        }

        isTransitioning = false;
        LockCursor();
    }

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