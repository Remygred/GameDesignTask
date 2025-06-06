using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
///  高级拳击 AI (调整版)
///     Probe → Combo → Evade
///     • 仅在非常近距离且随机概率下才闪避
///     • 否则更倾向主动 Combo
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class AdvancedEnemyAI : MonoBehaviour
{
    private enum Phase { Probe, Combo, Evade, Hit }

    [Header("距离参数")]
    [Tooltip("理想保持距离 (m)")]
    public float ideal = 3.2f;
    [Tooltip("进入攻击距离 (m)")]
    public float attackDist = 2.2f;  // 稍微增大攻击距离

    [Header("速度")]
    [Tooltip("圈步移动速度")]
    public float footSpeed = 2.8f;
    [Tooltip("冲刺/闪避速度")]
    public float rushSpeed = 3.8f;

    [Header("节奏时序")]
    [Tooltip("Probe 阶段时长区间 (s)")]
    public Vector2 probeDur = new Vector2(0.8f, 1.2f);  // 缩短 Probe 时长
    [Tooltip("Combo 连拳间隔区间 (s)")]
    public Vector2 comboGap = new Vector2(0.3f, 0.4f);  // 加快连拳节奏
    [Tooltip("Evade 后撤时长 (s)")]
    public float evadeDur = 0.5f;  // 缩短 Evade 时长

    [Header("闪避设置")]
    [Tooltip("检测玩家蓄力时闪避触发的最小距离")]
    public float dodgeDetectDist = 1.5f; // 只有非常近时才考虑闪避
    [Tooltip("检测到玩家蓄力后的闪避概率 (0~1)")]
    [Range(0f, 1f)] public float dodgeChance = 0.3f; // 30% 几率闪避

    private Phase phase;
    private float phaseTimer;
    private int comboPunches;

    private NavMeshAgent agent;
    private EnemyController ec;
    private Transform player;
    private PlayerCombat pc;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ec = GetComponent<EnemyController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pc = player.GetComponent<PlayerCombat>();
        EnterProbe();
    }

    void Update()
    {
        if (!player) return;
        if (phase == Phase.Hit) return; // AI 被硬直，暂停行为

        phaseTimer -= Time.deltaTime;

        switch (phase)
        {
            case Phase.Probe:
                Strafe();
                // 仅在非常近且满足随机概率时闪避
                if (pc && pc.IsCharging)
                {
                    float dist = Vector3.Distance(transform.position, player.position);
                    if (dist < dodgeDetectDist)
                    {
                        if (Random.value < dodgeChance)
                        {
#if !NO_LOG
                            Debug.Log("<color=#66ccff>[AdvAI]</color> 近距离检测到玩家蓄力，触发闪避");
#endif
                            StartCoroutine(Dodge());
                            return;
                        }
                    }
                }
                // Probe 时间到且在攻击距离时，进入 Combo
                float curDist = Vector3.Distance(transform.position, player.position);
                if (phaseTimer <= 0 && curDist < attackDist)
                    EnterCombo();
                break;

            case Phase.Combo:
                if (comboPunches > 0 && ec.CanAttack)
                {
#if !NO_LOG
                    Debug.Log("<color=#66ccff>[AdvAI]</color> 组合拳攻击 - 剩余 " + comboPunches);
#endif
                    ec.AttackPlayer(player);
                    comboPunches--;
                    phaseTimer = Random.Range(comboGap.x, comboGap.y);
                }
                // 连拳打完，再进入 Evade
                if (comboPunches == 0 && phaseTimer <= 0)
                    EnterEvade();
                break;

            case Phase.Evade:
                Vector3 away = (transform.position - player.position).normalized;
                if (agent.isOnNavMesh)
                    agent.Move(away * rushSpeed * Time.deltaTime);
//#if !NO_LOG
//                Debug.Log("<color=#66ccff>[AdvAI]</color> 后撤中");
//#endif
                // Evade 时间结束后，回到 Probe
                if (phaseTimer <= 0)
                    EnterProbe();
                break;
        }
    }

    //──────────── Phase 切换辅助 ─────────────
    void EnterProbe()
    {
        phase = Phase.Probe;
        phaseTimer = Random.Range(probeDur.x, probeDur.y);
#if !NO_LOG
        Debug.Log("<color=#66ccff>[AdvAI]</color> 进入 Probe");
#endif
    }
    void EnterCombo()
    {
        phase = Phase.Combo;
        comboPunches = Random.Range(2, 4); // 连续 2~3 拳
        phaseTimer = 0f;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[AdvAI]</color> 进入 Combo 连拳 x" + comboPunches);
#endif
    }
    void EnterEvade()
    {
        phase = Phase.Evade;
        phaseTimer = evadeDur;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[AdvAI]</color> 进入 Evade");
#endif
    }

    //──────────── Footwork ─────────────────
    void Strafe()
    {
        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 tangent = Vector3.Cross(Vector3.up, dir);
        Vector3 tgt = player.position + dir * ideal + tangent * Mathf.Sin(Time.time * 2f);

        agent.speed = footSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(tgt);

        transform.LookAt(player);
    }

    //──────────── Dodge ────────────────────
    IEnumerator Dodge()
    {
        phase = Phase.Hit;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[AdvAI]</color> 开始闪避");
#endif
        float t = 0.25f;
        Vector3 side = Vector3.Cross(Vector3.up, (transform.position - player.position)).normalized;
        agent.isStopped = true;
        while (t > 0f)
        {
            transform.Translate(side * rushSpeed * 1.5f * Time.deltaTime, Space.World);
            t -= Time.deltaTime;
            yield return null;
        }
        agent.isStopped = false;
        EnterEvade();
    }

    //──────────── 硬直回调 ─────────────────
    public void OnStunned(float duration)
    {
#if !NO_LOG
        Debug.Log($"<color=#66ccff>[AdvAI]</color> 被硬直 {duration} 秒");
#endif
        phase = Phase.Hit;
        Invoke(nameof(EnterProbe), duration);
    }
}
