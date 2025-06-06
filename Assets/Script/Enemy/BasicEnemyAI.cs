using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
///  基础拳击 AI (调整版)
///     Footwork → StepIn → Attack / Block / Retreat
///     • 提高主动攻击几率
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class BasicEnemyAI : MonoBehaviour
{
    private enum Brain { Footwork, StepIn, Attack, Block, Retreat, Hit }

    [Header("距离设置")]
    [Tooltip("理想保持距离 (m)")]
    public float preferDist = 2f;
    [Tooltip("进入攻击距离 (m)")]
    public float attackDist = 0.6f; // 稍微增大，让敌人更容易进入攻击状态

    [Header("决策节奏")]
    [Tooltip("决策间隔秒数范围")]
    public Vector2 decisionRange = new Vector2(0.8f, 1.2f); // 缩短决策时间，更频繁判断
    private float thinkTimer;

    [Header("移动速度")]
    [Tooltip("环绕移动速度")]
    public float strafeSpeed = 2.2f;
    [Tooltip("探步移动速度")]
    public float stepSpeed = 3.0f;
    [Tooltip("撤退持续时间 (s)")]
    public float retreatTime = 0.6f;

    private Brain brain;
    private Transform player;
    private NavMeshAgent agent;
    private EnemyController ec;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ec = GetComponent<EnemyController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ResetThink();
    }

    void Update()
    {
        if (!player) return;

        switch (brain)
        {
            case Brain.Footwork: Footwork(); break;
            case Brain.StepIn: StepIn(); break;
            case Brain.Attack: StartCoroutine(Attack()); break;
            case Brain.Block: StartCoroutine(Block()); break;
            case Brain.Retreat: StartCoroutine(Retreat()); break;
            case Brain.Hit:      /* 由 EnemyHealth 控制硬直 */ break;
        }
    }

    //──────────────── Footwork ─────────────────
    void Footwork()
    {
        thinkTimer -= Time.deltaTime;

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 tangent = Vector3.Cross(Vector3.up, dir);
        Vector3 target = player.position + dir * preferDist + tangent * Mathf.Sin(Time.time * 2f);
        agent.speed = strafeSpeed;

        if (agent.isOnNavMesh)
            agent.SetDestination(target);

        transform.LookAt(player);

        if (thinkTimer <= 0) DecideNext();
    }

    //──────────────── 探步靠近 ─────────────────
    void StepIn()
    {
        Vector3 dest = player.position + (transform.position - player.position).normalized * (preferDist * 0.5f);
        agent.speed = stepSpeed;

        if (agent.isOnNavMesh)
            agent.SetDestination(dest);

        transform.LookAt(player);

        // 距离判断：如果已进入更近的攻击范围，就直接攻击
        if (Vector3.Distance(transform.position, player.position) <= attackDist * 0.9f)
            brain = Brain.Attack;
    }

    //──────────────── 单拳攻击 ─────────────────
    IEnumerator Attack()
    {
        brain = Brain.Hit; // 锁定 AI 行为
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 发起单拳攻击");
#endif
        ec.AttackPlayer(player);
        yield return new WaitForSeconds(0.5f);
        ResetThink();
    }

    //──────────────── 格挡 ─────────────────────
    IEnumerator Block()
    {
        brain = Brain.Hit;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 进入格挡");
#endif
        ec.isBlocking = true;
        yield return new WaitForSeconds(0.8f); // 缩短格挡时间
        ec.isBlocking = false;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 退出格挡");
#endif
        ResetThink();
    }

    //──────────────── 撤退 ─────────────────────
    IEnumerator Retreat()
    {
        brain = Brain.Hit;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 执行后撤");
#endif
        float t = retreatTime;
        while (t > 0f)
        {
            Vector3 away = (transform.position - player.position).normalized;
            if (agent.isOnNavMesh)
                agent.Move(away * stepSpeed * Time.deltaTime);
            t -= Time.deltaTime;
            yield return null;
        }
        ResetThink();
    }

    //────────────── 决策逻辑 ────────────────
    void DecideNext()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > preferDist * 1.3f)
        {
            brain = Brain.StepIn;
            return;
        }

        int roll = Random.Range(0, 100);
        if (roll < 60)       // 60% 几率发起攻击
            brain = Brain.Attack;
        else if (roll < 75)  // 15% 几率格挡
            brain = Brain.Block;
        else                 // 25% 几率撤退
            brain = Brain.Retreat;

#if !NO_LOG
        Debug.Log($"<color=#66ccff>[BasicAI]</color> 角色决策: roll={roll}, 下一状态 = {brain}");
#endif
    }

    void ResetThink()
    {
        brain = Brain.Footwork;
        thinkTimer = Random.Range(decisionRange.x, decisionRange.y);
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 进入 Footwork");
#endif
    }

    // AI 硬直回调
    public void OnStunned(float duration)
    {
        brain = Brain.Hit;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 被硬直 " + duration + " 秒");
#endif
        Invoke(nameof(ResetThink), duration);
    }
}
