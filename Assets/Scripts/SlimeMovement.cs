
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlimeMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float groundAcceleration = 25f;
    public float airAcceleration = 8f;
    public float jumpForce = 7f;

    [Header("Air Control")]
    public float maxAirSpeed = 18f;

    // 空中で入力を離したときの減速
    // 0にすると空中の水平慣性を維持
    public float airDrag = 0.5f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.65f;
    public LayerMask groundMask = ~0;

    [Header("References")]
    public Transform cameraTransform;

    public bool IsGrounded { get; private set; }

    private Rigidbody rb;
    private Vector2 input;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        input = Vector2.ClampMagnitude(input, 1f);

        // Space入力を記録
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        CheckGround();

        // カメラ基準の移動方向
        Vector3 forward = cameraTransform != null
            ? cameraTransform.forward
            : transform.forward;

        Vector3 right = cameraTransform != null
            ? cameraTransform.right
            : transform.right;

        // 水平方向だけ使用
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * input.y + right * input.x;

        moveDirection = Vector3.ClampMagnitude(
            moveDirection, 1f
        );

        // 現在速度
        Vector3 velocity = rb.linearVelocity;

        // 水平速度だけ取り出す
        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        // 地上・空中で加速度を切り替える
        float acceleration = IsGrounded
            ? groundAcceleration
            : airAcceleration;

        Vector3 targetVelocity =
            moveDirection * moveSpeed;

        // 入力がある場合は方向を制御
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else if (IsGrounded)
        {
            // 地上では入力を離すと減速
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                groundAcceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // 空中では慣性を残しながら少しだけ減速
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                airDrag * Time.fixedDeltaTime
            );
        }

        // 空中水平速度の上限
        if (!IsGrounded)
        {
            horizontalVelocity = Vector3.ClampMagnitude(
                horizontalVelocity,
                maxAirSpeed
            );
        }

        // 水平速度のみ更新
        // Y方向はRigidbodyの重力に任せる
        rb.linearVelocity = new Vector3(
            horizontalVelocity.x,
            velocity.y,
            horizontalVelocity.z
        );

        // 通常ジャンプ
        if (jumpRequested && IsGrounded)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );
        }

        jumpRequested = false;
    }

    void CheckGround()
    {
        IsGrounded = Physics.SphereCast(
            transform.position,
            0.35f,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
