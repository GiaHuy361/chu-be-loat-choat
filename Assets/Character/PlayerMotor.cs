using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Speed")]
    public float walkSpeed = 5.5f;
    public float runSpeed = 9.0f;

    [Header("Rotation")]
    public float rotationSmooth = 16f;

    [Header("Acceleration")]
    public float acceleration = 30f;
    public float deceleration = 40f;

    [Header("Jump")]
    public float jumpHeight = 1.6f;     // tăng lên nếu muốn nhảy cao hơn
    public float gravity = -25f;        // bạn đang để -25 OK
    public float groundedStick = -2f;   // dính đất cho ổn định

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
        sfx = GetComponent<FootstepReceiver>(); // có thì phát sound, không có thì thôi
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A / D
        float v = Input.GetAxisRaw("Vertical");   // W / S
        bool run = Input.GetKey(KeyCode.LeftShift);

        bool grounded = cc.isGrounded;

        /* =====================
           SPEED
           ===================== */
        bool isMovingForwardOrBack = Mathf.Abs(v) > 0.01f;

        float baseSpeed = run ? runSpeed : walkSpeed;
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
            // vừa chạm đất
            if (!wasGrounded)
                sfx?.PlayLand();

            // giữ dính đất
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStick;

            // nhảy
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // v = sqrt(2gh)
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
            float speed01 = Mathf.InverseLerp(0f, runSpeed, currentSpeed);
            anim.SetFloat("Speed", speed01 * (run ? 4f : 1.6f));

            // Nếu sau này bạn thêm parameter thì mở:
            // anim.SetBool("Grounded", grounded);
            // anim.SetFloat("YVel", verticalVelocity);
        }
    }
}
