using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public Slider healthBar;        // 玩家血条（UI Slider）
    public GameObject winPanel;     // 胜利面板
    public GameObject losePanel;    // 失败面板

    public Image crosshair;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;

    public void HitFlash()
    {
        StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        Color ori = crosshair.color;
        crosshair.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        crosshair.color = ori;
    }


    void Start()
    {
        if (healthBar) healthBar.value = 1f;
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    // 更新血条（输入0~max之间的当前血量）
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar) healthBar.value = (float)currentHealth / maxHealth;
    }

    // 显示胜利界面
    public void ShowWin()
    {
        if (winPanel) winPanel.SetActive(true);
    }

    // 显示失败界面
    public void ShowLose()
    {
        if (losePanel) losePanel.SetActive(true);
    }
}
