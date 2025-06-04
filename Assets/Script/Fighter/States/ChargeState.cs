using UnityEngine;

namespace FighterGame
{
    /// <summary>
    /// 蓄力状态：角色积蓄能量，提高下次攻击威力。
    /// </summary>
    public class ChargeState : FighterState
    {
        [SerializeField] private float chargeRate = 30f;  // 蓄力速度，每秒增加的蓄力值
        private bool fullyCharged = false;                // 是否已经蓄力满

        public override void OnEnter(FighterController fighter)
        {
            // 进入蓄力状态：触发蓄力动画/特效
            if (fighter.Animator != null)
            {
                fighter.Animator.SetTrigger("Charge");  // *要求Animator有"Charge"触发器*
            }
            fullyCharged = false;
            Debug.Log("开始蓄力...");
        }

        public override void UpdateState(FighterController fighter)
        {
            if (!fullyCharged)
            {
                // 持续增加蓄力值
                fighter.AddCharge(chargeRate * Time.deltaTime);
                // 检查是否蓄力已满
                if (fighter.Charge >= fighter.MaxCharge)
                {
                    fullyCharged = true;
                    // 蓄力满后，可在此触发一个提示或效果
                    Debug.Log("蓄力已满！");
                    // 注意：我们并未在此自动退出蓄力状态，等待玩家松开按键或其他触发
                }
            }
            // 玩家松开蓄力键的检测由PlayerController处理，因此这里无需自动切换状态
            // 若需要AI使用蓄力，可考虑在AIController逻辑中根据情况退出蓄力状态
        }

        public override void OnExit(FighterController fighter)
        {
            // 离开蓄力状态：可停止蓄力动画或特效
            if (fighter.Animator != null)
            {
                // fighter.Animator.ResetTrigger("Charge");
            }
            // 如果在退出时尚未蓄满，也可以在此取消已积累的部分蓄力（本例不取消，保留进度）
            Debug.Log("蓄力结束，当前蓄力值:" + fighter.Charge);
        }
    }
}
