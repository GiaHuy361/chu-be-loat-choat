using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2.0f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Orbit Settings")]
    public float distance = 5f;
    public float height = 1.5f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.2f;
    public float collisionOffset = 0.1f;
    public float minDistance = 0.5f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;   // tăng lên
    public float rotationSmooth = 12f;        // giảm lại cho tự nhiên

    float yaw;
    float pitch;

    Vector3 currentVelocity;
    Vector3 smoothPosition;   // dùng biến riêng để smooth

    void Start()
    {
        if (!target) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

        smoothPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        // ===== 1. ROTATION INPUT =====
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        // ===== 2. CALCULATE PIVOT =====
        Vector3 pivot = target.position + Vector3.up * height;
        Vector3 direction = targetRotation * -Vector3.forward;

        float finalDistance = distance;

        // ===== 3. COLLISION =====
        RaycastHit hit;
        if (Physics.SphereCast(pivot, collisionRadius, direction, out hit, distance, collisionLayers))
        {
            float distanceToWall = hit.distance - collisionOffset;
            finalDistance = Mathf.Max(distanceToWall, minDistance);
        }

        Vector3 desiredPosition = pivot + direction * finalDistance;

        // ===== 4. SMOOTH POSITION (ổn định hơn) =====
        smoothPosition = Vector3.SmoothDamp(
            smoothPosition,
            desiredPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        transform.position = smoothPosition;

        // ===== 5. SMOOTH ROTATION =====
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}
