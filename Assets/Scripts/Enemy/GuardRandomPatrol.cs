using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GuardRandomPatrol : MonoBehaviour
{
    public enum GuardState { Patrol, Investigate, Chase }
    public GuardState currentState = GuardState.Patrol;

    [Header("Khu vực đi tuần")]
    [Tooltip("Để trống nếu đây là lính gác CỐ ĐỊNH 1 chỗ")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isForward = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Header("Cài đặt Tầm nhìn (Ngày & Đêm)")]
    public Transform player;
    public bool isNightTime = false;

    [Space]
    public float dayViewRadius = 15f;
    [Range(0, 360)] public float dayViewAngle = 90f;

    [Space]
    public float nightViewRadius = 7f;
    [Range(0, 360)] public float nightViewAngle = 60f;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    private NavMeshAgent agent;
    private Animator anim;
    private bool isPerformingAction = false;
    private Coroutine investigateCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        agent.speed = walkSpeed;

        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);

        StartCoroutine(FindPlayerWithDelay(0.2f));
    }

    void Update()
    {
        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        if (currentState == GuardState.Chase)
        {
            ChaseBehavior();
            return;
        }

        if (isPerformingAction) return;

        if (currentState == GuardState.Patrol)
        {
            PatrolBehavior();
        }
    }

    void PatrolBehavior()
    {
        agent.speed = walkSpeed;

        if (waypoints.Length == 0)
        {
            if (Vector3.Distance(transform.position, initialPosition) > 0.5f)
            {
                agent.SetDestination(initialPosition);
            }
            else if (!agent.pathPending && agent.remainingDistance <= 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * 5f);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            StartCoroutine(ActionSequenceRoutine());
        }
    }

    IEnumerator ActionSequenceRoutine()
    {
        isPerformingAction = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(1.5f);
        if (anim != null) anim.SetTrigger("Scout");
        yield return new WaitForSeconds(4f);

        if (anim != null) anim.SetTrigger("Turn");
        yield return new WaitForSeconds(1.867f);

        if (isForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = waypoints.Length - 2;
                isForward = false;
            }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 1;
                isForward = true;
            }
        }

        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        agent.isStopped = false;
        isPerformingAction = false;
    }

    public void HearSound(Vector3 soundPosition)
    {
        if (currentState == GuardState.Patrol)
        {
            StopAllCoroutines();
            StartCoroutine(FindPlayerWithDelay(0.2f));
            investigateCoroutine = StartCoroutine(InvestigateRoutine(soundPosition));
        }
    }

    IEnumerator InvestigateRoutine(Vector3 targetPos)
    {
        currentState = GuardState.Investigate;
        isPerformingAction = true;
        agent.isStopped = true;

        Vector3 dirToSound = (targetPos - transform.position).normalized;
        dirToSound.y = 0;

        if (dirToSound != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirToSound);
            float t = 0;
            while (t < 1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, t);
                t += Time.deltaTime * 5f;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);

        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(targetPos);

        while (agent.pathPending || agent.remainingDistance > 0.5f)
            yield return null;

        agent.isStopped = true;
        if (anim != null) anim.SetTrigger("Scout");
        yield return new WaitForSeconds(4f);

        if (anim != null) anim.SetTrigger("Turn");
        yield return new WaitForSeconds(1.867f);

        currentState = GuardState.Patrol;
        isPerformingAction = false;
        agent.isStopped = false;
    }

    void ChaseBehavior()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        agent.SetDestination(player.position);

        // ===== BẮT ĐƯỢC PLAYER =====
        if (Vector3.Distance(transform.position, player.position) <= 1.5f)
        {
            if (StealthMissionManager.Instance != null)
            {
                StealthMissionManager.Instance.FailMission("Kim Đồng bị bắt... cuộc liên lạc thất bại.");
            }

            agent.isStopped = true;
            if (anim != null) anim.SetBool("isChasing", false);
            this.enabled = false;
        }
    }

    IEnumerator FindPlayerWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            if (currentState != GuardState.Chase)
                FindVisiblePlayer();
        }
    }

    void FindVisiblePlayer()
    {
        float currentRadius = isNightTime ? nightViewRadius : dayViewRadius;
        float currentAngle = isNightTime ? nightViewAngle : dayViewAngle;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, currentRadius, playerMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;

            Vector3 eyePos = transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = target.position + Vector3.up * 1.2f;
            Vector3 dirToTarget = (targetPos - eyePos).normalized;

            if (Vector3.Angle(transform.forward, (target.position - transform.position).normalized) < currentAngle / 2)
            {
                float dstToTarget = Vector3.Distance(eyePos, targetPos);

                if (!Physics.Raycast(eyePos, dirToTarget, dstToTarget, obstacleMask))
                {
                    currentState = GuardState.Chase;
                    StopAllCoroutines();
                    isPerformingAction = false;
                    agent.isStopped = false;
                    agent.speed = runSpeed;

                    if (anim != null) anim.SetBool("isChasing", true);
                }
            }
        }
    }
}