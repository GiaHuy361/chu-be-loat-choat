using UnityEngine;

public class OrbitCompass : MonoBehaviour
{
    [Header("Cài đặt Quỹ đạo")]
    public Transform player;         // Nhân vật chính
    public float orbitRadius = 1.5f; // Bán kính vòng tròn (khoảng cách từ nhân vật đến mũi tên)
    public float heightOffset = 0.2f;// Độ cao cách mặt đất
    public float moveSpeed = 8f;     // Tốc độ trượt của mũi tên

    [Header("Giao diện")]
    public GameObject arrowVisual;   // Hình ảnh mũi tên

    void Update()
    {
        // --- SỬA LOGIC Ở ĐÂY: Lấy mục tiêu từ StealthMissionManager ---
        if (StealthMissionManager.Instance == null ||
            StealthMissionManager.Instance.currentObjective == null ||
            player == null)
        {
            SetArrowVisible(false);
            return;
        }

        SetArrowVisible(true);
        // --- SỬA Ở ĐÂY: Lấy mục tiêu từ StealthMissionManager ---
        Transform target = StealthMissionManager.Instance.currentObjective;

        Vector3 playerPosFlat = new Vector3(player.position.x, 0, player.position.z);
        Vector3 targetPosFlat = new Vector3(target.position.x, 0, target.position.z);

        Vector3 directionToTarget = (targetPosFlat - playerPosFlat).normalized;

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Vector3 desiredPosition = player.position + (directionToTarget * orbitRadius);
            desiredPosition.y = player.position.y + heightOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, moveSpeed * Time.deltaTime);

            // --- ĐÃ SỬA LỖI TOÁN HỌC CHO ẢNH 2D NẰM BẸP ---
            float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

            // Xoay 90 độ trục X để ép xuống đất. Trừ 90 độ ở Y để đầu nhọn hướng đúng đích
            Quaternion desiredRotation = Quaternion.Euler(90f, angle - 90f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, moveSpeed * Time.deltaTime);
        }
    }

    void SetArrowVisible(bool isVisible)
    {
        // Ẩn/Hiện hình ảnh mũi tên
        if (arrowVisual != null && arrowVisual.activeSelf != isVisible)
        {
            arrowVisual.SetActive(isVisible);
        }
    }
}