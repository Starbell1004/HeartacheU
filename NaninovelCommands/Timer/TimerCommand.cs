using Naninovel;
using Naninovel.Commands;
using UnityEngine; // Debug.LogError를 위해 추가

[CommandAlias("시간")]
public class TimerCommand : Command
{
    [ParameterAlias("초")]
    [RequiredParameter]
    public IntegerParameter duration;

    [ParameterAlias("씬")]
    public StringParameter restartScene;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Engine.Initialized)
        {
            Engine.Warn("엔진이 초기화되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        if (!duration.HasValue)
        {
            Engine.Warn("타이머 시간이 지정되지 않았습니다!");
            return UniTask.CompletedTask;
        }

        // 싱글톤에서 TimeAttackManager 인스턴스 가져오기
        var manager = TimeAttackManagerSingleton.Instance;

        // 매니저가 null인지 확인
        if (manager == null)
        {
            Debug.LogError("TimeAttackManager 인스턴스를 찾을 수 없습니다!");
            return UniTask.CompletedTask;
        }

        // 타이머 시작
        manager?.StartTimeAttack(duration.Value, restartScene?.Value);
        return UniTask.CompletedTask;
    }
}