using System.Collections;
using UnityEngine;

/// <summary>
/// ★ 玩家受击反馈
///     ├ Animator.SetTrigger("Hit")
///     ├ Rigidbody AddForce 击退
///     ├ 硬直期间 IsStunned=true → 可供 PlayerController 禁用输入
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerHitReaction : MonoBehaviour
{
    public Animator animator;               // 若空 → 自动获取
    [Tooltip("击退力度 (Impulse)")] public float knockForce = 6f;

    private Rigidbody rb;
    private bool hardStun;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    /// <param name="dir">攻击者→玩家方向</param>
    /// <param name="stunTime">硬直时长</param>
    public void PlayHit(Vector3 dir, float stunTime)
    {
        if (hardStun) return;               // 防止多次叠加
        StartCoroutine(HitRoutine(dir.normalized, stunTime));
    }

    private IEnumerator HitRoutine(Vector3 dir, float stunTime)
    {
        hardStun = true;
        if (animator) animator.SetTrigger("Hit");
        rb.AddForce(dir * knockForce, ForceMode.Impulse);

        yield return new WaitForSeconds(stunTime);
        hardStun = false;
    }

    public bool IsStunned => hardStun;
}