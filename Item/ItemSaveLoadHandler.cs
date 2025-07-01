using Naninovel;
using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 로드 시 아이템 UI를 복원하는 핸들러
/// </summary>
[InitializeAtRuntime]
public class ItemSaveLoadHandler : IEngineService
{
    private IStateManager stateManager;
    private ICustomVariableManager variableManager;
    private IUIManager uiManager;

    public UniTask InitializeService()
    {
        stateManager = Engine.GetService<IStateManager>();
        variableManager = Engine.GetService<ICustomVariableManager>();
        uiManager = Engine.GetService<IUIManager>();

        if (stateManager != null)
        {
            stateManager.OnGameLoadFinished += OnGameLoadFinished;
            Debug.Log("[ItemSaveLoadHandler] 초기화 완료");
        }

        return UniTask.CompletedTask;
    }

    private void OnGameLoadFinished(GameSaveLoadArgs args)
    {
        Debug.Log($"[ItemSaveLoadHandler] 게임 로드 완료 - 슬롯: {args.SlotId}");

        // 코루틴으로 지연 실행
        CoroutineRunner.StartCoroutine(RestoreItemUIAfterLoad());
    }

    private IEnumerator RestoreItemUIAfterLoad()
    {
        // UI가 완전히 로드될 때까지 충분히 대기
        yield return new WaitForSeconds(2f);

        Debug.Log("[ItemSaveLoadHandler] 아이템 UI 복원 시작");

        // ItemDisplayUI 찾기
        var itemDisplayUI = uiManager?.GetUI<ItemDisplayUI>();
        if (itemDisplayUI == null)
        {
            itemDisplayUI = GameObject.FindObjectOfType<ItemDisplayUI>();
        }

        if (itemDisplayUI == null)
        {
            Debug.LogError("[ItemSaveLoadHandler] ItemDisplayUI를 찾을 수 없습니다!");
            yield break;
        }

        // StateBasedItemSystem이 준비될 때까지 대기
        int waitCount = 0;
        while (StateBasedItemSystem.Instance == null && waitCount < 10)
        {
            yield return new WaitForSeconds(0.5f);
            waitCount++;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[ItemSaveLoadHandler] StateBasedItemSystem.Instance가 null입니다!");
            yield break;
        }

        // 모든 아이템 확인
        var itemDataMap = StateBasedItemSystem.Instance.GetAllItemData();
        int restoredCount = 0;

        foreach (var kvp in itemDataMap)
        {
            string itemId = kvp.Key;
            string variableName = $"item_{itemId}";

            try
            {
                // 변수 확인
                if (variableManager.VariableExists(variableName))
                {
                    var value = variableManager.GetVariableValue(variableName);

                    // 값이 "1"이면 소유 중
                    if (value != null && value.ToString() == "1")
                    {
                        var itemData = kvp.Value;
                        if (itemData != null)
                        {
                            // UI에 아이콘 추가
                            itemDisplayUI.AddItemIcon(itemData);
                            restoredCount++;
                            Debug.Log($"[ItemSaveLoadHandler] 아이템 UI 복원: {itemId}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ItemSaveLoadHandler] 아이템 복원 실패 {itemId}: {ex.Message}");
            }
        }

        if (restoredCount > 0)
        {
            // UI 표시
            itemDisplayUI.Show();
            Debug.Log($"[ItemSaveLoadHandler] UI 복원 완료 - {restoredCount}개 아이템");
        }
        else
        {
            Debug.Log("[ItemSaveLoadHandler] 복원할 아이템이 없습니다");
        }
    }

    public void ResetService()
    {
        // 리셋 시 처리
    }

    public void DestroyService()
    {
        if (stateManager != null)
        {
            stateManager.OnGameLoadFinished -= OnGameLoadFinished;
        }
    }
}