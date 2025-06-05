using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动&跳跃参数")]
    public float moveSpeed = 6f;            // 地面水平速度
    public float airControlPercent = 0.4f;  // 空中控制系数（0=无空中控制，1=与地面相同）
    public float jumpHeight = 1.6f;         // 跳跃高度（米）
    public float gravityMultiplier = 2f;    // 自定义重力倍增（>1 增强下落手感）

    [Header("接地检测")]
    public Transform groundCheck;           // 脚底检测点（若为空将自动创建）
    public float groundCheckRadius = 0.25f; // 探测球半径
    public LayerMask groundMask;            // 地面层

    private Rigidbody rb;
    private bool isGrounded;
    private Vector2 inputDir;               // 输入方向
    private float originalDrag;             // 初始阻尼

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;                 // 交给脚本控制转向
        originalDrag = rb.drag;

        // 若未指定 groundCheck，自动在脚底生成空对象
        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            CapsuleCollider col = GetComponent<CapsuleCollider>();
            groundCheck.localPosition = Vector3.down * (col.height * 0.5f - col.radius) + Vector3.up * 0.02f;
        }
    }

    void Update()
    {
        // 1️⃣ 采样输入
        inputDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // 2️⃣ 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // v = sqrt(2gh)
            float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * gravityMultiplier * jumpHeight);
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        }
    }

    void FixedUpdate()
    {
        //手动接地检测
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        //施加自定义重力（可增强下落速度）
        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);

        // 计算期望速度
        Vector3 wishDir = (transform.right * inputDir.x + transform.forward * inputDir.y).normalized;
        Vector3 wishVel = wishDir * moveSpeed;

        //应用速度（地面 vs 空中）
        float control = isGrounded ? 1f : airControlPercent;
        Vector3 velocity = rb.velocity;
        velocity.x = Mathf.Lerp(velocity.x, wishVel.x, control);
        velocity.z = Mathf.Lerp(velocity.z, wishVel.z, control);
        rb.velocity = velocity;

        //贴地时加大阻尼，空中减少阻尼
        rb.drag = isGrounded ? originalDrag : 0f;
    }

#if UNITY_EDITOR
    // 在 Scene 视图显示接地探测球
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
#endif
}
