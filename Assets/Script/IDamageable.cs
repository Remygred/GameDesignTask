/// <summary>
/// 任何可受伤单位都实现此接口，便于通用打击逻辑调用
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}
