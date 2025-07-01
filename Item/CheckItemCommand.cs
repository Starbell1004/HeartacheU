using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("CheckItemState")]
public class CheckItemStateCommand : Command
{
    [ParameterAlias("id")]
    [RequiredParameter]
    public StringParameter ItemId;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("CheckItemState: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[CheckItemState] StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        var id = ItemId?.Value;
        if (string.IsNullOrWhiteSpace(id))
        {
            Engine.Warn("CheckItemState: 아이템 ID가 비어 있습니다.");
            return UniTask.CompletedTask;
        }

        // 아이템 존재 여부 검증 추가
        var itemData = StateBasedItemSystem.Instance.GetItemData(id);
        if (itemData == null)
        {
            Debug.LogWarning($"[CheckItemState] 존재하지 않는 아이템 ID: {id}");

            // 존재하지 않는 아이템은 모두 기본값(NotOwned)으로 처리
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager != null)
            {
                try
                {
                    variableManager.SetVariableValue("item_state", new CustomVariableValue("0"));
                    variableManager.SetVariableValue("item_owned", new CustomVariableValue("0"));
                    variableManager.SetVariableValue("item_used", new CustomVariableValue("0"));
                    variableManager.SetVariableValue("item_ever_owned", new CustomVariableValue("0"));
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[CheckItemState] 변수 설정 오류: {ex.Message}");
                }
            }

            return UniTask.CompletedTask;
        }

        var state = StateBasedItemSystem.Instance.GetItemState(id);
        Debug.Log($"[CheckItemState] 아이템 '{id}' 상태: {state}({(int)state})");

        // 결과를 변수로 저장
        var variableManager2 = Engine.GetService<ICustomVariableManager>();
        if (variableManager2 != null)
        {
            try
            {
                variableManager2.SetVariableValue("item_state", new CustomVariableValue(((int)state).ToString()));
                variableManager2.SetVariableValue("item_owned", new CustomVariableValue(state == StateBasedItemSystem.ItemState.Owned ? "1" : "0"));
                variableManager2.SetVariableValue("item_used", new CustomVariableValue(state == StateBasedItemSystem.ItemState.Used ? "1" : "0"));
                variableManager2.SetVariableValue("item_ever_owned", new CustomVariableValue(state >= StateBasedItemSystem.ItemState.Owned ? "1" : "0"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CheckItemState] 변수 설정 오류: {ex.Message}");
            }
        }

        return UniTask.CompletedTask;
    }
}