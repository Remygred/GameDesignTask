using UnityEngine;
using UnityEngine.UI;

namespace FighterGame
{
    /// <summary>
    /// 血量条UI脚本：将Slider与角色生命值关联，实时更新显示。
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private FighterController fighter;  // 要跟踪生命值的角色
        private Slider slider;

        void Awake()
        {
            // 获取Slider组件引用
            slider = GetComponent<Slider>();
        }

        void Start()
        {
            if (fighter != null && slider != null)
            {
                // 初始化Slider的最大值和当前值
                slider.maxValue = fighter.MaxHealth;
                slider.value = fighter.Health;
            }
        }

        void Update()
        {
            if (fighter != null && slider != null)
            {
                // 每帧更新Slider值以反映角色当前生命值
                slider.value = fighter.Health;
            }
        }
    }
}
