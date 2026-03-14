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

    [Header("Cài đặt Nghi ngờ (Suspicion)")]
    [Tooltip("Thời gian lính gác nhìn chằm chằm trước khi phát hiện và đuổi theo")]
    public float timeToDetect = 1.5f;
    private float currentDetectionTime = 0f;

    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Âm thanh Bước chân (Lính gác)")]
    public AudioSource footstepAudioSource;
    public AudioClip enemyFootstepClip;
    [Range(0f, 1f)] public float walkVolume = 0.6f;
    [Range(0f, 1f)] public float runVolume = 0.8f;
    public float walkStepInterval = 0.6f;
    public float runStepInterval = 0.35f;
    private float stepTimer;

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

        if (footstepAudioSource == null) footstepAudioSource = GetComponent<AudioSource>();
        if (footstepAudioSource == null) footstepAudioSource = gameObject.AddComponent<AudioSource>();

        footstepAudioSource.spatialBlend = 1f;
        footstepAudioSource.rolloffMode = AudioRolloffMode.Linear;
        footstepAudioSource.minDistance = 3f;
        footstepAudioSource.maxDistance = 20f;

        // Gán sẵn clip vào AudioSource để tránh lỗi phát chồng âm
        footstepAudioSource.clip = enemyFootstepClip;

        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void Update()
    {
        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        HandleFootsteps();

        // Luôn kiểm tra tầm nhìn mỗi frame để xử lý độ nghi ngờ
        HandleDetection();

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

    // --- HỆ THỐNG XỬ LÝ ÂM THANH BƯỚC CHÂN (ĐÃ SỬA LỖI ĐỘI QUÂN) ---
    void HandleFootsteps()
    {
        if (footstepAudioSource == null || enemyFootstepClip == null) return;

        // Đảm bảo clip đã được gán và BẬT CHẾ ĐỘ LẶP LẠI (Loop)
        if (footstepAudioSource.clip != enemyFootstepClip)
        {
            footstepAudioSource.clip = enemyFootstepClip;
        }
        footstepAudioSource.loop = true;

        float currentSpeed = agent.velocity.magnitude;

        // Nếu lính gác đang di chuyển
        if (currentSpeed > 0.1f && !agent.isStopped)
        {
            // Nếu âm thanh chưa phát thì bắt đầu phát
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Play();
            }

            // Chỉnh tốc độ (pitch) và âm lượng trực tiếp trên clip đang phát
            if (currentState == GuardState.Chase)
            {
                // KHI ĐUỔI: Tua nhanh clip x1.5, âm lượng to
                footstepAudioSource.pitch = 2f;
                footstepAudioSource.volume = runVolume;
            }
            else
            {
                // KHI ĐI TUẦN: Tốc độ clip bình thường (x1), âm lượng nhỏ hơn
                footstepAudioSource.pitch = 1f;
                footstepAudioSource.volume = walkVolume;
            }
        }
        else
        {
            // Nếu lính gác đứng yên, TẠM DỪNG âm thanh
            if (footstepAudioSource.isPlaying)
            {
                // Dùng Pause() thay vì Stop() để khi đi tiếp, âm thanh nối nhịp tự nhiên hơn
                footstepAudioSource.Pause();
            }
        }
    }

    // --- HỆ THỐNG NGHI NGỜ (SUSPICION) ---
    void HandleDetection()
    {
        if (currentState == GuardState.Chase) return; // Đang đuổi rồi thì không cần tính nghi ngờ nữa

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            // Tăng thanh nghi ngờ
            currentDetectionTime += Time.deltaTime;

            // Bắt lính gác đứng lại và xoay mặt chằm chằm về phía Kim Đồng
            if (currentState == GuardState.Patrol)
            {
                agent.isStopped = true;
                Vector3 lookPos = player.position - transform.position;
                lookPos.y = 0;
                if (lookPos != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5f);
                }
            }

            // Nếu thời gian nhìn thấy đủ lâu -> CHASE
            if (currentDetectionTime >= timeToDetect)
            {
                currentState = GuardState.Chase;
                StopAllCoroutines();
                isPerformingAction = false;
                agent.isStopped = false;
                agent.speed = runSpeed;

                if (anim != null) anim.SetBool("isChasing", true);
            }
        }
        else
        {
            // Không nhìn thấy nữa -> Tụt thanh nghi ngờ từ từ
            if (currentDetectionTime > 0)
            {
                currentDetectionTime -= Time.deltaTime;
                if (currentDetectionTime <= 0)
                {
                    currentDetectionTime = 0;

                    // Nếu tụt hết nghi ngờ và không bận đi điều tra tiếng động, cho đi tuần lại
                    if (currentState == GuardState.Patrol && !isPerformingAction)
                    {
                        agent.isStopped = false;
                    }
                }
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        float currentRadius = isNightTime ? nightViewRadius : dayViewRadius;
        float currentAngle = isNightTime ? nightViewAngle : dayViewAngle;

        Vector3 eyePos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = player.position + Vector3.up * 1.2f; // Tâm người Kim Đồng
        Vector3 dirToTarget = (targetPos - eyePos).normalized;

        float dstToTarget = Vector3.Distance(eyePos, targetPos);

        // Kiểm tra xem có nằm trong bán kính không
        if (dstToTarget <= currentRadius)
        {
            // Kiểm tra xem có nằm trong góc nhìn không
            if (Vector3.Angle(transform.forward, dirToTarget) < currentAngle / 2)
            {
                // Bắn tia Raycast xem có bị tường cản không
                if (!Physics.Raycast(eyePos, dirToTarget, dstToTarget, obstacleMask))
                {
                    return true; // NHÌN THẤY
                }
            }
        }
        return false;
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
        if (player == null) return;

        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= 1.5f)
        {
            // Dòng code của bạn có vẻ đang gọi tới StealthMissionManager, nếu bị lỗi đỏ bạn hãy kiểm tra lại đường dẫn nhé
            if (StealthMissionManager.Instance != null)
            {
                StealthMissionManager.Instance.FailMission("Kim Đồng bị bắt... cuộc liên lạc thất bại.");
            }

            agent.isStopped = true;
            if (anim != null) anim.SetBool("isChasing", false);
            this.enabled = false;
        }
    }
}