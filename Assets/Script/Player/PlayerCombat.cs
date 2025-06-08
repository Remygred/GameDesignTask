using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerCombat : MonoBehaviour
{
    #region Inspector 参数
    [Header("引用")]
    [SerializeField] private Camera cam;              // 射线朝向――若留空自动 Camera.main
    [SerializeField] private LayerMask enemyMask;     // 只检测敌人的层
    [SerializeField] private LayerMask obstacleMask;  // 会阻挡射线的墙体层
    [SerializeField] private Transform muzzle;        // 射线发射点（为空默认用相机位置）

    [Header("攻击基础")]
    public int normalDamage = 10;   // 最低伤害
    public int heavyDamage = 25;   // 满蓄伤害
    public float hitForce = 5f;   // 最低击退力度
    public float attackRange = 2.5f; // 最大攻击距离
    public float attackRate = 0.5f; // 最低冷却
    public float heavyCooldown = 1.0f; // 满蓄冷却
    public float chargeThreshold = 3f;// 达到满蓄所需时间（秒）

    [Header("格挡 (右键长按)")]
    [Range(0f, 1f)] public float blockDamageRate = 0.15f;
    public AudioClip blockStartSfx, blockEndSfx;

    [Header("通用音效")]
    public AudioClip swingSfx, hitSfx;
    #endregion

    #region 私有字段
    private float nextAttackTime;
    private bool isCharging;
    private float chargeTimer;
    private bool isBlocking;
    private AudioSource audioSrc;
    #endregion

    // UI 订阅：蓄力百分比
    public event Action<float> OnChargeUpdate;

    public bool IsBlocking => isBlocking;
    public float BlockDamageRate => blockDamageRate;
    public bool IsCharging => isCharging;

    // ──────────────────────────────
    void Start()
    {
        if (!cam) cam = Camera.main;
        if (!muzzle) muzzle = cam.transform;
        audioSrc = GetComponent<AudioSource>();
        Debug.Log("<color=cyan>[PlayerCombat]</color> 初始化完毕");
    }

    void Update() => HandleInput();

    // ──────────────────────────────
    #region 输入处理
    private void HandleInput()
    {
        // —— 格挡 —— //
        if (Input.GetKeyDown(KeyCode.Mouse1)) BeginBlock();
        if (Input.GetKeyUp(KeyCode.Mouse1)) EndBlock();
        if (isBlocking) return; // 格挡期间不能攻击

        // —— 开始蓄力 —— //
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextAttackTime)
        {
            isCharging = true;
            chargeTimer = 0f;
            Debug.Log("<color=yellow>[PlayerCombat]</color> 开始蓄力");
        }

        // —— 蓄力中 —— //
        if (isCharging && Input.GetKey(KeyCode.Mouse0))
        {
            chargeTimer += Time.deltaTime;
            OnChargeUpdate?.Invoke(Mathf.Clamp01(chargeTimer / chargeThreshold));
        }

        // —— 松开出拳 —— //
        if (Input.GetKeyUp(KeyCode.Mouse0) && isCharging)
        {
            float scale = Mathf.Clamp01(chargeTimer / chargeThreshold); // 0~1
            int dmg = Mathf.RoundToInt(Mathf.Lerp(normalDamage, heavyDamage, scale));
            float force = Mathf.Lerp(hitForce, hitForce * heavyDamage / normalDamage, scale);
            float cd = Mathf.Lerp(attackRate, heavyCooldown, scale);

            Debug.Log($"<color=lime>[PlayerCombat]</color> 出拳—蓄力系数 {scale:P0} | 伤害 {dmg} | 击退 {force:F1} | CD {cd:F1}s");

            StartCoroutine(PerformAttack(dmg, force)); // 协程中 0.1s 对齐动画
            nextAttackTime = Time.time + cd;

            // 重置状态
            isCharging = false;
            OnChargeUpdate?.Invoke(0f);
        }
    }

    // 强制取消蓄力/攻击，用于被击中时
    public void CancelCharging()
    {
        if (isCharging)
        {
            isCharging = false;
            OnChargeUpdate?.Invoke(0f);
            Debug.Log("<color=grey>[PlayerCombat]</color> 蓄力被打断");
        }
        StopAllCoroutines();
    }
    #endregion

    // ──────────────────────────────
    #region 格挡
    private void BeginBlock()
    {
        if (isBlocking) return;
        isBlocking = true;
        //PlaySound(blockStartSfx);
        Debug.Log("<color=orange>[PlayerCombat]</color> 进入格挡状态");
    }
    private void EndBlock()
    {
        if (!isBlocking) return;
        isBlocking = false;
        //PlaySound(blockEndSfx);
        Debug.Log("<color=orange>[PlayerCombat]</color> 退出格挡状态");
    }
    #endregion

    // ──────────────────────────────
    #region 攻击核心
    private IEnumerator PerformAttack(int finalDamage, float finalForce)
    {
        //PlaySound(swingSfx);
        yield return new WaitForSeconds(0.1f); // 对齐动画峰值

        if (RayHitEnemy(out RaycastHit hit))
        {
            Debug.Log($"<color=green>[PlayerCombat]</color> 命中 {hit.collider.name} | 伤害 {finalDamage}");
            ApplyDamage(hit, finalDamage, finalForce);
        }
        else
        {
            Debug.Log("<color=grey>[PlayerCombat]</color> 未命中");
        }
    }

    /// <summary>
    /// 发射射线检测敌人；首碰到障碍立即终止
    /// </summary>
    private bool RayHitEnemy(out RaycastHit hitInfo)
    {
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hitInfo,
                            attackRange, obstacleMask | enemyMask,
                            QueryTriggerInteraction.Ignore))
        {
            bool isEnemy = ((1 << hitInfo.collider.gameObject.layer) & enemyMask.value) != 0;
            Debug.DrawLine(muzzle.position, hitInfo.point, isEnemy ? Color.red : Color.blue, 0.2f);
            return isEnemy;
        }
        return false;
    }

    private void ApplyDamage(RaycastHit hit, int dmg, float force)
    {
        PlaySound(hitSfx);

        // 击退（若敌人带刚体）
        if (hit.rigidbody)
            hit.rigidbody.AddForce(muzzle.forward * force, ForceMode.Impulse);

        // 命中敌人 — 调用双参数 TakeDamage(dmg, attacker)
        if (hit.collider.TryGetComponent(out EnemyController ec))
        {
            ec.TakeDamage(dmg, transform);
        }
    }
    #endregion

    // ──────────────────────────────
    private void PlaySound(AudioClip clip)
    {
        if (clip) audioSrc.PlayOneShot(clip);
    }
}
