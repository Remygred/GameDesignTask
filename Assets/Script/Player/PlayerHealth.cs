using System.Collections;
using UnityEngine;
/// <summary>
/// 玩家生命管理
///     ├ 处理格挡减伤
///     ├ 启动硬直 & i-Frame（无敌帧）
///     ├ 调用 PlayerHitReaction 播放受击反馈
///     └ HP≤0 → LevelManager.Instance.OnPlayerDefeated()
/// </summary>
[RequireComponent(typeof(PlayerHitReaction))]
public class PlayerHealth : MonoBehaviour
{
    [Header("生命值")]
    [Tooltip("玩家最大 HP")] public int maxHp = 120;
    [SerializeField]
    private int hp;
    public int Hp => hp;

    [Header("受击参数")]
    [Tooltip("受击硬直时间 (s)")] public float stunTime = 0.35f;
    [Tooltip("受击无敌帧时间 (s)")] public float iFrame = 0.9f;

    private bool invul; // 无敌帧标志

    // 组件缓存
    private PlayerHitReaction hitRx;
    private PlayerCombat combat;

    void Awake()
    {
        hp = maxHp;
        hitRx = GetComponent<PlayerHitReaction>();
        combat = GetComponent<PlayerCombat>();
    }

    /// <summary>
    /// 敌人调用：给玩家造成伤害
    /// </summary>
    /// <param name="dmg">原始伤害</param>
    /// <param name="attacker">攻击者 Transform（用于击退方向）</param>
    public void TakeDamage(int dmg, Transform attacker)
    {
        // 如果处于无敌帧，忽略伤害
        if (invul)
        {
#if !NO_LOG
            Debug.Log("<color=#ff6600>[PlayerHealth]</color> 处于无敌帧，忽略伤害");
#endif
            return;
        }

        // ── 格挡减伤 ────────────────────────
        if (combat && combat.IsBlocking)
        {
            int original = dmg;
            dmg = Mathf.CeilToInt(dmg * combat.BlockDamageRate);
#if !NO_LOG
            Debug.Log($"<color=#ff6600>[PlayerHealth]</color> 格挡生效，原伤害 {original} → 实际伤害 {dmg}");
#endif
        }

        // 扣血
        hp = Mathf.Max(hp - dmg, 0);
#if !NO_LOG
        Debug.Log($"<color=#ff6666>[PlayerHealth]</color> 受到 {dmg} 伤害 → HP {hp}/{maxHp}");
#endif

        // ── 播放受击反馈 ───────────────────
        Vector3 dir = (transform.position - attacker.position);
        hitRx.PlayHit(dir, stunTime);

        // ── 启动无敌帧 ─────────────────────
        StartCoroutine(IFrameRoutine());

        // ── HP 归零 ────────────────────────
        if (hp == 0)
        {
#if !NO_LOG
            Debug.Log("<color=#ff3333>[PlayerHealth]</color> 玩家倒地，触发失败菜单");
#endif
            LevelManager.Instance?.OnPlayerDefeated();
        }
    }

    /* 无敌帧协程 */
    private IEnumerator IFrameRoutine()
    {
#if !NO_LOG
        Debug.Log("<color=#ff6600>[PlayerHealth]</color> 进入无敌帧");
#endif
        invul = true;
        yield return new WaitForSeconds(iFrame);
        invul = false;
#if !NO_LOG
        Debug.Log("<color=#ff6600>[PlayerHealth]</color> 无敌帧结束");
#endif
    }
    // 外部只读属性
    public bool IsInvulnerable => invul;
    public float HPPercent => (float)hp / maxHp;
}
