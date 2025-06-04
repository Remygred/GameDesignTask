using UnityEngine;
using UnityEngine.UI;

namespace FighterGame
{
    /// <summary>
    /// 蓄力条UI脚本：将Slider与角色蓄力值关联，实时更新显示。
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ChargeBar : MonoBehaviour
    {
        [SerializeField] private FighterController fighter;  // 要跟踪蓄力值的角色
        private Slider slider;

        void Awake()
        {
            slider = GetComponent<Slider>();
        }

        void Start()
        {
            if (fighter != null && slider != null)
            {
                slider.maxValue = fighter.MaxCharge;
                slider.value = fighter.Charge;
            }
        }

        void Update()
        {
            if (fighter != null && slider != null)
            {
                slider.value = fighter.Charge;
            }
        }
    }
}
