using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("AddItem")]  // 기존 이름 유지로 호환성 보장
public class AddItemCommand : Command
{
    [ParameterAlias("id")]
    [RequiredParameter]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("AddItemCommand: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[AddItemCommand]  StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        var id = ItemId?.Value;
        if (string.IsNullOrWhiteSpace(id))
        {
            Engine.Warn("AddItemCommand: 아이템 ID가 비어 있습니다.");
            return UniTask.CompletedTask;
        }

        Debug.Log($"[AddItemCommand]  아이템 '{id}' 획득 시도");
        bool success = StateBasedItemSystem.Instance.AcquireItem(id);

        // 결과를 변수로 저장
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("last_acquired_item", new CustomVariableValue(success ? id : ""));
                variableManager.SetVariableValue("acquire_success", new CustomVariableValue(success ? "1" : "0"));

                // 기존 호환성을 위한 변수도 설정
                variableManager.SetVariableValue("item_add_success", new CustomVariableValue(success ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddItemCommand]  변수 설정 오류: {ex.Message}");
            }
        }

        Debug.Log($"[AddItemCommand] {(success ? "V" : "X")} 아이템 '{id}' 획득 결과: {(success ? "성공" : "실패")}");
        return UniTask.CompletedTask;
    }
}