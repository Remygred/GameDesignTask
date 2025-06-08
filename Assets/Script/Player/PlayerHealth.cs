using System.Collections;
using UnityEngine;
/// <summary>
/// �����������
///     �� ����񵲼���
///     �� ����Ӳֱ & i-Frame���޵�֡��
///     �� ���� PlayerHitReaction �����ܻ�����
///     �� HP��0 �� LevelManager.Instance.OnPlayerDefeated()
/// </summary>
[RequireComponent(typeof(PlayerHitReaction))]
public class PlayerHealth : MonoBehaviour
{
    [Header("����ֵ")]
    [Tooltip("������ HP")] public int maxHp = 120;
    [SerializeField]
    private int hp;
    public int Hp => hp;

    [Header("�ܻ�����")]
    [Tooltip("�ܻ�Ӳֱʱ�� (s)")] public float stunTime = 0.35f;
    [Tooltip("�ܻ��޵�֡ʱ�� (s)")] public float iFrame = 0.9f;

    private bool invul; // �޵�֡��־

    // �������
    private PlayerHitReaction hitRx;
    private PlayerCombat combat;

    void Awake()
    {
        hp = maxHp;
        hitRx = GetComponent<PlayerHitReaction>();
        combat = GetComponent<PlayerCombat>();
    }

    /// <summary>
    /// ���˵��ã����������˺�
    /// </summary>
    /// <param name="dmg">ԭʼ�˺�</param>
    /// <param name="attacker">������ Transform�����ڻ��˷���</param>
    public void TakeDamage(int dmg, Transform attacker)
    {
        // ��������޵�֡�������˺�
        if (invul)
        {
#if !NO_LOG
            Debug.Log("<color=#ff6600>[PlayerHealth]</color> �����޵�֡�������˺�");
#endif
            return;
        }

        // ���� �񵲼��� ������������������������������������������������
        if (combat && combat.IsBlocking)
        {
            int original = dmg;
            dmg = Mathf.CeilToInt(dmg * combat.BlockDamageRate);
#if !NO_LOG
            Debug.Log($"<color=#ff6600>[PlayerHealth]</color> ����Ч��ԭ�˺� {original} �� ʵ���˺� {dmg}");
#endif
        }

        // ��Ѫ
        hp = Mathf.Max(hp - dmg, 0);
#if !NO_LOG
        Debug.Log($"<color=#ff6666>[PlayerHealth]</color> �ܵ� {dmg} �˺� �� HP {hp}/{maxHp}");
#endif

        // ���� �����ܻ����� ��������������������������������������
        Vector3 dir = (transform.position - attacker.position);
        hitRx.PlayHit(dir, stunTime);

        // ���� �����޵�֡ ������������������������������������������
        StartCoroutine(IFrameRoutine());

        // ���� HP ���� ������������������������������������������������
        if (hp == 0)
        {
#if !NO_LOG
            Debug.Log("<color=#ff3333>[PlayerHealth]</color> ��ҵ��أ�����ʧ�ܲ˵�");
#endif
            LevelManager.Instance?.OnPlayerDefeated();
        }
    }

    /* �޵�֡Э�� */
    private IEnumerator IFrameRoutine()
    {
#if !NO_LOG
        Debug.Log("<color=#ff6600>[PlayerHealth]</color> �����޵�֡");
#endif
        invul = true;
        yield return new WaitForSeconds(iFrame);
        invul = false;
#if !NO_LOG
        Debug.Log("<color=#ff6600>[PlayerHealth]</color> �޵�֡����");
#endif
    }
    // �ⲿֻ������
    public bool IsInvulnerable => invul;
    public float HPPercent => (float)hp / maxHp;
}
