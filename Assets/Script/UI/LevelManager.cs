using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡管理器：
///  · 单敌人被击败 → 胜利菜单
///  · 玩家死亡      → 失败菜单
///  · 按钮回调：下一关 / 重新开始 / 主菜单
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI 面板")]
    public GameObject victoryPanel;   // 胜利面板
    public GameObject defeatPanel;    // 失败面板

    [Header("关卡流程")]
    [Tooltip("下一关场景名称（本关是最终关可留空）")]
    public string nextLevelName = "Level2";
    [Tooltip("勾选 = 最终关，胜利后显示重新开始而非下一关")]
    public bool isFinalLevel = false;

    [Header("主菜单场景名")]
    public string mainMenuScene = "MainMenu";

    [Header("玩家位置")]
    public Transform PlayerTransform;

    public GameObject BGM;

    [SerializeField]
    AudioSource audioS;//声音
    public AudioClip win,lose;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (victoryPanel) victoryPanel.SetActive(false);
        if (defeatPanel) defeatPanel.SetActive(false);
        Time.timeScale = 1f;
        audioS = GetComponent<AudioSource>();
    }

    /* ---------- 敌人 / 玩家 调用 ---------- */
    public void OnEnemyDefeated()
    {
        BGM.SetActive(false);
        audioS.PlayOneShot(win);
        Cursor.lockState = CursorLockMode.None;     // 解锁
        Cursor.visible = true;
        Debug.Log("[LevelManager] Enemy defeated");
        if (victoryPanel) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnPlayerDefeated()
    {
        BGM.SetActive(false);
        audioS.PlayOneShot(lose);
        audioS.Play();
        Cursor.lockState = CursorLockMode.None;     // 解锁
        Cursor.visible = true;
        Debug.Log("[LevelManager] Player defeated");
        if (defeatPanel) defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    /* ---------- 胜利菜单按钮 ---------- */
    public void BtnNextLevel()     // 第一关胜利
    {
        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogWarning("nextLevelName 未设置！");
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }

    public void BtnRestartLevel()  // 最终关胜利
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /* ---------- 失败菜单按钮 ---------- */
    public void BtnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /* ---------- 通用按钮 ---------- */
    public void BtnReturnMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
