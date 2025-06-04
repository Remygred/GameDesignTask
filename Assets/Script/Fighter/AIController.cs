using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace FighterGame
{
    /// <summary>
    /// AI控制脚本：用于控制敌方角色的行为逻辑，继承自基础角色控制类。
    /// </summary>
    public class AIController : FighterController
    {
        [SerializeField] private float detectionRange = 5f;  // 追击触发距离
        [SerializeField] private float attackRange = 1.2f;   // 攻击距离

        // 可以根据需要添加更多AI决策相关参数，例如格挡几率、蓄力几率等

        protected override void Start()
        {
            base.Start();
            // 如果未在Inspector中指定目标，则尝试自动查找场景中的玩家作为目标
            if (target == null)
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                    target = player;
            }
        }

        /// <summary>
        /// 每帧更新：根据与目标的距离切换AI状态或执行AI行为。
        /// </summary>
        protected override void Update()
        {
            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                // 如果当前状态是Idle且玩家进入追击范围，则切换为追击状态
                if (currentState is IdleState && distance <= detectionRange)
                {
                    ChangeState(new ChaseState());
                }
                // 如果当前状态是Chase（追击）：
                else if (currentState is ChaseState)
                {
                    // 若玩家进入攻击范围，切换为攻击状态
                    if (distance <= attackRange)
                    {
                        ChangeState(new AttackState());
                    }
                    // 若玩家跑出追击范围，切回Idle停止追击
                    else if (distance > detectionRange * 1.5f)
                    {
                        // *注：使用1.5倍追击范围作为丢失目标的阈值，避免频繁来回切换，可根据需要调整*
                        ChangeState(new IdleState());
                    }
                }
            }

            // 可选：添加其他AI行为决策，例如随机进入格挡或蓄力
            // if(currentState is IdleState && Random.value < 0.01f) { ChangeState(new BlockState()); }

            // 调用基类Update处理当前状态的内部更新逻辑
            base.Update();
        }
    }
}
