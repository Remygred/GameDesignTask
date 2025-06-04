using UnityEngine;

namespace FighterGame
{
    /// <summary>
    /// 基础角色控制类：提供角色共有的属性和状态机功能（玩家和AI均继承自此）。
    /// </summary>
    public class FighterController : MonoBehaviour
    {
        // 基础属性字段 —— 使用[SerializeField]使其在Inspector可见并可调，但保持封装性
        [Header("角色属性")]
        [SerializeField] private float moveSpeed = 3f;       // 移动速度
        [SerializeField] private int maxHealth = 100;        // 最大生命值
        [SerializeField] private int health = 100;           // 当前生命值
        [SerializeField] private int attackPower = 20;       // 攻击力数值（基本伤害）
        [SerializeField] private float maxCharge = 100f;     // 最大蓄力值（蓄力条满值）
        [SerializeField] private float charge = 0f;          // 当前蓄力值

        [Header("组件引用")]
        [SerializeField] private Animator animator;          // 动画组件引用（用于触发动画）
        // 如有刚体或碰撞器等组件也可引用，例如：
        // [SerializeField] private Rigidbody2D rb;

        [Header("目标引用")]
        [SerializeField] public FighterController target;    // 攻击/追踪目标（可由外部指定）

        // 当前状态（状态机） 
        protected FighterState currentState;

        // 属性封装
        public int MaxHealth => maxHealth;
        public int Health => health;
        public float MaxCharge => maxCharge;
        public float Charge => charge;
        public Animator Animator => animator;
        // 可以根据需要提供更多属性的get访问器，例如AttackPower等
        public int AttackPower => attackPower;
        public float MoveSpeed => moveSpeed;

        /// <summary>
        /// Unity生命周期：Start。在游戏开始时初始化角色状态。
        /// </summary>
        protected virtual void Start()
        {
            // 初始化时设定角色为Idle状态
            currentState = new IdleState();
            if (currentState != null)
            {
                currentState.OnEnter(this);
            }
        }

        /// <summary>
        /// Unity生命周期：Update。每帧调用当前状态的更新逻辑。
        /// </summary>
        protected virtual void Update()
        {
            // 每帧调用当前状态的逻辑（例如持续攻击判定、移动跟踪、计时等）
            if (currentState != null)
            {
                currentState.UpdateState(this);
            }
        }

        /// <summary>
        /// 切换角色状态的函数：负责调用状态退出/进入方法并更新当前状态引用。
        /// </summary>
        /// <param name="newState">要切换到的新状态对象</param>
        public void ChangeState(FighterState newState)
        {
            // 如果有当前状态，先调用其退出逻辑
            if (currentState != null)
            {
                currentState.OnExit(this);
            }
            // 切换到新状态
            currentState = newState;
            if (currentState != null)
            {
                currentState.OnEnter(this);
            }
        }

        /// <summary>
        /// 移动角色的方法，按照指定方向向量移动（基于moveSpeed速度）。
        /// </summary>
        /// <param name="direction">移动方向（仅XY平面，例如2D游戏中X轴）</param>
        public void Move(Vector2 direction)
        {
            // 按照方向和速度移动角色（这里假设简单的线性移动，没有使用物理引擎）
            transform.Translate(direction * moveSpeed * Time.deltaTime);
            // 可选：根据移动方向调整角色朝向
            if (direction.x != 0)
            {
                // 水平翻转角色朝向，使其面向移动方向（假设角色朝右为正向）
                Vector3 scale = transform.localScale;
                scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        /// <summary>
        /// 角色受到伤害时调用，减少生命值并处理受击反馈。
        /// </summary>
        /// <param name="damageAmount">伤害值</param>
        public void TakeDamage(int damageAmount)
        {
            if (damageAmount <= 0) return;
            health = Mathf.Max(health - damageAmount, 0);
            // 触发受击动画（若Animator和相应的动画参数已设置）
            if (animator != null)
            {
                animator.SetTrigger("Hit");  // *要求Animator中有名为"Hit"的触发参数*
            }
            // 若生命值降至0及以下，处理角色死亡（这里只简单禁用对象作为示例）
            if (health <= 0)
            {
                // 角色死亡逻辑：可以播放死亡动画、销毁对象等
                Debug.Log($"{gameObject.name} 死亡");
                // 在此示例中，简单停用角色
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 增加蓄力值的方法（用于蓄力状态）。正值增加蓄力，负值可以用于消耗蓄力。
        /// </summary>
        public void AddCharge(float amount)
        {
            if (amount == 0) return;
            // 更新蓄力值并限制在0到maxCharge范围内
            charge = Mathf.Clamp(charge + amount, 0f, maxCharge);
        }

        /// <summary>
        /// 消耗全部蓄力（用于释放蓄力后的清零处理）。
        /// </summary>
        public void ConsumeAllCharge()
        {
            charge = 0f;
        }
    }
}
