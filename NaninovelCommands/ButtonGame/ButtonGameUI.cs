using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;

public class ButtonGameUI : CustomUI
{
    [SerializeField] private Button[] puzzleButtons; // 6개 버튼 배열
    [SerializeField] private Image filledBar; // 타이밍 UI
    [SerializeField] private float buttonSpacing = 100f; // 버튼 간 최소 거리
    [SerializeField] private CanvasGroup canvasGroup; // 캔버스 그룹
    [SerializeField] private RectTransform buttonAreaRect;

    private float totalTime;
    private float currentTime;
    private int currentIndex;
    private Action onSuccess;
    private Action<string> onFail;
    private bool isRunning;
    private Dictionary<Button, string> buttonIdMap = new Dictionary<Button, string>();
    private List<string> correctSequence = new List<string>();
    private string shirtButtonId = "shirt";

    protected override void Awake()
    {
        base.Awake();

        // 캔버스 그룹이 할당되지 않았다면 찾거나 생성
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        // 초기 상태 설정 - 투명하게
        canvasGroup.alpha = 0;

        CompleteReset();
    }

    protected override void Update()
    {
        base.Update();
        if (!isRunning || filledBar == null) return;

        currentTime -= Time.deltaTime;
        filledBar.fillAmount = Mathf.Clamp01(currentTime / totalTime);

        if (currentTime <= 0)
        {
            Debug.Log("[UI] 시간초과로 실패 처리");
            FailGame("timeout");
        }
    }

    public override void Show()
    {
        base.Show();
        Debug.Log("[UI] Show() 호출됨");

        // UI 표시 (알파값을 1로)
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public override void Hide()
    {
        // UI 숨기기 (알파값을 0으로)
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        base.Hide();
    }

    /// <summary>
    /// ★ 핵심: 완전히 새로운 게임 시작 (항상 처음부터)
    /// 준서의 나니노벨 스크립트에서 15_2_0 라벨로 돌아올 때마다 호출됨
    /// </summary>
    public void StartNewGame(float duration, List<string> sequence, Action onSuccess, Action<string> onFail)
    {
        Debug.Log($"[StartNewGame] 완전 새 게임 시작 - duration={duration}, sequenceCount={sequence?.Count ?? -1}");

        // 완전히 초기화
        CompleteReset();

        // 새 게임 설정
        totalTime = currentTime = duration;
        this.onSuccess = onSuccess;
        this.onFail = onFail;

        // 시퀀스 설정
        if (sequence != null && sequence.Count > 0)
        {
            SetupButtonsWithCustomSequence(sequence);
        }
        else
        {
            SetupButtonsWithDefaultSequence();
        }

        // 버튼 위치 랜덤 배치
        ScatterButtons();

        isRunning = true;
        filledBar.fillAmount = 1f;

        Debug.Log($"[StartNewGame] 새 게임 준비 완료. currentTime={currentTime}, totalTime={totalTime}");

        // UI 표시
        Show();
    }

    /// <summary>
    /// 완전한 초기화 - 모든 상태를 처음으로 되돌림
    /// </summary>
    private void CompleteReset()
    {
        Debug.Log("[CompleteReset] 완전 초기화 시작");

        isRunning = false;
        currentIndex = 0;
        currentTime = 0f;
        correctSequence.Clear();
        buttonIdMap.Clear();

        if (filledBar != null)
            filledBar.fillAmount = 1f;

        // 모든 버튼 완전 초기화
        foreach (var button in puzzleButtons)
        {
            if (button != null)
            {
                // 클릭 이벤트 제거
                button.onClick.RemoveAllListeners();

                // 상호작용 활성화
                button.interactable = true;

                // 외관 완전 복원
                ResetButtonAppearance(button);

                // 비활성화
                button.gameObject.SetActive(false);
            }
        }

        Debug.Log("[CompleteReset] 완전 초기화 완료");
    }

    /// <summary>
    /// 버튼 외관 완전 복원
    /// </summary>
    private void ResetButtonAppearance(Button button)
    {
        Image buttonImage = button.GetComponent<Image>();
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = 1f; // 완전 불투명
            buttonImage.color = color;
        }

        if (buttonText != null)
        {
            Color color = buttonText.color;
            color.a = 1f; // 완전 불투명
            buttonText.color = color;
        }
    }

    // 사용자 지정 시퀀스로 버튼 설정
    private void SetupButtonsWithCustomSequence(List<string> sequence)
    {
        // 사용자 지정 시퀀스 복사
        correctSequence = new List<string>(sequence);

        // 모든 버튼 초기화
        buttonIdMap.Clear();

        // 시퀀스 + 와이셔츠 버튼만큼 활성화
        int activeCount = Mathf.Min(puzzleButtons.Length, sequence.Count + 1);

        // 정상 버튼 배치
        for (int i = 0; i < sequence.Count && i < puzzleButtons.Length - 1; i++)
        {
            Button button = puzzleButtons[i];
            string buttonId = sequence[i];

            buttonIdMap[button] = buttonId;

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = buttonId;
            }

            // 버튼 클릭 이벤트 설정
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonClick(button));
            button.interactable = true;
            button.gameObject.SetActive(true);
            ResetButtonAppearance(button);
        }

        // 와이셔츠 버튼 배치 (마지막 버튼으로)
        int shirtIndex = Mathf.Min(sequence.Count, puzzleButtons.Length - 1);
        Button shirtButton = puzzleButtons[shirtIndex];
        buttonIdMap[shirtButton] = shirtButtonId;

        TMP_Text shirtText = shirtButton.GetComponentInChildren<TMP_Text>();
        if (shirtText != null)
        {
            shirtText.text = "와이셔츠 단추";
        }

        // 와이셔츠 버튼 이벤트 설정
        shirtButton.onClick.RemoveAllListeners();
        shirtButton.onClick.AddListener(() => OnButtonClick(shirtButton));
        shirtButton.interactable = true;
        shirtButton.gameObject.SetActive(true);
        ResetButtonAppearance(shirtButton);

        // 남은 버튼 비활성화
        for (int i = activeCount; i < puzzleButtons.Length; i++)
        {
            puzzleButtons[i].gameObject.SetActive(false);
        }

        Debug.Log($"커스텀 버튼 시퀀스 설정: {string.Join(", ", correctSequence)}");
    }

    // 기본 시퀀스로 버튼 설정 (A, B, C, D, E)
    private void SetupButtonsWithDefaultSequence()
    {
        correctSequence.Clear();
        buttonIdMap.Clear();

        // 기본 알파벳 시퀀스 생성 (A-E)
        for (int i = 0; i < puzzleButtons.Length - 1; i++)
        {
            char letter = (char)('A' + i);
            correctSequence.Add(letter.ToString());
        }

        // 모든 버튼에 시퀀스 할당
        for (int i = 0; i < puzzleButtons.Length - 1; i++)
        {
            Button button = puzzleButtons[i];
            string buttonId = correctSequence[i];

            buttonIdMap[button] = buttonId;

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = buttonId;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonClick(button));
            button.interactable = true;
            button.gameObject.SetActive(true);
            ResetButtonAppearance(button);
        }

        // 와이셔츠 버튼 설정
        int shirtIndex = puzzleButtons.Length - 1;
        Button shirtButton = puzzleButtons[shirtIndex];
        buttonIdMap[shirtButton] = shirtButtonId;

        TMP_Text shirtText = shirtButton.GetComponentInChildren<TMP_Text>();
        if (shirtText != null)
        {
            shirtText.text = "와이셔츠 단추";
        }

        shirtButton.onClick.RemoveAllListeners();
        shirtButton.onClick.AddListener(() => OnButtonClick(shirtButton));
        shirtButton.interactable = true;
        shirtButton.gameObject.SetActive(true);
        ResetButtonAppearance(shirtButton);

        Debug.Log($"기본 버튼 시퀀스 설정: {string.Join(", ", correctSequence)}");
    }

    // 버튼 클릭 처리 - 즉시 실패 처리
    private void OnButtonClick(Button button)
    {
        if (!isRunning || !buttonIdMap.ContainsKey(button)) return;

        string buttonId = buttonIdMap[button];
        Debug.Log($"버튼 클릭: {buttonId}, 현재 인덱스: {currentIndex}");

        if (buttonId == shirtButtonId)
        {
            // 와이셔츠 단추 클릭 시 - 즉시 실패 처리
            Debug.Log("[ButtonClick] 와이셔츠 단추 클릭 - 즉시 실패");
            FailGame("shirt");
            return;
        }

        // 현재 인덱스와 버튼 ID 비교
        if (currentIndex < correctSequence.Count && buttonId == correctSequence[currentIndex])
        {
            // 정확한 시퀀스로 클릭
            button.interactable = false; // 중복 상호작용 비활성화

            // 페이드 아웃 효과 시작
            StartCoroutine(FadeOutButton(button));

            currentIndex++;

            // 모든 버튼을 시퀀스대로 클릭했는지 확인
            if (currentIndex >= correctSequence.Count)
            {
                CompleteGame();
            }
        }
        else
        {
            // 잘못된 시퀀스로 클릭 - 즉시 실패 처리
            Debug.Log($"[ButtonClick] 잘못된 시퀀스 클릭 - 즉시 실패. 예상: {(currentIndex < correctSequence.Count ? correctSequence[currentIndex] : "없음")}, 실제: {buttonId}");
            FailGame("wrong_sequence");
        }
    }

    private IEnumerator FadeOutButton(Button button)
    {
        // 버튼 이미지와 텍스트 컴포넌트 가져오기
        Image buttonImage = button.GetComponent<Image>();
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

        // 초기 색상 저장
        Color originalImageColor = buttonImage.color;
        Color originalTextColor = buttonText != null ? buttonText.color : Color.white;

        // 페이드 아웃 설정
        float duration = 0.5f;
        float elapsed = 0f;

        // 점진적으로 알파값 감소
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);

            // 이미지 알파값 조정
            Color imageColor = buttonImage.color;
            imageColor.a = Mathf.Lerp(originalImageColor.a, 0f, normalizedTime);
            buttonImage.color = imageColor;

            // 텍스트 알파값 조정
            if (buttonText != null)
            {
                Color textColor = buttonText.color;
                textColor.a = Mathf.Lerp(originalTextColor.a, 0f, normalizedTime);
                buttonText.color = textColor;
            }

            yield return null;
        }

        // 완전히 투명하게
        if (buttonImage != null)
        {
            Color finalColor = buttonImage.color;
            finalColor.a = 0f;
            buttonImage.color = finalColor;
        }

        if (buttonText != null)
        {
            Color finalTextColor = buttonText.color;
            finalTextColor.a = 0f;
            buttonText.color = finalTextColor;
        }
    }
    [SerializeField] private Transform[] buttonSlots; // Inspector에서 미리 정해진 슬롯 위치들
    
    private void ScatterButtons()
    {
        // 활성화된 버튼들 찾기
        List<Button> activeButtons = new List<Button>();
        foreach (var button in puzzleButtons)
        {
            if (button.gameObject.activeSelf)
            {
                activeButtons.Add(button);
            }
        }

        if (activeButtons.Count == 0) return;

        // 슬롯 인덱스 리스트 생성 및 셔플
        List<int> slotIndices = new List<int>();
        for (int i = 0; i < Mathf.Min(buttonSlots.Length, activeButtons.Count); i++)
        {
            slotIndices.Add(i);
        }

        // 슬롯 순서 셔플
        for (int i = 0; i < slotIndices.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, slotIndices.Count);
            int temp = slotIndices[i];
            slotIndices[i] = slotIndices[randomIndex];
            slotIndices[randomIndex] = temp;
        }

        // 버튼들을 셔플된 슬롯에 배치
        for (int i = 0; i < activeButtons.Count; i++)
        {
            if (i < slotIndices.Count && slotIndices[i] < buttonSlots.Length)
            {
                RectTransform buttonRect = activeButtons[i].GetComponent<RectTransform>();
                Transform targetSlot = buttonSlots[slotIndices[i]];

                // 슬롯 위치로 이동
                buttonRect.position = targetSlot.position;

                Debug.Log($"버튼 {i} -> 슬롯 {slotIndices[i]}에 배치");
            }
        }
    }

    public void StopGameImmediately()
    {
        Debug.Log("[StopGameImmediately] 게임 즉시 중단");

        isRunning = false;
        currentTime = 0f;

        if (filledBar != null)
            filledBar.fillAmount = 0f;

        Hide();
    }

    private void CompleteGame()
    {
        isRunning = false;
        Hide();
        onSuccess?.Invoke();
    }

    private void FailGame(string reason)
    {
        Debug.Log($"[FailGame] 실패 사유: {reason}");
        isRunning = false;
        Hide();
        onFail?.Invoke(reason);
    }

    private void OnEnable()
    {
        Debug.Log("[UI] ButtonGameUI 활성화됨!");
    }
}