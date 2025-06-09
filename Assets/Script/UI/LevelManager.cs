using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �ؿ���������
///  �� �����˱����� �� ʤ���˵�
///  �� �������      �� ʧ�ܲ˵�
///  �� ��ť�ص�����һ�� / ���¿�ʼ / ���˵�
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI ���")]
    public GameObject victoryPanel;   // ʤ�����
    public GameObject defeatPanel;    // ʧ�����

    [Header("�ؿ�����")]
    [Tooltip("��һ�س������ƣ����������չؿ����գ�")]
    public string nextLevelName = "Level2";
    [Tooltip("��ѡ = ���չأ�ʤ������ʾ���¿�ʼ������һ��")]
    public bool isFinalLevel = false;

    [Header("���˵�������")]
    public string mainMenuScene = "MainMenu";

    [Header("���λ��")]
    public Transform PlayerTransform;

    public GameObject BGM;

    [SerializeField]
    AudioSource audioS;//����
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

    /* ---------- ���� / ��� ���� ---------- */
    public void OnEnemyDefeated()
    {
        BGM.SetActive(false);
        audioS.PlayOneShot(win);
        Cursor.lockState = CursorLockMode.None;     // ����
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
        Cursor.lockState = CursorLockMode.None;     // ����
        Cursor.visible = true;
        Debug.Log("[LevelManager] Player defeated");
        if (defeatPanel) defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    /* ---------- ʤ���˵���ť ---------- */
    public void BtnNextLevel()     // ��һ��ʤ��
    {
        if (string.IsNullOrEmpty(nextLevelName))
        {
            Debug.LogWarning("nextLevelName δ���ã�");
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }

    public void BtnRestartLevel()  // ���չ�ʤ��
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /* ---------- ʧ�ܲ˵���ť ---------- */
    public void BtnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /* ---------- ͨ�ð�ť ---------- */
    public void BtnReturnMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
