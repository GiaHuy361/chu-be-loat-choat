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

    [Header("Cấu hình Mục Tiêu & Vị trí")]
    public Transform playerTransform;
    public Transform letter1_Cave;

    [Tooltip("Kéo toàn bộ các vùng Search Area chứa thư (Thật & Giả) ở đồn địch vào đây")]
    public Transform[] outpostSearchAreas; // Thay thế cho 1 bức thư đơn lẻ

    public Transform deliveryLocation;
    public Transform outpostSpawnPoint;

    [Header("Môi trường Ngày/Đêm")]
    public Light sunLight;
    public Material nightSkybox;
    public Color nightColor = new Color(0.1f, 0.15f, 0.3f);
    public float nightIntensity = 0.2f;

    [Header("Dữ liệu La bàn")]
    public Transform currentObjective;
    public bool isInsideSearchArea = false; // Báo cho Manager biết Player đã vào vùng hay chưa

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        // Bật thư 1
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(true);

        // Tắt toàn bộ các vùng tìm kiếm ở Màn 2
        foreach (Transform area in outpostSearchAreas)
        {
            if (area != null) area.gameObject.SetActive(false);
        }

        StartPhase1();
    }

    void Update()
    {
        if (currentPhase != MissionPhase.Completed && playerTransform != null)
        {
            // LOGIC MỚI: Tự động tìm vùng gần nhất ở Màn 2
            if (currentPhase == MissionPhase.Night_Outpost && currentState == MissionState.FindingLetter)
            {
                if (isInsideSearchArea)
                {
                    currentObjective = null; // Ở trong vùng thì tắt la bàn để tự tìm
                }
                else
                {
                    currentObjective = GetClosestActiveSearchArea(); // Ở ngoài thì chỉ đến vùng gần nhất
                }
            }

            // Cập nhật UI Khoảng cách
            if (currentObjective != null)
            {
                float dist = Vector3.Distance(playerTransform.position, currentObjective.position);
                UIManager.Instance.UpdateDistance(dist);
            }
            else
            {
                UIManager.Instance.UpdateDistance(-1); // Xóa text khoảng cách
            }
        }
    }

    // Hàm phụ: Quét danh sách các vùng để tìm ra vùng gần nhất chưa bị tắt
    Transform GetClosestActiveSearchArea()
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (Transform area in outpostSearchAreas)
        {
            // Chỉ tính những vùng đang bật (chưa bị người chơi lục soát và xóa đi)
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

    // ================= CÁC GIAI ĐOẠN NHIỆM VỤ =================

    void StartPhase1()
    {
        currentPhase = MissionPhase.Day_Cave;
        currentState = MissionState.FindingLetter;
        currentObjective = letter1_Cave;

        UIManager.Instance.UpdateMissionPanel("Mật Thư Hang Đá", "Lần theo La bàn để tìm mật thư trong hang.");
        UIManager.Instance.ShowSystemDialogue("Hệ thống: Đồng chí! Có một tài liệu mật được giấu trong hang đá. Hãy cẩn thận lính gác và lấy nó!", 4f);
    }

    public void OnLetterPickedUp()
    {
        currentState = MissionState.Delivering;
        isInsideSearchArea = false; // Reset lại để chắc chắn la bàn giao thư bật lên
        currentObjective = deliveryLocation;

        UIManager.Instance.ShowTopNotification("Đã nhặt: Tài Liệu Mật");
        UIManager.Instance.UpdateMissionPanel("Giao Thư Mật", "Rút lui và giao tài liệu đến Làng an toàn");

        if (currentPhase == MissionPhase.Day_Cave)
            UIManager.Instance.ShowSystemDialogue("Hệ thống: Tốt lắm! Giờ hãy luồn lách qua hàng rào địch và mang nó về Điểm Tập Kết.", 4f);
        else
            UIManager.Instance.ShowSystemDialogue("Hệ thống: Có được mật thư đồn địch rồi! Rút lui ngay trước khi bị phát hiện!", 4f);
    }

    // Gọi khi bước vào Vùng Làng an toàn
    public void OnDelivered()
    {
        if (currentState != MissionState.Delivering) return;

        if (currentPhase == MissionPhase.Day_Cave)
        {
            // Xong Màn 1 -> Chuyển sang Màn 2
            StartCoroutine(TransitionToPhase2());
        }
        else if (currentPhase == MissionPhase.Night_Outpost)
        {
            // Xong Màn 2 -> Chạy kịch bản kết thúc game
            StartCoroutine(EndGameSequence());
        }
    }

    // KỊCH BẢN KẾT THÚC GAME
    IEnumerator EndGameSequence()
    {
        currentPhase = MissionPhase.Completed;
        currentObjective = null;
        UIManager.Instance.UpdateDistance(-1); // Tắt la bàn

        // 1. Hiện dòng thoại như bạn muốn
        UIManager.Instance.ShowSystemDialogue("Kim Đồng: Thì ra đây là kế hoạch của chúng...", 4f);

        // Chờ 1.5 giây để người chơi kịp đọc một chút rồi mới từ từ tối màn hình
        yield return new WaitForSeconds(1.5f);

        // 2. Hiệu ứng đen màn hình dần dần
        float t = 0;
        while (t < 2f)
        {
            t += Time.deltaTime;
            Color c = UIManager.Instance.blackScreen.color;
            c.a = Mathf.Lerp(0, 1, t / 2f);
            UIManager.Instance.blackScreen.color = c;
            yield return null;
        }

        // 3. Hiện UI Chiến Thắng (Pháo hoa, chữ Mission Complete...)
        UIManager.Instance.ShowWinUI();
        UnlockCursor();
    }

    // ================= HIỆU ỨNG & CHUYỂN CẢNH =================

    IEnumerator TransitionToPhase2()
    {
        UIManager.Instance.ShowSystemDialogue("Hệ thống: Hoàn thành xuất sắc Nhiệm vụ 1! Đang chờ đêm xuống...", 3f);

        float t = 0;
        while (t < 2f)
        {
            t += Time.deltaTime;
            Color c = UIManager.Instance.blackScreen.color;
            c.a = Mathf.Lerp(0, 1, t / 2f);
            UIManager.Instance.blackScreen.color = c;
            yield return null;
        }

        // --- MÀN HÌNH ĐÃ ĐEN THUI ---

        currentPhase = MissionPhase.Night_Outpost;
        currentState = MissionState.FindingLetter;
        isInsideSearchArea = false;

        if (sunLight != null)
        {
            sunLight.color = nightColor;
            sunLight.intensity = nightIntensity;
        }
        if (nightSkybox != null) RenderSettings.skybox = nightSkybox;
        RenderSettings.ambientIntensity = 0.3f;

        // Bật thư tắt thư
        if (letter1_Cave != null) letter1_Cave.gameObject.SetActive(false);

        // Bật toàn bộ các vùng tìm kiếm lên
        foreach (Transform area in outpostSearchAreas)
        {
            if (area != null) area.gameObject.SetActive(true);
        }

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        playerTransform.position = outpostSpawnPoint.position;
        playerTransform.rotation = outpostSpawnPoint.rotation;
        if (cc != null) cc.enabled = true;

        yield return new WaitForSeconds(1f);

        // --- SÁNG MÀN HÌNH LÊN ---
        t = 2f;
        while (t > 0)
        {
            t -= Time.deltaTime;
            Color c = UIManager.Instance.blackScreen.color;
            c.a = Mathf.Lerp(0, 1, t / 2f);
            UIManager.Instance.blackScreen.color = c;
            yield return null;
        }

        UIManager.Instance.UpdateMissionPanel("Đột Nhập Đồn Địch", "Tìm kiếm các khu vực tình nghi để lấy Mật thư số 2.");
        UIManager.Instance.ShowSystemDialogue("Hệ thống: Có nhiều khu vực tình nghi. Khi đến gần, la bàn sẽ tắt. Tự mò mẫm cẩn thận nhé!", 6f);
    }

    // ================= GAMEOVER =================

    public void GameOver()
    {
        if (currentPhase == MissionPhase.Completed) return;

        currentPhase = MissionPhase.Completed;
        currentObjective = null;
        UIManager.Instance.UpdateDistance(-1);

        string[] funnyLoseTexts = {
            "Địch: 'Bắt được gà rồi anh em ơi!'",
            "Hệ thống: Thôi xong, đồng chí đã bị tóm. Lần sau nhớ đi rón rén thôi nhé!",
            "Hệ thống: Lộ liễu quá! Cứ thế này thì hỏng hết việc lớn."
        };
        string loseMsg = funnyLoseTexts[Random.Range(0, funnyLoseTexts.Length)];

        UIManager.Instance.ShowLoseUI(loseMsg);
        UnlockCursor();
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}