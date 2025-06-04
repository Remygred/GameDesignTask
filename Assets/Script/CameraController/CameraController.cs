using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController: MonoBehaviour
{
    [Header("绑定目标")]
    [Tooltip("需要跟随的目标物体")]
    public Transform target;

    [Header("位置参数")]
    [Tooltip("相机与目标的水平距离")]
    public float distance = 10f;
    [Tooltip("相机的固定高度")]
    public float height = 5f;
    [Tooltip("垂直观察偏移（看向目标上方）")]
    public float verticalLookOffset = 1.5f;

    [Header("移动参数")]
    [Tooltip("跟随平滑时间")]
    public float smoothTime = 0.15f;
    [Tooltip("预判距离")]
    public float lookAheadDist = 2f;
    [Tooltip("预判响应速度")]
    public float lookAheadSpeed = 3f;

    [Header("防穿墙设置")]
    [Tooltip("启用碰撞检测")]
    public bool enableCollisionDetection = true;
    [Tooltip("碰撞检测半径")]
    public float collisionRadius = 0.5f;
    [Tooltip("最小相机距离")]
    public float minDistance = 2f;
    [Header("屏幕震动")]
    [Tooltip("最大震动幅度")]
    public float maxShakeMagnitude = 0.5f;
    [Tooltip("震动衰减曲线")]
    public AnimationCurve shakeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    private Vector3 originalLocalPosition;
    private Coroutine shakeCoroutine;

    private Vector3 velocity;
    private Vector3 currentLookAhead;
    private Vector3 lastTargetPos;
    private float originalDistance;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("未绑定目标物体！");
            enabled = false;
            return;
        }

        originalDistance = distance;
        lastTargetPos = target.position;
        InitializeCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null) return;

        UpdateLookAhead();
        Vector3 targetPosition = CalculateIdealPosition();

        if (enableCollisionDetection)
            HandleObstacles(ref targetPosition);

        SmoothFollow(targetPosition);
        LookAtTarget();

        lastTargetPos = target.position;
    }

    void InitializeCameraPosition()
    {
        Vector3 backDirection = -target.forward.normalized;
        transform.position = target.position +
                            backDirection * originalDistance +
                            Vector3.up * height;

        transform.LookAt(target.position + Vector3.up * verticalLookOffset);
    }

    void UpdateLookAhead()
    {
        Vector3 delta = target.position - lastTargetPos;
        Vector3 horizontalDelta = new Vector3(delta.x, 0, delta.z);

        if (horizontalDelta.magnitude > 0.1f)
        {
            Vector3 direction = horizontalDelta.normalized;
            currentLookAhead = Vector3.Lerp(
                currentLookAhead,
                direction * lookAheadDist,
                Time.deltaTime * lookAheadSpeed);
        }
        else
        {
            currentLookAhead = Vector3.Lerp(
                currentLookAhead,
                Vector3.zero,
                Time.deltaTime * lookAheadSpeed);
        }
    }

    Vector3 CalculateIdealPosition()
    {
        Vector3 backDirection = -target.forward.normalized;
        return target.position +
              backDirection * distance +
              Vector3.up * height +
              currentLookAhead;
    }

    void HandleObstacles(ref Vector3 targetPos)
    {
        RaycastHit hit;
        Vector3 direction = (targetPos - target.position).normalized;
        float maxDistance = Vector3.Distance(target.position, targetPos);

        if (Physics.SphereCast(
            target.position,
            collisionRadius,
            direction,
            out hit,
            maxDistance))
        {
            distance = Mathf.Clamp(
                hit.distance - collisionRadius,
                minDistance,
                originalDistance);
        }
        else
        {
            distance = originalDistance;
        }
    }

    void LookAtTarget()
    {
        Vector3 lookPoint = target.position +
                           Vector3.up * verticalLookOffset +
                           currentLookAhead * 0.3f;

        transform.LookAt(lookPoint);
    }

    // 调试绘制
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(target.position, transform.position);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(CalculateIdealPosition(), 0.5f);

        if (enableCollisionDetection)
        {
            Gizmos.color = Color.cyan;
            Vector3 direction = (transform.position - target.position).normalized;
            Gizmos.DrawLine(target.position, target.position + direction * distance);
        }
    }

    /// <summary>
    /// 触发屏幕震动
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="magnitude">震动强度（0~1）</param>
    public void TriggerShake(float duration = 0.5f, float magnitude = 0.3f)
    {
        // 确保强度在安全范围内
        magnitude = Mathf.Clamp01(magnitude);

        // 如果已有震动正在执行，先停止
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude * maxShakeMagnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        originalLocalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 使用曲线控制衰减
            float curveProgress = shakeCurve.Evaluate(elapsed / duration);

            // 生成随机偏移（三维震动）
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-0.2f, 0.2f)) * magnitude * curveProgress;

            // 应用偏移（保持原有跟随逻辑）
            transform.localPosition = originalLocalPosition + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复原始位置
        transform.localPosition = originalLocalPosition;
        shakeCoroutine = null;
    }

    // 修改原有SmoothFollow方法，避免与震动冲突
    void SmoothFollow(Vector3 targetPosition)
    {
        // 先执行基础跟随
        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime);

        // 保留震动偏移量
        if (shakeCoroutine != null)
        {
            smoothPosition += transform.localPosition - originalLocalPosition;
        }

        transform.position = smoothPosition;
    }
}