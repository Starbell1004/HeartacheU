using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("hasitem")]
public class HasItemCommand : Command
{
    [ParameterAlias("id")]
    [RequiredParameter]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("HasItemCommand: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[HasItemCommand] StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        var id = ItemId?.Value;
        if (string.IsNullOrWhiteSpace(id))
        {
            Engine.Warn("HasItemCommand: 아이템 ID가 비어 있습니다.");
            return UniTask.CompletedTask;
        }

        // 아이템 존재 여부 검증 추가
        var itemData = StateBasedItemSystem.Instance.GetItemData(id);
        if (itemData == null)
        {
            Debug.LogWarning($"[HasItemCommand] 존재하지 않는 아이템 ID: {id}");

            // 존재하지 않는 아이템은 false로 처리
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager != null)
            {
                try
                {
                    variableManager.SetVariableValue("hasitem_result", new CustomVariableValue("0"));
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[HasItemCommand] 변수 설정 오류: {ex.Message}");
                }
            }

            return UniTask.CompletedTask;
        }

        bool hasItem = StateBasedItemSystem.Instance.HasItem(id);
        Debug.Log($"[HasItemCommand] 아이템 '{id}' 보유 여부: {hasItem}");

        // 결과를 변수로 저장 (기존 호환성 유지)
        var variableManager2 = Engine.GetService<ICustomVariableManager>();
        if (variableManager2 != null)
        {
            try
            {
                variableManager2.SetVariableValue("hasitem_result", new CustomVariableValue(hasItem ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HasItemCommand] 변수 설정 오류: {ex.Message}");
            }
        }

        return UniTask.CompletedTask;
    }
}