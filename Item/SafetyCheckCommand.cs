using Naninovel;
using Naninovel.Commands;
using UnityEngine;

/// <summary>
/// 세이프티 아이템 보유 여부 확인
/// 사용법: @SafetyCheck
/// 결과: has_safety_items (1=있음, 0=없음)
/// </summary>
[CommandAlias("SafetyCheck")]
public class SafetyCheckCommand : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("SafetyCheck: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[SafetyCheck] StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        var ownedItems = StateBasedItemSystem.Instance.GetOwnedItems();
        bool hasSafetyItems = ownedItems.Count > 0;

        Debug.Log($"[SafetyCheck] 세이프티 아이템 보유: {hasSafetyItems} (개수: {ownedItems.Count})");

        // 결과를 변수로 저장
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("has_safety_items", new CustomVariableValue(hasSafetyItems ? "1" : "0"));
                variableManager.SetVariableValue("safety_item_count", new CustomVariableValue(ownedItems.Count.ToString()));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SafetyCheck] 변수 설정 오류: {ex.Message}");
            }
        }

        return UniTask.CompletedTask;
    }
}

/// <summary>
/// 가장 오래된 아이템 자동 사용 (안전한 버전)
/// 사용법: @UseOldest
/// 결과: used_item (사용된 아이템 ID)
/// </summary>
[CommandAlias("UseOldest")]
public class UseOldestCommand : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("UseOldest: 나니노벨 엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (StateBasedItemSystem.Instance == null)
        {
            Debug.LogError("[UseOldest] StateBasedItemSystem.Instance가 null입니다!");
            return UniTask.CompletedTask;
        }

        string usedItem = StateBasedItemSystem.Instance.UseOldestItem();
        bool success = !string.IsNullOrEmpty(usedItem);

        Debug.Log($"[UseOldest] 가장 오래된 아이템 사용: {(success ? usedItem : "실패")}");

        // 결과를 변수로 저장 (안전하게)
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                // 기본적으로 빈 문자열로 초기화
                string safeUsedItem = usedItem ?? "";
                string safeSceneName = "";

                // 아이템이 성공적으로 사용된 경우에만 라벨 이름 설정 (Scene 접미사 제거)
                if (success && !string.IsNullOrEmpty(usedItem))
                {
                    safeSceneName = usedItem; // "Scene" 접미사를 붙이지 않음
                }

                variableManager.SetVariableValue("used_item", new CustomVariableValue(safeUsedItem));
                variableManager.SetVariableValue("used_item_scene", new CustomVariableValue(safeSceneName));
                variableManager.SetVariableValue("use_oldest_success", new CustomVariableValue(success ? "1" : "0"));

                Debug.Log($"[UseOldest] 변수 설정 완료 - used_item: '{safeUsedItem}', used_item_scene: '{safeSceneName}'");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UseOldest] 변수 설정 오류: {ex.Message}");

                // 오류 발생시 안전한 기본값으로 설정
                try
                {
                    variableManager.SetVariableValue("used_item", new CustomVariableValue(""));
                    variableManager.SetVariableValue("used_item_scene", new CustomVariableValue(""));
                    variableManager.SetVariableValue("use_oldest_success", new CustomVariableValue("0"));
                }
                catch (System.Exception fallbackEx)
                {
                    Debug.LogError($"[UseOldest] 기본값 설정도 실패: {fallbackEx.Message}");
                }
            }
        }

        return UniTask.CompletedTask;
    }
}