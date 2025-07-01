using Naninovel.UI;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ItemDisplayUI : CustomUI
{
    [Tooltip("개별 아이템 아이콘을 생성할 때 사용할 프리팹 (ItemIconUI.cs 부착)")]
    [SerializeField] private GameObject itemIconPrefab;
    [Tooltip("아이콘들이 정렬될 부모 RectTransform (HorizontalLayoutGroup 부착 필수)")]
    [SerializeField] private RectTransform iconContainer;
    [Tooltip("아이콘 사라짐/등장 애니메이션 시간")]
    [SerializeField] private float animationDuration = 0.3f;

    private List<ItemIconUI> activeIcons = new();
    private HorizontalLayoutGroup _horizontalLayoutGroup;
    private CanvasGroup _canvasGroup;

    // 프린터 연동 관련 변수
    private bool _isHiddenByPrinter = false;
    private bool _wasVisibleBeforePrinterHide = false;
    private ITextPrinterManager _textPrinterManager;
    private Coroutine _printerMonitorCoroutine;

    // 게임 상태 관리용
    private IStateManager _stateManager;

    public void ClearAllIcons()
    {
        Debug.Log($"[ItemDisplayUI] 모든 아이콘 제거 시작 - 현재 {activeIcons.Count}개");

        // 역순으로 제거 (리스트 수정 중 인덱스 문제 방지)
        for (int i = activeIcons.Count - 1; i >= 0; i--)
        {
            var icon = activeIcons[i];
            if (icon != null)
            {
                activeIcons.RemoveAt(i);
                Destroy(icon.gameObject);
            }
        }

        activeIcons.Clear();
        Debug.Log("[ItemDisplayUI] 모든 아이콘 제거 완료");
    }

    protected override void Awake()
    {
        base.Awake();

        // CanvasGroup 확인/추가
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // IconContainer 검증
        if (iconContainer != null)
        {
            _horizontalLayoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
            if (_horizontalLayoutGroup == null)
            {
                Debug.LogError("ItemDisplayUI: iconContainer에 HorizontalLayoutGroup 컴포넌트가 필요합니다.");
            }
        }
        else
        {
            Debug.LogError("ItemDisplayUI: iconContainer가 할당되지 않았습니다.");
        }

        if (itemIconPrefab == null)
        {
            Debug.LogError("ItemDisplayUI: itemIconPrefab이 할당되지 않았습니다.");
        }

        // 초기 상태 설정
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    protected override void Start()
    {
        base.Start();

        // 나니노벨 엔진 초기화 대기
        StartCoroutine(WaitForInitialization());
    }

    private IEnumerator WaitForInitialization()
    {
        // 엔진 초기화 대기
        while (!Engine.Initialized)
        {
            yield return null;
        }

        // 추가 대기 (다른 서비스들이 완전히 초기화되도록)
        yield return new WaitForSeconds(0.5f);

        InitializeServices();

        // 초기 복원 시도
        RestoreItemIconsFromState();
    }

    private void InitializeServices()
    {
        Debug.Log("[ItemDisplayUI] 서비스 초기화 시작");

        try
        {
            // 상태 관리자 연결
            _stateManager = Engine.GetService<IStateManager>();
            if (_stateManager != null)
            {
                // 게임 로드 완료 이벤트 구독
                _stateManager.OnGameLoadFinished += OnGameLoadFinished;
                Debug.Log("[ItemDisplayUI] StateManager 연결 및 이벤트 구독 완료");
            }

            // 프린터 관리자 연결
            _textPrinterManager = Engine.GetService<ITextPrinterManager>();
            if (_textPrinterManager != null)
            {
                Debug.Log("[ItemDisplayUI] TextPrinterManager 연결 성공");
                // 프린터 상태 모니터링 시작
                _printerMonitorCoroutine = StartCoroutine(MonitorPrinterVisibility());
            }
            else
            {
                Debug.LogWarning("[ItemDisplayUI] TextPrinterManager를 찾을 수 없습니다.");
            }

            // 초기 아이템 UI 복원 (게임 시작 시)
            RestoreItemIconsFromState();

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemDisplayUI] 서비스 초기화 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 게임 로드 완료 시 호출되는 이벤트 핸들러
    /// </summary>
    private void OnGameLoadFinished(GameSaveLoadArgs args)
    {
        Debug.Log($"[ItemDisplayUI] 게임 로드 완료 - 아이템 UI 복원 시작 (슬롯: {args.SlotId})");

        // 약간의 지연 후 복원 (다른 시스템들이 완전히 로드될 때까지 대기)
        StartCoroutine(DelayedRestoreItems());
    }

    private IEnumerator DelayedRestoreItems()
    {
        // 0.5초 대기 후 복원
        yield return new WaitForSeconds(0.5f);
        RestoreItemIconsFromState();
    }

    /// <summary>
    /// StateBasedItemSystem에서 현재 보유 아이템을 읽어와서 UI 복원
    /// </summary>
    public void RestoreItemIconsFromState()
    {
        Debug.Log("[ItemDisplayUI] 아이템 복원 시작");

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogWarning("[ItemDisplayUI] StateBasedItemSystem.Instance가 null입니다. 나중에 재시도");
            // 1초 후 재시도
            StartCoroutine(RetryRestoreItems());
            return;
        }

        try
        {
            // 기존 아이콘 전체 제거
            ClearAllIcons();

            // 현재 소유 아이템 가져오기
            var ownedItems = StateBasedItemSystem.Instance.GetOwnedItems();

            Debug.Log($"[ItemDisplayUI] 복원할 아이템 수: {ownedItems.Count}");

            foreach (string itemId in ownedItems)
            {
                var itemData = StateBasedItemSystem.Instance.GetItemData(itemId);
                if (itemData != null)
                {
                    Debug.Log($"[ItemDisplayUI] 아이템 복원: {itemId}");
                    AddItemIconInternal(itemData, false); // 애니메이션 없이 복원
                }
                else
                {
                    Debug.LogWarning($"[ItemDisplayUI] 아이템 데이터를 찾을 수 없음: {itemId}");
                }
            }

            // 아이템이 있으면 UI 표시
            if (activeIcons.Count > 0)
            {
                ShowUIWithIcons();
                Debug.Log($"[ItemDisplayUI] 아이템 UI 복원 완료 - {activeIcons.Count}개 아이템");
            }
            else
            {
                Debug.Log("[ItemDisplayUI] 복원할 아이템이 없습니다");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemDisplayUI] 아이템 복원 오류: {ex.Message}");
        }
    }

    private IEnumerator RetryRestoreItems()
    {
        yield return new WaitForSeconds(1f);
        RestoreItemIconsFromState();
    }

    private IEnumerator MonitorPrinterVisibility()
    {
        bool lastPrinterVisibility = true;

        while (true)
        {
            float waitTime = 0.02f;

            try
            {
                if (_textPrinterManager != null)
                {
                    bool currentPrinterVisibility = false;

                    var uiManager = Engine.GetService<IUIManager>();
                    if (uiManager != null)
                    {
                        // "TextPrinter" 대신 "Dialogue" 사용
                        var printerUI = uiManager.GetUI("Dialogue");
                        if (printerUI != null)
                        {
                            currentPrinterVisibility = printerUI.Visible;
                        }
                    }

                    // 상태 변화 감지
                    if (lastPrinterVisibility != currentPrinterVisibility)
                    {
                        Debug.Log($"[ItemDisplayUI] Dialogue UI 가시성 변화 감지: {lastPrinterVisibility} → {currentPrinterVisibility}");
                        OnPrinterVisibilityChanged(currentPrinterVisibility);
                        lastPrinterVisibility = currentPrinterVisibility;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ItemDisplayUI] 프린터 모니터링 오류: {ex.Message}");
                waitTime = 1f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnPrinterVisibilityChanged(bool isVisible)
    {
        if (!isVisible) // 프린터 숨김
        {
            if (activeIcons.Count > 0 && gameObject.activeInHierarchy && !_isHiddenByPrinter)
            {
                Debug.Log("[ItemDisplayUI] 프린터 숨김에 따라 아이템 UI 숨김");
                _wasVisibleBeforePrinterHide = true;
                _isHiddenByPrinter = true;
                HideUI();
            }
        }
        else // 프린터 표시
        {
            if (_isHiddenByPrinter && _wasVisibleBeforePrinterHide)
            {
                Debug.Log("[ItemDisplayUI] 프린터 표시에 따라 아이템 UI 복원");
                _isHiddenByPrinter = false;
                _wasVisibleBeforePrinterHide = false;

                // 아이템이 있으면 다시 표시
                if (activeIcons.Count > 0)
                {
                    ShowUIWithIcons();
                }
            }
        }
    }

    public void AddItemIcon(ItemData itemData)
    {
        AddItemIconInternal(itemData, true);
    }

    private void AddItemIconInternal(ItemData itemData, bool withAnimation)
    {
        if (itemData == null || itemIconPrefab == null || iconContainer == null)
        {
            Debug.LogError($"[ItemDisplayUI] AddItemIcon 실패 - 필수 컴포넌트가 없습니다. ItemData: {itemData != null}, Prefab: {itemIconPrefab != null}, Container: {iconContainer != null}");
            return;
        }

        // 중복 체크
        if (activeIcons.Any(icon => icon.ItemId == itemData.itemId))
        {
            Debug.LogWarning($"ItemDisplayUI: 이미 {itemData.itemId} 아이콘이 존재합니다.");
            return;
        }

        Debug.Log($"[ItemDisplayUI] 아이템 아이콘 생성 시작: {itemData.itemId}");

        // 아이콘 생성
        GameObject iconGO = Instantiate(itemIconPrefab, iconContainer);
        iconGO.name = $"ItemIcon_{itemData.itemId}"; // 디버깅용 이름 설정

        ItemIconUI itemIconUI = iconGO.GetComponent<ItemIconUI>();
        if (itemIconUI == null)
        {
            Debug.LogError("ItemDisplayUI: itemIconPrefab에 ItemIconUI 컴포넌트가 없습니다.");
            Destroy(iconGO);
            return;
        }

        // 아이콘 설정
        itemIconUI.Setup(itemData, animationDuration);
        activeIcons.Add(itemIconUI);

        Debug.Log($"[ItemDisplayUI] 아이콘 생성 완료: {itemData.itemId}, 총 아이콘 수: {activeIcons.Count}");

        // 프린터에 의해 숨겨진 상태가 아닐 때만 UI 표시
        if (!_isHiddenByPrinter)
        {
            ShowUIWithIcons();
        }
        else
        {
            Debug.Log("[ItemDisplayUI] 프린터가 숨겨진 상태이므로 아이템 UI 표시 연기");
        }

        // 레이아웃 즉시 업데이트
        StartCoroutine(ForceLayoutUpdate());

        // 애니메이션 실행 (복원 시에는 애니메이션 없이)
        if (withAnimation)
        {
            itemIconUI.PlayAppearAnimation();
        }
        else
        {
            // 복원 시에는 즉시 표시
            var canvasGroup = itemIconUI.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }

    private void ShowUIWithIcons()
    {
        if (activeIcons.Count > 0 && !_isHiddenByPrinter)
        {
            // CustomUI Show 호출
            Show();

            // GameObject 활성화 확인
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            // CanvasGroup 알파 설정
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            // 레이아웃 강제 업데이트
            StartCoroutine(ForceLayoutUpdate());

            Debug.Log($"[ItemDisplayUI] UI 표시 완료 - 활성화: {gameObject.activeInHierarchy}, 알파: {(_canvasGroup?.alpha ?? 1f)}");
        }
    }

    private IEnumerator ForceLayoutUpdate()
    {
        // 다음 프레임까지 대기
        yield return null;

        // 레이아웃 강제 업데이트
        if (iconContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(iconContainer);
        }

        // HorizontalLayoutGroup 강제 업데이트
        if (_horizontalLayoutGroup != null)
        {
            _horizontalLayoutGroup.enabled = false;
            _horizontalLayoutGroup.enabled = true;
        }

        Debug.Log("[ItemDisplayUI] 레이아웃 강제 업데이트 완료");
    }

    public void RemoveItemIcon(string itemId)
    {
        ItemIconUI iconToRemove = activeIcons.FirstOrDefault(icon => icon.ItemId == itemId);
        if (iconToRemove == null)
        {
            Debug.LogWarning($"ItemDisplayUI: 제거할 아이콘을 찾지 못했습니다 - {itemId}");
            return;
        }

        Debug.Log($"[ItemDisplayUI] 아이콘 제거 시작: {itemId}");

        iconToRemove.PlayDisappearAnimation(() =>
        {
            activeIcons.Remove(iconToRemove);
            Debug.Log($"[ItemDisplayUI] 아이콘 제거 완료: {itemId}, 남은 아이콘 수: {activeIcons.Count}");

            // 모든 아이템이 사라졌으면 UI 숨기기
            if (activeIcons.Count == 0)
            {
                HideUI();
            }
        });
    }

    private void HideUI()
    {
        Debug.Log("[ItemDisplayUI] UI 숨기기 시작");

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        Hide(); // CustomUI Hide 호출
    }

    public string GetOldestItemId()
    {
        if (activeIcons.Count > 0)
        {
            return activeIcons[0].ItemId;
        }
        return null;
    }

    public bool RemoveOldestItemIcon()
    {
        string oldestItemId = GetOldestItemId();
        if (!string.IsNullOrEmpty(oldestItemId))
        {
            RemoveItemIcon(oldestItemId);
            return true;
        }
        return false;
    }

    // 외부에서 호출할 수 있는 복원 메서드
    [ContextMenu("Restore Items From State")]
    public void ManualRestoreItems()
    {
        RestoreItemIconsFromState();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 이벤트 구독 해제
        Engine.OnInitializationFinished -= InitializeServices;

        if (_stateManager != null)
        {
            _stateManager.OnGameLoadFinished -= OnGameLoadFinished;
        }

        // 코루틴 정리
        if (_printerMonitorCoroutine != null)
        {
            StopCoroutine(_printerMonitorCoroutine);
        }

        Debug.Log("[ItemDisplayUI] 정리 완료");
    }

    // 디버깅용 메서드
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"[ItemDisplayUI] 현재 상태:");
        Debug.Log($"  - GameObject 활성화: {gameObject.activeInHierarchy}");
        Debug.Log($"  - CanvasGroup 알파: {(_canvasGroup?.alpha ?? 1f)}");
        Debug.Log($"  - 활성 아이콘 수: {activeIcons.Count}");
        Debug.Log($"  - IconContainer 자식 수: {(iconContainer?.childCount ?? 0)}");
        Debug.Log($"  - 프린터에 의해 숨겨짐: {_isHiddenByPrinter}");
        Debug.Log($"  - 프린터 숨김 전 표시 상태: {_wasVisibleBeforePrinterHide}");

        if (StateBasedItemSystem.Instance != null)
        {
            var ownedItems = StateBasedItemSystem.Instance.GetOwnedItems();
            Debug.Log($"  - StateBasedItemSystem 보유 아이템 수: {ownedItems.Count}");
            foreach (var item in ownedItems)
            {
                Debug.Log($"    - {item}");
            }
        }

        foreach (var icon in activeIcons)
        {
            if (icon != null)
            {
                Debug.Log($"    - 아이콘: {icon.ItemId}, 활성화: {icon.gameObject.activeInHierarchy}");
            }
        }
    }
}