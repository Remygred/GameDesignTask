using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerCombat : MonoBehaviour
{

    [Header("引用")]
    [SerializeField] private Camera cam;            // 射线朝向——若留空自动 Camera.main
    [SerializeField] private LayerMask enemyMask;   // 只检测敌人的层
    [SerializeField] private LayerMask obstacleMask;// 会阻挡射线的墙体层
    [SerializeField] private Transform muzzle;      // 射线发射点（为空默认用相机位置）

    [Header("普通攻击")]
    public int normalDamage = 10;   // 普通攻击伤害
    public float attackRange = 2.5f; // 最大攻击距离（米）
    public float attackRate = 0.5f; // 普攻冷却（秒）
    public float hitForce = 5f;   // 命中击退力度（对刚体敌人）

    [Header("蓄力攻击")]
    public int heavyDamage = 25;  // 蓄力攻击伤害
    public float chargeThreshold = 3f;// 长按多少秒视为蓄力完成
    public float heavyCooldown = 1.0f;// 蓄力攻击冷却（秒）

    [Header("格挡 (右键长按)")]
    [Range(0f, 1f)]
    public float blockDamageRate = 0.15f; // 格挡时保留多少伤害(0=全免,1=全吃)
    public AudioClip blockStartSfx;        // 开始格挡音
    public AudioClip blockEndSfx;          // 结束格挡音

    [Header("通用音效")]
    public AudioClip swingSfx; // 挥拳风声
    public AudioClip hitSfx;   // 命中音


    private float nextAttackTime; // 当前冷却到期时间戳
    private bool isCharging;     // 是否处于蓄力阶段
    private float chargeTimer;    // 已蓄力时间
    private bool isBlocking;     // 当前是否按住右键格挡
    private AudioSource audioSrc; // 播放器缓存

    // UI 订阅事件：蓄力条百分比 (0~1)
    public event Action<float> OnChargeUpdate;

    // 提供给其它脚本（Health）读取的只读属性
    public bool IsBlocking => isBlocking;
    public float BlockDamageRate => blockDamageRate;


    private void Start()
    {
        if (!cam) cam = Camera.main;     // 自动找主相机
        if (!muzzle) muzzle = cam.transform; // 默认射线从相机发射
        audioSrc = GetComponent<AudioSource>();

        Debug.Log("<color=cyan>[PlayerCombat]</color> 初始化完毕");
    }

    private void Update() => HandleInput();

    private void HandleInput()
    {

        if (Input.GetKeyDown(KeyCode.Mouse1))
            BeginBlock();

        if (Input.GetKeyUp(KeyCode.Mouse1))
            EndBlock();

        // 格挡时禁止攻击、蓄力
        if (isBlocking) return;

        // 鼠标左键按下：开始计时
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextAttackTime)
        {
            isCharging = true;
            chargeTimer = 0f;
            Debug.Log("<color=yellow>[PlayerCombat]</color> 开始蓄力");
        }

        // 按住左键：累积蓄力时间（UI 百分比回调）
        if (isCharging && Input.GetKey(KeyCode.Mouse0))
        {
            chargeTimer += Time.deltaTime;
            OnChargeUpdate?.Invoke(Mathf.Clamp01(chargeTimer / chargeThreshold));

            Debug.Log($"<color=yellow>[PlayerCombat]</color> 蓄力进度: {chargeTimer:F1}s");
        }

        // 左键松开：根据蓄力长短判断出手方式
        if (Input.GetKeyUp(KeyCode.Mouse0) && isCharging)
        {
            if (chargeTimer >= chargeThreshold)
            {
                // 蓄力攻击
                Debug.Log("<color=lime>[PlayerCombat]</color> 触发蓄力攻击");
                StartCoroutine(PerformHeavyAttack());
                nextAttackTime = Time.time + heavyCooldown;
            }
            else
            {
                // 普通攻击
                Debug.Log("<color=lime>[PlayerCombat]</color> 触发普通攻击");
                PerformNormalAttack();
                nextAttackTime = Time.time + attackRate;
            }

            // 重置蓄力状态 & UI
            isCharging = false;
            OnChargeUpdate?.Invoke(0f);
        }
    }

    private void BeginBlock()
    {
        if (isBlocking) return;
        isBlocking = true;
        //PlaySound(blockStartSfx);
        // TODO：在此触发格挡动画 / UI 高亮
        Debug.Log("<color=orange>[PlayerCombat]</color> 进入格挡状态");
    }

    private void EndBlock()
    {
        if (!isBlocking) return;
        isBlocking = false;
        //PlaySound(blockEndSfx);
        // TODO：关闭格挡动画 / UI
        Debug.Log("<color=orange>[PlayerCombat]</color> 退出格挡状态");
    }

    private void PerformNormalAttack()
    {
        PlaySound(swingSfx);

        // 若射线命中敌人，ApplyDamage 返回 true
        if (RayHitEnemy(out RaycastHit hit))
        {
            Debug.Log("<color=green>[PlayerCombat]</color> 普攻命中 -> " + hit.collider.name);
            ApplyDamage(hit, normalDamage);
        }
        else
        {
            Debug.Log("<color=grey>[PlayerCombat]</color> 普攻未命中");
        }
    }

    /// <summary>
    /// 协程用于等待动画/特效中的“出拳峰值”帧
    /// </summary>
    private IEnumerator PerformHeavyAttack()
    {
        //PlaySound(swingSfx);
        yield return new WaitForSeconds(0.1f); // 0.1 秒后判定命中

        if (RayHitEnemy(out RaycastHit hit))
        {
            ApplyDamage(hit, heavyDamage);
            Debug.Log("<color=red>[PlayerCombat]</color> 蓄力命中 -> " + hit.collider.name);
        }
        else
        {
            Debug.Log("<color=grey>[PlayerCombat]</color> 蓄力未命中");
        }
    }

    /// <summary>
    /// 发射一条射线，优先检测障碍→敌人；只在可攻击距离内返回真
    /// </summary>
    private bool RayHitEnemy(out RaycastHit hitInfo)
    {
        if (Physics.Raycast(muzzle.position, muzzle.forward,
                            out hitInfo,
                            attackRange,
                            obstacleMask | enemyMask,
                            QueryTriggerInteraction.Ignore))
        {
            // 判断首个命中是否敌人 (在 enemyMask 内)
            bool isEnemy = ((1 << hitInfo.collider.gameObject.layer) & enemyMask.value) != 0;
            return isEnemy;
        }
        return false;
    }

    /// <summary>
    /// 对命中的敌人扣血 / 击退，并发送 UI 命中反馈
    /// </summary>
    private void ApplyDamage(RaycastHit hit, int dmg)
    {
        PlaySound(hitSfx);

        // 击退（若敌人带刚体）
        if (hit.rigidbody)
            hit.rigidbody.AddForce(muzzle.forward * hitForce, ForceMode.Impulse);

        // 扣血
        if (hit.collider.TryGetComponent(out EnemyAI enemy))
            enemy.TakeDamage(dmg);

        // 命中 UI 闪烁（可在 UIManager 实现）
        FindObjectOfType<UIManager>()?.HitFlash();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip) audioSrc.PlayOneShot(clip);
    }
}
