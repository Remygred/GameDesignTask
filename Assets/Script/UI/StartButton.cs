using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        // ֱ�Ӽ���Build Settings�еĵ�0�ų���
        SceneManager.LoadScene(0);
    }
}