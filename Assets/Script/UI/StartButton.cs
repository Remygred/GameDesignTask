using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        // 直接加载Build Settings中的第0号场景
        SceneManager.LoadScene(0);
    }
}