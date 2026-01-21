using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Speed")]
    public float walkSpeed = 5.5f;
    public float runSpeed = 9.0f;

    [Header("Crouch / Prone Speed")]
    public float crouchSpeed = 2.5f;   // tốc độ khi khom
    public float proneSpeed = 1.5f;    // tốc độ khi bò

    [Header("Fast Boost (Shift)")]
    public float crouchFastMul = 1.6f; // Shift khi khom
    public float proneFastMul = 1.4f;  // Shift khi bò

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
        float h = Input.GetAxisRaw("Horizontal"); // A / D
        float v = Input.GetAxisRaw("Vertical");   // W / S
        bool shift = Input.GetKey(KeyCode.LeftShift); // Shift (boost)
        bool run = shift; // giữ biến run để không sửa nhiều chỗ

        bool grounded = cc.isGrounded;

        /* =====================
           CROUCH / PRONE TOGGLE
           ===================== */
        if (anim != null)
        {
            // Crouch toggle (C)
            if (Input.GetKeyDown(KeyCode.C))
            {
                bool nextCrouch = !anim.GetBool("IsCrouch");
                anim.SetBool("IsCrouch", nextCrouch);

                if (nextCrouch) anim.SetBool("IsProne", false);
            }

            // Prone toggle (Z)
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
           SPEED
           ===================== */
        bool isMovingForwardOrBack = Mathf.Abs(v) > 0.01f;

        float baseSpeed;

        // ===== ĐỨNG =====
        if (!isCrouch && !isProne)
        {
            baseSpeed = run ? runSpeed : walkSpeed;
        }
        // ===== KHOM =====
        else if (isCrouch)
        {
            baseSpeed = crouchSpeed;
            if (shift) baseSpeed *= crouchFastMul; // Shift = khom nhanh
        }
        // ===== BÒ =====
        else // isProne
        {
            baseSpeed = proneSpeed;
            if (shift) baseSpeed *= proneFastMul; // Shift = bò nhanh
        }

        float targetSpeed = isMovingForwardOrBack ? baseSpeed : 0f;

        if (currentSpeed < targetSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime * baseSpeed);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime * baseSpeed);

        /* =====================
           CAMERA DIRECTION
           ===================== */
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;

        /* =====================
           ROTATION
           ===================== */
        if (isMovingForwardOrBack)
        {
            Vector3 faceDir = camForward;
            faceDir.y = 0f;

            Quaternion targetRot = Quaternion.LookRotation(faceDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmooth * Time.deltaTime);
        }

        /* =====================
           JUMP + GRAVITY
           ===================== */
        if (grounded)
        {
            if (!wasGrounded)
                sfx?.PlayLand();

            if (verticalVelocity < 0f)
                verticalVelocity = groundedStick;

            // đang crouch/prone thì không cho nhảy
            if (!isCrouch && !isProne && Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                sfx?.PlayJump();
            }
        }
        else
        {
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
            // Nếu bạn dùng Speed để blend, để 0..1 ổn định nhất
            float speed01 = Mathf.InverseLerp(0f, runSpeed, currentSpeed);
            anim.SetFloat("Speed", speed01);
        }
    }
}
