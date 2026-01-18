using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                 // nvkimdong
    public Vector3 offset = new Vector3(0, 1.6f, -3.5f);
    public float followSpeed = 10f;
    public float rotateSpeed = 120f;

    void LateUpdate()
    {
        if (!target) return;

        // Follow vị trí (offset theo world, KHÔNG xoay theo rotation nhân vật)
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // Look at (nhìn vào ngực / đầu nhân vật)
        Vector3 lookPoint = target.position + Vector3.up * 1.4f;
        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotateSpeed * Time.deltaTime);
    }
}
