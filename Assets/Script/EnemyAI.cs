using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase, Attack, Stunned, Dead }
    public State currentState = State.Idle;

    public Transform player;            // 玩家目标
    public float chaseDistance = 10f;   // 追击触发距离
    public float attackDistance = 2f;   // 攻击触发距离
    public float moveSpeed = 3f;        // 移动速度
    public float turnSpeed = 5f;        // 转身速度
    public bool hardMode = false;       // 困难模式开关

    private Animator animator;
    private Health healthComponent;
    private float stunTimer = 0f;
    private float maxStunTime = 1.0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthComponent = GetComponent<Health>();
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                animator.SetBool("IsMoving", false);
                // 如果玩家靠近则进入追击状态
                if (distanceToPlayer < chaseDistance)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                // 旋转面向玩家
                Vector3 dir = (player.position - transform.position).normalized;
                Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
                // 移动接近玩家
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
                animator.SetBool("IsMoving", true);
                // 距离足够近，切换到攻击状态
                if (distanceToPlayer < attackDistance)
                {
                    currentState = State.Attack;
                }
                break;

            case State.Attack:
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("Attack");
                // 攻击动画触发时，通过 Animation Event 调用 DealDamage()
                // 攻击结束后返回追击或待机
                // 此处简单用协程延时模拟动画完成
                StartCoroutine(ResetAfterAttack(1.0f));
                currentState = State.Stunned; // 临时用Stunned状态等待动画完成
                break;

            case State.Stunned:
                stunTimer += Time.deltaTime;
                if (stunTimer >= maxStunTime)
                {
                    stunTimer = 0f;
                    // 如果玩家近，继续攻击，否则追击
                    currentState = (distanceToPlayer < attackDistance) ? State.Attack : State.Chase;
                }
                break;
        }
    }

    // 受到伤害时调用
    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;
        healthComponent.TakeDamage(damage);
        animator.SetTrigger("Hit");
        currentState = State.Stunned;
        stunTimer = 0f;
        // 如果血量降为0，进入死亡
        if (healthComponent.CurrentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ResetAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 回到追击状态
        currentState = State.Chase;
    }

    void Die()
    {
        currentState = State.Dead;
        animator.SetBool("IsDead", true);
        // 可在动画中添加事件销毁或禁用物体
        Destroy(gameObject, 2f);
    }
}
