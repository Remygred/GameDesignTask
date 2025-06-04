using UnityEngine;
using UnityEngine.UI;

namespace FighterGame
{
    /// <summary>
    /// 玩家控制脚本：处理玩家输入控制角色行为，继承自基础角色控制类。
    /// </summary>
    public class PlayerController : FighterController
    {
        // 可以在Inspector中设置对应的输入按钮名称（根据项目需要调整）
        [SerializeField] private string horizontalAxis = "Horizontal";  // 水平移动轴名称
        [SerializeField] private string attackButton = "Fire1";        // 攻击按钮名称
        [SerializeField] private string blockButton = "Fire2";         // 防御按钮名称
        [SerializeField] private string chargeButton = "Fire3";        // 蓄力按钮名称

        /// <summary>
        /// 每帧更新：读取玩家输入并根据输入改变角色状态或移动角色。
        /// </summary>
        protected override void Update()
        {
            // 读取水平移动输入
            float moveInput = Input.GetAxis(horizontalAxis);
            // 只有在Idle状态下角色才能自由移动（避免攻击、受击等状态下移动）
            if (currentState is IdleState && Mathf.Abs(moveInput) > 0.01f)
            {
                // 按输入方向移动角色
                Vector2 moveDir = new Vector2(moveInput, 0f);
                Move(moveDir);
            }

            // 攻击输入：当按下攻击键且当前不在攻击或眩晕状态时，进入攻击状态
            if (Input.GetButtonDown(attackButton))
            {
                // 只有在非攻击、非眩晕状态下才能响应攻击输入
                if (!(currentState is AttackState) && !(currentState is StunnedState))
                {
                    ChangeState(new AttackState());
                }
            }

            // 防御输入：当按下防御键且当前不在防御状态时，进入格挡状态
            if (Input.GetButtonDown(blockButton))
            {
                if (!(currentState is BlockState) && !(currentState is StunnedState))
                {
                    ChangeState(new BlockState());
                }
            }
            // 防御键松开：如果当前在格挡状态，松开按键后恢复Idle状态
            if (Input.GetButtonUp(blockButton))
            {
                if (currentState is BlockState)
                {
                    ChangeState(new IdleState());
                }
            }

            // 蓄力输入：当按下蓄力键且当前不在蓄力或其他特殊状态时，进入蓄力状态
            if (Input.GetButtonDown(chargeButton))
            {
                if (!(currentState is ChargeState) && !(currentState is StunnedState))
                {
                    ChangeState(new ChargeState());
                }
            }
            // 蓄力键松开：如果当前在蓄力状态，松开后结束蓄力，回到Idle状态
            if (Input.GetButtonUp(chargeButton))
            {
                if (currentState is ChargeState)
                {
                    ChangeState(new IdleState());
                }
            }

            // 调用基础类Update以继续处理当前状态的更新逻辑（例如攻击计时、眩晕恢复等）
            base.Update();
        }
    }
}
