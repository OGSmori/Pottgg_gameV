
using UnityEngine;

public class SlimeCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera")]
    public float distance = 6f;
    public float height = 1.5f;
    public float mouseSensitivity = 3f;
    public float pitchMin = -35f;
    public float pitchMax = 70f;
    public float followSmoothness = 15f;

    private float yaw;
    private float pitch = 10f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("SlimeCamera: Targetが未設定です");
            enabled = false;
            return;
        }

        yaw = target.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // マウスで照準方向を操作
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(
            pitch,
            yaw,
            0f
        );

        // カメラの注視位置
        Vector3 focusPoint =
            target.position + Vector3.up * height;

        // カメラをプレイヤーの後方へ配置
        Vector3 desiredPosition =
            focusPoint - rotation * Vector3.forward * distance;

        // 滑らかに追従
        float smooth =
            1f - Mathf.Exp(-followSmoothness * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smooth
        );

        transform.rotation = rotation;
    }
}
