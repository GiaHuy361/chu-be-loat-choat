using UnityEngine;

public class PlaneRotate : MonoBehaviour
{
    public float moveRadius = 30f;   // bán kính bay vòng
    public float moveSpeed = 1f;     // t?c ?? bay
    public float height = 10f;       // ?? cao

    Vector3 centerPos;

    void Start()
    {
        // l?y v? trí ban ??u làm tâm ???ng tròn
        centerPos = transform.position;
    }

    void Update()
    {
        float angle = Time.time * moveSpeed;
        float x = Mathf.Cos(angle) * moveRadius;
        float z = Mathf.Sin(angle) * moveRadius;

        Vector3 newPos = centerPos + new Vector3(x, height, z);
        transform.position = newPos;

        // quay m?i máy bay theo h??ng bay
        transform.LookAt(centerPos);
        transform.Rotate(0, 180f, 0); // n?u m?i quay ng??c thì b? dòng này
    }
}
