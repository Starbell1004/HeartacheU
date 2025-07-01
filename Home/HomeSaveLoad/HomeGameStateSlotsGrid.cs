using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Naninovel;

public class HomeGameStateSlotsGrid : MonoBehaviour
{
    [Header("그리드 설정")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject slotPrefab;

    public List<HomeGameStateSlot> Slots { get; private set; } = new List<HomeGameStateSlot>();

    private Action<int> onSlotClicked;
    private Action<int> onDeleteClicked;
    private Func<int, UniTask<GameStateMap>> loadStateAt;

    public async UniTask Initialize(int itemsCount, Action<int> onSlotClicked,
        Action<int> onDeleteClicked, Func<int, UniTask<GameStateMap>> loadStateAt)
    {
        Debug.Log($"[HomeGameStateSlotsGrid] Initialize - itemsCount: {itemsCount}");

        this.onSlotClicked = onSlotClicked;
        this.onDeleteClicked = onDeleteClicked;
        this.loadStateAt = loadStateAt;

        await InitializeSlots(itemsCount);
    }

    public virtual void BindSlot(int slotNumber, GameStateMap state)
    {
        var slot = Slots.FirstOrDefault(s => s.SlotNumber == slotNumber);
        slot?.Bind(slotNumber, state);

        Debug.Log($"[HomeGameStateSlotsGrid] BindSlot - 슬롯 {slotNumber} 바인딩");
    }

    private async UniTask InitializeSlots(int itemsCount)
    {
        Debug.Log($"[HomeGameStateSlotsGrid] InitializeSlots 시작 - itemsCount: {itemsCount}");

        // 기존 슬롯들 정리
        ClearSlots();
        Debug.Log("[HomeGameStateSlotsGrid] ClearSlots 완료");

        // Container 확인
        if (container == null)
        {
            container = transform.Find("Container") ?? transform;
            Debug.Log($"[HomeGameStateSlotsGrid] Container 자동 설정: {container.name}");
        }
        else
        {
            Debug.Log($"[HomeGameStateSlotsGrid] Container 이미 설정됨: {container.name}");
        }

        if (slotPrefab == null)
        {
            Debug.LogError("[HomeGameStateSlotsGrid] SlotPrefab이 설정되지 않았습니다!");
            return;
        }
        else
        {
            Debug.Log($"[HomeGameStateSlotsGrid] SlotPrefab 확인됨: {slotPrefab.name}");
        }

        Debug.Log("[HomeGameStateSlotsGrid] 슬롯 생성 시작");

        // 슬롯 생성 및 초기화
        for (int i = 0; i < itemsCount; i++)
        {
            Debug.Log($"[HomeGameStateSlotsGrid] 슬롯 {i} 생성 중...");
            await CreateAndBindSlot(i);
            Debug.Log($"[HomeGameStateSlotsGrid] 슬롯 {i} 생성 완료");
        }

        Debug.Log($"[HomeGameStateSlotsGrid] {Slots.Count}개 슬롯 초기화 완료");
    }

    private async UniTask CreateAndBindSlot(int itemIndex)
    {
        // 슬롯 프리팹 생성
        GameObject slotObj = Instantiate(slotPrefab, container);
        slotObj.name = $"Slot_{itemIndex + 1:D2}";

        // HomeGameStateSlot 컴포넌트 확인/추가
        HomeGameStateSlot slot = slotObj.GetComponent<HomeGameStateSlot>();
        if (slot == null)
        {
            slot = slotObj.AddComponent<HomeGameStateSlot>();
            Debug.LogWarning($"[HomeGameStateSlotsGrid] 슬롯 {itemIndex}에 HomeGameStateSlot 컴포넌트 추가됨");
        }

        // 슬롯 초기화
        InitializeSlot(slot);
        Slots.Add(slot);

        // 슬롯 데이터 바인딩
        await BindSlotAsync(slot, itemIndex);
    }

    private void InitializeSlot(HomeGameStateSlot slot)
    {
        slot.Initialize(onSlotClicked, onDeleteClicked);
    }

    private async UniTask BindSlotAsync(HomeGameStateSlot slot, int itemIndex)
    {
        var slotNumber = itemIndex + 1;
        var state = await loadStateAt(slotNumber);

        if (slot != null) // 슬롯이 파괴되지 않았는지 확인
        {
            slot.Bind(slotNumber, state);
        }
    }

    private void ClearSlots()
    {
        foreach (var slot in Slots)
        {
            if (slot != null && slot.gameObject != null)
                DestroyImmediate(slot.gameObject);
        }

        Slots.Clear();

        // Container의 남은 자식들도 정리
        if (container != null)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.GetChild(i).gameObject);
            }
        }
    }

    // 공개 메서드들 (외부에서 호출 가능)
    public void RefreshAllSlots()
    {
        Debug.Log("[HomeGameStateSlotsGrid] 모든 슬롯 새로고침");

        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i] != null)
            {
                RefreshSlot(i + 1);
            }
        }
    }

    public async void RefreshSlot(int slotNumber)
    {
        var state = await loadStateAt(slotNumber);
        BindSlot(slotNumber, state);
    }
}