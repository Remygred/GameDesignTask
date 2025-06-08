using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("������")]
    [Tooltip("���������")]
    public float lookSensitivity = 100f;
    [Tooltip("ͷ�����ƫ���Ƕ� (��)")]
    public float maxHeadTurnAngle = 22.5f;
    [Tooltip("��ɫת���ٶ� (��/s)")]
    public float bodyTurnSpeed = 90f;

    private Transform player;       // ��� Transform
    [SerializeField]
    private Transform headTransform; // �����Ҫ������ Inspector ָ��

    private float pitch = 0f;       // ������
    private float yawOffset = 0f;   // ƫ���Ƕ�

    void Start()
    {
        // ����겢����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // �ҵ����
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("CameraLook: δ�ҵ���ǩΪ Player �Ķ���");
            enabled = false;
            return;
        }
        player = playerObj.transform;
        yawOffset = 0f;
    }

    void Update()
    {
        if (player == null) return;

        // �������
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        // ����
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        // ƫ���ۼ�
        yawOffset += mouseX;
        if (yawOffset > 180f) yawOffset -= 360f;
        if (yawOffset < -180f) yawOffset += 360f;

        // ����ͷ����ת��Χʱ��ת����ɫ
        if (Mathf.Abs(yawOffset) > maxHeadTurnAngle)
        {
            float sign = Mathf.Sign(yawOffset);
            float rotateAmount = sign * bodyTurnSpeed * Time.deltaTime;
            if (Mathf.Abs(rotateAmount) > Mathf.Abs(yawOffset))
                rotateAmount = yawOffset;
            // ��ת�������
            player.Rotate(0f, rotateAmount, 0f);
            yawOffset -= rotateAmount;
        }

        // Ӧ���������ת
        transform.rotation = Quaternion.Euler(pitch, player.eulerAngles.y + yawOffset, 0f);
    }
}
