using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Mouse")]
    public float mouseSensitivity = 2.5f; // giảm chút cho đỡ giật
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Orbit")]
    public float distance = 5f;
    public float height = 1.7f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.05f; // nhỏ = bám chặt, không rung
    public float rotationSmooth = 18f;

    float yaw;
    float pitch;
    Vector3 posVel;

    void Start()
    {
        if (!target) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        // IMPORTANT: KHÔNG nhân Time.deltaTime và KHÔNG *100
        yaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 pivot = target.position + Vector3.up * height;
        Vector3 desiredPos = pivot + rot * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref posVel, positionSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSmooth * Time.deltaTime);
    }
}
