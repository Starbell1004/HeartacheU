using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Naninovel;

public class HomeGameStateSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image screenshotImage;
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI gameTimeText;
    [SerializeField] private TextMeshProUGUI realTimeText;
    [SerializeField] private Button slotButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject emptyOverlay;

    [Header("설정")]
    [SerializeField] private string dateFormat = "MM/dd HH:mm";
    [SerializeField] private Color emptySlotColor = Color.gray;

    // Naninovel 스타일 프로퍼티들
    public virtual int SlotNumber { get; private set; }
    public virtual GameStateMap State { get; private set; }
    public virtual bool Empty => State == null;
    public virtual string SlotId => $"CustomSlot_{SlotNumber - 1}"; // 0-based index

    [ManagedText("DefaultUI")]
    protected static string EmptySlotLabel = "저장하기";

    private Action<int> onSlotClicked;
    private Action<int> onDeleteClicked;
    private IStateManager stateManager;

    private void Awake()
    {
        // 컴포넌트 자동 찾기
        FindUIComponents();

        // 이벤트 연결
        SetupEventListeners();

        // Naninovel 서비스 가져오기
        if (Engine.Initialized)
        {
            stateManager = Engine.GetServiceOrErr<IStateManager>();
        }
        else
        {
            Engine.OnInitializationFinished += () => stateManager = Engine.GetServiceOrErr<IStateManager>();
        }
    }

    public virtual void Initialize(Action<int> onSlotClicked, Action<int> onDeleteClicked)
    {
        this.onSlotClicked = onSlotClicked;
        this.onDeleteClicked = onDeleteClicked;

        Debug.Log($"[HomeGameStateSlot] Initialize - 슬롯 이벤트 연결됨");
    }

    public virtual void Bind(int slotNumber, GameStateMap state)
    {
        SlotNumber = slotNumber;
        State = state;

        Debug.Log($"[HomeGameStateSlot] Bind - 슬롯 {slotNumber}, Empty: {Empty}");

        if (Empty)
            SetEmptyState();
        else
            SetNonEmptyState();
    }

    private void FindUIComponents()
    {
        // 컴포넌트들 자동 찾기
        if (screenshotImage == null) screenshotImage = GetComponentInChildren<Image>();
        if (slotButton == null) slotButton = GetComponent<Button>();
        if (deleteButton == null) deleteButton = GetComponentInChildren<Button>();

        // 텍스트 컴포넌트들 찾기
        var texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            if (text.name.ToLower().Contains("slot") && slotNumberText == null)
                slotNumberText = text;
            else if (text.name.ToLower().Contains("game") && gameTimeText == null)
                gameTimeText = text;
            else if (text.name.ToLower().Contains("real") && realTimeText == null)
                realTimeText = text;
        }

        // EmptyOverlay 찾기
        if (emptyOverlay == null)
        {
            var overlayTransform = transform.Find("EmptyOverlay");
            if (overlayTransform != null)
                emptyOverlay = overlayTransform.gameObject;
        }
    }

    private void SetupEventListeners()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(HandleSlotClicked);

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(HandleDeleteClicked);
            deleteButton.gameObject.SetActive(false); // 기본적으로 숨김
        }
    }

    private void SetEmptyState()
    {
        Debug.Log($"[HomeGameStateSlot] SetEmptyState - 슬롯 {SlotNumber}");

        // 삭제 버튼 숨김
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);

        // 빈 슬롯 오버레이 표시
        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);

        // 슬롯 번호 설정
        SetSlotNumberText();

        // 빈 슬롯 텍스트
        if (gameTimeText != null)
            gameTimeText.text = EmptySlotLabel;

        if (realTimeText != null)
            realTimeText.text = "";

        // 빈 슬롯 이미지
        if (screenshotImage != null)
        {
            screenshotImage.sprite = null;
            screenshotImage.color = emptySlotColor;
        }
    }

    private void SetNonEmptyState()
    {
        Debug.Log($"[HomeGameStateSlot] SetNonEmptyState - 슬롯 {SlotNumber}");

        Debug.Log($"[HomeGameStateSlot] SetNonEmptyState - 슬롯 {SlotNumber}");

        // 삭제 버튼 항상 숨김 (삭제 기능 비활성화)
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);

        // 빈 슬롯 오버레이 숨김
        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);
        // 슬롯 번호 설정
        SetSlotNumberText();

        // 게임 시간 표시
        SetGameTimeText();

        // 저장 날짜 표시
        if (realTimeText != null && State != null)
            realTimeText.text = State.SaveDateTime.ToString(dateFormat);

        // 스크린샷 표시
        if (screenshotImage != null && State?.Thumbnail != null)
        {
            Sprite sprite = Sprite.Create(
                State.Thumbnail,
                new Rect(0, 0, State.Thumbnail.width, State.Thumbnail.height),
                Vector2.one * 0.5f
            );
            screenshotImage.sprite = sprite;
            screenshotImage.color = Color.white;
        }
    }

    private void SetSlotNumberText()
    {
        if (slotNumberText != null)
            slotNumberText.text = $"Slot {SlotNumber:D2}";
    }

    private void SetGameTimeText()
    {
        if (gameTimeText == null) return;

        try
        {
            // Naninovel 변수에서 게임 시간 가져오기
            var varManager = Engine.GetService<ICustomVariableManager>();
            if (varManager != null)
            {
                string dayStr = GetStringVariableValue(varManager, "day", "0");
                string hourStr = GetStringVariableValue(varManager, "hour", "0");
                string minStr = GetStringVariableValue(varManager, "min", "0");

                // 문자열을 숫자로 변환
                int.TryParse(dayStr, out int day);
                int.TryParse(hourStr, out int hour);
                int.TryParse(minStr, out int min);

                // 모두 0이면 기본 메시지
                if (day == 0 && hour == 0 && min == 0)
                {
                    gameTimeText.text = "저장된 게임";
                }
                else
                {
                    gameTimeText.text = $"2월 {day}일 {hour:D2}:{min:D2}";  // 2주 → 2월
                }
            }
            else
            {
                gameTimeText.text = "저장된 게임";
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[HomeGameStateSlot] 게임 시간 표시 실패: {ex.Message}");
            gameTimeText.text = "저장된 게임";
        }
    }

    private string GetStringVariableValue(ICustomVariableManager varManager, string variableName, string defaultValue)
    {
        try
        {
            var variable = varManager.GetVariableValue(variableName);
            if (variable != null)
                return variable.ToString();
        }
        catch { }
        return defaultValue;
    }

    // 이벤트 핸들러들
    private void HandleSlotClicked()
    {
        Debug.Log($"[HomeGameStateSlot] 슬롯 {SlotNumber} 클릭됨");
        onSlotClicked?.Invoke(SlotNumber);
    }

    private void HandleDeleteClicked()
    {
        Debug.Log($"[HomeGameStateSlot] 슬롯 {SlotNumber} 삭제 버튼 클릭됨");
        onDeleteClicked?.Invoke(SlotNumber);
    }

    // 마우스 호버 이벤트
    // 마우스 호버 이벤트
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 삭제 기능 비활성화
        // if (deleteButton != null && !Empty)
        //     deleteButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // if (deleteButton != null)
        //     deleteButton.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        if (slotButton != null)
            slotButton.onClick.RemoveListener(HandleSlotClicked);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(HandleDeleteClicked);
    }
}