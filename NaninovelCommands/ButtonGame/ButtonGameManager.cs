using UnityEngine;
using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;

public class ButtonGameManager : IEngineService
{
    public static ButtonGameManager Instance { get; private set; }

    private ButtonGameUI gameUI;
    private string failReason = "none";
    private bool puzzleCompleted = false;
    private bool isGameActive = false; // ★ 누락된 변수 추가

    public ButtonGameManager()
    {
        Instance = this;
    }

    public UniTask InitializeService()
    {
        if (Engine.Initialized)
            InitUI();
        else
            Engine.OnInitializationFinished += InitUI;

        return UniTask.CompletedTask;
    }

    private void InitUI()
    {
        var uiManager = Engine.GetService<UIManager>();
        uiManager.OnUICreated += OnUICreated;
        gameUI = uiManager.GetUI<ButtonGameUI>();
        gameUI?.Hide();
    }

    private void OnUICreated(CustomUI ui)
    {
        if (ui is ButtonGameUI buttonUI)
        {
            gameUI = buttonUI;
            gameUI.Hide();
        }
    }

    /// <summary>
    /// 완전히 새로운 게임 시작 (항상 처음부터)
    /// 준서의 15_2_0 라벨로 돌아올 때마다 호출됨
    /// </summary>
    public void StartNewGame(float duration, List<string> sequence = null)
    {
        if (gameUI == null)
        {
            Debug.LogError("ButtonGameUIを찾을 수 없습니다.");
            return;
        }

        // ★ 핵심: 게임 시작 전에 모든 변수 초기화
        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                // 이전 게임 결과 변수들 모두 초기화
                variableManager.SetVariableValue("button_game_finished", new CustomVariableValue("false"));
                variableManager.SetVariableValue("success", new CustomVariableValue("false"));
                variableManager.SetVariableValue("failType", new CustomVariableValue("none"));

                Debug.Log("[ButtonGameManager] 게임 시작 전 변수 초기화 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonGameManager] 변수 초기화 오류: {ex.Message}");
            }
        }

        // 이전 게임 상태 완전 초기화
        gameUI.StopGameImmediately();

        puzzleCompleted = false;
        failReason = "none";
        isGameActive = true;

        Debug.Log($"[ButtonGameManager] 새 게임 시작 - duration: {duration}, sequence: {(sequence != null ? string.Join(",", sequence) : "기본")}");

        // 완전히 새로운 게임으로 시작
        gameUI.StartNewGame(duration, sequence, OnSuccess, OnFail);
    }

    private void OnSuccess()
    {
        isGameActive = false;
        puzzleCompleted = true; // ★ 성공 플래그도 설정

        Debug.Log("[ButtonGameManager] 게임 성공!");

        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("success", new CustomVariableValue("true"));
                variableManager.SetVariableValue("failType", new CustomVariableValue("success"));

                // ★ 게임 완료 플래그 설정 - 나니노벨이 즉시 다음으로 진행
                variableManager.SetVariableValue("button_game_finished", new CustomVariableValue("true"));

                Debug.Log("[ButtonGameManager] 성공 변수 설정 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonGameManager] 변수 설정 오류: {ex.Message}");
            }
        }
    }

    private void OnFail(string reason)
    {
        isGameActive = false;
        failReason = reason; // ★ 실패 사유도 저장

        Debug.Log($"[ButtonGameManager] 게임 실패 - 사유: {reason}");

        var variableManager = Engine.GetService<ICustomVariableManager>();
        if (variableManager != null)
        {
            try
            {
                variableManager.SetVariableValue("success", new CustomVariableValue("false"));
                variableManager.SetVariableValue("failType", new CustomVariableValue(reason));

                // ★ 게임 완료 플래그 설정 - 나니노벨이 즉시 다음으로 진행
                variableManager.SetVariableValue("button_game_finished", new CustomVariableValue("true"));

                Debug.Log($"[ButtonGameManager] 실패 변수 설정 완료 - failType: {reason}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonGameManager] 변수 설정 오류: {ex.Message}");
            }
        }
    }

    // 현재 게임 상태 확인
    public bool IsGameActive() => isGameActive; // ★ 메서드 추가
    public bool IsPuzzleCompleted() => puzzleCompleted;
    public string GetFailReason() => failReason;

    public void ResetService()
    {
        isGameActive = false; // ★ 리셋시 비활성화
        puzzleCompleted = false;
        failReason = "none";
    }

    public void DestroyService()
    {
        var uiManager = Engine.GetService<UIManager>();
        if (uiManager != null)
            uiManager.OnUICreated -= OnUICreated;

        if (Instance == this)
            Instance = null;
    }
}