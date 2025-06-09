using UnityEngine;
using UnityEngine.UI;

public class SoundToggleController : MonoBehaviour
{
    // ���л��ֶΣ�������Inspector�и�ֵ
    [Header("UI Settings")]
    [Tooltip("��ť��Image����������л�ͼ��")]
    [SerializeField] private Image buttonImage;

    [Tooltip("��������ʱ��ʾ��ͼ��")]
    [SerializeField] private Sprite soundOnSprite;

    [Tooltip("�����ر�ʱ��ʾ��ͼ��")]
    [SerializeField] private Sprite soundOffSprite;

    [Header("Audio Settings")]
    [Tooltip("���Ʊ������ֵ�AudioSource")]
    [SerializeField] private AudioSource bgmAudioSource;

    // ��ǰ����״̬
    private bool isSoundOn = true;

    // ��ʼ��
    private void Start()
    {
        // ��PlayerPrefs���ر����״̬������У�
        if (PlayerPrefs.HasKey("SoundEnabled"))
        {
            isSoundOn = PlayerPrefs.GetInt("SoundEnabled") == 1;
        }

        // Ӧ�ó�ʼ״̬
        UpdateAudioState();
        UpdateButtonAppearance();
    }

    // ��ť���ʱ����
    public void OnSoundButtonClicked()
    {
        // �л�״̬
        isSoundOn = !isSoundOn;

        // ������Ƶ��UI
        UpdateAudioState();
        UpdateButtonAppearance();

        // ����״̬
        PlayerPrefs.SetInt("SoundEnabled", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ������Ƶ״̬
    private void UpdateAudioState()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.mute = !isSoundOn;
        }
    }

    // ���°�ť���
    private void UpdateButtonAppearance()
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }
}