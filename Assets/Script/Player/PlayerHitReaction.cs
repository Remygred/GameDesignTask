using System.Collections;
using UnityEngine;

/// <summary>
/// �� ����ܻ�����
///     �� Animator.SetTrigger("Hit")
///     �� Rigidbody AddForce ����
///     �� Ӳֱ�ڼ� IsStunned=true �� �ɹ� PlayerController ��������
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerHitReaction : MonoBehaviour
{
    public Animator animator;               // ���� �� �Զ���ȡ
    [Tooltip("�������� (Impulse)")] public float knockForce = 6f;

    private Rigidbody rb;
    private bool hardStun;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    /// <param name="dir">�����ߡ���ҷ���</param>
    /// <param name="stunTime">Ӳֱʱ��</param>
    public void PlayHit(Vector3 dir, float stunTime)
    {
        if (hardStun) return;               // ��ֹ��ε���
        StartCoroutine(HitRoutine(dir.normalized, stunTime));
    }

    private IEnumerator HitRoutine(Vector3 dir, float stunTime)
    {
        hardStun = true;
        rb.AddForce(dir * knockForce, ForceMode.Impulse);

        yield return new WaitForSeconds(stunTime);
        hardStun = false;
    }

    public bool IsStunned => hardStun;
}