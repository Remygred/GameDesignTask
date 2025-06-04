using UnityEngine;

namespace FighterGame
{
    /// <summary>
    /// 追击状态：AI角色朝目标移动的状态。
    /// </summary>
    public class ChaseState : FighterState
    {
        public override void OnEnter(FighterController fighter)
        {
            // 进入追击状态：触发奔跑/追击动画
            if (fighter.Animator != null)
            {
                fighter.Animator.SetTrigger("Run");  // 假设Animator有名为"Run"的动画
            }
        }

        public override void UpdateState(FighterController fighter)
        {
            // 持续朝目标移动
            if (fighter.target != null)
            {
                // 计算朝向目标的方向向量（单位化）
                Vector3 direction = (fighter.target.transform.position - fighter.transform.position).normalized;
                // 仅保留水平向量（假设只在水平方向追击，例如2D游戏）
                direction.y = 0;
                // 调用基础移动方法朝目标方向移动
                fighter.Move(new Vector2(direction.x, direction.y));
            }
            // *状态切换判断*：是否在AIController中处理。通常AIController.Update会根据距离条件调用ChangeState，不在此内部直接改变状态。
            // 但可以在这里添加安全检查，例如目标丢失时直接切换回Idle：
            // if (fighter.target == null) { fighter.ChangeState(new IdleState()); }
        }

        public override void OnExit(FighterController fighter)
        {
            // 离开追击状态：可在此停止移动或重置动画状态
            if (fighter.Animator != null)
            {
                // 停止奔跑动画，可能切换为Idle动画（在进入IdleState时会触发Idle，这里可选）
                // fighter.Animator.ResetTrigger("Run");
            }
        }
    }
}
