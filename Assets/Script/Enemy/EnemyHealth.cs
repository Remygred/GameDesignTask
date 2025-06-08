﻿using System.Collections;
using UnityEngine;

/// <summary>
/// ★ 敌人生命 / 硬直 / 无敌帧 / 死亡
///     ├ 如果 isBlocking，则减少伤害
///     ├ 播放受击动画 + 音效
///     ├ 击退 + 通知 AI 进入硬直
///     ├ i-Frame（无敌帧）后恢复
///     └ HP≤0 → LevelManager.Instance.OnEnemyDefeated()
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("生命值")]
    [Tooltip("敌人最大 HP")] public int maxHealth = 100;
    [SerializeField]
    private int hp;

    public int Hp => hp; // 只读属性，供外部查询当前 HP

    [Header("受击参数")]
    [Tooltip("受击硬直时长 (s)")] public float stunTime = 0.35f;
    [Tooltip("受击无敌帧时长 (s)")] public float invulTime = 0.4f;
    [Tooltip("击退力度")] public float knockForce = 4.5f;
    [Tooltip("格挡时保留伤害比例 (0~1)")][Range(0f, 1f)] public float blockDamageRate = 0.15f;
    public AudioClip hitSfx;
    public AudioClip deathSfx;

    private bool invul;
    private float stunTimer;

    private Animator anim;
    private AudioSource audioSrc;
    private Rigidbody rb;

    /* 提供给 AI 判断是否硬直中 */
    public bool IsStunned => stunTimer > 0f;

    void Awake()
    {
        hp = maxHealth;
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    // 单参兼容
    public void TakeDamage(int amount) => TakeDamage(amount, null);

    // 核心流程
    public void TakeDamage(int amount, Transform attacker)
    {
        if (hp <= 0 || invul) return;

        int finalDamage = amount;
        // 格挡减伤
        if (TryGetComponent(out EnemyController ec) && ec.isBlocking)
        {
            int original = finalDamage;
            finalDamage = Mathf.CeilToInt(finalDamage * blockDamageRate);
#if !NO_LOG
            Debug.Log($"<color=#ff9966>[EnemyHealth]</color> 格挡生效，原伤害 {original} → 实际 {finalDamage}");
#endif
        }
        hp = Mathf.Max(hp - finalDamage, 0);
#if !NO_LOG
        Debug.Log($"<color=#ff9966>[EnemyHealth]</color> {gameObject.name} 受伤 {finalDamage} → HP {hp}/{maxHealth}");
#endif
        // 反馈
        if (hitSfx && audioSrc) audioSrc.PlayOneShot(hitSfx);
        if (attacker && rb)
        {
            Vector3 dir = (transform.position - attacker.position).normalized;
            rb.AddForce(dir * knockForce, ForceMode.Impulse);
        }
        StartCoroutine(HitRoutine());
        if (hp == 0) Die();
    }

    private IEnumerator HitRoutine()
    {
        stunTimer = stunTime;
        invul = true;
        SendMessage("OnStunned", stunTime, SendMessageOptions.DontRequireReceiver);
        while (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(invulTime);
        invul = false;
#if !NO_LOG
        Debug.Log("<color=#ff9966>[EnemyHealth]</color> 无敌帧结束");
#endif
    }

    private void Die()
    {
        if (deathSfx && audioSrc) audioSrc.PlayOneShot(deathSfx);
        if (anim) anim.SetTrigger("Die");
#if !NO_LOG
        Debug.Log($"<color=#66ff66>[EnemyHealth]</color> {gameObject.name} 死亡，触发胜利");
#endif
        LevelManager.Instance.OnEnemyDefeated();
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 1.2f);
    }
}