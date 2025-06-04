using UnityEngine;

namespace FighterGame
{
    /// <summary>
    /// 格挡状态：角色进入防御姿态，减少或抵挡所受伤害。
    /// </summary>
    public class BlockState : FighterState
    {
        private float maxBlockTime = 2f;  // 格挡的最大持续时间（秒）
        private float timer;

        public override void OnEnter(FighterController fighter)
        {
            // 进入格挡状态：触发防御姿态动画
            if (fighter.Animator != null)
            {
                fighter.Animator.SetTrigger("Block");  // *要求Animator有"Block"触发器*
            }
            // 重置格挡计时（用于AI自动解除格挡）
            timer = maxBlockTime;
            // 在格挡状态下，可以降低角色移动速度或者使其无法移动
            // 在此示例中，简单处理为不主动移动（PlayerController/AIController在检测到BlockState时已不执行Move）
        }

        public override void UpdateState(FighterController fighter)
        {
            // 如果是AI角色，则使用计时器自动退出格挡
            if (!(fighter is PlayerController))
            {
                if (timer > 0f)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        // AI格挡一段时间后自动解除，回到Idle
                        fighter.ChangeState(new IdleState());
                    }
                }
            }
            // 对于玩家角色，格挡状态的退出由输入（松开防御键）处理，
            // 因此这里不自动切换状态。但依然保留计时作为最长格挡时长的限制（可选）。
        }

        public override void OnExit(FighterController fighter)
        {
            // 离开格挡状态：可重置格挡相关效果
            // 例如：fighter.Animator.ResetTrigger("Block");
            // 如有降低移动速度或开启的防御特效，可以在此恢复/关闭。
        }
    }
}
