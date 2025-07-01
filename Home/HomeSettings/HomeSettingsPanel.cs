using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;

/// <summary>
/// 홈화면 전용 설정 패널 - ISettingsUI 제거, CustomUI 유지
/// </summary>
public class HomeSettingsPanel : CustomUI
{
    [Header("UI 기본")]
    [SerializeField] private Button closeButton;

    [Header("개별 설정 컴포넌트들")]
    [SerializeField] private HomeTextSpeedSettings textSpeedSettings;
    [SerializeField] private HomeScreenModeSettings screenModeSettings;
    [SerializeField] private HomeBGMVolumeSettings bgmVolumeSettings;
    [SerializeField] private HomeSFXVolumeSettings sfxVolumeSettings;

    // 나니노벨 서비스
    private IStateManager stateManager;

    public override async UniTask Initialize()
    {
        Debug.Log("[HomeSettingsPanel] Initialize 시작");

        // 나니노벨 서비스 가져오기
        stateManager = Engine.GetServiceOrErr<IStateManager>();

        SetupCloseButton();

        Debug.Log("[HomeSettingsPanel] Initialize 완료");
    }

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[HomeSettingsPanel] Awake 호출");

        // 서비스 초기화
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void InitializeServices()
    {
        try
        {
            stateManager = Engine.GetService<IStateManager>();
            Debug.Log("[HomeSettingsPanel] 나니노벨 서비스 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSettingsPanel] 서비스 초기화 오류: {ex.Message}");
        }
    }

    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => SaveSettingsAndHide().Forget());
            Debug.Log("[HomeSettingsPanel] 닫기 버튼 이벤트 등록");
        }
        else
        {
            Debug.LogError("[HomeSettingsPanel] closeButton이 null입니다!");
        }
    }

    public override void Show()
    {
        Debug.Log("[HomeSettingsPanel] Show 호출");
        gameObject.SetActive(true);
        base.Show();

        RefreshAllSettings();

        // 텍스트 미리보기 시작
        if (textSpeedSettings != null)
        {
            textSpeedSettings.ShowPreview();
        }
    }

    public override void Hide()
    {
        Debug.Log("[HomeSettingsPanel] Hide 호출");

        // 텍스트 미리보기 중지
        if (textSpeedSettings != null)
        {
            textSpeedSettings.HidePreview();
        }

        SaveSettingsAndHide().Forget();
    }

    // 나니노벨 호환 - 설정 저장 후 숨기기
    public virtual async UniTask SaveSettingsAndHide()
    {
        Debug.Log("[HomeSettingsPanel] 설정 저장 중...");

        try
        {
            using (new InteractionBlocker())
            {
                if (stateManager != null)
                {
                    await stateManager.SaveSettings();
                    Debug.Log("[HomeSettingsPanel] 나니노벨 설정 저장 완료");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSettingsPanel] 설정 저장 오류: {ex.Message}");
        }

        gameObject.SetActive(false);
        base.Hide();
    }

    private void RefreshAllSettings()
    {
        Debug.Log("[HomeSettingsPanel] 모든 설정 새로고침");

        // 각 컴포넌트의 설정 새로고침
        if (textSpeedSettings != null)
        {
            textSpeedSettings.RefreshSettings();
        }

        if (screenModeSettings != null)
        {
            screenModeSettings.RefreshSettings();
        }

        if (bgmVolumeSettings != null)
        {
            bgmVolumeSettings.RefreshSettings();
        }

        if (sfxVolumeSettings != null)
        {
            sfxVolumeSettings.RefreshSettings();
        }

        Debug.Log("[HomeSettingsPanel] 설정 새로고침 완료");
    }

    // 외부에서 호출용
    public void RefreshSettings()
    {
        RefreshAllSettings();
    }

    // 개별 컴포넌트 상태 확인용
    private void Start()
    {
        Debug.Log("[HomeSettingsPanel] Start - 컴포넌트 연결 상태 확인");
        Debug.Log($"textSpeedSettings: {(textSpeedSettings != null ? "연결됨" : "null")}");
        Debug.Log($"screenModeSettings: {(screenModeSettings != null ? "연결됨" : "null")}");
        Debug.Log($"bgmVolumeSettings: {(bgmVolumeSettings != null ? "연결됨" : "null")}");
        Debug.Log($"sfxVolumeSettings: {(sfxVolumeSettings != null ? "연결됨" : "null")}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Engine.OnInitializationFinished -= InitializeServices;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }
    }
}