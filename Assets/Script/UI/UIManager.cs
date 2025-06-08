using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Player UI")]
    public Slider PlayerhealthBar;
    public Slider ChargingBar;

    [Header("Enemy UI")]
    public Slider EnemyhealthBar;

    [Header("依赖引用")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public EnemyHealth enemyHealth;

    private float currentPlayerHealthDisplay;
    private float currentEnemyHealthDisplay;

    [Header("动画设置")]
    [SerializeField] float smoothSpeed = 5f; // 插值速度


    void OnEnable()
    {
        // 订阅蓄力事件
        if (playerCombat != null)
        {
            playerCombat.OnChargeUpdate += HandleChargeUpdate;
        }
    }

    void OnDisable()
    {
        // 取消订阅防止内存泄漏
        if (playerCombat != null)
        {
            playerCombat.OnChargeUpdate -= HandleChargeUpdate;
        }
    }

    private void Awake()
    {
        PlayerhealthBar.value = playerHealth ? (float)playerHealth.Hp / playerHealth.maxHp : 0f;
        EnemyhealthBar.value = enemyHealth ? (float)enemyHealth.Hp / enemyHealth.maxHealth : 0f;
        ChargingBar.value = 0f; // 初始化蓄力条   
    }

    void Update()
    {
        // 目标值计算
        float targetPlayerHealth = playerHealth ?
            (float)playerHealth.Hp / playerHealth.maxHp : 0f;
        float targetEnemyHealth = enemyHealth ?
            (float)enemyHealth.Hp / enemyHealth.maxHealth : 0f;

        // 平滑插值
        currentPlayerHealthDisplay = Mathf.Lerp(currentPlayerHealthDisplay,targetPlayerHealth,smoothSpeed * Time.deltaTime);
        currentEnemyHealthDisplay = Mathf.Lerp(currentEnemyHealthDisplay,targetEnemyHealth,smoothSpeed * Time.deltaTime);

        // 更新UI
        PlayerhealthBar.value = currentPlayerHealthDisplay;
        EnemyhealthBar.value = currentEnemyHealthDisplay;

        if (PlayerhealthBar.value == 0)
        {
            Color currentColor = PlayerhealthBar.fillRect.GetComponent<Image>().color;
            currentColor.a = 0f;
            PlayerhealthBar.fillRect.GetComponent<Image>().color = currentColor;
        }
        if (ChargingBar.value == 0)
        {
            Color currentColor = ChargingBar.fillRect.GetComponent<Image>().color;
            currentColor.a = 0f;
            ChargingBar.fillRect.GetComponent<Image>().color = currentColor;
        }
        if(EnemyhealthBar.value == 0)
        {
            Color currentColor = EnemyhealthBar.fillRect.GetComponent<Image>().color;
            currentColor.a = 0f;
            EnemyhealthBar.fillRect.GetComponent<Image>().color = currentColor;
        }
    }

    // 蓄力事件处理器
    private void HandleChargeUpdate(float chargePercent)
    {
        ChargingBar.value = chargePercent;

        // 可选：添加视觉反馈
        if (chargePercent >= 0.5f)
        {
            ChargingBar.fillRect.GetComponent<Image>().color = new Color(1f, 128 / 255f, 0);
        }
        else
        {
            ChargingBar.fillRect.GetComponent<Image>().color = Color.blue;
        }
    }
}
