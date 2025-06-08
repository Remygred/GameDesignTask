using UnityEngine;

/// <summary>
///  ���˹�����ȭ������
///     �� ��ȴ / ��ȭ����
///     �� Ԥ����Ч
///     �� isBlocking �� AI ����
///     �� TakeDamage ת�� EnemyHealth
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider))]
public class EnemyController : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("��ȭ���˺�ֵ")]
    public int damage = 12;
    [Tooltip("�����ҵ����߾���")]
    public float atkRange = 2.0f;
    [Tooltip("����ҵĻ�������")]
    public float hitForce = 6f;
    [Tooltip("���γ�ȭ֮�����ȴʱ�� (s)")]
    public float cooldown = 1.2f;

    [Header("���")]
    [Tooltip("��ȭ���� (ͨ������ȭͷ���ؿ�)")]
    public Transform punchOrigin;
    [Tooltip("������� Layer������ Physics.SphereCast")]
    public LayerMask playerMask;

    [Header("��Ч")]
    public AudioClip punchSfx;
    private AudioSource audioSrc;

    [HideInInspector]
    public bool isBlocking;   // �� AI ���ƣ���ʱ���ܹ���

    public Animator anim;
    private EnemyHealth hp;
    private float nextAtk; // ��һ����������ʱ���

    void Awake()
    {
        hp = GetComponent<EnemyHealth>();
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        if (!punchOrigin) punchOrigin = transform;
    }

    /// <summary>AI ���ã����Գ�ȭ</summary>
    public void AttackPlayer(Transform player, PlayerCombat combat)
    {
        // 1. ��ȴ��״̬У��
        if (!CanAttack) return;

        // 2. ת����ң�����ˮƽ�߶�
        Vector3 lookTarget = player.position;
        lookTarget.y = transform.position.y;
        transform.LookAt(lookTarget);

        // 3. ���ų�ȭ��������Ч
        if (anim) anim.SetTrigger("Punch");
        if (punchSfx && audioSrc) audioSrc.PlayOneShot(punchSfx);

#if !NO_LOG
        Debug.Log("<color=#ff6600>[EnemyController]</color> ���ڳ�ȭ��");
#endif

        // 4. �ӳ� 0.1s ֮������˺��ж������붯���ĳ�ȭ��ֵͬ����
        Invoke(nameof(DealDamage), 0.1f);
        combat.CancelCharging();

        // 5. ������һ�οɹ���ʱ��
        nextAtk = Time.time + cooldown;
    }

    /// <summary>
    /// �������˺��ж��߼���SphereCast ��ȭ��Χ�ڵ���ҲŻ�����
    /// </summary>
    private void DealDamage()
    {
        // 1. ����һ���� punchOrigin ָ����ҵ�λ������
        Vector3 dirToPlayer = (LevelManager.Instance.PlayerTransform.position - punchOrigin.position).normalized;

        // 2. �� SphereCast ��Ϊ���� dirToPlayer ����
        if (Physics.SphereCast(punchOrigin.position,
                               1f,                // �뾶
                               dirToPlayer,         // ����
                               out RaycastHit hit,
                               atkRange,
                               playerMask))
        {
            if (hit.collider.TryGetComponent(out PlayerHealth ph))
            {
#if !NO_LOG
                Debug.Log("<color=#ff6600>[EnemyController]</color> ������ң����ڿ�Ѫ");
#endif
                ph.TakeDamage(damage, transform);
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(dirToPlayer * hitForce, ForceMode.Impulse);
            }
        }
        else
        {
#if !NO_LOG
            Debug.Log("<color=#ff6600>[EnemyController]</color> ��ȭδ�������");
#endif
        }
    }


    /// <summary>
    /// �ⲿ���� PlayerCombat�����ã����Լ�����˺�
    /// </summary>
    public void TakeDamage(int dmg, Transform attacker)
    {
#if !NO_LOG
        Debug.Log($"<color=#ff6600>[EnemyController]</color> �յ��˺� {dmg}������ EnemyHealth.TakeDamage");
#endif
        hp.TakeDamage(dmg, attacker);
    }

    /// <summary>AI �ж��Ƿ��ܳ�ȭ����ȴ��ϡ���Ӳֱ���Ǹ񵲣�</summary>
    public bool CanAttack => Time.time >= nextAtk && !hp.IsStunned && !isBlocking;
}
