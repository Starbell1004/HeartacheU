using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("UseItem")]
public class UseItemCommand : Command
{
    [ParameterAlias("id")]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("UseItemCommand: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[UseItemCommand]  StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        string usedItemId = null;
        bool success = false;

        if (!string.IsNullOrEmpty(ItemId?.Value))
        {
            // 특정 아이템 사용
            Debug.Log($"[UseItemCommand]  특정 아이템 사용: {ItemId.Value}");
            success = StateBasedItemSystem.Instance.UseItem(ItemId.Value);
            if (success) usedItemId = ItemId.Value;
        }
        else
        {
            // 가장 오래된 아이템 사용 (FIFO)
            Debug.Log("[UseItemCommand]  가장 오래된 아이템 사용");
            usedItemId = StateBasedItemSystem.Instance.UseOldestItem();
            success = !string.IsNullOrEmpty(usedItemId);
        }

        // 결과를 변수로 저장
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("last_used_item", new CustomVariableValue(usedItemId ?? ""));
                variableManager.SetVariableValue("item_use_success", new CustomVariableValue(success ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UseItemCommand]  변수 설정 오류: {ex.Message}");
            }
        }

        if (success)
        {
            Debug.Log($"[UseItemCommand]  아이템 사용 성공: {usedItemId}");
        }
        else
        {
            Debug.LogWarning("[UseItemCommand]  사용할 아이템이 없거나 실패했습니다.");
        }

        return UniTask.CompletedTask;
    }
}