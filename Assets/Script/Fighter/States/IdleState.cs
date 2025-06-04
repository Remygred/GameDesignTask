namespace FighterGame
{
    /// <summary>
    /// 待机状态：角色默认状态，静止等待输入或目标。
    /// </summary>
    public class IdleState : FighterState
    {
        public override void OnEnter(FighterController fighter)
        {
            // 进入待机状态：触发Idle待机动画
            if (fighter.Animator != null)
            {
                fighter.Animator.SetTrigger("Idle");  // 假设Animator有名为"Idle"的待机动画触发
            }
            // 可以在这里重置某些状态参数，如移动速度恢复正常，停止移动等
            // 示例：停止角色速度（如果使用刚体移动，可将速度设为0）
            // fighter.GetComponent<Rigidbody2D>()?.velocity = Vector2.zero;
        }

        public override void UpdateState(FighterController fighter)
        {
            // 待机状态下一般不主动转换状态，由玩家输入或AI逻辑决定何时切换。
            // 这里可以留空或添加一些待机时的行为，例如轻微摇摆动画或观察四周等。
        }

        public override void OnExit(FighterController fighter)
        {
            // 离开待机状态：通常无需特殊处理。如果需要，可在此清理或记录状态退出。
            // 例如：fighter.Animator.ResetTrigger("Idle");
        }
    }
}
