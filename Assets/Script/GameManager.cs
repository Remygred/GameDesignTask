using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject winPanel;    // 胜利界面
    public GameObject losePanel;   // 失败界面
    private int enemiesRemaining;

    void Awake()
    {
        // 单例模式
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 统计场景中所有敌人
        enemiesRemaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
        // 隐藏胜负界面
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    // 敌人死亡时调用
    public void OnEnemyDefeated()
    {
        enemiesRemaining--;
        if (enemiesRemaining <= 0)
        {
            WinGame();
        }
    }

    // 玩家死亡时调用
    public void OnPlayerDefeated()
    {
        LoseGame();
    }

    void WinGame()
    {
        if (winPanel) winPanel.SetActive(true);
        Time.timeScale = 0f;  // 暂停游戏
    }

    void LoseGame()
    {
        if (losePanel) losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // 可用于按钮重新开始游戏
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
