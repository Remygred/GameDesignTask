using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// ͳһ���� / ������꣬��������ͣ�˵�
/// ������ DontDestroyOnLoad �ĵ���������������� GameManager �ϣ�
/// </summary>
public class CursorPauseManager : MonoBehaviour
{
    [Header("�˵����")]
    public GameObject pausePanel;               // ��ͣ�˵� UI��Canvas �ڣ�

    [Header("�Զ����¼�")]
    public UnityEvent onGamePaused;             // ���� Inspector �����ͣ������ͣ������
    public UnityEvent onGameResumed;            // �ָ�ʱ�ص�

    private bool isPaused;

    /* ������ѡ ���� ����糡������
    public static CursorPauseManager Instance { get; private set; }
    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
    }
    */

    void Start()
    {
        LockCursor(true);               // ��Ϸ��ʼ�������������
        if (pausePanel) pausePanel.SetActive(false);
    }

    void Update()
    {
        // Esc ���л���ͣ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    #region Public API ����ť���Ľű�����
    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;            // ȫ����ͣ
        LockCursor(false);              // ��������ʾ���
        if (pausePanel) pausePanel.SetActive(true);
        onGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;
        LockCursor(true);               // ������������
        if (pausePanel) pausePanel.SetActive(false);
        onGameResumed?.Invoke();
    }

    public void QuitToMenu(string menuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        LockCursor(false);              // �����˵�����������
        SceneManager.LoadScene(menuSceneName);
    }

    // ������ǰ�ؿ�
    public void Restart()
    {
        ResetGameState(); // ������״̬
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // �ؼ�������������Ϸ״̬��ʱ��+��꣩
    private void ResetGameState()
    {
        Time.timeScale = 1f; // ȷ��ʱ��ָ�
        LockCursor(true); // �ָ���Ϸʱ�������
    }
    #endregion

    #region Cursor Helper
    /// <summary>
    /// lockCursor = true  �� CursorLockMode.Locked + invisible  
    /// lockCursor = false �� CursorLockMode.None   + visible
    /// </summary>
    private void LockCursor(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;   // �����������
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;     // ����
            Cursor.visible = true;
        }
    }

    // ���������»�ý��㣬����Ϸ����ͣҲ��������꣨��ֹ Alt-Tab ��ס��
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isPaused)
            LockCursor(true);
    }
    #endregion
}
