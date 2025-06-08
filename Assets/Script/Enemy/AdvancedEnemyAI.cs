using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 高级拳击 AI – 更激进版
/// Phases: ChaseFar → ImmediateAttack → Probe → Approach → Combo → BlockPhase → Evade → Hit
/// 1. 远程追击距离 > chaseThreshold
/// 2. 只要 dist ≤ attackDist 且 CanAttack，就立刻进入 Combo（ImmediateAttack）
/// 3. 否则在 Probe/Approach 才考虑格挡/闪避
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class AdvancedEnemyAI : MonoBehaviour
{
    private enum Phase { Probe, ChaseFar, Approach, Combo, BlockPhase, Evade, Hit }
    private Phase phase;

    [Header("距离参数")]
    public float ideal = 3.2f;                   // 最佳拉锯距离
    public float attackDist = 2.2f;              // 发起Combo距离
    public float chaseThreshold = 8f;            // 超出此距离时主动远程追击

    [Header("速度")]
    public float footSpeed = 3.2f;               // 圈步速度
    public float chaseSpeed = 4f;                // 远程追击速度
    public float rushSpeed = 4.2f;               // 冲刺/躲避速度

    [Header("连击设置")]
    public Vector2 comboGap = new Vector2(0.2f, 0.35f);
    public int comboMin = 3;
    public int comboMax = 5;
    private int comboPunches;
    private float phaseTimer;

    [Header("格挡设置")]
    public float blockDetectDist = 1.8f;
    [Range(0f, 1f)] public float blockChance = 0.3f;     // 降低为 30%
    public float blockDuration = 0.6f;

    [Header("闪避设置")]
    public float dodgeDetectDist = 1.5f;
    [Range(0f, 1f)] public float dodgeChance = 0.1f;     // 降低为 10%
    public float evadeDur = 0.6f;

    private NavMeshAgent agent;
    private EnemyController ec;
    private Animator anim;
    private Transform player;
    private PlayerCombat pc;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ec = GetComponent<EnemyController>();
        anim = ec.anim;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pc = player.GetComponent<PlayerCombat>();
        EnterProbe();
    }

    void Update()
    {
        if (player == null || phase == Phase.Hit) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 1. 远程追击
        if (dist > chaseThreshold)
        {
            if (phase != Phase.ChaseFar)
            {
                phase = Phase.ChaseFar;
                Debug.Log("<color=#EE3377>[AdvAI]</color> 进入远程追击");
            }
            ChaseFar();
            return;
        }

        // 2. **立刻攻击**：只要到达攻击距离并且冷却允许，就优先进入 Combo
        if (dist <= attackDist && ec.CanAttack)
        {
            if (phase != Phase.Combo)
            {
                EnterCombo(); // 重置连击次数
            }
            // Combo 阶段处理会在后面 switch 中执行
        }
        else
        {
            // 3. 仅在 Probe/Approach 时才考虑格挡或闪避
            if (phase == Phase.Probe || phase == Phase.Approach)
            {
                // 普攻检测 → BlockPhase
                if (!pc.IsCharging && dist < blockDetectDist && Random.value < blockChance)
                {
                    EnterBlockPhase();
                    return;
                }
                // 蓄力检测 → Dodge
                if (pc.IsCharging && dist < dodgeDetectDist && Random.value < dodgeChance)
                {
                    StartCoroutine(Dodge());
                    return;
                }
            }
        }

        // 4. 按剩余阶段逻辑执行
        switch (phase)
        {
            case Phase.Probe:
                RunProbe();
                break;
            case Phase.Approach:
                RunApproach();
                break;
            case Phase.Combo:
                RunCombo();
                break;
            case Phase.BlockPhase:
                // BlockPhase 协程中处理
                break;
            case Phase.Evade:
                RunEvade();
                break;
        }
    }

    //────────────────── 远程追击 ──────────────────
    void ChaseFar()
    {
        UpdateMoveAnim(player.position - transform.position);
        agent.speed = chaseSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(player.position);
        transform.LookAt(player);
    }

    //────────────────── Probe 阶段 ──────────────────
    void RunProbe()
    {
        phaseTimer -= Time.deltaTime;
        // 圈步
        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 tan = Vector3.Cross(Vector3.up, dir);
        Vector3 target = player.position + dir * ideal + tan * Mathf.Sin(Time.time * 2f);
        UpdateMoveAnim(target - transform.position);

        agent.speed = footSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(target);
        transform.LookAt(player);

        if (phaseTimer <= 0f)
        {
            if (Vector3.Distance(transform.position, player.position) > attackDist)
                EnterApproach();
            else
                EnterCombo();
        }
    }

    //────────────────── Approach 阶段 ──────────────────
    void RunApproach()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        UpdateMoveAnim(toPlayer);
        agent.speed = footSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(player.position - toPlayer * (attackDist * 0.8f));
        transform.LookAt(player);

        if (Vector3.Distance(transform.position, player.position) <= attackDist)
            EnterCombo();
    }

    //────────────────── Combo 阶段 ──────────────────
    void RunCombo()
    {
        if (comboPunches > 0 && ec.CanAttack)
        {
            ResetMoveAnim();
            anim.SetTrigger("Punch");
            ec.AttackPlayer(player, pc);
            comboPunches--;
            phaseTimer = Random.Range(comboGap.x, comboGap.y);
        }
        else if (comboPunches <= 0 && phaseTimer <= 0f)
        {
            EnterEvade();
        }
    }

    //────────────────── Block & Retreat ──────────────────
    void EnterBlockPhase()
    {
        phase = Phase.BlockPhase;
        StartCoroutine(BlockAndRetreat());
    }

    // … 省略前面代码 …

    IEnumerator BlockAndRetreat()
    {
        // 1) 先做格挡
        ec.isBlocking = true;
        anim.SetBool("Block", true);
        yield return new WaitForSeconds(blockDuration);

        // 2) 结束格挡
        ec.isBlocking = false;
        anim.SetBool("Block", false);

        // 3) 计算一次性后撤目标点（距离 retreatStep）
        Vector3 away = (transform.position - player.position).normalized;
        float retreatStep = 0.2f; // 想退多远就设置这个值（米）
        Vector3 targetPos = transform.position + away * retreatStep;

        // 动画
        UpdateMoveAnim(away);

        // 用 NavMeshAgent 走过去（如果不在 NavMesh 上可直接 Transform）
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(targetPos);
            // 等待短暂时间让它移动过去
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // fallback：直接位移一小段
            transform.Translate(away * retreatStep, Space.World);
            yield return new WaitForSeconds(0.2f);
        }

        // 4) 收尾，回到 Probe
        ResetMoveAnim();
        EnterProbe();
    }


    //────────────────── Evade 阶段 ──────────────────
    void RunEvade()
    {
        // 改成只退一次
        Vector3 away = (transform.position - player.position).normalized;
        UpdateMoveAnim(-player.forward);

        float evadeStep = 1.2f; // 退一小段
        if (agent.isOnNavMesh)
            agent.SetDestination(transform.position + away * evadeStep);
        else
            transform.Translate(away * evadeStep, Space.World);

        // 等待短暂时间
        Invoke(nameof(StopEvade), 0.2f);
    }

    private void StopEvade()
    {
        ResetMoveAnim();
        EnterProbe();
    }


    //────────────────── Dodge ──────────────────
    IEnumerator Dodge()
    {
        phase = Phase.Hit;
        ResetMoveAnim();
        anim.SetTrigger("Dodge");
        float t = 0.3f;
        while (t > 0f)
        {
            transform.Translate(-player.right * rushSpeed * Time.deltaTime, Space.World);
            t -= Time.deltaTime;
            yield return null;
        }
        EnterEvade();
    }

    //────────────────── 进入各阶段 ──────────────────
    void EnterProbe()
    {
        phase = Phase.Probe;
        phaseTimer = Random.Range(0.5f, 0.8f); // 更快决策
        ResetMoveAnim();
    }
    void EnterApproach() { phase = Phase.Approach; ResetMoveAnim(); }
    void EnterCombo()
    {
        phase = Phase.Combo;
        comboPunches = Random.Range(comboMin, comboMax + 1);
        phaseTimer = 0f;
    }
    void EnterEvade()
    {
        phase = Phase.Evade;
        phaseTimer = evadeDur;
    }

    public void OnStunned(float duration)
    {
        phase = Phase.Hit;
        // 硬直结束后，直接重新开始 Probe（继续拉锯/接近/进攻）
        Invoke(nameof(EnterProbe), duration);
    }


    //────────────────── 动画辅助 ──────────────────
    private void UpdateMoveAnim(Vector3 worldDir)
    {
        Vector3 ld = transform.InverseTransformDirection(worldDir.normalized);
        anim.SetBool("MoveForward", ld.z > 0.2f);
        anim.SetBool("MoveBackward", ld.z < -0.2f);
        anim.SetBool("MoveRight", ld.x > 0.2f);
        anim.SetBool("MoveLeft", ld.x < -0.2f);
    }
    private void ResetMoveAnim()
    {
        anim.SetBool("MoveForward", false);
        anim.SetBool("MoveBackward", false);
        anim.SetBool("MoveRight", false);
        anim.SetBool("MoveLeft", false);
    }
}
