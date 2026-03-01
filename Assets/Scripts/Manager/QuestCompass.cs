using UnityEngine;
using TMPro;

public class QuestCompass : MonoBehaviour
{
    [Header("Cài đặt Mũi tên (3D)")]
    public float rotationSpeed = 5f;
    public TextMeshProUGUI distanceText; // Kéo chữ hiển thị số mét vào đây (nếu có)

    // Khai báo các thành phần hình ảnh của mũi tên để dễ tắt/bật
    private MeshRenderer arrowMesh;

    void Start()
    {
        arrowMesh = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // KIỂM TRA LOGIC MỚI: Liên kết thẳng với StealthMissionManager
        // Nếu không có Manager, hoặc Manager không có mục tiêu (currentObjective == null) thì tắt la bàn
        if (StealthMissionManager.Instance == null || StealthMissionManager.Instance.currentObjective == null)
        {
            SetArrowVisible(false);
            return;
        }

        // Đã có mục tiêu -> Bật la bàn lên
        SetArrowVisible(true);
        Transform target = StealthMissionManager.Instance.currentObjective;

        // Xoay mũi tên 3D về phía mục tiêu (Chỉ xoay trục Y để mũi tên không bị chúi xuống đất)
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Hiển thị khoảng cách
        if (distanceText != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            distanceText.text = Mathf.CeilToInt(dist) + "m"; // Dùng CeilToInt (làm tròn lên) cho đồng bộ với UI Task
        }
    }

    void SetArrowVisible(bool isVisible)
    {
        if (arrowMesh != null) arrowMesh.enabled = isVisible;
        if (distanceText != null) distanceText.gameObject.SetActive(isVisible);
    }
}