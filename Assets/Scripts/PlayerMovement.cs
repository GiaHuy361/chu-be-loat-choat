using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float move = Input.GetAxis("Vertical"); // W/S
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = Mathf.Abs(move) * (isRunning ? runSpeed : walkSpeed);

        animator.SetFloat("Speed", speed);
    }
}
