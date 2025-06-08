using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject menuList;
    private bool isMenuOpen = false;
    private CursorLockMode previousCursorState;

    void Start()
    {
        // ȷ����Ϸ��ʼʱ�˵��رա�ʱ���������������
        if (menuList != null) menuList.SetActive(false);
        ResetGameState(); // ��ʼ����Ϸ״̬
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // �л��˵���ESC����
    public void ToggleMenu()
    {
        if (menuList == null) return;

        isMenuOpen = !isMenuOpen;
        menuList.SetActive(isMenuOpen);

        if (isMenuOpen)
        {
            // �򿪲˵�����ͣ��Ϸ + ��ʾ���
            Time.timeScale = 0f;
            previousCursorState = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // �رղ˵����ָ���Ϸ
            ResetGameState();
        }
    }

    // ������Ϸ����ESC�رղ˵���ͬ��
    public void BackGame()
    {
        if (menuList != null)
        {
            isMenuOpen = false;
            menuList.SetActive(false);
            ResetGameState(); // �ؼ��㣺��ȫ�ָ���Ϸ״̬
        }
    }

    // ������ǰ�ؿ�
    public void Restart()
    {
        ResetGameState(); // ������״̬
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // �˳������˵�
    public void QuitToMainMenu()
    {
        ResetGameState(); // ������״̬
        SceneManager.LoadScene("MainMenu"); // ȷ����������ȷ
    }

    // �ؼ�������������Ϸ״̬��ʱ��+��꣩
    private void ResetGameState()
    {
        Time.timeScale = 1f; // ȷ��ʱ��ָ�
        Cursor.lockState = CursorLockMode.Locked; // ������꣨������Ϸ���������
        Cursor.visible = false; // ���ع��
    }
}