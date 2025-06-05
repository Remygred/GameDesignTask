using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int CurrentHealth { get; private set; }
    public UnityEvent onHealthChanged;  // 可用于更新UI
    public UnityEvent onDeath;          // 可用于触发死亡事件

    void Awake()
    {
        CurrentHealth = maxHealth;
        if (onHealthChanged == null) onHealthChanged = new UnityEvent();
        if (onDeath == null) onDeath = new UnityEvent();
    }

    // 扣血
    public void TakeDamage(int amount)
    {
        // ① 如果自己带 PlayerCombat 组件且正在格挡 → 减伤
        if (TryGetComponent(out PlayerCombat pc) && pc.IsBlocking)
        {
            amount = Mathf.CeilToInt(amount * pc.BlockDamageRate);
            // ② 同时可跳过硬直 / 眩晕逻辑
            //    （这里直接不触发 Stun，或者缩短 Stun 时间）
        }

        CurrentHealth -= amount;
        onHealthChanged.Invoke();

        if (CurrentHealth <= 0) Die();
    }

    // 加血
    public void Heal(int amount)
    {
        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
        onHealthChanged.Invoke();
    }

    void Die()
    {
        onDeath.Invoke();
        // 如果是敌人可销毁物体，如果是玩家可触发失败
        // 具体处理可在 GameManager 等类中订阅 onDeath 事件
    }
}
