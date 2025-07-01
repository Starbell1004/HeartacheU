using Naninovel;
using UnityEngine;

public static class ButtonGameManagerSingleton
{
    private static ButtonGameManager _instance;

    public static ButtonGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ButtonGameManager();

                if (Engine.Initialized)
                {
                    _instance.InitializeService().Forget();
                    Debug.Log("ButtonGameManager 초기화 완료");
                }
                else
                {
                    Debug.LogWarning("엔진 초기화 전. ButtonGameManager 초기화 대기 중");
                }
            }

            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Debug.Log("ButtonGameManagerSingleton 등록 완료");
    }
}