using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;

/// <summary>
/// Ȩȭ�� ���� ���� �г� - ISettingsUI ����, CustomUI ����
/// </summary>
public class HomeSettingsPanel : CustomUI
{
    [Header("UI �⺻")]
    [SerializeField] private Button closeButton;

    [Header("���� ���� ������Ʈ��")]
    [SerializeField] private HomeTextSpeedSettings textSpeedSettings;
    [SerializeField] private HomeScreenModeSettings screenModeSettings;
    [SerializeField] private HomeBGMVolumeSettings bgmVolumeSettings;
    [SerializeField] private HomeSFXVolumeSettings sfxVolumeSettings;

    // ���ϳ뺧 ����
    private IStateManager stateManager;

    public override async UniTask Initialize()
    {
        Debug.Log("[HomeSettingsPanel] Initialize ����");

        // ���ϳ뺧 ���� ��������
        stateManager = Engine.GetServiceOrErr<IStateManager>();

        SetupCloseButton();

        Debug.Log("[HomeSettingsPanel] Initialize �Ϸ�");
    }

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[HomeSettingsPanel] Awake ȣ��");

        // ���� �ʱ�ȭ
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
            Debug.Log("[HomeSettingsPanel] ���ϳ뺧 ���� �ʱ�ȭ �Ϸ�");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSettingsPanel] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => SaveSettingsAndHide().Forget());
            Debug.Log("[HomeSettingsPanel] �ݱ� ��ư �̺�Ʈ ���");
        }
        else
        {
            Debug.LogError("[HomeSettingsPanel] closeButton�� null�Դϴ�!");
        }
    }

    public override void Show()
    {
        Debug.Log("[HomeSettingsPanel] Show ȣ��");
        gameObject.SetActive(true);
        base.Show();

        RefreshAllSettings();

        // �ؽ�Ʈ �̸����� ����
        if (textSpeedSettings != null)
        {
            textSpeedSettings.ShowPreview();
        }
    }

    public override void Hide()
    {
        Debug.Log("[HomeSettingsPanel] Hide ȣ��");

        // �ؽ�Ʈ �̸����� ����
        if (textSpeedSettings != null)
        {
            textSpeedSettings.HidePreview();
        }

        SaveSettingsAndHide().Forget();
    }

    // ���ϳ뺧 ȣȯ - ���� ���� �� �����
    public virtual async UniTask SaveSettingsAndHide()
    {
        Debug.Log("[HomeSettingsPanel] ���� ���� ��...");

        try
        {
            using (new InteractionBlocker())
            {
                if (stateManager != null)
                {
                    await stateManager.SaveSettings();
                    Debug.Log("[HomeSettingsPanel] ���ϳ뺧 ���� ���� �Ϸ�");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSettingsPanel] ���� ���� ����: {ex.Message}");
        }

        gameObject.SetActive(false);
        base.Hide();
    }

    private void RefreshAllSettings()
    {
        Debug.Log("[HomeSettingsPanel] ��� ���� ���ΰ�ħ");

        // �� ������Ʈ�� ���� ���ΰ�ħ
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

        Debug.Log("[HomeSettingsPanel] ���� ���ΰ�ħ �Ϸ�");
    }

    // �ܺο��� ȣ���
    public void RefreshSettings()
    {
        RefreshAllSettings();
    }

    // ���� ������Ʈ ���� Ȯ�ο�
    private void Start()
    {
        Debug.Log("[HomeSettingsPanel] Start - ������Ʈ ���� ���� Ȯ��");
        Debug.Log($"textSpeedSettings: {(textSpeedSettings != null ? "�����" : "null")}");
        Debug.Log($"screenModeSettings: {(screenModeSettings != null ? "�����" : "null")}");
        Debug.Log($"bgmVolumeSettings: {(bgmVolumeSettings != null ? "�����" : "null")}");
        Debug.Log($"sfxVolumeSettings: {(sfxVolumeSettings != null ? "�����" : "null")}");
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