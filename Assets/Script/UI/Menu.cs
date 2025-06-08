using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject menuList;
    private bool isMenuOpen = false;
    private CursorLockMode previousCursorState;

    void Start()
    {
        // 确保游戏开始时菜单关闭、时间正常、光标锁定
        if (menuList != null) menuList.SetActive(false);
        ResetGameState(); // 初始化游戏状态
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // 切换菜单（ESC键）
    public void ToggleMenu()
    {
        if (menuList == null) return;

        isMenuOpen = !isMenuOpen;
        menuList.SetActive(isMenuOpen);

        if (isMenuOpen)
        {
            // 打开菜单：暂停游戏 + 显示光标
            Time.timeScale = 0f;
            previousCursorState = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 关闭菜单：恢复游戏
            ResetGameState();
        }
    }

    // 返回游戏（和ESC关闭菜单相同）
    public void BackGame()
    {
        if (menuList != null)
        {
            isMenuOpen = false;
            menuList.SetActive(false);
            ResetGameState(); // 关键点：完全恢复游戏状态
        }
    }

    // 重启当前关卡
    public void Restart()
    {
        ResetGameState(); // 先重置状态
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 退出到主菜单
    public void QuitToMainMenu()
    {
        ResetGameState(); // 先重置状态
        SceneManager.LoadScene("MainMenu"); // 确保场景名正确
    }

    // 关键方法：重置游戏状态（时间+光标）
    private void ResetGameState()
    {
        Time.timeScale = 1f; // 确保时间恢复
        Cursor.lockState = CursorLockMode.Locked; // 锁定光标（根据游戏需求调整）
        Cursor.visible = false; // 隐藏光标
    }
}