using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;
using Naninovel.UI;
using System;

/// <summary>
/// SaveLoadMenu를 상속받아서 커스텀 기능을 추가한 저장/불러오기 UI
/// </summary>
public class SaveLoadPanel : SaveLoadMenu
{
    [Header("더미 토글들 (나니노벨 요구사항)")]
    [SerializeField] private Toggle dummySaveToggle;
    [SerializeField] private Toggle dummyLoadToggle;
    [SerializeField] private Toggle dummyQuickLoadToggle;

    [Header("실제 사용할 그리드들")]
    [SerializeField] private CustomGameStateSlotsGrid saveGrid;
    [SerializeField] private CustomGameStateSlotsGrid loadGrid;
    [SerializeField] private CustomGameStateSlotsGrid quickLoadGrid;

    [Header("커스텀 UI 요소들")]
    [SerializeField] private TextMeshProUGUI customTitleText;
    [SerializeField] private Button customCloseButton;

    [Header("커스텀 탭 버튼들")]
    [SerializeField] private Button customSaveButton;
    [SerializeField] private Button customLoadButton;
    [SerializeField] private Button customQuickLoadButton;

    [Header("커스텀 버튼 텍스트들")]
    [SerializeField] private TextMeshProUGUI customSaveText;
    [SerializeField] private TextMeshProUGUI customLoadText;
    [SerializeField] private TextMeshProUGUI customQuickLoadText;

    [Header("커스텀 버튼 스타일")]
    [SerializeField] private Color activeButtonColor = Color.white;
    [SerializeField] private Color inactiveButtonColor = Color.gray;
    [SerializeField] private Color activeTextColor = Color.black;
    [SerializeField] private Color inactiveTextColor = Color.white;

    // 나니노벨 요구사항 충족
    protected override Toggle SaveToggle => dummySaveToggle;
    protected override Toggle LoadToggle => dummyLoadToggle;
    protected override Toggle QuickLoadToggle => dummyQuickLoadToggle;
    protected override GameStateSlotsGrid SaveGrid => saveGrid;
    protected override GameStateSlotsGrid LoadGrid => loadGrid;
    protected override GameStateSlotsGrid QuickLoadGrid => quickLoadGrid;

    #region 나니노벨 시스템 + 커스텀 기능 통합

    public override async UniTask Initialize()
    {
        Debug.Log("[SaveLoadPanel] 커스텀 Initialize 시작");

        try
        {
            // 필요한 서비스들 먼저 가져오기
            var stateManager = Engine.GetService<IStateManager>();
            var confirmationUI = Engine.GetService<IUIManager>()?.GetUI<IConfirmationUI>();

            if (stateManager == null)
            {
                Debug.LogError("[SaveLoadPanel] StateManager를 찾을 수 없습니다!");
                return;
            }

            Debug.Log("[SaveLoadPanel] 서비스 획득 완료");

            // 더미 오브젝트들 숨기기 (나니노벨 요구사항 우회)
            HideDummyObjects();

            // 커스텀 그리드들 초기화
            var initTasks = new UniTask[]
            {
                InitializeCustomGrid(saveGrid, stateManager.Configuration.SaveSlotLimit, "Save"),
                InitializeCustomGrid(loadGrid, stateManager.Configuration.SaveSlotLimit, "Load"),
                InitializeCustomGrid(quickLoadGrid, stateManager.Configuration.QuickSaveSlotLimit, "QuickLoad")
            };

            await UniTask.WhenAll(initTasks);

            Debug.Log("[SaveLoadPanel] 그리드 초기화 완료");

            // 커스텀 UI 설정
            SetupCustomUI();

            // 부모 클래스 초기화는 나중에 (그리드들이 이미 설정된 후)
            await InitializeParentServices();

            Debug.Log("[SaveLoadPanel] 커스텀 Initialize 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] 커스텀 Initialize 실패: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private async UniTask InitializeCustomGrid(CustomGameStateSlotsGrid grid, int slotLimit, string gridType)
    {
        if (grid == null)
        {
            Debug.LogWarning($"[SaveLoadPanel] {gridType} 그리드가 null입니다!");
            return;
        }

        try
        {
            await grid.Initialize(slotLimit,
                GetSlotClickHandler(gridType),
                GetDeleteClickHandler(gridType),
                GetLoadStateHandler(gridType));

            Debug.Log($"[SaveLoadPanel] {gridType} 그리드 초기화 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] {gridType} 그리드 초기화 실패: {ex.Message}");
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
            // 부모 클래스의 필수 서비스들만 초기화
            var stateManager = Engine.GetServiceOrErr<IStateManager>();
            var scripts = Engine.GetServiceOrErr<IScriptManager>();
            var scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            var confirmationUI = Engine.GetServiceOrErr<IUIManager>().GetUI<IConfirmationUI>();

            // 리플렉션을 통해 부모 클래스의 필드들 설정
            var type = typeof(SaveLoadMenu);

            var stateManagerField = type.GetField("stateManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stateManagerField != null) stateManagerField.SetValue(this, stateManager);

            var scriptsField = type.GetField("scripts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (scriptsField != null) scriptsField.SetValue(this, scripts);

            var scriptPlayerField = type.GetField("scriptPlayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (scriptPlayerField != null) scriptPlayerField.SetValue(this, scriptPlayer);

            var confirmationUIField = type.GetField("confirmationUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (confirmationUIField != null) confirmationUIField.SetValue(this, confirmationUI);

            // 이벤트 리스너 등록
            stateManager.OnGameSaveStarted += HandleGameSaveStarted;
            stateManager.OnGameSaveFinished += HandleGameSaveFinished;

            Debug.Log("[SaveLoadPanel] 부모 서비스 초기화 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] 부모 서비스 초기화 실패: {ex.Message}");
        }
    }

    #endregion

    #region 더미 오브젝트 숨기기

    private void HideDummyObjects()
    {
        Debug.Log("[SaveLoadPanel] 더미 오브젝트들 숨기기");

        if (dummySaveToggle != null)
        {
            dummySaveToggle.gameObject.SetActive(false);
            dummySaveToggle.isOn = false;
        }
        if (dummyLoadToggle != null)
        {
            dummyLoadToggle.gameObject.SetActive(false);
            dummyLoadToggle.isOn = true; // 기본값
        }
        if (dummyQuickLoadToggle != null)
        {
            dummyQuickLoadToggle.gameObject.SetActive(false);
            dummyQuickLoadToggle.isOn = false;
        }
    }

    #endregion

    #region 커스텀 UI 설정

    private void SetupCustomUI()
    {
        Debug.Log("[SaveLoadPanel] 커스텀 UI 설정");

        SetupCustomCloseButton();
        SetupCustomTabButtons();

        // 기본 모드 설정
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

    #region Show/Hide 오버라이드

    public override void Show()
    {
        Debug.Log("[SaveLoadPanel] Show 호출");

        base.Show();
        UpdateCustomUI();

        // 그리드 새로고침
        RefreshAllGrids();
    }

    public override void Hide()
    {
        Debug.Log("[SaveLoadPanel] Hide 호출");
        base.Hide();
    }

    private void RefreshAllGrids()
    {
        try
        {
            saveGrid?.RefreshAllSlots();
            loadGrid?.RefreshAllSlots();
            quickLoadGrid?.RefreshAllSlots();
            Debug.Log("[SaveLoadPanel] 모든 그리드 새로고침 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadPanel] 그리드 새로고침 실패: {ex.Message}");
        }
    }

    #endregion

    #region 프레젠테이션 모드 오버라이드

    protected override void SetPresentationMode(SaveLoadUIPresentationMode value)
    {
        // 더미 토글들 상태 업데이트 (나니노벨 호환성)
        UpdateDummyToggles(value);

        // 부모 클래스 호출
        base.SetPresentationMode(value);

        Debug.Log($"[SaveLoadPanel] 모드 전환: {value}");

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

        // 그리드 표시/숨기기
        if (saveGrid != null) saveGrid.gameObject.SetActive(mode == SaveLoadUIPresentationMode.Save);
        if (loadGrid != null) loadGrid.gameObject.SetActive(mode == SaveLoadUIPresentationMode.Load);
        if (quickLoadGrid != null) quickLoadGrid.gameObject.SetActive(mode == SaveLoadUIPresentationMode.QuickLoad);

        // 버튼 상태 업데이트
        UpdateCustomButtons(mode);

        // 제목 업데이트
        UpdateCustomTitle(mode);
    }

    private void UpdateCustomButtons(SaveLoadUIPresentationMode mode)
    {
        // 모든 버튼 비활성화
        SetCustomButtonState(customSaveButton, customSaveText, false);
        SetCustomButtonState(customLoadButton, customLoadText, false);
        SetCustomButtonState(customQuickLoadButton, customQuickLoadText, false);

        // 현재 모드 버튼만 활성화
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
            SaveLoadUIPresentationMode.Save => "게임 저장",
            SaveLoadUIPresentationMode.Load => "게임 불러오기",
            SaveLoadUIPresentationMode.QuickLoad => "퀵 로드",
            _ => "저장/불러오기"
        };
    }

    #endregion

    #region 정리

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