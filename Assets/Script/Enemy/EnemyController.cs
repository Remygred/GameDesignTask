using UnityEngine;

/// <summary>
///  敌人公共出拳控制器
///     ├ 冷却 / 出拳动画
///     ├ 预留音效
///     ├ isBlocking 由 AI 设置
///     └ TakeDamage 转交 EnemyHealth
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class EnemyController : MonoBehaviour
{
    [Header("攻击参数")]
    [Tooltip("出拳的伤害值")]
    public int damage = 12;
    [Tooltip("检测玩家的射线距离")]
    public float atkRange = 2.0f;
    [Tooltip("给玩家的击退力度")]
    public float hitForce = 6f;
    [Tooltip("两次出拳之间的冷却时间 (s)")]
    public float cooldown = 1.2f;

    [Header("检测")]
    [Tooltip("出拳检测点 (通常挂在拳头或胸口)")]
    public Transform punchOrigin;
    [Tooltip("玩家所在 Layer，用于 Physics.SphereCast")]
    public LayerMask playerMask;

    [Header("音效")]
    public AudioClip punchSfx;
    private AudioSource audioSrc;

    [HideInInspector]
    public bool isBlocking;   // 由 AI 控制，格挡时不能攻击

    public Animator anim;
    private EnemyHealth hp;
    private float nextAtk; // 下一次允许攻击的时间戳

    void Awake()
    {
        hp = GetComponent<EnemyHealth>();
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        if (!punchOrigin) punchOrigin = transform;
    }

    /// <summary>AI 调用：尝试出拳</summary>
    public void AttackPlayer(Transform player, PlayerCombat combat)
    {
        // 1. 冷却与状态校验
        if (!CanAttack) return;

        // 2. 转向玩家，保持水平高度
        Vector3 lookTarget = player.position;
        lookTarget.y = transform.position.y;
        transform.LookAt(lookTarget);

        // 3. 播放出拳动画或音效
        if (anim) anim.SetTrigger("Punch");
        if (punchSfx && audioSrc) audioSrc.PlayOneShot(punchSfx);

#if !NO_LOG
        Debug.Log("<color=#ff6600>[EnemyController]</color> 正在出拳！");
#endif

        // 4. 延迟 0.1s 之后进行伤害判定（可与动画的出拳峰值同步）
        Invoke(nameof(DealDamage), 0.1f);
        combat.CancelCharging();

        // 5. 设置下一次可攻击时间
        nextAtk = Time.time + cooldown;
    }

    /// <summary>
    /// 真正的伤害判定逻辑：SphereCast 出拳范围内的玩家才会受伤
    /// </summary>
    private void DealDamage()
    {
        // 1. 先做一个从 punchOrigin 指向玩家的位置向量
        Vector3 dirToPlayer = (LevelManager.Instance.PlayerTransform.position - punchOrigin.position).normalized;

        // 2. 把 SphereCast 改为沿着 dirToPlayer 发射
        if (Physics.SphereCast(punchOrigin.position,
                               1f,                // 半径
                               dirToPlayer,         // 方向
                               out RaycastHit hit,
                               atkRange,
                               playerMask))
        {
            if (hit.collider.TryGetComponent(out PlayerHealth ph))
            {
#if !NO_LOG
                Debug.Log("<color=#ff6600>[EnemyController]</color> 击中玩家，正在扣血");
#endif
                ph.TakeDamage(damage, transform);
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(dirToPlayer * hitForce, ForceMode.Impulse);
            }
        }
        else
        {
#if !NO_LOG
            Debug.Log("<color=#ff6600>[EnemyController]</color> 出拳未命中玩家");
#endif
        }
    }


    /// <summary>
    /// 外部（如 PlayerCombat）调用：对自己造成伤害
    /// </summary>
    public void TakeDamage(int dmg, Transform attacker)
    {
#if !NO_LOG
        Debug.Log($"<color=#ff6600>[EnemyController]</color> 收到伤害 {dmg}，调用 EnemyHealth.TakeDamage");
#endif
        hp.TakeDamage(dmg, attacker);
    }

    /// <summary>AI 判断是否能出拳（冷却完毕、非硬直、非格挡）</summary>
    public bool CanAttack => Time.time >= nextAtk && !hp.IsStunned && !isBlocking;
}
