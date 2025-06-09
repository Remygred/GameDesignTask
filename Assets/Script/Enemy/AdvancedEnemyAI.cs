using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 高级拳击 AI – 取消后撤，只做追击/拉锯/进攻/硬直流程
/// Phases:
///   Probe     – 拉锯巡航（圈步）
///   ChaseFar  – 远程追击（玩家跑远时）
///   Approach  – 接近冲刺（未到攻击距离）
///   Combo     – 组合拳连击
///   Hit       – 硬直（被击打时）
/// 
/// 行为逻辑：
/// 1. 如果玩家距离 > chaseThreshold → ChaseFar  
/// 2. 否则如果距离 ≤ attackDist 且能攻击 → 立即 Combo  
/// 3. 否则在 Probe/Approach 状态下持续拉锯或接近  
/// 4. Combo 连击打完后直接回 Probe  
/// 5. OnStunned 硬直后直接回 Probe  
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyController))]
public class AdvancedEnemyAI : MonoBehaviour
{
    // AI 当前阶段
    private enum Phase { Probe, ChaseFar, Approach, Combo, Hit }
    private Phase phase;

    [Header("距离参数")]
    [Tooltip("拉锯时与玩家保持的理想距离")]
    public float ideal = 3.2f;
    [Tooltip("进入攻击/连击的最大距离")]
    public float attackDist = 2.2f;
    [Tooltip("玩家距离超过此值时触发远程追击")]
    public float chaseThreshold = 8f;

    [Header("移动速度")]
    [Tooltip("拉锯/接近时的行走速度")]
    public float footSpeed = 3.2f;
    [Tooltip("远程追击时的冲刺速度")]
    public float chaseSpeed = 4f;

    [Header("连击设置")]
    [Tooltip("连击间隔范围 (s)")]
    public Vector2 comboGap = new Vector2(0.2f, 0.35f);
    [Tooltip("最少连击次数")]
    public int comboMin = 3;
    [Tooltip("最多连击次数")]
    public int comboMax = 5;
    private int comboPunches;   // 当前剩余连击次数
    private float phaseTimer;   // 用于控制 Probe/Combo 的计时

    // 内部引用
    private NavMeshAgent agent;
    private EnemyController ec;
    private Animator anim;
    private Transform player;
    private PlayerCombat pc;

    void Awake()
    {
        // 缓存组件和引用
        agent = GetComponent<NavMeshAgent>();
        ec = GetComponent<EnemyController>();
        anim = ec.anim;  // EnemyController 中应公开 Animator
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pc = player.GetComponent<PlayerCombat>();

        // 进入初始拉锯阶段
        EnterProbe();
    }

    void Update()
    {
        // 没有玩家或处于硬直时不执行任何 AI 行为
        if (player == null || phase == Phase.Hit)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 1. 远程追击：玩家跑太远时，优先追上
        if (dist > chaseThreshold)
        {
            phase = Phase.ChaseFar;
            ChaseFar();
            return;
        }

        // 2. 立即攻击：只要到达攻击距离并且冷却允许，就切到 Combo
        if (dist <= attackDist && ec.CanAttack)
        {
            if (phase != Phase.Combo)
                EnterCombo();
        }

        // 3. 否则根据当前阶段继续拉锯或接近
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
        }
    }

    /// <summary>
    /// ChaseFar 阶段：高速追击玩家
    /// </summary>
    void ChaseFar()
    {
        // 更新行走动画
        UpdateMoveAnim(player.position - transform.position);

        // 设置速度与目标
        agent.speed = chaseSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(player.position);

        // 始终面向玩家
        transform.LookAt(player);
    }

    /// <summary>
    /// Probe 阶段：围绕拉锯，等时间到或距离合适时切换状态
    /// </summary>
    void RunProbe()
    {
        phaseTimer -= Time.deltaTime;

        // 计算围绕玩家的圈步目标点
        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 tan = Vector3.Cross(Vector3.up, dir);
        Vector3 target = player.position + dir * ideal + tan * Mathf.Sin(Time.time * 2f);

        UpdateMoveAnim(target - transform.position);

        agent.speed = footSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(target);

        transform.LookAt(player);

        // 时间到：距离大于攻击距离则接近，否则直接连击
        if (phaseTimer <= 0f)
        {
            if (distToPlayer() > attackDist)
                EnterApproach();
            else
                EnterCombo();
        }
    }

    /// <summary>
    /// Approach 阶段：直接冲到攻击范围
    /// </summary>
    void RunApproach()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        UpdateMoveAnim(toPlayer);

        agent.speed = footSpeed;
        if (agent.isOnNavMesh)
            // 冲刺到玩家前方 attackDist * 0.8 的位置
            agent.SetDestination(player.position - toPlayer * (attackDist * 0.8f));

        transform.LookAt(player);

        // 一旦到达攻击距离，切连击
        if (distToPlayer() <= attackDist)
            EnterCombo();
    }

    /// <summary>
    /// Combo 阶段：执行多次攻击后回 Probe
    /// </summary>
    void RunCombo()
    {
        if (comboPunches > 0 && ec.CanAttack)
        {
            // 播放攻击动画
            ResetMoveAnim();
            anim.SetTrigger("Punch");

            // 让 EnemyController 负责实际命中判断和扣血
            ec.AttackPlayer(player, pc);

            comboPunches--;
            // 等待随机间隔后下一拳
            phaseTimer = Random.Range(comboGap.x, comboGap.y);
        }
        // 所有连击打完且间隔到，回拉锯
        else if (comboPunches <= 0 && phaseTimer <= 0f)
        {
            EnterProbe();
        }
    }

    /// <summary>
    /// 受到硬直通知：仅硬直后回 Probe
    /// </summary>
    /// <param name="duration">硬直时长</param>
    public void OnStunned(float duration)
    {
        phase = Phase.Hit;
        // 动画播放或刚体受力等可以在 PlayerController/EnemyController 里处理
        Invoke(nameof(EnterProbe), duration);
    }

    //────────────────── 阶段切换方法 ──────────────────

    /// <summary>进入拉锯阶段</summary>
    void EnterProbe()
    {
        phase = Phase.Probe;
        // 拉锯时长随机，避免节奏僵硬
        phaseTimer = Random.Range(0.5f, 1f);
        ResetMoveAnim();
    }

    /// <summary>进入接近阶段</summary>
    void EnterApproach()
    {
        phase = Phase.Approach;
        ResetMoveAnim();
    }

    /// <summary>进入连击阶段，初始化连击次数</summary>
    void EnterCombo()
    {
        phase = Phase.Combo;
        comboPunches = Random.Range(comboMin, comboMax + 1);
        phaseTimer = 0f;
    }

    //────────────────── 动画辅助 ──────────────────

    /// <summary>
    /// 根据世界空间移动方向设置四向移动动画参数
    /// </summary>
    private void UpdateMoveAnim(Vector3 worldDir)
    {
        Vector3 localDir = transform.InverseTransformDirection(worldDir.normalized);
        anim.SetBool("MoveForward", localDir.z > 0.2f);
        anim.SetBool("MoveBackward", localDir.z < -0.2f);
        anim.SetBool("MoveRight", localDir.x > 0.2f);
        anim.SetBool("MoveLeft", localDir.x < -0.2f);
    }

    /// <summary>关闭所有移动动画参数</summary>
    private void ResetMoveAnim()
    {
        anim.SetBool("MoveForward", false);
        anim.SetBool("MoveBackward", false);
        anim.SetBool("MoveRight", false);
        anim.SetBool("MoveLeft", false);
    }

    /// <summary>获取当前与玩家的距离</summary>
    private float distToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }
}
