using UnityEngine;
using UnityEngine.AI;

public class AgentFaceVelocity : MonoBehaviour
{
    public float turnSpeed = 12f;
    public bool flip180 = true; // bật nếu model quay ngược

    NavMeshAgent agent;

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Update()
    {
        if (agent == null) return;

        Vector3 v = agent.velocity;
        v.y = 0;
        if (v.sqrMagnitude < 0.01f) return;

        Quaternion look = Quaternion.LookRotation(v.normalized);
        if (flip180) look *= Quaternion.Euler(0, 180f, 0); // FIX “đi lùi”

        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
    }
}