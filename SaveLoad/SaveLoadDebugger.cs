using UnityEngine;
using UnityEngine.UI;
using Naninovel;

/// <summary>
/// SaveLoad ������ �ӽ� ��ũ��Ʈ
/// </summary>
public class SaveLoadDebugger : MonoBehaviour
{
    [Header("�׽�Ʈ ��ư")]
    [SerializeField] private Button testInitializeButton;
    [SerializeField] private Button testCreateSlotsButton;

    [Header("��� �г�")]
    [SerializeField] private SaveLoadPanel saveLoadPanel;
    [SerializeField] private CustomGameStateSlotsGrid[] customGrids;

    private void Start()
    {
        if (testInitializeButton != null)
            testInitializeButton.onClick.AddListener(TestInitialize);

        if (testCreateSlotsButton != null)
            testCreateSlotsButton.onClick.AddListener(TestCreateSlots);
    }

    [ContextMenu("Force Initialize SaveLoadPanel")]
    public async void TestInitialize()
    {
        Debug.Log("[SaveLoadDebugger] SaveLoadPanel ���� �ʱ�ȭ ����");

        if (saveLoadPanel != null)
        {
            try
            {
                await saveLoadPanel.Initialize();
                Debug.Log("[SaveLoadDebugger] SaveLoadPanel �ʱ�ȭ �Ϸ�");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadDebugger] SaveLoadPanel �ʱ�ȭ ����: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("[SaveLoadDebugger] SaveLoadPanel�� ������� �ʾҽ��ϴ�!");
        }
    }

    [ContextMenu("Force Create Slots")]
    public async void TestCreateSlots()
    {
        Debug.Log("[SaveLoadDebugger] ���� ���� ���� ����");

        if (customGrids == null || customGrids.Length == 0)
        {
            Debug.LogError("[SaveLoadDebugger] CustomGameStateSlotsGrid�� ������� �ʾҽ��ϴ�!");
            return;
        }

        foreach (var grid in customGrids)
        {
            if (grid != null)
            {
                try
                {
                    // ���� ��������Ʈ�� �ʱ�ȭ �׽�Ʈ
                    await grid.Initialize(18,
                        (slotNumber) => Debug.Log($"���� {slotNumber} Ŭ����"),
                        (slotNumber) => Debug.Log($"���� {slotNumber} ������"),
                        async (slotNumber) =>
                        {
                            Debug.Log($"���� {slotNumber} �ε� �õ�");
                            return null; // �׽�Ʈ������ null ��ȯ
                        });

                    Debug.Log($"[SaveLoadDebugger] {grid.name} �ʱ�ȭ �Ϸ�");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveLoadDebugger] {grid.name} �ʱ�ȭ ����: {ex.Message}");
                }
            }
        }
    }

    [ContextMenu("Check Grid Settings")]
    public void CheckGridSettings()
    {
        Debug.Log("[SaveLoadDebugger] �׸��� ���� Ȯ��");

        if (customGrids == null || customGrids.Length == 0)
        {
            Debug.LogError("[SaveLoadDebugger] CustomGameStateSlotsGrid�� ������� �ʾҽ��ϴ�!");
            return;
        }

        foreach (var grid in customGrids)
        {
            if (grid != null)
            {
                // ���÷������� private �ʵ� Ȯ��
                var gridType = typeof(CustomGameStateSlotsGrid);

                var containerField = gridType.GetField("slotsContainer",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var prefabField = gridType.GetField("slotPrefab",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var container = containerField?.GetValue(grid) as Transform;
                var prefab = prefabField?.GetValue(grid) as GameObject;

                Debug.Log($"[SaveLoadDebugger] {grid.name}:");
                Debug.Log($"  - Container: {(container ? container.name : "NULL")}");
                Debug.Log($"  - Prefab: {(prefab ? prefab.name : "NULL")}");
                Debug.Log($"  - GameObject Active: {grid.gameObject.activeInHierarchy}");
            }
        }
    }

    [ContextMenu("Check Engine State")]
    public void CheckEngineState()
    {
        Debug.Log("[SaveLoadDebugger] ���ϳ뺧 ���� ���� Ȯ��");

        Debug.Log($"Engine Initialized: {Engine.Initialized}");

        if (Engine.Initialized)
        {
            var stateManager = Engine.GetService<IStateManager>();
            Debug.Log($"StateManager: {(stateManager != null ? "OK" : "NULL")}");

            if (stateManager != null)
            {
                Debug.Log($"Save Slot Limit: {stateManager.Configuration.SaveSlotLimit}");
                Debug.Log($"Quick Save Slot Limit: {stateManager.Configuration.QuickSaveSlotLimit}");
            }
        }
    }
}