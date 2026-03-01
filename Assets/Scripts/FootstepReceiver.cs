using UnityEngine;

public class FootstepReceiver : MonoBehaviour
{
    [Header("Tham chiếu cơ bản")]
    public AudioSource audioSource;
    public Animator animator;

    [Header("Clips")]
    public AudioClip[] footstepClips;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float footstepVolume = 0.8f;
    [Range(0f, 1f)] public float jumpVolume = 0.9f;
    [Range(0f, 1f)] public float landVolume = 0.9f;

    [Header("Cài đặt Báo động Stealth (Tiếng ồn)")]
    [Tooltip("HÃY CHỌN LAYER ENEMY Ở ĐÂY NHÉ!")]
    public LayerMask enemyLayer;
    public float sprintNoiseRadius = 12f;
    public float runNoiseRadius = 7f;
    public float crouchNoiseRadius = 3f;
    public float proneNoiseRadius = 1f;
    public float jumpNoiseRadius = 5f;
    public float landNoiseRadius = 10f;

    [Header("Cài đặt Timer (Tự động phát tiếng)")]
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;
    public float crouchStepInterval = 0.6f;
    public float proneStepInterval = 0.8f;
    private float stepTimer;

    // Biến theo dõi trạng thái nhảy
    private bool wasGrounded = true;

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        if (animator == null) return;

        // --- XỬ LÝ ÂM THANH NHẢY / TIẾP ĐẤT TỰ ĐỘNG ---
        bool isGrounded = animator.GetBool("IsGrounded");

        if (!isGrounded && wasGrounded)
        {
            PlayJump(); // Vừa rời khỏi mặt đất
        }
        else if (isGrounded && !wasGrounded)
        {
            PlayLand(); // Vừa chạm mặt đất
        }
        wasGrounded = isGrounded;

        // --- XỬ LÝ ÂM THANH BƯỚC CHÂN ---
        float speedPercent = animator.GetFloat("Speed");
        bool isCrouching = animator.GetBool("IsCrouch");
        bool isProne = animator.GetBool("IsProne");

        if (speedPercent > 0.1f && isGrounded) // Chỉ phát tiếng bước chân khi chạm đất
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                OnFootstep();

                if (isProne) stepTimer = proneStepInterval;
                else if (isCrouching) stepTimer = crouchStepInterval;
                else if (speedPercent > 0.8f) stepTimer = runStepInterval;
                else stepTimer = walkStepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    public void OnFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0 && audioSource)
        {
            var clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip, footstepVolume);
        }

        if (animator != null)
        {
            float speedPercent = animator.GetFloat("Speed");
            bool isCrouching = animator.GetBool("IsCrouch");
            bool isProne = animator.GetBool("IsProne");

            float currentRadius = 0f;
            if (isProne) currentRadius = proneNoiseRadius;
            else if (isCrouching) currentRadius = crouchNoiseRadius;
            else if (speedPercent > 0.8f) currentRadius = sprintNoiseRadius;
            else currentRadius = runNoiseRadius;

            EmitNoise(currentRadius);
        }
    }

    public void PlayJump()
    {
        if (jumpClip && audioSource) audioSource.PlayOneShot(jumpClip, jumpVolume);
        EmitNoise(jumpNoiseRadius);
    }

    public void PlayLand()
    {
        if (landClip && audioSource) audioSource.PlayOneShot(landClip, landVolume);
        EmitNoise(landNoiseRadius);
    }

    private void EmitNoise(float radius)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        foreach (Collider enemy in enemies)
        {
            GuardRandomPatrol guard = enemy.GetComponent<GuardRandomPatrol>();
            if (guard != null)
            {
                guard.HearSound(transform.position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, runNoiseRadius);
    }
}