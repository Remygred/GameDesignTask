using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 统一锁定 / 解锁鼠标，并管理暂停菜单
/// （挂在 DontDestroyOnLoad 的单例空物体或主场景 GameManager 上）
/// </summary>
public class CursorPauseManager : MonoBehaviour
{
    [Header("菜单面板")]
    public GameObject pausePanel;               // 暂停菜单 UI（Canvas 内）

    [Header("自定义事件")]
    public UnityEvent onGamePaused;             // 可在 Inspector 里绑定暂停音、暂停动画等
    public UnityEvent onGameResumed;            // 恢复时回调

    private bool isPaused;

    /* 单例可选 —— 方便跨场景访问
    public static CursorPauseManager Instance { get; private set; }
    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
    }
    */

    void Start()
    {
        LockCursor(true);               // 游戏开始即锁定隐藏鼠标
        if (pausePanel) pausePanel.SetActive(false);
    }

    void Update()
    {
        // Esc 键切换暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    #region Public API 给按钮或别的脚本调用
    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;            // 全局暂停
        LockCursor(false);              // 解锁并显示鼠标
        if (pausePanel) pausePanel.SetActive(true);
        onGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;
        LockCursor(true);               // 重新锁定隐藏
        if (pausePanel) pausePanel.SetActive(false);
        onGameResumed?.Invoke();
    }

    public void QuitToMenu(string menuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        LockCursor(false);              // 回主菜单可让鼠标解锁
        SceneManager.LoadScene(menuSceneName);
    }

    // 重启当前关卡
    public void Restart()
    {
        ResetGameState(); // 先重置状态
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 关键方法：重置游戏状态（时间+光标）
    private void ResetGameState()
    {
        Time.timeScale = 1f; // 确保时间恢复
        LockCursor(true); // 恢复游戏时锁定鼠标
    }
    #endregion

    #region Cursor Helper
    /// <summary>
    /// lockCursor = true  → CursorLockMode.Locked + invisible  
    /// lockCursor = false → CursorLockMode.None   + visible
    /// </summary>
    private void LockCursor(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;   // 鼠标锁在中心
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;     // 解锁
            Cursor.visible = true;
        }
    }

    // 当窗口重新获得焦点，若游戏非暂停也重新锁鼠标（防止 Alt-Tab 后卡住）
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isPaused)
            LockCursor(true);
    }
    #endregion
}
