using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;
using Naninovel.UI;
using System;

/// <summary>
/// SaveLoadMenu�� ��ӹ޾Ƽ� Ŀ���� ����� �߰��� ����/�ҷ����� UI
/// </summary>
public class SaveLoadPanel : SaveLoadMenu
{
    [Header("���� ��۵� (���ϳ뺧 �䱸����)")]
    [SerializeField] private Toggle dummySaveToggle;
    [SerializeField] private Toggle dummyLoadToggle;
    [SerializeField] private Toggle dummyQuickLoadToggle;

    [Header("���� ����� �׸����")]
    [SerializeField] private CustomGameStateSlotsGrid saveGrid;
    [SerializeField] private CustomGameStateSlotsGrid loadGrid;
    [SerializeField] private CustomGameStateSlotsGrid quickLoadGrid;

    [Header("Ŀ���� UI ��ҵ�")]
    [SerializeField] private TextMeshProUGUI customTitleText;
    [SerializeField] private Button customCloseButton;

    [Header("Ŀ���� �� ��ư��")]
    [SerializeField] private Button customSaveButton;
    [SerializeField] private Button customLoadButton;
    [SerializeField] private Button customQuickLoadButton;

    [Header("Ŀ���� ��ư �ؽ�Ʈ��")]
    [SerializeField] private TextMeshProUGUI customSaveText;
    [SerializeField] private TextMeshProUGUI customLoadText;
    [SerializeField] private TextMeshProUGUI customQuickLoadText;

    [Header("Ŀ���� ��ư ��Ÿ��")]
    [SerializeField] private Color activeButtonColor = Color.white;
    [SerializeField] private Color inactiveButtonColor = Color.gray;
    [SerializeField] private Color activeTextColor = Color.black;
    [SerializeField] private Color inactiveTextColor = Color.white;

    // ���ϳ뺧 �䱸���� ����
    protected override Toggle SaveToggle => dummySaveToggle;
    protected override Toggle LoadToggle => dummyLoadToggle;
    protected override Toggle QuickLoadToggle => dummyQuickLoadToggle;
    protected override GameStateSlotsGrid SaveGrid => saveGrid;
    protected override GameStateSlotsGrid LoadGrid => loadGrid;
    protected override GameStateSlotsGrid QuickLoadGrid => quickLoadGrid;

    #region ���ϳ뺧 �ý��� + Ŀ���� ��� ����

    public override async UniTask Initialize()
    {
        Debug.Log("[SaveLoadPanel] Ŀ���� Initialize ����");

        try
        {
            // �ʿ��� ���񽺵� ���� ��������
            var stateManager = Engine.GetService<IStateManager>();
            var confirmationUI = Engine.GetService<IUIManager>()?.GetUI<IConfirmationUI>();

            if (stateManager == null)
            {
                Debug.LogError("[SaveLoadPanel] StateManager�� ã�� �� �����ϴ�!");
                return;
            }

            Debug.Log("[SaveLoadPanel] ���� ȹ�� �Ϸ�");

            // ���� ������Ʈ�� ����� (���ϳ뺧 �䱸���� ��ȸ)
            HideDummyObjects();

            // Ŀ���� �׸���� �ʱ�ȭ
            var initTasks = new UniTask[]
            {
                InitializeCustomGrid(saveGrid, stateManager.Configuration.SaveSlotLimit, "Save"),
                InitializeCustomGrid(loadGrid, stateManager.Configuration.SaveSlotLimit, "Load"),
                InitializeCustomGrid(quickLoadGrid, stateManager.Configuration.QuickSaveSlotLimit, "QuickLoad")
            };

            await UniTask.WhenAll(initTasks);

            Debug.Log("[SaveLoadPanel] �׸��� �ʱ�ȭ �Ϸ�");

            // Ŀ���� UI ����
            SetupCustomUI();

            // �θ� Ŭ���� �ʱ�ȭ�� ���߿� (�׸������ �̹� ������ ��)
            await InitializeParentServices();

            Debug.Log("[SaveLoadPanel] Ŀ���� Initialize �Ϸ�");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] Ŀ���� Initialize ����: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private async UniTask InitializeCustomGrid(CustomGameStateSlotsGrid grid, int slotLimit, string gridType)
    {
        if (grid == null)
        {
            Debug.LogWarning($"[SaveLoadPanel] {gridType} �׸��尡 null�Դϴ�!");
            return;
        }

        try
        {
            await grid.Initialize(slotLimit,
                GetSlotClickHandler(gridType),
                GetDeleteClickHandler(gridType),
                GetLoadStateHandler(gridType));

            Debug.Log($"[SaveLoadPanel] {gridType} �׸��� �ʱ�ȭ �Ϸ�");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] {gridType} �׸��� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private Action<int> GetSlotClickHandler(string gridType)
    {
        return gridType switch
        {
            "Save" => HandleSaveSlotClicked,
            "Load" => HandleLoadSlotClicked,
            "QuickLoad" => HandleQuickLoadSlotClicked,
            _ => HandleLoadSlotClicked
        };
    }

    private Action<int> GetDeleteClickHandler(string gridType)
    {
        return gridType switch
        {
            "QuickLoad" => HandleDeleteQuickLoadSlotClicked,
            _ => HandleDeleteSlotClicked
        };
    }

    private Func<int, UniTask<GameStateMap>> GetLoadStateHandler(string gridType)
    {
        return gridType switch
        {
            "QuickLoad" => LoadQuickSaveSlot,
            _ => LoadSaveSlot
        };
    }

    private async UniTask InitializeParentServices()
    {
        try
        {
            // �θ� Ŭ������ �ʼ� ���񽺵鸸 �ʱ�ȭ
            var stateManager = Engine.GetServiceOrErr<IStateManager>();
            var scripts = Engine.GetServiceOrErr<IScriptManager>();
            var scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            var confirmationUI = Engine.GetServiceOrErr<IUIManager>().GetUI<IConfirmationUI>();

            // ���÷����� ���� �θ� Ŭ������ �ʵ�� ����
            var type = typeof(SaveLoadMenu);

            var stateManagerField = type.GetField("stateManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stateManagerField != null) stateManagerField.SetValue(this, stateManager);

            var scriptsField = type.GetField("scripts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (scriptsField != null) scriptsField.SetValue(this, scripts);

            var scriptPlayerField = type.GetField("scriptPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (scriptPlayerField != null) scriptPlayerField.SetValue(this, scriptPlayer);

            var confirmationUIField = type.GetField("confirmationUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (confirmationUIField != null) confirmationUIField.SetValue(this, confirmationUI);

            // �̺�Ʈ ������ ���
            stateManager.OnGameSaveStarted += HandleGameSaveStarted;
            stateManager.OnGameSaveFinished += HandleGameSaveFinished;

            Debug.Log("[SaveLoadPanel] �θ� ���� �ʱ�ȭ �Ϸ�");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] �θ� ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    #endregion

    #region ���� ������Ʈ �����

    private void HideDummyObjects()
    {
        Debug.Log("[SaveLoadPanel] ���� ������Ʈ�� �����");

        if (dummySaveToggle != null)
        {
            dummySaveToggle.gameObject.SetActive(false);
            dummySaveToggle.isOn = false;
        }
        if (dummyLoadToggle != null)
        {
            dummyLoadToggle.gameObject.SetActive(false);
            dummyLoadToggle.isOn = true; // �⺻��
        }
        if (dummyQuickLoadToggle != null)
        {
            dummyQuickLoadToggle.gameObject.SetActive(false);
            dummyQuickLoadToggle.isOn = false;
        }
    }

    #endregion

    #region Ŀ���� UI ����

    private void SetupCustomUI()
    {
        Debug.Log("[SaveLoadPanel] Ŀ���� UI ����");

        SetupCustomCloseButton();
        SetupCustomTabButtons();

        // �⺻ ��� ����
        SetPresentationMode(SaveLoadUIPresentationMode.Load);
    }

    private void SetupCustomCloseButton()
    {
        if (customCloseButton != null)
        {
            customCloseButton.onClick.RemoveAllListeners();
            customCloseButton.onClick.AddListener(Hide);
        }
    }

    private void SetupCustomTabButtons()
    {
        if (customSaveButton != null)
        {
            customSaveButton.onClick.RemoveAllListeners();
            customSaveButton.onClick.AddListener(() => SetPresentationMode(SaveLoadUIPresentationMode.Save));
        }

        if (customLoadButton != null)
        {
            customLoadButton.onClick.RemoveAllListeners();
            customLoadButton.onClick.AddListener(() => SetPresentationMode(SaveLoadUIPresentationMode.Load));
        }

        if (customQuickLoadButton != null)
        {
            customQuickLoadButton.onClick.RemoveAllListeners();
            customQuickLoadButton.onClick.AddListener(() => SetPresentationMode(SaveLoadUIPresentationMode.QuickLoad));
        }
    }

    #endregion

    #region Show/Hide �������̵�

    public override void Show()
    {
        Debug.Log("[SaveLoadPanel] Show ȣ��");

        base.Show();
        UpdateCustomUI();

        // �׸��� ���ΰ�ħ
        RefreshAllGrids();
    }

    public override void Hide()
    {
        Debug.Log("[SaveLoadPanel] Hide ȣ��");
        base.Hide();
    }

    private void RefreshAllGrids()
    {
        try
        {
            saveGrid?.RefreshAllSlots();
            loadGrid?.RefreshAllSlots();
            quickLoadGrid?.RefreshAllSlots();
            Debug.Log("[SaveLoadPanel] ��� �׸��� ���ΰ�ħ �Ϸ�");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] �׸��� ���ΰ�ħ ����: {ex.Message}");
        }
    }

    #endregion

    #region ���������̼� ��� �������̵�

    protected override void SetPresentationMode(SaveLoadUIPresentationMode value)
    {
        // ���� ��۵� ���� ������Ʈ (���ϳ뺧 ȣȯ��)
        UpdateDummyToggles(value);

        // �θ� Ŭ���� ȣ��
        base.SetPresentationMode(value);

        Debug.Log($"[SaveLoadPanel] ��� ��ȯ: {value}");

        UpdateCustomUI();
    }

    private void UpdateDummyToggles(SaveLoadUIPresentationMode mode)
    {
        if (dummySaveToggle != null) dummySaveToggle.isOn = (mode == SaveLoadUIPresentationMode.Save);
        if (dummyLoadToggle != null) dummyLoadToggle.isOn = (mode == SaveLoadUIPresentationMode.Load);
        if (dummyQuickLoadToggle != null) dummyQuickLoadToggle.isOn = (mode == SaveLoadUIPresentationMode.QuickLoad);
    }

    private void UpdateCustomUI()
    {
        var mode = PresentationMode;

        // �׸��� ǥ��/�����
        if (saveGrid != null) saveGrid.gameObject.SetActive(mode == SaveLoadUIPresentationMode.Save);
        if (loadGrid != null) loadGrid.gameObject.SetActive(mode == SaveLoadUIPresentationMode.Load);
        if (quickLoadGrid != null) quickLoadGrid.gameObject.SetActive(mode == SaveLoadUIPresentationMode.QuickLoad);

        // ��ư ���� ������Ʈ
        UpdateCustomButtons(mode);

        // ���� ������Ʈ
        UpdateCustomTitle(mode);
    }

    private void UpdateCustomButtons(SaveLoadUIPresentationMode mode)
    {
        // ��� ��ư ��Ȱ��ȭ
        SetCustomButtonState(customSaveButton, customSaveText, false);
        SetCustomButtonState(customLoadButton, customLoadText, false);
        SetCustomButtonState(customQuickLoadButton, customQuickLoadText, false);

        // ���� ��� ��ư�� Ȱ��ȭ
        switch (mode)
        {
            case SaveLoadUIPresentationMode.Save:
                SetCustomButtonState(customSaveButton, customSaveText, true);
                break;
            case SaveLoadUIPresentationMode.Load:
                SetCustomButtonState(customLoadButton, customLoadText, true);
                break;
            case SaveLoadUIPresentationMode.QuickLoad:
                SetCustomButtonState(customQuickLoadButton, customQuickLoadText, true);
                break;
        }
    }

    private void SetCustomButtonState(Button button, TextMeshProUGUI text, bool isActive)
    {
        if (button == null) return;

        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? activeButtonColor : inactiveButtonColor;
        }

        if (text != null)
        {
            text.color = isActive ? activeTextColor : inactiveTextColor;
        }

        button.interactable = !isActive;
    }

    private void UpdateCustomTitle(SaveLoadUIPresentationMode mode)
    {
        if (customTitleText == null) return;

        customTitleText.text = mode switch
        {
            SaveLoadUIPresentationMode.Save => "���� ����",
            SaveLoadUIPresentationMode.Load => "���� �ҷ�����",
            SaveLoadUIPresentationMode.QuickLoad => "�� �ε�",
            _ => "����/�ҷ�����"
        };
    }

    #endregion

    #region ����

    protected override void OnDestroy()
    {
        if (customCloseButton != null)
            customCloseButton.onClick.RemoveAllListeners();
        if (customSaveButton != null)
            customSaveButton.onClick.RemoveAllListeners();
        if (customLoadButton != null)
            customLoadButton.onClick.RemoveAllListeners();
        if (customQuickLoadButton != null)
            customQuickLoadButton.onClick.RemoveAllListeners();

        base.OnDestroy();
    }

    #endregion
}