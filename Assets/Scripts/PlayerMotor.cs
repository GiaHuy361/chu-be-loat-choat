using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 720f; // độ/giây

    [Header("Gravity")]
    public float gravity = -20f;

    private CharacterController cc;
    private Animator anim;
    private float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Input cũ (vì bạn đã bật Both)
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S
        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);

        bool run = Input.GetKey(KeyCode.LeftShift);
        float speed = run ? runSpeed : walkSpeed;

        // hướng theo camera (đi hướng nào camera nhìn)
        Transform cam = Camera.main.transform;
        Vector3 camF = cam.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = cam.right; camR.y = 0; camR.Normalize();
        Vector3 moveDir = camF * input.z + camR * input.x;

        // xoay nhân vật theo hướng di chuyển
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // gravity
        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

<<<<<<< Updated upstream:Assets/Character/PlayerMotor.cs
        Vector3 velocity = moveDir * speed;
=======
            // giữ dính đất
            if (verticalVelocity < 0f)
                verticalVelocity = groundedStick;

            // nhảy
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // v = sqrt(2gh)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                sfx?.PlayJump();

                // [ĐÃ THÊM MỚI]: Kích hoạt Trigger nhảy
                if (anim != null) anim.SetTrigger("Jump");
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
>>>>>>> Stashed changes:Assets/Scripts/PlayerMotor.cs
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);

        // Animator Speed (0 idle, >0 walk/run)
        if (anim != null)
        {
<<<<<<< Updated upstream:Assets/Character/PlayerMotor.cs
            // dùng độ lớn input để điều khiển chuyển state
            float animSpeed = input.magnitude * (run ? 4f : 1f);
            anim.SetFloat("Speed", animSpeed);
=======
            float speed01 = Mathf.InverseLerp(0f, runSpeed, currentSpeed);
            anim.SetFloat("Speed", speed01 * (run ? 4f : 1.6f));

            // [ĐÃ THÊM MỚI]: Cập nhật trạng thái chạm đất cho Animator
            // Lưu ý: Trong Animator phải có parameter tên là "IsGrounded" (kiểu Bool)
            anim.SetBool("IsGrounded", grounded);
>>>>>>> Stashed changes:Assets/Scripts/PlayerMotor.cs
        }
    }
}