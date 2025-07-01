using Naninovel.UI;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ItemDisplayUI : CustomUI
{
    [Tooltip("���� ������ �������� ������ �� ����� ������ (ItemIconUI.cs ����)")]
    [SerializeField] private GameObject itemIconPrefab;
    [Tooltip("�����ܵ��� ���ĵ� �θ� RectTransform (HorizontalLayoutGroup ���� �ʼ�)")]
    [SerializeField] private RectTransform iconContainer;
    [Tooltip("������ �����/���� �ִϸ��̼� �ð�")]
    [SerializeField] private float animationDuration = 0.3f;

    private List<ItemIconUI> activeIcons = new();
    private HorizontalLayoutGroup _horizontalLayoutGroup;
    private CanvasGroup _canvasGroup;

    // ������ ���� ���� ����
    private bool _isHiddenByPrinter = false;
    private bool _wasVisibleBeforePrinterHide = false;
    private ITextPrinterManager _textPrinterManager;
    private Coroutine _printerMonitorCoroutine;

    // ���� ���� ������
    private IStateManager _stateManager;

    public void ClearAllIcons()
    {
        Debug.Log($"[ItemDisplayUI] ��� ������ ���� ���� - ���� {activeIcons.Count}��");

        // �������� ���� (����Ʈ ���� �� �ε��� ���� ����)
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
        Debug.Log("[ItemDisplayUI] ��� ������ ���� �Ϸ�");
    }

    protected override void Awake()
    {
        base.Awake();

        // CanvasGroup Ȯ��/�߰�
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // IconContainer ����
        if (iconContainer != null)
        {
            _horizontalLayoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
            if (_horizontalLayoutGroup == null)
            {
                Debug.LogError("ItemDisplayUI: iconContainer�� HorizontalLayoutGroup ������Ʈ�� �ʿ��մϴ�.");
            }
        }
        else
        {
            Debug.LogError("ItemDisplayUI: iconContainer�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        if (itemIconPrefab == null)
        {
            Debug.LogError("ItemDisplayUI: itemIconPrefab�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        // �ʱ� ���� ����
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    protected override void Start()
    {
        base.Start();

        // ���ϳ뺧 ���� �ʱ�ȭ ���
        StartCoroutine(WaitForInitialization());
    }

    private IEnumerator WaitForInitialization()
    {
        // ���� �ʱ�ȭ ���
        while (!Engine.Initialized)
        {
            yield return null;
        }

        // �߰� ��� (�ٸ� ���񽺵��� ������ �ʱ�ȭ�ǵ���)
        yield return new WaitForSeconds(0.5f);

        InitializeServices();

        // �ʱ� ���� �õ�
        RestoreItemIconsFromState();
    }

    private void InitializeServices()
    {
        Debug.Log("[ItemDisplayUI] ���� �ʱ�ȭ ����");

        try
        {
            // ���� ������ ����
            _stateManager = Engine.GetService<IStateManager>();
            if (_stateManager != null)
            {
                // ���� �ε� �Ϸ� �̺�Ʈ ����
                _stateManager.OnGameLoadFinished += OnGameLoadFinished;
                Debug.Log("[ItemDisplayUI] StateManager ���� �� �̺�Ʈ ���� �Ϸ�");
            }

            // ������ ������ ����
            _textPrinterManager = Engine.GetService<ITextPrinterManager>();
            if (_textPrinterManager != null)
            {
                Debug.Log("[ItemDisplayUI] TextPrinterManager ���� ����");
                // ������ ���� ����͸� ����
                _printerMonitorCoroutine = StartCoroutine(MonitorPrinterVisibility());
            }
            else
            {
                Debug.LogWarning("[ItemDisplayUI] TextPrinterManager�� ã�� �� �����ϴ�.");
            }

            // �ʱ� ������ UI ���� (���� ���� ��)
            RestoreItemIconsFromState();

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemDisplayUI] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    /// <summary>
    /// ���� �ε� �Ϸ� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    /// </summary>
    private void OnGameLoadFinished(GameSaveLoadArgs args)
    {
        Debug.Log($"[ItemDisplayUI] ���� �ε� �Ϸ� - ������ UI ���� ���� (����: {args.SlotId})");

        // �ణ�� ���� �� ���� (�ٸ� �ý��۵��� ������ �ε�� ������ ���)
        StartCoroutine(DelayedRestoreItems());
    }

    private IEnumerator DelayedRestoreItems()
    {
        // 0.5�� ��� �� ����
        yield return new WaitForSeconds(0.5f);
        RestoreItemIconsFromState();
    }

    /// <summary>
    /// StateBasedItemSystem���� ���� ���� �������� �о�ͼ� UI ����
    /// </summary>
    public void RestoreItemIconsFromState()
    {
        Debug.Log("[ItemDisplayUI] ������ ���� ����");

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogWarning("[ItemDisplayUI] StateBasedItemSystem.Instance�� null�Դϴ�. ���߿� ��õ�");
            // 1�� �� ��õ�
            StartCoroutine(RetryRestoreItems());
            return;
        }

        try
        {
            // ���� ������ ��ü ����
            ClearAllIcons();

            // ���� ���� ������ ��������
            var ownedItems = StateBasedItemSystem.Instance.GetOwnedItems();

            Debug.Log($"[ItemDisplayUI] ������ ������ ��: {ownedItems.Count}");

            foreach (string itemId in ownedItems)
            {
                var itemData = StateBasedItemSystem.Instance.GetItemData(itemId);
                if (itemData != null)
                {
                    Debug.Log($"[ItemDisplayUI] ������ ����: {itemId}");
                    AddItemIconInternal(itemData, false); // �ִϸ��̼� ���� ����
                }
                else
                {
                    Debug.LogWarning($"[ItemDisplayUI] ������ �����͸� ã�� �� ����: {itemId}");
                }
            }

            // �������� ������ UI ǥ��
            if (activeIcons.Count > 0)
            {
                ShowUIWithIcons();
                Debug.Log($"[ItemDisplayUI] ������ UI ���� �Ϸ� - {activeIcons.Count}�� ������");
            }
            else
            {
                Debug.Log("[ItemDisplayUI] ������ �������� �����ϴ�");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ItemDisplayUI] ������ ���� ����: {ex.Message}");
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
                        // "TextPrinter" ��� "Dialogue" ���
                        var printerUI = uiManager.GetUI("Dialogue");
                        if (printerUI != null)
                        {
                            currentPrinterVisibility = printerUI.Visible;
                        }
                    }

                    // ���� ��ȭ ����
                    if (lastPrinterVisibility != currentPrinterVisibility)
                    {
                        Debug.Log($"[ItemDisplayUI] Dialogue UI ���ü� ��ȭ ����: {lastPrinterVisibility} �� {currentPrinterVisibility}");
                        OnPrinterVisibilityChanged(currentPrinterVisibility);
                        lastPrinterVisibility = currentPrinterVisibility;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ItemDisplayUI] ������ ����͸� ����: {ex.Message}");
                waitTime = 1f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnPrinterVisibilityChanged(bool isVisible)
    {
        if (!isVisible) // ������ ����
        {
            if (activeIcons.Count > 0 && gameObject.activeInHierarchy && !_isHiddenByPrinter)
            {
                Debug.Log("[ItemDisplayUI] ������ ���迡 ���� ������ UI ����");
                _wasVisibleBeforePrinterHide = true;
                _isHiddenByPrinter = true;
                HideUI();
            }
        }
        else // ������ ǥ��
        {
            if (_isHiddenByPrinter && _wasVisibleBeforePrinterHide)
            {
                Debug.Log("[ItemDisplayUI] ������ ǥ�ÿ� ���� ������ UI ����");
                _isHiddenByPrinter = false;
                _wasVisibleBeforePrinterHide = false;

                // �������� ������ �ٽ� ǥ��
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
            Debug.LogError($"[ItemDisplayUI] AddItemIcon ���� - �ʼ� ������Ʈ�� �����ϴ�. ItemData: {itemData != null}, Prefab: {itemIconPrefab != null}, Container: {iconContainer != null}");
            return;
        }

        // �ߺ� üũ
        if (activeIcons.Any(icon => icon.ItemId == itemData.itemId))
        {
            Debug.LogWarning($"ItemDisplayUI: �̹� {itemData.itemId} �������� �����մϴ�.");
            return;
        }

        Debug.Log($"[ItemDisplayUI] ������ ������ ���� ����: {itemData.itemId}");

        // ������ ����
        GameObject iconGO = Instantiate(itemIconPrefab, iconContainer);
        iconGO.name = $"ItemIcon_{itemData.itemId}"; // ������ �̸� ����

        ItemIconUI itemIconUI = iconGO.GetComponent<ItemIconUI>();
        if (itemIconUI == null)
        {
            Debug.LogError("ItemDisplayUI: itemIconPrefab�� ItemIconUI ������Ʈ�� �����ϴ�.");
            Destroy(iconGO);
            return;
        }

        // ������ ����
        itemIconUI.Setup(itemData, animationDuration);
        activeIcons.Add(itemIconUI);

        Debug.Log($"[ItemDisplayUI] ������ ���� �Ϸ�: {itemData.itemId}, �� ������ ��: {activeIcons.Count}");

        // �����Ϳ� ���� ������ ���°� �ƴ� ���� UI ǥ��
        if (!_isHiddenByPrinter)
        {
            ShowUIWithIcons();
        }
        else
        {
            Debug.Log("[ItemDisplayUI] �����Ͱ� ������ �����̹Ƿ� ������ UI ǥ�� ����");
        }

        // ���̾ƿ� ��� ������Ʈ
        StartCoroutine(ForceLayoutUpdate());

        // �ִϸ��̼� ���� (���� �ÿ��� �ִϸ��̼� ����)
        if (withAnimation)
        {
            itemIconUI.PlayAppearAnimation();
        }
        else
        {
            // ���� �ÿ��� ��� ǥ��
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
            // CustomUI Show ȣ��
            Show();

            // GameObject Ȱ��ȭ Ȯ��
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            // CanvasGroup ���� ����
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            // ���̾ƿ� ���� ������Ʈ
            StartCoroutine(ForceLayoutUpdate());

            Debug.Log($"[ItemDisplayUI] UI ǥ�� �Ϸ� - Ȱ��ȭ: {gameObject.activeInHierarchy}, ����: {(_canvasGroup?.alpha ?? 1f)}");
        }
    }

    private IEnumerator ForceLayoutUpdate()
    {
        // ���� �����ӱ��� ���
        yield return null;

        // ���̾ƿ� ���� ������Ʈ
        if (iconContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(iconContainer);
        }

        // HorizontalLayoutGroup ���� ������Ʈ
        if (_horizontalLayoutGroup != null)
        {
            _horizontalLayoutGroup.enabled = false;
            _horizontalLayoutGroup.enabled = true;
        }

        Debug.Log("[ItemDisplayUI] ���̾ƿ� ���� ������Ʈ �Ϸ�");
    }

    public void RemoveItemIcon(string itemId)
    {
        ItemIconUI iconToRemove = activeIcons.FirstOrDefault(icon => icon.ItemId == itemId);
        if (iconToRemove == null)
        {
            Debug.LogWarning($"ItemDisplayUI: ������ �������� ã�� ���߽��ϴ� - {itemId}");
            return;
        }

        Debug.Log($"[ItemDisplayUI] ������ ���� ����: {itemId}");

        iconToRemove.PlayDisappearAnimation(() =>
        {
            activeIcons.Remove(iconToRemove);
            Debug.Log($"[ItemDisplayUI] ������ ���� �Ϸ�: {itemId}, ���� ������ ��: {activeIcons.Count}");

            // ��� �������� ��������� UI �����
            if (activeIcons.Count == 0)
            {
                HideUI();
            }
        });
    }

    private void HideUI()
    {
        Debug.Log("[ItemDisplayUI] UI ����� ����");

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        Hide(); // CustomUI Hide ȣ��
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

    // �ܺο��� ȣ���� �� �ִ� ���� �޼���
    [ContextMenu("Restore Items From State")]
    public void ManualRestoreItems()
    {
        RestoreItemIconsFromState();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // �̺�Ʈ ���� ����
        Engine.OnInitializationFinished -= InitializeServices;

        if (_stateManager != null)
        {
            _stateManager.OnGameLoadFinished -= OnGameLoadFinished;
        }

        // �ڷ�ƾ ����
        if (_printerMonitorCoroutine != null)
        {
            StopCoroutine(_printerMonitorCoroutine);
        }

        Debug.Log("[ItemDisplayUI] ���� �Ϸ�");
    }

    // ������ �޼���
    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"[ItemDisplayUI] ���� ����:");
        Debug.Log($"  - GameObject Ȱ��ȭ: {gameObject.activeInHierarchy}");
        Debug.Log($"  - CanvasGroup ����: {(_canvasGroup?.alpha ?? 1f)}");
        Debug.Log($"  - Ȱ�� ������ ��: {activeIcons.Count}");
        Debug.Log($"  - IconContainer �ڽ� ��: {(iconContainer?.childCount ?? 0)}");
        Debug.Log($"  - �����Ϳ� ���� ������: {_isHiddenByPrinter}");
        Debug.Log($"  - ������ ���� �� ǥ�� ����: {_wasVisibleBeforePrinterHide}");

        if (StateBasedItemSystem.Instance != null)
        {
            var ownedItems = StateBasedItemSystem.Instance.GetOwnedItems();
            Debug.Log($"  - StateBasedItemSystem ���� ������ ��: {ownedItems.Count}");
            foreach (var item in ownedItems)
            {
                Debug.Log($"    - {item}");
            }
        }

        foreach (var icon in activeIcons)
        {
            if (icon != null)
            {
                Debug.Log($"    - ������: {icon.ItemId}, Ȱ��ȭ: {icon.gameObject.activeInHierarchy}");
            }
        }
    }
}