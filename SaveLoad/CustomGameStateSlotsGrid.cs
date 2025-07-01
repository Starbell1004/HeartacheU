using System;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using Naninovel.UI;
using UnityEngine.UI;

/// <summary>
/// Ŀ���� ���� ���� ���� �׸��� - Naninovel.UI.GameStateSlotsGrid�� ��ӹ���
/// </summary>
public class CustomGameStateSlotsGrid : GameStateSlotsGrid
{
    [Header("Ŀ���� ���� ������")]
    [SerializeField] private CustomGameStateSlot slotPrefab;

    [Header("�׸��� ����")]
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
        Debug.Log($"[CustomGameStateSlotsGrid] Initialize ���� - ���� ��: {slotLimit}");

        onSlotClicked = slotClickHandler;
        onDeleteClicked = deleteClickHandler;
        loadStateHandler = stateLoader;

        // ���� ���Ե� ����
        ClearSlots();

        // �� ���Ե� ����
        await CreateSlots(slotLimit);

        Debug.Log($"[CustomGameStateSlotsGrid] Initialize �Ϸ�");
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
            Debug.LogError("[CustomGameStateSlotsGrid] ���� �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        if (slotsContainer == null)
            slotsContainer = transform;

        Debug.Log($"[CustomGameStateSlotsGrid] ���� ���� ���� - �����̳�: {slotsContainer.name}");

        for (int i = 0; i < slotLimit; i++)
        {
            var slotObj = Instantiate(slotPrefab, slotsContainer);
            var customSlot = slotObj.GetComponent<CustomGameStateSlot>();

            if (customSlot != null)
            {
                // ���� ��ȣ ����
                var slotNumber = i + 1;

                // ���� �ʱ�ȭ
                await InitializeSlot(customSlot, slotNumber);

                customSlots.Add(customSlot);

                Debug.Log($"[CustomGameStateSlotsGrid] ���� {slotNumber} ���� �Ϸ�");
            }
        }

        // Layout ���� ������Ʈ�� ����
        ForceLayoutUpdate();
    }

    private void ForceLayoutUpdate()
    {
        // Layout ���� ������Ʈ
        if (slotsContainer != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(slotsContainer as RectTransform);
        }

        // �θ��� Scroll Rect�� ������Ʈ
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
            // ���� ��ȣ ������ ���� ���÷��� ���
            var slotNumberField = typeof(GameStateSlot).GetField("slotNumber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (slotNumberField != null)
                slotNumberField.SetValue(slot, slotNumber);

            // ���� �ε� �� ����
            var state = await loadStateHandler?.Invoke(slotNumber);

            if (state != null)
            {
                // ���÷����� ���� SetNonEmptyState ȣ��
                var method = typeof(GameStateSlot).GetMethod("SetNonEmptyState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(slot, new object[] { slotNumber, state });
            }
            else
            {
                // ���÷����� ���� SetEmptyState ȣ��
                var method = typeof(GameStateSlot).GetMethod("SetEmptyState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                    method.Invoke(slot, new object[] { slotNumber });
            }

            // �̺�Ʈ ����
            SetupSlotEvents(slot, slotNumber);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CustomGameStateSlotsGrid] ���� {slotNumber} �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private void SetupSlotEvents(CustomGameStateSlot slot, int slotNumber)
    {
        // ������Ʈ���� ���� ��ư�� ã�� (��� ���� ȸ��)
        var buttons = slot.GetComponentsInChildren<Button>();

        Button mainButton = null;
        Button deleteButton = null;

        foreach (var btn in buttons)
        {
            // ���� ��ư�� ���� ��Ʈ�� ����
            if (btn.transform == slot.transform)
            {
                mainButton = btn;
            }
            // ���� ��ư�� �̸����� ã��
            else if (btn.name.ToLower().Contains("delete"))
            {
                deleteButton = btn;
            }
        }

        // ���� ��ư �̺�Ʈ
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => onSlotClicked?.Invoke(slotNumber));
        }

        // ���� ��ư �̺�Ʈ
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => onDeleteClicked?.Invoke(slotNumber));
        }

        Debug.Log($"[CustomGameStateSlotsGrid] ���� {slotNumber} �̺�Ʈ ���� - Main: {mainButton != null}, Delete: {deleteButton != null}");
    }

    public void RefreshAllSlots()
    {
        Debug.Log("[CustomGameStateSlotsGrid] ��� ���� ���ΰ�ħ");

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
            Debug.LogError($"[CustomGameStateSlotsGrid] ���� {slotNumber} ���ΰ�ħ ����: {ex.Message}");
        }
    }

    protected override void OnDestroy()
    {
        ClearSlots();
        base.OnDestroy();
    }
}