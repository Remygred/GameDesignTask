using UnityEngine;

namespace FighterGame
{
    /// <summary>
    /// 攻击状态：角色执行攻击动作的状态。
    /// </summary>
    public class AttackState : FighterState
    {
        // 攻击持续时间（秒），用于模拟攻击动画时间和硬直
        private float attackDuration = 0.5f;
        private float timer;

        public override void OnEnter(FighterController fighter)
        {
            // 攻击开始：触发攻击动画
            if (fighter.Animator != null)
            {
                fighter.Animator.SetTrigger("Attack");  // *要求Animator有"Attack"触发器*
            }
            timer = attackDuration;  // 重置计时器

            // 检查并应用伤害
            FighterController target = fighter.target;
            if (target != null)
            {
                // 判断目标是否在攻击范围内
                float dist = Vector3.Distance(fighter.transform.position, target.transform.position);
                if (dist <= (fighter is AIController ai ? ai.AttackPower : fighter.AttackPower))
                {
                    // 上面简单假设攻击距离，可改为fighter的攻击范围属性。如果AIController有attackRange，可用ai.attackRange。

                    // 判断目标是否处于格挡状态
                    if (target.currentState is BlockState)
                    {
                        // 目标在格挡，减少伤害或完全抵挡
                        int reducedDamage = Mathf.Max(fighter.AttackPower / 2, 1);
                        target.TakeDamage(reducedDamage);
                        // 格挡情况下不触发眩晕效果（仅少量扣血），可以在此添加格挡特效
                        Debug.Log("攻击被格挡，伤害减半");
                    }
                    else
                    {
                        // 目标未格挡，造成正常伤害
                        target.TakeDamage(fighter.AttackPower);
                        // 检查是否为蓄力满的重击，如果是则使目标进入眩晕状态
                        if (fighter.Charge >= fighter.MaxCharge && target.Health > 0)
                        {
                            // 将目标切换为眩晕状态，并清空攻击者的蓄力值
                            target.ChangeState(new StunnedState());
                            fighter.ConsumeAllCharge();
                            Debug.Log("重击成功！目标进入眩晕状态");
                        }
                    }
                }
            }
        }

        public override void UpdateState(FighterController fighter)
        {
            // 攻击状态计时：等待攻击动画/硬直结束
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    // 攻击动作结束，切换回Idle待机状态
                    fighter.ChangeState(new IdleState());
                }
            }
        }

        public override void OnExit(FighterController fighter)
        {
            // 攻击状态结束：这里可以处理攻击结束后的收尾工作
            // 例如：重置攻击动画触发器，使Animator回到Idle状态（IdleState的OnEnter也会触发Idle动画）
            // fighter.Animator.ResetTrigger("Attack");
        }
    }
}
