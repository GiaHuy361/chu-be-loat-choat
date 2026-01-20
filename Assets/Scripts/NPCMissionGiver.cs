using TMPro; // Thêm thư viện này
using UnityEngine;

public class NPCMissionGiver : MonoBehaviour
{
    [Header("Nội dung")]
    [TextArea] public string dialogueContent = "Chào con, hãy mang vật phẩm này về căn cứ.";
    [TextArea] public string missionContent = "Giao hàng về căn cứ.";

    [Header("Cài đặt")]
    public float interactDistance = 3.0f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Gán UI & Animation")]
    public Animator npcAnimator;
    public GameObject interactUI;  // Kéo cái Text "Ấn E" vào đây (Cái nằm ngoài Panel)

    private Transform player;
    private MissionManager missionManager;
    private bool isTalking = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        missionManager = MissionManager.Instance; // Gọi Singleton cho gọn
        if (npcAnimator == null) npcAnimator = GetComponent<Animator>();

        // Đảm bảo chữ "Ấn E" có nội dung và tắt đi lúc đầu
        if (interactUI != null)
        {
            // Mẹo: Tự điền chữ vào để tránh bị lỗi mất chữ
            TMP_Text txt = interactUI.GetComponent<TMP_Text>();
            if (txt != null) txt.text = "Ấn E";

            interactUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || missionManager == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= interactDistance)
        {
            // Nếu chưa nói chuyện -> Hiện chữ "Ấn E"
            if (!isTalking && interactUI != null) interactUI.SetActive(true);

            // Bấm E để mở hội thoại
            if (Input.GetKeyDown(interactKey) && !isTalking)
            {
                StartConversation();
            }
        }
        else
        {
            // Đi xa -> Tắt hết
            if (interactUI != null) interactUI.SetActive(false);
            if (isTalking) EndConversation();
        }
    }

    void StartConversation()
    {
        isTalking = true;
        if (interactUI != null) interactUI.SetActive(false); // Tắt chữ Ấn E đi

        // Gọi MissionManager mở bảng to
        missionManager.OpenDialogue(dialogueContent, missionContent);

        // Animation đứng thẳng
        if (npcAnimator != null) npcAnimator.SetBool("isStanding", true);
    }

    void EndConversation()
    {
        isTalking = false;
        missionManager.CloseDialogue(); // Đóng bảng to

        // Animation về gù lưng
        if (npcAnimator != null) npcAnimator.SetBool("isStanding", false);
    }
}