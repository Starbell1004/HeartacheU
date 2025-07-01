using Naninovel;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

[InitializeAtRuntime]
public class StateBasedItemSystem : IEngineService
{
    public static StateBasedItemSystem Instance { get; private set; }

    public enum ItemState
    {
        NotOwned = 0,
        Owned = 1,
        Used = 2
    }

    private ItemDisplayUI _itemDisplayUI;
    private Dictionary<string, ItemData> _itemDataMap = new();
    private Dictionary<string, ItemState> _itemStateCache = new();

    private IStateManager _stateManager;
    private ICustomVariableManager _variableManager;
    private IScriptPlayer _scriptPlayer;
    private bool _isInitialized = false;
    private bool _hasRestoredOnce = false;

    public StateBasedItemSystem()
    {
        Instance = this;
        Debug.Log("[ItemSystem] 생성자 호출");
    }

    public UniTask InitializeService()
    {
        Debug.Log("[ItemSystem] InitializeService 시작");

        // ItemData 로드
        _itemDataMap = Resources.LoadAll<ItemData>("ItemData")
            .ToDictionary(data => data.itemId, data => data);

        Debug.Log($"[ItemSystem] ItemData 로드 완료: {_itemDataMap.Count}개");

        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }

        return UniTask.CompletedTask;
    }

    private void InitializeServices()
    {
        if (_isInitialized) return;

        Engine.OnInitializationFinished -= InitializeServices;

        Debug.Log("[ItemSystem] 서비스 초기화 시작");

        _variableManager = Engine.GetService<ICustomVariableManager>();
        _stateManager = Engine.GetService<IStateManager>();
        _scriptPlayer = Engine.GetService<IScriptPlayer>();

        if (_stateManager != null)
        {
            _stateManager.OnGameLoadFinished += OnGameLoadFinished;
            _stateManager.OnRollbackFinished += OnRollbackFinished;
            Debug.Log("[ItemSystem] 상태 관리자 이벤트 등록 완료");
        }

        // 스크립트 플레이어 이벤트 등록 - Script 매개변수 추가
        if (_scriptPlayer != null)
        {
            _scriptPlayer.OnPlay += OnScriptPlay;
        }

        _isInitialized = true;
        Debug.Log("[ItemSystem] 서비스 초기화 완료");

        // 초기 복원 시도 (게임 시작 시)
        CoroutineRunner.StartCoroutine(InitialRestore());
    }

    private IEnumerator InitialRestore()
    {
        yield return new WaitForSeconds(1f);

        if (!_hasRestoredOnce)
        {
            FindItemDisplayUI();
            RestoreItemsFromVariables();
            _hasRestoredOnce = true;
        }
    }

    private void OnScriptPlay(Script script)
    {
        // 스크립트가 처음 실행될 때만
        if (!_hasRestoredOnce && _scriptPlayer != null)
        {
            Debug.Log("[ItemSystem] 스크립트 실행 시작");

            // 코루틴으로 지연 실행
            CoroutineRunner.StartCoroutine(DelayedInitialization());

            // 한 번만 실행되도록
            _hasRestoredOnce = true;
        }
    }

    private IEnumerator DelayedInitialization()
    {
        // UI가 생성될 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);

        FindItemDisplayUI();
        RestoreItemsFromVariables();
    }

    private void FindItemDisplayUI()
    {
        var uiManager = Engine.GetService<IUIManager>();
        _itemDisplayUI = uiManager?.GetUI<ItemDisplayUI>();

        if (_itemDisplayUI == null)
        {
            // 다른 방법으로 찾기 시도
            _itemDisplayUI = GameObject.FindObjectOfType<ItemDisplayUI>();
        }

        if (_itemDisplayUI == null)
        {
            Debug.LogWarning("[ItemSystem] ItemDisplayUI를 찾을 수 없음");
        }
        else
        {
            Debug.Log("[ItemSystem] ItemDisplayUI 찾기 성공");
        }
    }

    private void EnsureItemDisplayUI()
    {
        if (_itemDisplayUI == null)
        {
            FindItemDisplayUI();
        }
    }

    private void RestoreItemsFromVariables()
    {
        if (_variableManager == null) return;

        Debug.Log("[ItemSystem] 변수에서 아이템 복원 시작");

        _itemStateCache.Clear();
        var ownedItems = new List<string>();

        foreach (var itemId in _itemDataMap.Keys)
        {
            try
            {
                // 변수명 확인
                string variableName = $"item_{itemId}";

                // 변수가 존재하는지 먼저 확인
                if (_variableManager.VariableExists(variableName))
                {
                    var value = _variableManager.GetVariableValue(variableName);

                    if (int.TryParse(value.ToString(), out int stateValue))
                    {
                        var state = (ItemState)stateValue;
                        _itemStateCache[itemId] = state;

                        if (state == ItemState.Owned)
                        {
                            ownedItems.Add(itemId);
                            Debug.Log($"[ItemSystem] 소유 아이템 발견: {itemId}");
                        }
                    }
                }
                else
                {
                    _itemStateCache[itemId] = ItemState.NotOwned;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ItemSystem] 변수 복원 실패 {itemId}: {ex.Message}");
                _itemStateCache[itemId] = ItemState.NotOwned;
            }
        }

        // UI 복원
        if (ownedItems.Count > 0)
        {
            EnsureItemDisplayUI();
            if (_itemDisplayUI != null)
            {
                _itemDisplayUI.RestoreItemIconsFromState();
                Debug.Log($"[ItemSystem] UI 복원 요청 - {ownedItems.Count}개 아이템");
            }
        }

        Debug.Log($"[ItemSystem] 변수 복원 완료 - 소유: {ownedItems.Count}개");
    }

    private void OnGameLoadFinished(GameSaveLoadArgs args)
    {
        Debug.Log($"[ItemSystem] 게임 로드 완료 - 슬롯: {args.SlotId}");

        // 코루틴으로 지연 실행
        CoroutineRunner.StartCoroutine(AutoRestoreItemsAfterLoad());
    }

    private IEnumerator AutoRestoreItemsAfterLoad()
    {
        // UI가 완전히 로드될 때까지 대기
        yield return new WaitForSeconds(1f);

        Debug.Log("[ItemSystem] 자동 아이템 복원 시작");

        // 변수를 읽어서 아이템 UI만 복원
        if (_variableManager != null)
        {
            var restoredCount = 0;

            foreach (var kvp in _itemDataMap)
            {
                string itemId = kvp.Key;
                string variableName = $"item_{itemId}";

                try
                {
                    // 변수가 1이면 (Owned 상태)
                    var value = _variableManager.GetVariableValue(variableName);
                    if (value != null && value.ToString() == "1")
                    {
                        // 캐시 업데이트
                        _itemStateCache[itemId] = ItemState.Owned;

                        // UI 복원
                        EnsureItemDisplayUI();
                        if (_itemDisplayUI != null && kvp.Value != null)
                        {
                            _itemDisplayUI.AddItemIcon(kvp.Value);
                            restoredCount++;
                            Debug.Log($"[ItemSystem] 아이템 UI 복원: {itemId}");
                        }
                    }
                }
                catch
                {
                    // 변수가 없으면 무시
                }
            }

            Debug.Log($"[ItemSystem] 자동 복원 완료 - {restoredCount}개 아이템");
        }
    }

    private IEnumerator DelayedRestoreAfterLoad()
    {
        // 다른 시스템들이 로드 완료되도록 대기
        yield return new WaitForSeconds(0.5f);

        FindItemDisplayUI();
        RestoreItemsFromVariables();
    }

    private void OnRollbackFinished()
    {
        Debug.Log("[ItemSystem] 롤백 완료 - 아이템 복원");
        RestoreItemsFromVariables();
    }

    public ItemState GetItemState(string itemId)
    {
        if (_itemStateCache.TryGetValue(itemId, out ItemState cachedState))
        {
            return cachedState;
        }

        if (_variableManager != null)
        {
            try
            {
                // 변수가 존재하는지 먼저 확인
                if (_variableManager.VariableExists($"item_{itemId}"))
                {
                    var value = _variableManager.GetVariableValue($"item_{itemId}");
                    if (int.TryParse(value.ToString(), out int stateValue))
                    {
                        var state = (ItemState)stateValue;
                        _itemStateCache[itemId] = state;
                        return state;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ItemSystem] 변수 읽기 실패 item_{itemId}: {ex.Message}");
            }
        }

        _itemStateCache[itemId] = ItemState.NotOwned;
        return ItemState.NotOwned;
    }

    public void SetItemState(string itemId, ItemState state)
    {
        if (!ValidateItem(itemId)) return;

        _itemStateCache[itemId] = state;

        // 변수명에 prefix 추가하여 충돌 방지
        _variableManager?.SetVariableValue($"item_{itemId}", new CustomVariableValue(((int)state).ToString()));

        Debug.Log($"[ItemSystem] 아이템 상태 변경: {itemId} -> {state}");
    }

    public bool AcquireItem(string itemId)
    {
        if (!ValidateItem(itemId)) return false;

        var currentState = GetItemState(itemId);
        if (currentState == ItemState.Owned)
        {
            Debug.LogWarning($"[ItemSystem] 이미 소유한 아이템: {itemId}");
            return false;
        }

        SetItemState(itemId, ItemState.Owned);

        // UI 업데이트
        EnsureItemDisplayUI();
        if (_itemDisplayUI != null && _itemDataMap.TryGetValue(itemId, out ItemData itemData))
        {
            _itemDisplayUI.AddItemIcon(itemData);
            Debug.Log($"[ItemSystem] 아이템 획득 완료: {itemId}");
        }
        else
        {
            Debug.LogWarning($"[ItemSystem] UI 업데이트 실패: {itemId}");
        }

        return true;
    }

    public bool UseItem(string itemId)
    {
        if (!ValidateItem(itemId)) return false;

        var currentState = GetItemState(itemId);
        if (currentState != ItemState.Owned) return false;

        SetItemState(itemId, ItemState.Used);

        EnsureItemDisplayUI();
        _itemDisplayUI?.RemoveItemIcon(itemId);

        Debug.Log($"[ItemSystem] 아이템 사용: {itemId}");
        return true;
    }

    public string UseOldestItem()
    {
        var ownedItems = GetOwnedItems();
        return ownedItems.Count > 0 ? UseItem(ownedItems[0]) ? ownedItems[0] : null : null;
    }

    private bool ValidateItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError("[ItemSystem] 아이템 ID가 비어있습니다");
            return false;
        }

        if (!_itemDataMap.ContainsKey(itemId))
        {
            Debug.LogError($"[ItemSystem] 존재하지 않는 아이템 ID: {itemId}");
            return false;
        }

        if (_variableManager == null)
        {
            Debug.LogError("[ItemSystem] VariableManager가 null입니다");
            return false;
        }

        return true;
    }

    public List<string> GetItemsByState(ItemState state) =>
        _itemDataMap.Keys.Where(id => GetItemState(id) == state).ToList();

    public List<string> GetOwnedItems() => GetItemsByState(ItemState.Owned);
    public List<string> GetUsedItems() => GetItemsByState(ItemState.Used);

    public bool HasItem(string itemId) => GetItemState(itemId) == ItemState.Owned;
    public bool HasUsedItem(string itemId) => GetItemState(itemId) == ItemState.Used;
    public bool HasEverOwnedItem(string itemId) => GetItemState(itemId) >= ItemState.Owned;

    public Dictionary<string, ItemData> GetAllItemData() => _itemDataMap;

    public ItemData GetItemData(string itemId) =>
        _itemDataMap.TryGetValue(itemId, out ItemData data) ? data : null;

    public void ResetService()
    {
        _itemStateCache.Clear();
        _isInitialized = false;
        _hasRestoredOnce = false;
    }

    public void DestroyService()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (_stateManager != null)
        {
            _stateManager.OnGameLoadFinished -= OnGameLoadFinished;
            _stateManager.OnRollbackFinished -= OnRollbackFinished;
        }

        if (_scriptPlayer != null)
        {
            _scriptPlayer.OnPlay -= OnScriptPlay;
        }

        Instance = null;
    }
}

// 코루틴 실행을 위한 헬퍼 클래스
public static class CoroutineRunner
{
    private static MonoBehaviour _runner;

    private static MonoBehaviour Runner
    {
        get
        {
            if (_runner == null)
            {
                var go = new GameObject("CoroutineRunner");
                GameObject.DontDestroyOnLoad(go);
                _runner = go.AddComponent<CoroutineRunnerBehaviour>();
            }
            return _runner;
        }
    }

    public static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return Runner.StartCoroutine(coroutine);
    }

    private class CoroutineRunnerBehaviour : MonoBehaviour { }
}