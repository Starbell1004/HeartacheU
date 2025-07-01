using UnityEngine;
using UnityEngine.UI;
using Naninovel;

/// <summary>
/// SaveLoad 디버깅용 임시 스크립트
/// </summary>
public class SaveLoadDebugger : MonoBehaviour
{
    [Header("테스트 버튼")]
    [SerializeField] private Button testInitializeButton;
    [SerializeField] private Button testCreateSlotsButton;

    [Header("대상 패널")]
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
        Debug.Log("[SaveLoadDebugger] SaveLoadPanel 강제 초기화 시작");

        if (saveLoadPanel != null)
        {
            try
            {
                await saveLoadPanel.Initialize();
                Debug.Log("[SaveLoadDebugger] SaveLoadPanel 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveLoadDebugger] SaveLoadPanel 초기화 실패: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("[SaveLoadDebugger] SaveLoadPanel이 연결되지 않았습니다!");
        }
    }

    [ContextMenu("Force Create Slots")]
    public async void TestCreateSlots()
    {
        Debug.Log("[SaveLoadDebugger] 슬롯 강제 생성 시작");

        if (customGrids == null || customGrids.Length == 0)
        {
            Debug.LogError("[SaveLoadDebugger] CustomGameStateSlotsGrid가 연결되지 않았습니다!");
            return;
        }

        foreach (var grid in customGrids)
        {
            if (grid != null)
            {
                try
                {
                    // 더미 델리게이트로 초기화 테스트
                    await grid.Initialize(18,
                        (slotNumber) => Debug.Log($"슬롯 {slotNumber} 클릭됨"),
                        (slotNumber) => Debug.Log($"슬롯 {slotNumber} 삭제됨"),
                        async (slotNumber) =>
                        {
                            Debug.Log($"슬롯 {slotNumber} 로드 시도");
                            return null; // 테스트용으로 null 반환
                        });

                    Debug.Log($"[SaveLoadDebugger] {grid.name} 초기화 완료");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveLoadDebugger] {grid.name} 초기화 실패: {ex.Message}");
                }
            }
        }
    }

    [ContextMenu("Check Grid Settings")]
    public void CheckGridSettings()
    {
        Debug.Log("[SaveLoadDebugger] 그리드 설정 확인");

        if (customGrids == null || customGrids.Length == 0)
        {
            Debug.LogError("[SaveLoadDebugger] CustomGameStateSlotsGrid가 연결되지 않았습니다!");
            return;
        }

        foreach (var grid in customGrids)
        {
            if (grid != null)
            {
                // 리플렉션으로 private 필드 확인
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
        Debug.Log("[SaveLoadDebugger] 나니노벨 엔진 상태 확인");

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