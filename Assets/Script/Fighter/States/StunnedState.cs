using UnityEngine;

namespace FighterGame
{
    /// <summary>
    /// 眩晕状态：角色暂时无法行动，通常由遭受重击后触发。
    /// </summary>
    public class StunnedState : FighterState
    {
        private float stunDuration = 2f;  // 眩晕持续时间
        private float timer;

        public override void OnEnter(FighterController fighter)
        {
            // 进入眩晕状态：触发眩晕动画
            if (fighter.Animator != null)
            {
                fighter.Animator.SetTrigger("Stunned");  // *要求Animator有"Stunned"触发器*
            }
            // 可以在此播放眩晕特效，比如星星旋转等
            timer = stunDuration;
            Debug.Log($"{fighter.gameObject.name} 进入眩晕状态！");
        }

        public override void UpdateState(FighterController fighter)
        {
            // 倒计时眩晕持续时间
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    // 眩晕时间结束，恢复Idle状态
                    fighter.ChangeState(new IdleState());
                }
            }
        }

        public override void OnExit(FighterController fighter)
        {
            // 离开眩晕状态：可清理眩晕特效
            Debug.Log($"{fighter.gameObject.name} 结束眩晕，恢复行动");
        }
    }
}
