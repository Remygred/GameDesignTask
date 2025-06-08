using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("鼠标控制")]
    [Tooltip("鼠标灵敏度")]
    public float lookSensitivity = 100f;
    [Tooltip("头部最大偏航角度 (°)")]
    public float maxHeadTurnAngle = 22.5f;
    [Tooltip("角色转身速度 (°/s)")]
    public float bodyTurnSpeed = 90f;

    private Transform player;       // 玩家 Transform
    [SerializeField]
    private Transform headTransform; // 如果需要，可在 Inspector 指定

    private float pitch = 0f;       // 俯仰角
    private float yawOffset = 0f;   // 偏航角度

    void Start()
    {
        // 锁鼠标并隐藏
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 找到玩家
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("CameraLook: 未找到标签为 Player 的对象！");
            enabled = false;
            return;
        }
        player = playerObj.transform;
        yawOffset = 0f;
    }

    void Update()
    {
        if (player == null) return;

        // 鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        // 俯仰
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        // 偏航累加
        yawOffset += mouseX;
        if (yawOffset > 180f) yawOffset -= 360f;
        if (yawOffset < -180f) yawOffset += 360f;

        // 超过头部可转范围时，转动角色
        if (Mathf.Abs(yawOffset) > maxHeadTurnAngle)
        {
            float sign = Mathf.Sign(yawOffset);
            float rotateAmount = sign * bodyTurnSpeed * Time.deltaTime;
            if (Mathf.Abs(rotateAmount) > Mathf.Abs(yawOffset))
                rotateAmount = yawOffset;
            // 旋转玩家身体
            player.Rotate(0f, rotateAmount, 0f);
            yawOffset -= rotateAmount;
        }

        // 应用摄像机旋转
        transform.rotation = Quaternion.Euler(pitch, player.eulerAngles.y + yawOffset, 0f);
    }
}
