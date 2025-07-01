using Naninovel;
using Naninovel.Commands;
using System.Collections.Generic;
using UnityEngine;

[CommandAlias("단추게임")]
public class ButtonGameCommand : Command
{
    [ParameterAlias("초")]
    [RequiredParameter]
    public IntegerParameter duration;

    [ParameterAlias("순서")]
    public StringParameter sequence;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        Debug.Log("ButtonGameCommand.Execute 시작");

        if (!Engine.Initialized)
        {
            Engine.Warn("엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (!duration.HasValue)
        {
            Engine.Warn("단추 시간이 지정되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        Debug.Log($"ButtonGameCommand - 지정된 시간: {duration.Value}초");

        var manager = ButtonGameManagerSingleton.Instance;

        if (manager == null)
        {
            Debug.LogError("ButtonGameManager 인스턴스를 찾을 수 없습니다!");
            return UniTask.CompletedTask;
        }

        // 순서 파라미터 처리
        List<string> buttonSequence = null;
        if (sequence.HasValue)
        {
            // 쉼표로 분리된 순서 문자열을 리스트로 변환
            buttonSequence = new List<string>(sequence.Value.Split(','));
            Debug.Log($"지정된 단추 순서: {string.Join(", ", buttonSequence)}");
        }

        Debug.Log("ButtonGameManager.StartNewGame 호출 중");

        // ★ 핵심: 항상 완전히 새로운 게임으로 시작
        manager.StartNewGame(duration.Value, buttonSequence);

        Debug.Log("ButtonGameManager.StartNewGame 호출 완료");

        return UniTask.CompletedTask;
    }
}