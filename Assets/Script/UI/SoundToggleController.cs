using UnityEngine;
using UnityEngine.UI;

public class SoundToggleController : MonoBehaviour
{
    // 序列化字段，可以在Inspector中赋值
    [Header("UI Settings")]
    [Tooltip("按钮的Image组件，用于切换图标")]
    [SerializeField] private Image buttonImage;

    [Tooltip("声音开启时显示的图标")]
    [SerializeField] private Sprite soundOnSprite;

    [Tooltip("声音关闭时显示的图标")]
    [SerializeField] private Sprite soundOffSprite;

    [Header("Audio Settings")]
    [Tooltip("控制背景音乐的AudioSource")]
    [SerializeField] private AudioSource bgmAudioSource;

    // 当前声音状态
    private bool isSoundOn = true;

    // 初始化
    private void Start()
    {
        // 从PlayerPrefs加载保存的状态（如果有）
        if (PlayerPrefs.HasKey("SoundEnabled"))
        {
            isSoundOn = PlayerPrefs.GetInt("SoundEnabled") == 1;
        }

        // 应用初始状态
        UpdateAudioState();
        UpdateButtonAppearance();
    }

    // 按钮点击时调用
    public void OnSoundButtonClicked()
    {
        // 切换状态
        isSoundOn = !isSoundOn;

        // 更新音频和UI
        UpdateAudioState();
        UpdateButtonAppearance();

        // 保存状态
        PlayerPrefs.SetInt("SoundEnabled", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 更新音频状态
    private void UpdateAudioState()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.mute = !isSoundOn;
        }
    }

    // 更新按钮外观
    private void UpdateButtonAppearance()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }
}