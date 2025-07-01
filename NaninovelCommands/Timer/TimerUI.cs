using UnityEngine;
using UnityEngine.UI;
using Naninovel.UI;
using System;
using Naninovel;

public class TimerUI : CustomUI
{
    [SerializeField] private Image filledBar;

    private float totalTime;
    private float currentTime;
    private Action onComplete;
    private bool isRunning;

    protected override void Awake()
    {
        base.Awake();
        ResetTimer();
    }

    protected override void Update()
    {
        base.Update();

        if (!isRunning || filledBar == null) return;

        currentTime -= Time.deltaTime;
        filledBar.fillAmount = Mathf.Clamp01(currentTime / totalTime);

        if (currentTime <= 0)
        {
            isRunning = false;
            onComplete?.Invoke();
            Hide();
        }
    }

    public void StartTimer(float duration, Action onComplete)
    {
        ResetTimer();
        totalTime = currentTime = duration;
        this.onComplete = onComplete;
        isRunning = true;
        filledBar.fillAmount = 1f;
        base.Show();
    }

    public void StopImmediately()
    {
        // 진행 중인 타이머를 확실히 중단
        isRunning = false;   // <-- 반드시 추가!
        currentTime = 0f;    // <-- 타이머 강제 종료!
        filledBar.fillAmount = 0f; // UI바도 즉시 초기화!

        onComplete = null;
        base.Hide();
    }

    public override void Hide()
    {
        ResetTimer();
        base.Hide();
    }

    public override void Show()
    {
        base.Show();
    }

    private void ResetTimer()
    {
        isRunning = false;
        currentTime = 0f;
        filledBar.fillAmount = 1f;
        onComplete = null;
    }
}
