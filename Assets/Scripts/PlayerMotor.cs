using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.5f;
    public float sprintSpeed = 8.0f;
    public float speedSmoothTime = 0.1f;

    [Header("Crouch / Prone")]
    public float crouchSpeed = 2.0f;
    public float proneSpeed = 1.0f;

    [Header("Rotation")]
    public float turnSmoothTime = 0.1f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -15.0f;
    public float groundedGravity = -5.0f;

    private CharacterController cc;
    private Animator anim;
    private FootstepReceiver sfx;
    private Transform camTransform;

    private float currentSpeed;
    private float speedSmoothVelocity;
    private float turnSmoothVelocity;
    private float verticalVelocity;

    private bool isCrouching;
    private bool isProne;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        sfx = GetComponent<FootstepReceiver>();

        if (Camera.main != null)
            camTransform = Camera.main.transform;
        else
            Debug.LogError("Không tìm thấy Main Camera!");
    }

    void Update()
    {
        HandleStance();

        Vector3 horizontalMove = HandleMovement();
        HandleGravityAndMove(horizontalMove);
    }

    // =========================
    // STANCE
    // =========================
    void HandleStance()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            if (isCrouching) isProne = false;
            UpdateAnimatorStance();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            isProne = !isProne;
            if (isProne) isCrouching = false;
            UpdateAnimatorStance();
        }
    }

    void UpdateAnimatorStance()
    {
        if (anim == null) return;

        anim.SetBool("IsCrouch", isCrouching);
        anim.SetBool("IsProne", isProne);
    }

    // =========================
    // MOVEMENT (KHÔNG Move ở đây)
    // =========================
    Vector3 HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(h, 0f, v).normalized;

        float targetSpeed = 0f;

        if (direction.magnitude >= 0.1f)
        {
            if (isProne)
                targetSpeed = proneSpeed;
            else if (isCrouching)
                targetSpeed = crouchSpeed;
            else
                targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : runSpeed;
        }

        // Smooth tốc độ
        currentSpeed = Mathf.SmoothDamp(
            currentSpeed,
            targetSpeed,
            ref speedSmoothVelocity,
            speedSmoothTime
        );

        Vector3 horizontalMove = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg
                                + camTransform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            horizontalMove = moveDir.normalized * currentSpeed;
        }
        else
        {
            // Momentum nhẹ khi thả phím
            horizontalMove = transform.forward * currentSpeed;
        }

        // Update Animator
        if (anim != null)
        {
            float animationSpeedPercent = currentSpeed / sprintSpeed;
            anim.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime);
        }

        return horizontalMove;
    }

    // =========================
    // GRAVITY + FINAL MOVE (Move 1 lần duy nhất)
    // =========================
    void HandleGravityAndMove(Vector3 horizontalMove)
    {
        bool isGrounded = cc.isGrounded;

        if (anim != null)
            anim.SetBool("IsGrounded", isGrounded);

        if (isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = groundedGravity;

            if (Input.GetKeyDown(KeyCode.Space) && !isCrouching && !isProne)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (anim != null)
                    anim.SetTrigger("Jump");

                if (sfx != null)
                    sfx.PlayJump();
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMove =
            horizontalMove * Time.deltaTime +
            Vector3.up * verticalVelocity * Time.deltaTime;

        cc.Move(finalMove);
    }

    // =========================
    // Animation Events
    // =========================
    public void OnLand()
    {
        if (sfx != null)
            sfx.PlayLand();
    }
}
