using UnityEngine;
using UnityEngine.AI;

public class EscortNPCFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 2.2f;
    public float stopDistance = 1.8f;

    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
        }
    }

    void Update()
    {
        if (StealthMissionManager.Instance == null) return;

        // chỉ chạy khi NV3 đang active và đã start escort
        if (StealthMissionManager.Instance.currentPhase != StealthMissionManager.MissionPhase.Mission3_Escort)
        {
            SetAnim(0);
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (!StealthMissionManager.Instance.mission3_EscortActive)
        {
            SetAnim(0);
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (player == null || agent == null) return;

        float d = Vector3.Distance(transform.position, player.position);

        if (d > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
        }

        SetAnim(agent.velocity.magnitude);
    }

    void SetAnim(float speed)
    {
        if (anim != null) anim.SetFloat("Speed", speed);
    }
}