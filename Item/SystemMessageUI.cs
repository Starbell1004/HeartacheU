using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 시스템 메시지를 표시하고 자동으로 사라지게 하는 UI
/// 나니노벨 Configuration > UI > CustomUI에 등록 필요
/// </summary>
public class SystemMessageUI : CustomUI
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Text messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float defaultDisplayDuration = 2f;

    private Coroutine _currentMessageCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // 컴포넌트 자동 설정
        if (messageText == null)
            messageText = GetComponentInChildren<Text>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // 초기 상태 설정
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // 초기에는 숨김
        Hide();
    }

    /// <summary>
    /// 시스템 메시지를 표시합니다
    /// </summary>
    /// <param name="text">표시할 메시지</param>
    /// <param name="duration">표시 시간 (null이면 기본값 사용)</param>
    public void ShowMessage(string text, float? duration = null)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // 이전 메시지 코루틴이 실행 중이면 중단
        if (_currentMessageCoroutine != null)
        {
            StopCoroutine(_currentMessageCoroutine);
        }

        _currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(text, duration ?? defaultDisplayDuration));
    }

    private IEnumerator ShowMessageCoroutine(string text, float displayDuration)
    {
        // 메시지 설정
        if (messageText != null)
            messageText.text = text;

        // UI 보이기
        Show();

        // 페이드 인
        if (canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // 표시 시간 대기
        yield return new WaitForSeconds(displayDuration);

        // 페이드 아웃
        if (canvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        // UI 숨기기
        Hide();

        _currentMessageCoroutine = null;
    }

    /// <summary>
    /// 현재 표시 중인 메시지를 즉시 숨깁니다
    /// </summary>
    public void HideMessage()
    {
        if (_currentMessageCoroutine != null)
        {
            StopCoroutine(_currentMessageCoroutine);
            _currentMessageCoroutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        Hide();
    }
}