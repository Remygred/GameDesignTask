using UnityEngine;
using UnityEngine.UI;

public class CrosshairColorSwitcher : MonoBehaviour
{
    [Header("������")]
    public Transform cam;                     // ��������������Զ� Camera.main��
    public float checkRange = 50f;         // �����Զ����
    public LayerMask enemyMask;            // ���˲�
    public LayerMask obstacleMask;         // �ϰ��㣨ǽ��ȣ������赲��

    [Header("��ɫ")]
    public Color normalColor = Color.green; // Ĭ����ɫ
    public Color targetColor = Color.red;   // ���е���ʱ��ɫ

    private Image img;                      // ���� Image �������

    void Awake()
    {
        img = GetComponent<Image>();
        if (cam == null) cam = Camera.main.transform;
    }

    void Update()
    {
        // ���������ǰ������һ������
        Ray ray = new Ray(cam.position, cam.forward);
        // ����һ�Ρ��Ƿ�ǽ��ס���ļ��
        if (Physics.Raycast(ray, out RaycastHit hit, checkRange, obstacleMask | enemyMask,
                            QueryTriggerInteraction.Ignore))
        {
            // �����һ��ײ���ľ��ǵ��� �� ��ɫ
            bool isEnemy =
                ((1 << hit.collider.gameObject.layer) & enemyMask.value) != 0;
            img.color = isEnemy ? targetColor : normalColor;
        }
        else
        {
            // ����ʲô��ûײ�� �� ��ɫ
            img.color = normalColor;
        }
    }
}
