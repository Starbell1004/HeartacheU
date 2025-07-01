using Naninovel;
using UnityEngine;

public static class TimeAttackManagerSingleton
{
    private static TimeAttackManager _instance;

    public static TimeAttackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TimeAttackManager();
                // 엔진이 초기화된 후에만 서비스 초기화 시도
                if (Engine.Initialized)
                {
                    _instance.InitializeService().Forget();
                    Debug.Log("TimeAttackManager 초기화 완료");
                }
                else
                {
                    Debug.LogWarning("엔진이 아직 초기화되지 않아 TimeAttackManager 초기화가 지연됩니다.");
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Unity 시작 시 초기화
        Debug.Log("TimeAttackManagerSingleton이 등록되었습니다.");
    }
}