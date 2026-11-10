
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class SlimeGrapple : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform launchPoint;

    [Header("Grapple")]
    public float maxDistance = 35f;
    public float pullAcceleration = 35f;
    public float maxPullSpeed = 25f;
    public float stopDistance = 0f;
    public LayerMask grappleMask = ~0;

    [Header("Release Jump")]
    public float releaseJumpForce = 4f;

    [Header("Line")]
    public float lineWidth = 0.06f;

    private Rigidbody rb;
    private LineRenderer line;

    private Vector3 anchorPoint;
    private bool isGrappling;

    public bool IsGrappling => isGrappling;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.useWorldSpace = true;
        line.enabled = false;
    }

    void Update()
    {
        if (playerCamera == null)
            return;

        // 右クリック：触手を発射
        if (Input.GetMouseButtonDown(1))
        {
            TryGrapple();
        }

        // 張り付き中にSpace：解除＋ジャンプ
        if (isGrappling &&
            Input.GetKeyDown(KeyCode.Space))
        {
            ReleaseGrapple(true);
        }

        // 左クリック：通常解除のみ
        if (isGrappling &&
            Input.GetMouseButtonDown(0))
        {
            ReleaseGrapple(false);
        }

        UpdateLine();
    }

    void FixedUpdate()
    {
        if (!isGrappling)
            return;

        Vector3 toAnchor =
            anchorPoint - transform.position;

        float distance = toAnchor.magnitude;

        // 接続先に到達したら張り付き状態を解除
        if (distance <= stopDistance)
        {
            ReleaseGrapple(false);
            return;
        }

        Vector3 direction = toAnchor.normalized;

        // 接続先へ引き寄せる
        rb.AddForce(
            direction * pullAcceleration,
            ForceMode.Acceleration
        );

        // 引き寄せ方向の速度に上限を設定
        Vector3 velocity = rb.linearVelocity;

        float speedTowardAnchor =
            Vector3.Dot(velocity, direction);

        if (speedTowardAnchor > maxPullSpeed)
        {
            rb.linearVelocity =
                velocity -
                direction * (speedTowardAnchor - maxPullSpeed);
        }
    }

    void TryGrapple()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDistance,
            grappleMask,
            QueryTriggerInteraction.Ignore))
        {
            // 自分自身には接続しない
            if (hit.transform.root == transform.root)
                return;

            // 接続先を記録
            anchorPoint = hit.point;

            // 張り付き開始
            isGrappling = true;

            // 触手の線を表示
            line.enabled = true;
        }
    }

    void ReleaseGrapple(bool jump)
    {
        if (!isGrappling)
            return;

        // 張り付き解除
        isGrappling = false;

        // 触手の線を消す
        line.enabled = false;

        // Spaceで解除した場合だけジャンプ
        if (jump)
        {
            // 上方向へジャンプ
            rb.AddForce(
                Vector3.up * releaseJumpForce,
                ForceMode.Impulse
            );
        }
    }

    void UpdateLine()
    {
        if (!isGrappling || launchPoint == null)
            return;

        line.SetPosition(0, launchPoint.position);
        line.SetPosition(1, anchorPoint);
    }
}
