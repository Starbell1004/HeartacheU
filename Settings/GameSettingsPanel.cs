using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;
using Naninovel.UI;

/// <summary>
/// GameSettingsMenu를 상속받아 우리 기능을 추가한 설정 UI
/// 나니노벨의 모든 시스템(Modal, Input, Save 등)을 그대로 활용
/// </summary>
public class GameSettingsPanel : GameSettingsMenu
{
    [Header("우리 커스텀 UI 요소들")]
    [SerializeField] private Button customCloseButton;

    [Header("BGM 설정")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;

    [Header("SFX 설정")]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Button testSfxButton;

    [Header("텍스트 속도")]
    [SerializeField] private Slider textSpeedSlider;
    [SerializeField] private TextMeshProUGUI textSpeedText;

    [Header("화면 모드")]
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button windowedButton;

    [Header("화면 모드 스프라이트")]
    [SerializeField] private Sprite fullscreenActiveSprite;
    [SerializeField] private Sprite fullscreenInactiveSprite;
    [SerializeField] private Sprite windowActiveSprite;
    [SerializeField] private Sprite windowInactiveSprite;

    [Header("창 모드 크기")]
    [SerializeField] private int windowWidth = 1280;
    [SerializeField] private int windowHeight = 720;

    // 나니노벨 서비스들
    private IAudioManager audioManager;
    private ITextPrinterManager printerManager;

    #region 나니노벨 시스템 + 우리 기능 통합

    public override async UniTask Initialize()
    {
        Debug.Log("[GameSettingsPanel] GameSettingsMenu 상속 Initialize 시작");

        // 부모 클래스의 모든 시스템 먼저 초기화
        await base.Initialize();

        // 서비스 가져오기
        audioManager = Engine.GetService<IAudioManager>();
        printerManager = Engine.GetService<ITextPrinterManager>();

        // 우리 커스텀 UI 설정
        SetupCustomUI();
        RefreshAllCustomSettings();

        Debug.Log("[GameSettingsPanel] 커스텀 기능 추가 완료");
    }

    protected override void Awake()
    {
        //  부모 클래스의 모든 기능 먼저 실행
        base.Awake();

        Debug.Log("[GameSettingsPanel] GameSettingsMenu 상속 Awake 완료");
    }

    #endregion

    #region 커스텀 UI 설정

    private void SetupCustomUI()
    {
        Debug.Log("[GameSettingsPanel] 커스텀 UI 설정 시작");

        SetupCustomCloseButton();
        SetupBGMSlider();
        SetupSFXSlider();
        SetupTextSpeedSlider();
        SetupScreenModeButtons();
    }

    private void SetupCustomCloseButton()
    {
        if (customCloseButton != null)
        {
            customCloseButton.onClick.RemoveAllListeners();
            //  부모 클래스의 SaveSettingsAndHide() 활용
            customCloseButton.onClick.AddListener(() => SaveSettingsAndHide().Forget());
        }
    }

    private void SetupBGMSlider()
    {
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.minValue = 0f;
            bgmVolumeSlider.maxValue = 1f;
            bgmVolumeSlider.onValueChanged.RemoveAllListeners();
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
    }

    private void SetupSFXSlider()
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (testSfxButton != null)
        {
            testSfxButton.onClick.RemoveAllListeners();
            testSfxButton.onClick.AddListener(PlayTestSfx);
        }
    }

    private void SetupTextSpeedSlider()
    {
        if (textSpeedSlider != null)
        {
            textSpeedSlider.minValue = 0.1f;
            textSpeedSlider.maxValue = 3f;
            textSpeedSlider.onValueChanged.RemoveAllListeners();
            textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
        }
    }

    private void SetupScreenModeButtons()
    {
        if (fullscreenButton != null)
        {
            fullscreenButton.onClick.RemoveAllListeners();
            fullscreenButton.onClick.AddListener(SetFullscreen);
        }

        if (windowedButton != null)
        {
            windowedButton.onClick.RemoveAllListeners();
            windowedButton.onClick.AddListener(SetWindowed);
        }
    }

    #endregion

    #region Show/Hide 오버라이드

    public override void Show()
    {
        Debug.Log("[GameSettingsPanel] ★ 상속된 Show 호출 ★");

        //  부모 클래스의 모든 Modal 시스템 활용
        base.Show();

        // 우리 설정 새로고침
        RefreshAllCustomSettings();
    }

    public override void Hide()
    {
        Debug.Log("[GameSettingsPanel] ★ 상속된 Hide 호출 ★");

        //  부모 클래스의 모든 Modal 시스템 활용
        base.Hide();
    }

    #endregion

    #region 커스텀 설정 새로고침

    private void RefreshAllCustomSettings()
    {
        RefreshBGMVolume();
        RefreshSFXVolume();
        RefreshTextSpeed();
        RefreshScreenMode();
    }

    private void RefreshBGMVolume()
    {
        if (bgmVolumeSlider == null) return;

        float volume = audioManager?.BgmVolume ?? PlayerPrefs.GetFloat("BGMVolume", 1f);
        bgmVolumeSlider.value = volume;
        UpdateBGMDisplay(volume);
    }

    private void RefreshSFXVolume()
    {
        if (sfxVolumeSlider == null) return;

        float volume = audioManager?.SfxVolume ?? PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxVolumeSlider.value = volume;
        UpdateSFXDisplay(volume);
    }

    private void RefreshTextSpeed()
    {
        if (textSpeedSlider == null) return;

        float speed = printerManager?.BaseRevealSpeed ?? PlayerPrefs.GetFloat("TextSpeed", 1f);
        textSpeedSlider.value = speed;
        UpdateTextSpeedDisplay(speed);
    }

    private void RefreshScreenMode()
    {
        UpdateScreenModeButtons();
    }

    #endregion

    #region BGM 볼륨

    private void OnBGMVolumeChanged(float volume)
    {
        if (audioManager != null)
            audioManager.BgmVolume = volume;
        else
            PlayerPrefs.SetFloat("BGMVolume", volume);

        UpdateBGMDisplay(volume);
    }

    private void UpdateBGMDisplay(float volume)
    {
        if (bgmVolumeText != null)
            bgmVolumeText.text = $"{volume:F1}";
    }

    #endregion

    #region SFX 볼륨

    private void OnSFXVolumeChanged(float volume)
    {
        if (audioManager != null)
            audioManager.SfxVolume = volume;
        else
            PlayerPrefs.SetFloat("SFXVolume", volume);

        UpdateSFXDisplay(volume);
    }

    private void UpdateSFXDisplay(float volume)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = $"{volume:F1}";
    }

    private void PlayTestSfx()
    {
        // 간단한 테스트 톤 생성 (440Hz)
        GameObject tempObj = new GameObject("GameSettings_TestSound");
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();

        // 0.3초 길이의 테스트 톤
        int sampleRate = 44100;
        float frequency = 440f;
        float duration = 0.3f;
        int samples = Mathf.RoundToInt(sampleRate * duration);

        AudioClip clip = AudioClip.Create("TestTone", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.3f;
        }

        clip.SetData(data, 0);
        tempSource.clip = clip;
        tempSource.volume = sfxVolumeSlider?.value ?? 0.5f;
        tempSource.Play();

        // 재생 완료 후 삭제
        Destroy(tempObj, duration + 0.1f);
    }

    #endregion

    #region 텍스트 속도

    private void OnTextSpeedChanged(float speed)
    {
        Debug.Log($"[GameSettingsPanel] 텍스트 속도 변경: {speed}");

        if (printerManager != null)
        {
            printerManager.BaseRevealSpeed = speed;
            Debug.Log($"[GameSettingsPanel] 실제 적용된 속도: {printerManager.BaseRevealSpeed}");
        }
        else
        {
            PlayerPrefs.SetFloat("TextSpeed", speed);
            Debug.Log("[GameSettingsPanel] printerManager null - PlayerPrefs 저장");
        }

        // 강제로 설정 저장
        PlayerPrefs.SetFloat("TextSpeed", speed);
        PlayerPrefs.Save();

        UpdateTextSpeedDisplay(speed);

        Debug.Log($"[GameSettingsPanel] 최종 확인 - printerManager.BaseRevealSpeed: {printerManager?.BaseRevealSpeed ?? -1}");
    }

    private void UpdateTextSpeedDisplay(float speed)
    {
        if (textSpeedText != null)
            textSpeedText.text = $"{speed:F1}x";
    }

    #endregion

    #region 화면 모드

    private void SetFullscreen()
    {
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        UpdateScreenModeButtons();
    }

    private void SetWindowed()
    {
        Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);
        UpdateScreenModeButtons();
    }

    private void UpdateScreenModeButtons()
    {
        if (fullscreenButton == null || windowedButton == null) return;

        Image fullscreenImg = fullscreenButton.GetComponent<Image>();
        Image windowedImg = windowedButton.GetComponent<Image>();

        if (fullscreenImg == null || windowedImg == null) return;

        bool isFullscreen = Screen.fullScreenMode == FullScreenMode.FullScreenWindow ||
                           Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;

        if (isFullscreen)
        {
            if (fullscreenActiveSprite != null && windowInactiveSprite != null)
            {
                fullscreenImg.sprite = fullscreenActiveSprite;
                windowedImg.sprite = windowInactiveSprite;
            }
        }
        else
        {
            if (windowActiveSprite != null && fullscreenInactiveSprite != null)
            {
                fullscreenImg.sprite = fullscreenInactiveSprite;
                windowedImg.sprite = windowActiveSprite;
            }
        }
    }

    #endregion

    #region 정리

    protected override void OnDestroy()
    {
        // 우리 이벤트 리스너 정리
        CleanupCustomEventListeners();

        //  부모 클래스 정리
        base.OnDestroy();
    }

    private void CleanupCustomEventListeners()
    {
        if (customCloseButton != null)
            customCloseButton.onClick.RemoveAllListeners();
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (testSfxButton != null)
            testSfxButton.onClick.RemoveAllListeners();
        if (textSpeedSlider != null)
            textSpeedSlider.onValueChanged.RemoveAllListeners();
        if (fullscreenButton != null)
            fullscreenButton.onClick.RemoveAllListeners();
        if (windowedButton != null)
            windowedButton.onClick.RemoveAllListeners();
    }

    #endregion
}