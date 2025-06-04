namespace FighterGame
{
    /// <summary>
    /// 抽象状态基类：定义角色状态的接口，所有具体状态应继承此类。
    /// </summary>
    public abstract class FighterState
    {
        /// <summary>
        /// 进入该状态时调用。可用于初始化状态所需的数据或触发动画。
        /// </summary>
        public abstract void OnEnter(FighterController fighter);

        /// <summary>
        /// 在该状态下每帧调用。包含该状态持续期间的逻辑，如移动、检测条件等。
        /// </summary>
        public abstract void UpdateState(FighterController fighter);

        /// <summary>
        /// 离开该状态时调用。可用于清理或重置状态相关的数据。
        /// </summary>
        public abstract void OnExit(FighterController fighter);
    }
}
