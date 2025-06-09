using UnityEngine;
using UnityEngine.UI;

public class CrosshairColorSwitcher : MonoBehaviour
{
    [Header("检测参数")]
    public Transform cam;                     // 主摄像机（若空自动 Camera.main）
    public float checkRange = 50f;         // 检测最远距离
    public LayerMask enemyMask;            // 敌人层
    public LayerMask obstacleMask;         // 障碍层（墙体等，优先阻挡）

    [Header("颜色")]
    public Color normalColor = Color.green; // 默认绿色
    public Color targetColor = Color.red;   // 命中敌人时红色

    private Image img;                      // 自身 Image 组件缓存

    void Awake()
    {
        img = GetComponent<Image>();
        if (cam == null) cam = Camera.main.transform;
    }

    void Update()
    {
        // 从摄像机正前方发射一条射线
        Ray ray = new Ray(cam.position, cam.forward);
        // 先做一次“是否被墙挡住”的检测
        if (Physics.Raycast(ray, out RaycastHit hit, checkRange, obstacleMask | enemyMask,
                            QueryTriggerInteraction.Ignore))
        {
            // 如果第一个撞到的就是敌人 → 红色
            bool isEnemy =
                ((1 << hit.collider.gameObject.layer) & enemyMask.value) != 0;
            img.color = isEnemy ? targetColor : normalColor;
        }
        else
        {
            // 射线什么都没撞到 → 绿色
            img.color = normalColor;
        }
    }
}
