using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Speed")]
    public float walkSpeed = 5.5f;
    public float runSpeed = 9.0f;

    [Header("Crouch / Prone Speed")]
    public float crouchSpeed = 2.5f;
    public float proneSpeed = 1.5f;

    [Header("Fast Boost (Shift)")]
    public float crouchFastMul = 1.6f;
    public float proneFastMul = 1.4f;

    [Header("Rotation")]
    public float rotationSmooth = 16f;

    [Header("Acceleration")]
    public float acceleration = 30f;
    public float deceleration = 40f;

    [Header("Jump")]
    public float jumpHeight = 1.6f;
    public float gravity = -25f;
    public float groundedStick = -2f;

    private CharacterController cc;
    private Animator anim;
    private FootstepReceiver sfx;

    private float verticalVelocity;
    private float currentSpeed;
    private bool wasGrounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        sfx = GetComponent<FootstepReceiver>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool shift = Input.GetKey(KeyCode.LeftShift);

        bool grounded = cc.isGrounded;

        /* =====================
           CROUCH / PRONE TOGGLE
           ===================== */
        if (anim != null)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                bool nextCrouch = !anim.GetBool("IsCrouch");
                anim.SetBool("IsCrouch", nextCrouch);
                if (nextCrouch) anim.SetBool("IsProne", false);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                bool nextProne = !anim.GetBool("IsProne");
                anim.SetBool("IsProne", nextProne);
                if (nextProne) anim.SetBool("IsCrouch", false);
            }
        }

        bool isCrouch = anim != null && anim.GetBool("IsCrouch");
        bool isProne = anim != null && anim.GetBool("IsProne");

        /* =====================
           CAMERA DIRECTION
           ===================== */
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;
        bool isMoving = moveDir.sqrMagnitude > 0.01f;

        /* =====================
           SPEED
           ===================== */
        float baseSpeed;

        if (!isCrouch && !isProne)
        {
            baseSpeed = shift ? runSpeed : walkSpeed;
        }
        else if (isCrouch)
        {
            baseSpeed = crouchSpeed;
            if (shift) baseSpeed *= crouchFastMul;
        }
        else
        {
            baseSpeed = proneSpeed;
            if (shift) baseSpeed *= proneFastMul;
        }

        float targetSpeed = isMoving ? baseSpeed : 0f;
        float accel = currentSpeed < targetSpeed ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            accel * Time.deltaTime
        );

        /* =====================
           ROTATION
           ===================== */
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmooth * Time.deltaTime
            );
        }

        /* =====================
           JUMP + GRAVITY
           ===================== */
        if (grounded)
        {
            if (!wasGrounded)
                sfx?.PlayLand();

            if (verticalVelocity < 0)
                verticalVelocity = groundedStick;

            anim?.SetBool("IsGrounded", true);

            if (!isCrouch && !isProne && Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                anim?.SetTrigger("Jump");
                sfx?.PlayJump();
            }
        }
        else
        {
            anim?.SetBool("IsGrounded", false);
            verticalVelocity += gravity * Time.deltaTime;
        }

        wasGrounded = grounded;

        /* =====================
           MOVE
           ===================== */
        Vector3 velocity = moveDir.normalized * currentSpeed;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);

        /* =====================
           ANIMATOR
           ===================== */
        if (anim != null)
        {
            float speed01 = Mathf.InverseLerp(0f, runSpeed, currentSpeed);
            anim.SetFloat("Speed", speed01);
        }
    }

    // ==== PHẦN MỚI THÊM: HÀM NHẬN ANIMATION EVENT OnLand ====
    // Gắn hàm này để clip JumpLand gọi, tránh lỗi "AnimationEvent 'OnLand' ... has no receiver"
    public void OnLand()
    {
        // đảm bảo trạng thái grounded đúng khi animation chạm đất
        if (anim != null)
        {
            anim.SetBool("IsGrounded", true);
        }

        // reset vận tốc rơi để nhân vật ổn định sau khi đáp
        if (verticalVelocity < 0)
        {
            verticalVelocity = groundedStick;
        }

        // chơi âm thanh đáp đất nếu có
        sfx?.PlayLand();

        Debug.Log("OnLand() called from AnimationEvent");
    }
}
