using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public float lookSensitivity = 100f;    // 鼠标灵敏度
    public float maxHeadTurnAngle = 22.5f;    // 头部相对角色前方的最大偏航角度阈值（超过后旋转身体）
    public float bodyTurnSpeed = 90f;       // 角色旋转速度（度/秒）
    public float headHeight = 1f;         // 如果无 Head 节点，摄像机相对于角色底部的高度（米）

    private Transform player;               // 玩家主体 Transform
    private Transform headTransform;        // 玩家头部 Transform 或玩家自身
    private float pitch = 0f;               // 摄像机俯仰角度
    private float yawOffset = 0f;           // 摄像机相对角色的偏航角度（度）

    void Start()
    {
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 查找玩家对象
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("未找到标签为 Player 的对象！");
            return;
        }
        player = playerObj.transform;

        // 尝试获取名为 "Head" 的子节点作为头部
        headTransform = player.Find("Head");
        if (headTransform != null)
        {
            transform.SetParent(headTransform);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            // 若无 Head 节点，则将摄像机挂到玩家上并使用预设偏移
            transform.SetParent(player);
            transform.localPosition = new Vector3(0, headHeight, 0);
        }
        transform.localRotation = Quaternion.identity;

        // 初始化偏航角（与角色初始朝向对齐）
        yawOffset = 0f;
    }

    void Update()
    {
        // 获取鼠标增量输入
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        // 调整俯仰角度并限制范围，避免上下翻转
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        // 累加偏航角度
        yawOffset += mouseX;
        // 将 yawOffset 控制在 [-180,180] 范围内
        if (yawOffset > 180f) yawOffset -= 360f;
        if (yawOffset < -180f) yawOffset += 360f;

        // 如果偏航角度超过阈值，则以固定速度旋转角色本身
        if (Mathf.Abs(yawOffset) > maxHeadTurnAngle)
        {
            float sign = Mathf.Sign(yawOffset);
            float rotateAmount = sign * bodyTurnSpeed * Time.deltaTime;
            // 若旋转量大于剩余偏航，限制旋转量
            if (Mathf.Abs(rotateAmount) > Mathf.Abs(yawOffset))
                rotateAmount = yawOffset;
            // 旋转角色
            player.Rotate(0, rotateAmount, 0);
            // 调整偏航角，使其慢慢恢复至阈值范围内
            yawOffset -= rotateAmount;
        }

        // 应用摄像机局部旋转（先俯仰后偏航偏移）
        transform.localRotation = Quaternion.Euler(pitch, yawOffset, 0);
    }
}
