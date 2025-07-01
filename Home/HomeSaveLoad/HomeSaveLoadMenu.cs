using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;
using Naninovel.UI;

[RequireComponent(typeof(CanvasGroup))]
public class HomeSaveLoadMenu : CustomUI
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    [Header("모드 전환 토글들")]
    [SerializeField] private Toggle saveToggle;
    [SerializeField] private Toggle loadToggle;
    [SerializeField] private Toggle quickLoadToggle;

    [Header("그리드들")]
    [SerializeField] private HomeGameStateSlotsGrid saveGrid;
    [SerializeField] private HomeGameStateSlotsGrid loadGrid;
    [SerializeField] private HomeGameStateSlotsGrid quickLoadGrid;

    // ISaveLoadUI 구현
    public virtual SaveLoadUIPresentationMode PresentationMode
    {
        get => presentationMode;
        set => SetPresentationMode(value);
    }

    // 나니노벨 메시지들
    [ManagedText("DefaultUI")]
    protected static string OverwriteSaveSlotMessage = "기존 저장 데이터를 덮어쓰시겠습니까?";
    [ManagedText("DefaultUI")]
    protected static string DeleteSaveSlotMessage = "저장 데이터를 삭제하시겠습니까?";

    private SaveLoadUIPresentationMode presentationMode;
    private IStateManager stateManager;
    private IScriptPlayer scriptPlayer;
    private IScriptManager scripts;
    private IConfirmationUI confirmationUI;
    private ISaveSlotManager<GameStateMap> slotManager => stateManager?.GameSlotManager;

    public override async UniTask Initialize()
    {
        Debug.Log("[HomeSaveLoadMenu] Initialize 시작");

        stateManager = Engine.GetServiceOrErr<IStateManager>();
        scripts = Engine.GetServiceOrErr<IScriptManager>();
        scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
        confirmationUI = Engine.GetServiceOrErr<IUIManager>().GetUI<IConfirmationUI>();

        if (confirmationUI is null)
            throw new Exception("Confirmation UI is missing.");

        // 나니노벨 방식: 각 그리드별로 다른 핸들러 할당
        await UniTask.WhenAll(
            saveGrid.Initialize(stateManager.Configuration.SaveSlotLimit,
                HandleSaveSlotClicked, HandleDeleteSaveSlotClicked, LoadSaveSlot),

            loadGrid.Initialize(stateManager.Configuration.SaveSlotLimit,
                HandleLoadSlotClicked, HandleDeleteLoadSlotClicked, LoadSaveSlot),

            quickLoadGrid.Initialize(stateManager.Configuration.QuickSaveSlotLimit,
                HandleQuickLoadSlotClicked, HandleDeleteQuickLoadSlotClicked, LoadQuickSaveSlot)
        );

        // 버튼 이벤트 연결
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        // 토글 이벤트 연결
        if (saveToggle != null)
            saveToggle.onValueChanged.AddListener((isOn) => { if (isOn) PresentationMode = SaveLoadUIPresentationMode.Save; });
        if (loadToggle != null)
            loadToggle.onValueChanged.AddListener((isOn) => { if (isOn) PresentationMode = SaveLoadUIPresentationMode.Load; });
        if (quickLoadToggle != null)
            quickLoadToggle.onValueChanged.AddListener((isOn) => { if (isOn) PresentationMode = SaveLoadUIPresentationMode.QuickLoad; });

        Debug.Log("[HomeSaveLoadMenu] Initialize 완료");
    }

    public enum HomeSaveLoadMode
    {
        Save, Load, QuickLoad
    }
    public virtual SaveLoadUIPresentationMode GetLastLoadMode()
    {
        // 간단히 Load 모드 반환 (필요시 추후 로직 추가)
        return SaveLoadUIPresentationMode.Load;
    }

    protected override void Awake()
    {
        base.Awake();

        // 필수 컴포넌트 체크
        if (loadGrid == null)
            loadGrid = GetComponentInChildren<HomeGameStateSlotsGrid>();
    }

    private void SetPresentationMode(SaveLoadUIPresentationMode value)
    {
        Debug.Log($"[HomeSaveLoadMenu] SetPresentationMode: {value}");

        presentationMode = value;

        // 나니노벨 방식: 그리드 표시/숨김
        if (saveGrid != null) saveGrid.gameObject.SetActive(value == SaveLoadUIPresentationMode.Save);
        if (loadGrid != null) loadGrid.gameObject.SetActive(value == SaveLoadUIPresentationMode.Load);
        if (quickLoadGrid != null) quickLoadGrid.gameObject.SetActive(value == SaveLoadUIPresentationMode.QuickLoad);

        // 토글 상태 업데이트 (순환 참조 방지)
        UpdateToggleStates(value);
        UpdateTitleText();
    }

    private void UpdateToggleStates(SaveLoadUIPresentationMode mode)
    {
        // 나니노벨 방식: 토글 표시/숨김 + 상태 설정
        switch (mode)
        {
            case SaveLoadUIPresentationMode.Save:
                if (saveToggle != null)
                {
                    saveToggle.gameObject.SetActive(true);
                    saveToggle.SetIsOnWithoutNotify(true);
                }
                if (loadToggle != null) loadToggle.gameObject.SetActive(false);
                if (quickLoadToggle != null) quickLoadToggle.gameObject.SetActive(false);
                break;

            case SaveLoadUIPresentationMode.Load:
                if (saveToggle != null) saveToggle.gameObject.SetActive(false);
                if (loadToggle != null)
                {
                    loadToggle.gameObject.SetActive(true);
                    loadToggle.SetIsOnWithoutNotify(true);
                }
                if (quickLoadToggle != null) quickLoadToggle.gameObject.SetActive(true);
                break;

            case SaveLoadUIPresentationMode.QuickLoad:
                if (saveToggle != null) saveToggle.gameObject.SetActive(false);
                if (loadToggle != null) loadToggle.gameObject.SetActive(true);
                if (quickLoadToggle != null)
                {
                    quickLoadToggle.gameObject.SetActive(true);
                    quickLoadToggle.SetIsOnWithoutNotify(true);
                }
                break;
        }
    }

    private void UpdateTitleText()
    {
        if (titleText == null) return;

        string newText = presentationMode switch
        {
            SaveLoadUIPresentationMode.Save => "저장하기",
            SaveLoadUIPresentationMode.Load => "불러오기",
            SaveLoadUIPresentationMode.QuickLoad => "퀵 로드",
            _ => "저장/불러오기"
        };

        titleText.text = newText;
    }

    // === Save 전용 핸들러들 ===
    private void HandleSaveSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        HandleSaveSlotClicked(slotId, slotNumber);
    }

    private async void HandleSaveSlotClicked(string slotId, int slotNumber)
    {
        if (slotManager.SaveSlotExists(slotId) &&
            !await confirmationUI.Confirm(OverwriteSaveSlotMessage)) return;

        using (new InteractionBlocker())
        {
            var state = await stateManager.SaveGame(slotId);
            saveGrid?.BindSlot(slotNumber, state);
            loadGrid?.BindSlot(slotNumber, state); // Load 그리드도 동기화
        }

        Debug.Log($"슬롯 {slotNumber} 저장 완료");
    }

    private async void HandleDeleteSaveSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        if (!slotManager.SaveSlotExists(slotId)) return;

        if (!await confirmationUI.Confirm(DeleteSaveSlotMessage)) return;

        slotManager.DeleteSaveSlot(slotId);
        saveGrid?.BindSlot(slotNumber, null);
        loadGrid?.BindSlot(slotNumber, null);

        Debug.Log($"Save 슬롯 {slotNumber} 삭제 완료");
    }

    // === Load 전용 핸들러들 ===
    private void HandleLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        HandleLoadSlotClicked(slotId);
    }

    private async void HandleLoadSlotClicked(string slotId)
    {
        if (!slotManager.SaveSlotExists(slotId)) return;

        await PlayTitleLoad();

        using (await LoadingScreen.Show())
        {
            Hide();
            Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
            await stateManager.LoadGame(slotId);
        }

        Debug.Log($"슬롯 로드 완료: {slotId}");
    }

    private async void HandleDeleteLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        if (!slotManager.SaveSlotExists(slotId)) return;

        if (!await confirmationUI.Confirm(DeleteSaveSlotMessage)) return;

        slotManager.DeleteSaveSlot(slotId);
        saveGrid?.BindSlot(slotNumber, null);
        loadGrid?.BindSlot(slotNumber, null);

        Debug.Log($"Load 슬롯 {slotNumber} 삭제 완료");
    }

    // === QuickLoad 전용 핸들러들 ===
    private void HandleQuickLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
        HandleLoadSlotClicked(slotId); // 로드 로직은 동일
    }

    private async void HandleDeleteQuickLoadSlotClicked(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
        if (!slotManager.SaveSlotExists(slotId)) return;

        if (!await confirmationUI.Confirm(DeleteSaveSlotMessage)) return;

        slotManager.DeleteSaveSlot(slotId);
        quickLoadGrid?.BindSlot(slotNumber, null);

        Debug.Log($"퀵로드 슬롯 {slotNumber} 삭제 완료");
    }

    // === 유틸리티 메서드들 ===
    private async UniTask<GameStateMap> LoadSaveSlot(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
        var state = slotManager.SaveSlotExists(slotId) ? await slotManager.Load(slotId) : null;
        return state;
    }

    private async UniTask<GameStateMap> LoadQuickSaveSlot(int slotNumber)
    {
        var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
        var state = slotManager.SaveSlotExists(slotId) ? await slotManager.Load(slotId) : null;
        return state;
    }

    private async UniTask PlayTitleLoad()
    {
        const string label = "OnLoad";

        var scriptPath = scripts.Configuration.TitleScript;
        if (string.IsNullOrEmpty(scriptPath)) return;

        var script = (Script)await scripts.ScriptLoader.LoadOrErr(scripts.Configuration.TitleScript);
        if (!script.LabelExists(label)) return;

        scriptPlayer.ResetService();
        await scriptPlayer.LoadAndPlayAtLabel(scriptPath, label);
        await UniTask.WaitWhile(() => scriptPlayer.Playing);
    }

    // === Show/Hide 오버라이드 ===
    public override void Show()
    {
        Debug.Log("=== HomeSaveLoadMenu Show() 호출됨! ===");
        gameObject.SetActive(true);
        base.Show();

        // stateManager가 null이면 즉시 설정
        if (stateManager == null)
        {
            Debug.Log("stateManager 즉시 초기화 중...");
            stateManager = Engine.GetServiceOrErr<IStateManager>();
            scripts = Engine.GetServiceOrErr<IScriptManager>();
            scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            confirmationUI = Engine.GetServiceOrErr<IUIManager>().GetUI<IConfirmationUI>();

            // ★ Initialize 강제 호출!
            _ = ForceInitializeAsync();
        }

        // 그리드들이 초기화 안 되어있으면 강제 초기화
        Debug.Log($"LoadGrid Slots Count: {loadGrid?.Slots?.Count ?? -1}");

        // Initialize 후에 RefreshActiveGrid 호출하도록 수정
        if (loadGrid?.Slots?.Count > 0)
        {
            RefreshActiveGrid();
        }
    }

    private async UniTask ForceInitializeAsync()
    {
        Debug.Log("ForceInitializeAsync 시작");
        await Initialize();
        Debug.Log("ForceInitializeAsync 완료 - 이제 RefreshActiveGrid 호출");
        RefreshActiveGrid();
    }

    public override void Hide()
    {
        Debug.Log("=== HomeSaveLoadMenu Hide() 호출됨! ===");
        gameObject.SetActive(false);
        base.Hide();
    }

    private void RefreshActiveGrid()
    {
        Debug.Log($"RefreshActiveGrid 호출 - 현재 모드: {presentationMode}");

        // 현재 활성 그리드 새로고침
        switch (presentationMode)
        {
            case SaveLoadUIPresentationMode.Save:
                Debug.Log("SaveGrid RefreshAllSlots 호출");
                saveGrid?.RefreshAllSlots();
                break;
            case SaveLoadUIPresentationMode.Load:
                Debug.Log("LoadGrid RefreshAllSlots 호출");
                loadGrid?.RefreshAllSlots();
                break;
            case SaveLoadUIPresentationMode.QuickLoad:
                Debug.Log("QuickLoadGrid RefreshAllSlots 호출");
                quickLoadGrid?.RefreshAllSlots();
                break;
        }
    }
}