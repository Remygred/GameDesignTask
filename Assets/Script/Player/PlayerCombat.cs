using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerCombat : MonoBehaviour
{
    #region Inspector ����
    [Header("����")]
    [SerializeField] private Camera cam;              // ���߳���D�D�������Զ� Camera.main
    [SerializeField] private LayerMask enemyMask;     // ֻ�����˵Ĳ�
    [SerializeField] private LayerMask obstacleMask;  // ���赲���ߵ�ǽ���
    [SerializeField] private Transform muzzle;        // ���߷���㣨Ϊ��Ĭ�������λ�ã�

    [Header("��������")]
    public int normalDamage = 10;   // ����˺�
    public int heavyDamage = 25;   // �����˺�
    public float hitForce = 5f;   // ��ͻ�������
    public float attackRange = 2.5f; // ��󹥻�����
    public float attackRate = 0.5f; // �����ȴ
    public float heavyCooldown = 1.0f; // ������ȴ
    public float chargeThreshold = 3f;// �ﵽ��������ʱ�䣨�룩

    [Header("�� (�Ҽ�����)")]
    [Range(0f, 1f)] public float blockDamageRate = 0.15f;
    public AudioClip blockStartSfx, blockEndSfx;

    [Header("ͨ����Ч")]
    public AudioClip swingSfx, hitSfx;
    #endregion

    #region ˽���ֶ�
    private float nextAttackTime;
    private bool isCharging;
    private float chargeTimer;
    private bool isBlocking;
    private AudioSource audioSrc;
    #endregion

    // UI ���ģ������ٷֱ�
    public event Action<float> OnChargeUpdate;

    public bool IsBlocking => isBlocking;
    public float BlockDamageRate => blockDamageRate;
    public bool IsCharging => isCharging;

    // ������������������������������������������������������������
    void Start()
    {
        if (!cam) cam = Camera.main;
        if (!muzzle) muzzle = cam.transform;
        audioSrc = GetComponent<AudioSource>();
        Debug.Log("<color=cyan>[PlayerCombat]</color> ��ʼ�����");
    }

    void Update() => HandleInput();

    // ������������������������������������������������������������
    #region ���봦��
    private void HandleInput()
    {
        // ���� �� ���� //
        if (Input.GetKeyDown(KeyCode.Mouse1)) BeginBlock();
        if (Input.GetKeyUp(KeyCode.Mouse1)) EndBlock();
        if (isBlocking) return; // ���ڼ䲻�ܹ���

        // ���� ��ʼ���� ���� //
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextAttackTime)
        {
            isCharging = true;
            chargeTimer = 0f;
            Debug.Log("<color=yellow>[PlayerCombat]</color> ��ʼ����");
        }

        // ���� ������ ���� //
        if (isCharging && Input.GetKey(KeyCode.Mouse0))
        {
            chargeTimer += Time.deltaTime;
            OnChargeUpdate?.Invoke(Mathf.Clamp01(chargeTimer / chargeThreshold));
        }

        // ���� �ɿ���ȭ ���� //
        if (Input.GetKeyUp(KeyCode.Mouse0) && isCharging)
        {
            float scale = Mathf.Clamp01(chargeTimer / chargeThreshold); // 0~1
            int dmg = Mathf.RoundToInt(Mathf.Lerp(normalDamage, heavyDamage, scale));
            float force = Mathf.Lerp(hitForce, hitForce * heavyDamage / normalDamage, scale);
            float cd = Mathf.Lerp(attackRate, heavyCooldown, scale);

            Debug.Log($"<color=lime>[PlayerCombat]</color> ��ȭ������ϵ�� {scale:P0} | �˺� {dmg} | ���� {force:F1} | CD {cd:F1}s");

            StartCoroutine(PerformAttack(dmg, force)); // Э���� 0.1s ���붯��
            nextAttackTime = Time.time + cd;

            // ����״̬
            isCharging = false;
            OnChargeUpdate?.Invoke(0f);
        }
    }

    // ǿ��ȡ������/���������ڱ�����ʱ
    public void CancelCharging()
    {
        if (isCharging)
        {
            isCharging = false;
            OnChargeUpdate?.Invoke(0f);
            Debug.Log("<color=grey>[PlayerCombat]</color> ���������");
        }
        StopAllCoroutines();
    }
    #endregion

    // ������������������������������������������������������������
    #region ��
    private void BeginBlock()
    {
        if (isBlocking) return;
        isBlocking = true;
        //PlaySound(blockStartSfx);
        Debug.Log("<color=orange>[PlayerCombat]</color> �����״̬");
    }
    private void EndBlock()
    {
        if (!isBlocking) return;
        isBlocking = false;
        //PlaySound(blockEndSfx);
        Debug.Log("<color=orange>[PlayerCombat]</color> �˳���״̬");
    }
    #endregion

    // ������������������������������������������������������������
    #region ��������
    private IEnumerator PerformAttack(int finalDamage, float finalForce)
    {
        //PlaySound(swingSfx);
        yield return new WaitForSeconds(0.1f); // ���붯����ֵ

        if (RayHitEnemy(out RaycastHit hit))
        {
            Debug.Log($"<color=green>[PlayerCombat]</color> ���� {hit.collider.name} | �˺� {finalDamage}");
            ApplyDamage(hit, finalDamage, finalForce);
        }
        else
        {
            Debug.Log("<color=grey>[PlayerCombat]</color> δ����");
        }
    }

    /// <summary>
    /// �������߼����ˣ��������ϰ�������ֹ
    /// </summary>
    private bool RayHitEnemy(out RaycastHit hitInfo)
    {
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hitInfo,
                            attackRange, obstacleMask | enemyMask,
                            QueryTriggerInteraction.Ignore))
        {
            bool isEnemy = ((1 << hitInfo.collider.gameObject.layer) & enemyMask.value) != 0;
            Debug.DrawLine(muzzle.position, hitInfo.point, isEnemy ? Color.red : Color.blue, 0.2f);
            return isEnemy;
        }
        return false;
    }

    private void ApplyDamage(RaycastHit hit, int dmg, float force)
    {
        PlaySound(hitSfx);

        // ���ˣ������˴����壩
        if (hit.rigidbody)
            hit.rigidbody.AddForce(muzzle.forward * force, ForceMode.Impulse);

        // ���е��� �� ����˫���� TakeDamage(dmg, attacker)
        if (hit.collider.TryGetComponent(out EnemyController ec))
        {
            ec.TakeDamage(dmg, transform);
        }
    }
    #endregion

    // ������������������������������������������������������������
    private void PlaySound(AudioClip clip)
    {
        if (clip) audioSrc.PlayOneShot(clip);
    }
}
