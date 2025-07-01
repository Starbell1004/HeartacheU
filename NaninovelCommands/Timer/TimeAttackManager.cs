using Naninovel;
using Naninovel.UI;
using UnityEngine;

public class TimeAttackManager : IEngineService
{
    public static TimeAttackManager Instance { get; private set; }

    private TimerUI timerUI;
    private ChoiceHandlerPanel choiceHandler;
    private string restartScene;
    private bool choiceMade;

    public TimeAttackManager() => Instance = this;

    public UniTask InitializeService()
    {
        if (Engine.Initialized)
            InitializeUIComponents();
        else
            Engine.OnInitializationFinished += InitializeUIComponents;

        return UniTask.CompletedTask;
    }

    private void InitializeUIComponents()
    {
        var uiManager = Engine.GetService<UIManager>();
        uiManager.OnUICreated += OnUICreated;

        timerUI = uiManager.GetUI<TimerUI>();
        choiceHandler = uiManager.GetUI<ChoiceHandlerPanel>();

        if (choiceHandler != null)
            choiceHandler.OnChoiceMade += HandleChoice;

        timerUI?.Hide();
    }

    private void OnUICreated(CustomUI ui)
    {
        if (ui is TimerUI timer)
        {
            timerUI = timer;
            timerUI.Hide();
        }
        else if (ui is ChoiceHandlerPanel choice)
        {
            if (choiceHandler != null)
                choiceHandler.OnChoiceMade -= HandleChoice;

            choiceHandler = choice;
            choiceHandler.OnChoiceMade += HandleChoice;
        }
    }

    public void StartTimeAttack(float duration, string restartSceneId)
    {
        if (timerUI == null)
            return;

        restartScene = restartSceneId;
        choiceMade = false;
        timerUI.StartTimer(duration, OnTimeout);
    }

    private void HandleChoice(int _)
    {
        choiceMade = true;
        timerUI?.StopImmediately();
    }

    private async void OnTimeout()
    {
        if (choiceMade) return;

        choiceHandler?.Hide();
        timerUI?.Hide();

        // 씬 지정 안 된 경우 = 그냥 그 자리에 남아서 다음 스크립트 줄로 진행
        if (string.IsNullOrEmpty(restartScene))
            return;

        var scriptPlayer = Engine.GetService<ScriptPlayer>();
        await scriptPlayer.PlayAsync(restartScene);
    }

    public void StopListeningToChoice()
    {
        if (choiceHandler != null)
            choiceHandler.OnChoiceMade -= HandleChoice;
    }

    public void ResumeListeningToChoice()
    {
        if (choiceHandler != null)
        {
            choiceHandler.OnChoiceMade -= HandleChoice;
            choiceHandler.OnChoiceMade += HandleChoice;
        }
    }

    public void ResetService()
    {
        if (choiceHandler != null)
            choiceHandler.OnChoiceMade -= HandleChoice;
    }

    public void DestroyService()
    {
        var uiManager = Engine.GetService<UIManager>();
        uiManager.OnUICreated -= OnUICreated;

        if (choiceHandler != null)
            choiceHandler.OnChoiceMade -= HandleChoice;

        if (Instance == this)
            Instance = null;
    }
}
