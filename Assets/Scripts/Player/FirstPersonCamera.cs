using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 2.0f; // Để bằng y hệt TPP
    public float minPitch = -80f;         // FPP thường cho phép cúi/ngẩng sâu hơn TPP
    public float maxPitch = 80f;

    [Header("Smoothing")]
    public float rotationSmooth = 12f;    // Để bằng y hệt TPP cho đồng bộ cảm giác

    private float yaw;
    private float pitch;

    void OnEnable()
    {
        // [QUAN TRỌNG NHẤT]: Khi vừa bấm 'V' để bật FPP, 
        // nó sẽ "copy" góc nhìn hiện tại để không bị giật camera.
        Vector3 currentAngles = transform.eulerAngles;
        pitch = currentAngles.x;
        yaw = currentAngles.y;

        // Xử lý hệ trục toạ độ của Unity (Unity lưu góc âm thành 360)
        if (pitch > 180f) pitch -= 360f;
    }

    void LateUpdate()
    {
        // ===== 1. ROTATION INPUT =====
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        // ===== 2. SMOOTH ROTATION =====
        // Dùng y hệt công thức Lerp của TPP để chuột không bị sượng
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}