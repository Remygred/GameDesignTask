using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// ★ 基础拳击 AI
///     Footwork → StepIn → Attack / Block / Retreat
///     • 包含前后左右移动动画控制
///     • 包含格挡动画和恢复
///     • 包含攻击触发动画
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class BasicEnemyAI : MonoBehaviour
{
    private enum Brain { Footwork, StepIn, Attack, Block, Retreat, Hit }

    [Header("距离设置")]
    public float preferDist = 2f;
    public float attackDist = 0.6f;

    [Header("决策节奏")]
    public Vector2 decisionRange = new Vector2(0.8f, 1.2f);
    private float thinkTimer;

    [Header("移动速度")]
    public float strafeSpeed = 2.2f;
    public float stepSpeed = 3.0f;
    public float retreatTime = 0.6f;

    private Brain brain;
    private Transform player;
    private PlayerCombat combat;
    private NavMeshAgent agent;
    private EnemyController ec;
    private Animator anim;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ec = GetComponent<EnemyController>();
        anim = ec.anim; // 使用 Controller 上的 Animator 缓存
        player = GameObject.FindGameObjectWithTag("Player").transform;
        combat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
        ResetThink();
    }

    void Update()
    {
        if (!player) return;
        if (brain == Brain.Hit) return;

        switch (brain)
        {
            case Brain.Footwork: Footwork(); break;
            case Brain.StepIn: StepIn(); break;
            case Brain.Attack: StartCoroutine(Attack()); break;
            case Brain.Block: StartCoroutine(Block()); break;
            case Brain.Retreat: StartCoroutine(Retreat()); break;
        }
    }

    //──────────────── 环绕移动 ─────────────────
    void Footwork()
    {
        thinkTimer -= Time.deltaTime;

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 tangent = Vector3.Cross(Vector3.up, dir);
        Vector3 target = player.position + dir * preferDist + tangent * Mathf.Sin(Time.time * 2f);

        UpdateMoveAnim(target - transform.position);

        agent.speed = strafeSpeed;
        if (agent.isOnNavMesh) agent.SetDestination(target);

        transform.LookAt(player);

        if (thinkTimer <= 0) DecideNext();
    }

    //──────────────── 探步靠近 ─────────────────
    void StepIn()
    {
        Vector3 dest = player.position + (transform.position - player.position).normalized * (preferDist * 0.5f);
        UpdateMoveAnim(dest - transform.position);

        agent.speed = stepSpeed;
        if (agent.isOnNavMesh) agent.SetDestination(dest);

        transform.LookAt(player);

        if (Vector3.Distance(transform.position, player.position) <= attackDist * 0.9f)
            brain = Brain.Attack;
    }

    //──────────────── 单拳攻击 ─────────────────
    IEnumerator Attack()
    {
        brain = Brain.Hit;
        ResetMoveAnim();
        anim.SetTrigger("Punch");
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 发起单拳攻击");
#endif
        ec.AttackPlayer(player, combat);
        yield return new WaitForSeconds(0.5f);
        ResetThink();
    }

    //──────────────── 格挡 ─────────────────────
    IEnumerator Block()
    {
        brain = Brain.Hit;
        ResetMoveAnim();

        ec.isBlocking = true;
        anim.SetBool("Block", true);
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 进入格挡动画");
#endif
        yield return new WaitForSeconds(0.8f);

        ec.isBlocking = false;
        anim.SetBool("Block", false);
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 退出格挡动画");
#endif
        ResetThink();
    }

    //──────────────── 撤退 ─────────────────────
    IEnumerator Retreat()
    {
        brain = Brain.Hit;
        Vector3 away = (transform.position - player.position).normalized;
        UpdateMoveAnim(away);
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 执行后撤动画");
#endif
        float t = retreatTime;
        while (t > 0f)
        {
            if (agent.isOnNavMesh)
                agent.Move(away * stepSpeed * Time.deltaTime);
            t -= Time.deltaTime;
            yield return null;
        }
        ResetThink();
    }

    //──────────── 更新移动动画 ───────────────
    private void UpdateMoveAnim(Vector3 worldDir)
    {
        Vector3 localDir = transform.InverseTransformDirection(worldDir.normalized);
        anim.SetBool("MoveForward", localDir.z > 0.2f);
        anim.SetBool("MoveBackward", localDir.z < -0.2f);
        anim.SetBool("MoveRight", localDir.x > 0.2f);
        anim.SetBool("MoveLeft", localDir.x < -0.2f);
    }

    //──────────── 重置移动动画 ───────────────
    private void ResetMoveAnim()
    {
        anim.SetBool("MoveForward", false);
        anim.SetBool("MoveBackward", false);
        anim.SetBool("MoveRight", false);
        anim.SetBool("MoveLeft", false);
    }

    //──────────── 决策逻辑 ────────────────
    void DecideNext()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > preferDist * 1.3f)
        {
            brain = Brain.StepIn; return;
        }

        int roll = Random.Range(0, 100);
        if (roll < 60) brain = Brain.Attack;
        else if (roll < 75) brain = Brain.Block;
        else brain = Brain.Retreat;
#if !NO_LOG
        Debug.Log($"<color=#66ccff>[BasicAI]</color> 决策 roll={roll}, 下一状态={brain}");
#endif
    }

    void ResetThink()
    {
        brain = Brain.Footwork;
        thinkTimer = Random.Range(decisionRange.x, decisionRange.y);
        ResetMoveAnim();
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 进入 Footwork");
#endif
    }

    public void OnStunned(float duration)
    {
        brain = Brain.Hit;
#if !NO_LOG
        Debug.Log("<color=#66ccff>[BasicAI]</color> 被硬直" + duration);
#endif
        Invoke(nameof(ResetThink), duration);
    }
}