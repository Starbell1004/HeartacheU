using System;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using Naninovel.UI;
using UnityEngine.UI;

/// <summary>
/// 커스텀 게임 상태 슬롯 그리드 - Naninovel.UI.GameStateSlotsGrid를 상속받음
/// </summary>
public class CustomGameStateSlotsGrid : GameStateSlotsGrid
{
    [Header("커스텀 슬롯 프리팹")]
    [SerializeField] private CustomGameStateSlot slotPrefab;

    [Header("그리드 설정")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private int slotsPerRow = 3;

    private List<CustomGameStateSlot> customSlots = new List<CustomGameStateSlot>();
    private Action<int> onSlotClicked;
    private Action<int> onDeleteClicked;
    private Func<int, UniTask<GameStateMap>> loadStateHandler;

    public async UniTask Initialize(int slotLimit,
        Action<int> slotClickHandler,
        Action<int> deleteClickHandler,
        Func<int, UniTask<GameStateMap>> stateLoader)
    {
        Debug.Log($"[CustomGameStateSlotsGrid] Initialize 시작 - 슬롯 수: {slotLimit}");

        onSlotClicked = slotClickHandler;
        onDeleteClicked = deleteClickHandler;
        loadStateHandler = stateLoader;

        // 기존 슬롯들 정리
        ClearSlots();

        // 새 슬롯들 생성
        await CreateSlots(slotLimit);

        Debug.Log($"[CustomGameStateSlotsGrid] Initialize 완료");
    }

    private void ClearSlots()
    {
        foreach (var slot in customSlots)
        {
            if (slot != null)
                DestroyImmediate(slot.gameObject);
        }
        customSlots.Clear();
    }

    private async UniTask CreateSlots(int slotLimit)
    {
        if (slotPrefab == null)
        {
            Debug.LogError("[CustomGameStateSlotsGrid] 슬롯 프리팹이 설정되지 않았습니다!");
            return;
        }

        if (slotsContainer == null)
            slotsContainer = transform;

        Debug.Log($"[CustomGameStateSlotsGrid] 슬롯 생성 시작 - 컨테이너: {slotsContainer.name}");

        for (int i = 0; i < slotLimit; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsContainer);
            var customSlot = slotObj.GetComponent<CustomGameStateSlot>();

            if (customSlot != null)
            {
                // 슬롯 번호 설정
                var slotNumber = i + 1;

                // 슬롯 초기화
                await InitializeSlot(customSlot, slotNumber);

                customSlots.Add(customSlot);

                Debug.Log($"[CustomGameStateSlotsGrid] 슬롯 {slotNumber} 생성 완료");
            }
        }

        // Layout 강제 업데이트만 실행
        ForceLayoutUpdate();
    }

    private void ForceLayoutUpdate()
    {
        // Layout 강제 업데이트
        if (slotsContainer != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(slotsContainer as RectTransform);
        }

        // 부모의 Scroll Rect도 업데이트
        var scrollRect = GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null && scrollRect.content != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        }
    }

    private async UniTask InitializeSlot(CustomGameStateSlot slot, int slotNumber)
    {
        try
        {
            // 슬롯 번호 설정을 위한 리플렉션 사용
            var slotNumberField = typeof(GameStateSlot).GetField("slotNumber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (slotNumberField != null)
                slotNumberField.SetValue(slot, slotNumber);

            // 상태 로드 및 설정
            var state = await loadStateHandler?.Invoke(slotNumber);

            if (state != null)
            {
                // 리플렉션을 통해 SetNonEmptyState 호출
                var method = typeof(GameStateSlot).GetMethod("SetNonEmptyState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(slot, new object[] { slotNumber, state });
            }
            else
            {
                // 리플렉션을 통해 SetEmptyState 호출
                var method = typeof(GameStateSlot).GetMethod("SetEmptyState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(slot, new object[] { slotNumber });
            }

            // 이벤트 연결
            SetupSlotEvents(slot, slotNumber);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CustomGameStateSlotsGrid] 슬롯 {slotNumber} 초기화 실패: {ex.Message}");
        }
    }

    private void SetupSlotEvents(CustomGameStateSlot slot, int slotNumber)
    {
        // 컴포넌트에서 직접 버튼들 찾기 (상속 문제 회피)
        var buttons = slot.GetComponentsInChildren<Button>();

        Button mainButton = null;
        Button deleteButton = null;

        foreach (var btn in buttons)
        {
            // 메인 버튼은 보통 루트에 있음
            if (btn.transform == slot.transform)
            {
                mainButton = btn;
            }
            // 삭제 버튼은 이름으로 찾기
            else if (btn.name.ToLower().Contains("delete"))
            {
                deleteButton = btn;
            }
        }

        // 메인 버튼 이벤트
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => onSlotClicked?.Invoke(slotNumber));
        }

        // 삭제 버튼 이벤트
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDeleteClicked?.Invoke(slotNumber));
        }

        Debug.Log($"[CustomGameStateSlotsGrid] 슬롯 {slotNumber} 이벤트 설정 - Main: {mainButton != null}, Delete: {deleteButton != null}");
    }

    public void RefreshAllSlots()
    {
        Debug.Log("[CustomGameStateSlotsGrid] 모든 슬롯 새로고침");

        for (int i = 0; i < customSlots.Count; i++)
        {
            var slot = customSlots[i];
            var slotNumber = i + 1;

            RefreshSlot(slot, slotNumber);
        }
    }

    private async void RefreshSlot(CustomGameStateSlot slot, int slotNumber)
    {
        try
        {
            var state = await loadStateHandler?.Invoke(slotNumber);

            if (state != null)
            {
                var method = typeof(GameStateSlot).GetMethod("SetNonEmptyState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(slot, new object[] { slotNumber, state });
            }
            else
            {
                var method = typeof(GameStateSlot).GetMethod("SetEmptyState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(slot, new object[] { slotNumber });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CustomGameStateSlotsGrid] 슬롯 {slotNumber} 새로고침 실패: {ex.Message}");
        }
    }

    protected override void OnDestroy()
    {
        ClearSlots();
        base.OnDestroy();
    }
}