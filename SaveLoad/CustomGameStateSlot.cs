using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Naninovel;
using Naninovel.UI;

/// <summary>
/// 인게임용 GameStateSlot - Naninovel.UI.GameStateSlot을 상속받음
/// </summary>
public class CustomGameStateSlot : GameStateSlot
{
    [Header("커스텀 UI 컴포넌트")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RawImage thumbnailImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject emptyOverlay;

    [Header("스타일 설정")]
    [SerializeField] private Color emptySlotColor = Color.gray;
    [SerializeField] private Color normalSlotColor = Color.white;

    protected override void Awake()
    {
        base.Awake();

        // UI 컴포넌트 자동 찾기
        FindUIComponents();

        Debug.Log($"[CustomGameStateSlot] Awake - 컴포넌트 설정 완료");
    }

    protected override void SetEmptyState(int slotNumber)
    {
        base.SetEmptyState(slotNumber);

        Debug.Log($"[CustomGameStateSlot] SetEmptyState - 슬롯 {slotNumber}");

        // 커스텀 빈 슬롯 UI 설정
        SetCustomEmptyState();
    }

    protected override void SetNonEmptyState(int slotNumber, GameStateMap state)
    {
        base.SetNonEmptyState(slotNumber, state);

        Debug.Log($"[CustomGameStateSlot] SetNonEmptyState - 슬롯 {slotNumber}");

        // 커스텀 데이터 있는 슬롯 UI 설정
        SetCustomNonEmptyState(state);
    }

    private void FindUIComponents()
    {
        // 프리팹 구조에 맞게 컴포넌트 찾기

        // 배경 이미지 (루트의 Image 컴포넌트)
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // 썸네일 이미지 (Screenshot의 RawImage)
        if (thumbnailImage == null)
        {
            var screenshotTransform = transform.Find("Screenshot");
            if (screenshotTransform != null)
                thumbnailImage = screenshotTransform.GetComponent<RawImage>();
        }

        // 메인 버튼 (루트의 Button 컴포넌트)
        if (mainButton == null)
            mainButton = GetComponent<Button>();

        // 삭제 버튼 (DeleteButton의 Button)
        if (deleteButton == null)
        {
            var deleteButtonTransform = transform.Find("DeleteButton");
            if (deleteButtonTransform != null)
                deleteButton = deleteButtonTransform.GetComponent<Button>();
        }

        // 텍스트 컨테이너에서 텍스트들 찾기
        var textContainer = transform.Find("TextContainer");
        if (textContainer != null)
        {
            // 게임 시간 텍스트
            if (titleText == null)
            {
                var gameTimeTransform = textContainer.Find("GameTimeText");
                if (gameTimeTransform != null)
                    titleText = gameTimeTransform.GetComponent<TextMeshProUGUI>();
            }

            // 실시간 텍스트
            if (dateText == null)
            {
                var realTimeTransform = textContainer.Find("RealTimeText");
                if (realTimeTransform != null)
                    dateText = realTimeTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        // 빈 슬롯 오버레이
        if (emptyOverlay == null)
        {
            var emptyOverlayTransform = transform.Find("EmptyOverlay");
            if (emptyOverlayTransform != null)
                emptyOverlay = emptyOverlayTransform.gameObject;
        }

        // 컴포넌트 찾기 결과 로그
        LogComponentStatus();
    }

    private void LogComponentStatus()
    {
        Debug.Log($"[CustomGameStateSlot] 컴포넌트 상태:");
        Debug.Log($"  - backgroundImage: {(backgroundImage != null ? "OK" : "Missing")}");
        Debug.Log($"  - thumbnailImage: {(thumbnailImage != null ? "OK" : "Missing")}");
        Debug.Log($"  - titleText: {(titleText != null ? "OK" : "Missing")}");
        Debug.Log($"  - dateText: {(dateText != null ? "OK" : "Missing")}");
        Debug.Log($"  - mainButton: {(mainButton != null ? "OK" : "Missing")}");
        Debug.Log($"  - deleteButton: {(deleteButton != null ? "OK" : "Missing")}");
        Debug.Log($"  - emptyOverlay: {(emptyOverlay != null ? "OK" : "Missing")}");
    }

    private void SetCustomEmptyState()
    {
        // 빈 슬롯 오버레이 표시
        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);

        // 삭제 버튼 숨기기
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);

        // 배경 색상
        if (backgroundImage != null)
            backgroundImage.color = emptySlotColor;

        // 썸네일 숨기기
        if (thumbnailImage != null)
        {
            thumbnailImage.texture = null;
            thumbnailImage.color = emptySlotColor;
        }

        // 텍스트 설정
        if (titleText != null)
            titleText.text = $"슬롯 {SlotNumber:D2}";

        if (dateText != null)
            dateText.text = "빈 슬롯";
    }

    private void SetCustomNonEmptyState(GameStateMap state)
    {
        // 빈 슬롯 오버레이 숨기기
        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);

        // 배경 색상
        if (backgroundImage != null)
            backgroundImage.color = normalSlotColor;

        // 썸네일 설정
        if (thumbnailImage != null && state?.Thumbnail != null)
        {
            thumbnailImage.texture = state.Thumbnail;
            thumbnailImage.color = Color.white;
        }

        // 텍스트 설정
        if (titleText != null)
        {
            // 게임 내 시간이나 진행도 표시 (필요에 따라 수정)
            titleText.text = $"슬롯 {SlotNumber:D2}";
        }

        if (dateText != null && state != null)
        {
            // 실제 저장 시간 표시
            dateText.text = state.SaveDateTime.ToString("MM/dd HH:mm");
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // 삭제 버튼 표시 (데이터가 있을 때만)
        if (deleteButton != null && !Empty)
            deleteButton.gameObject.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // 삭제 버튼 숨기기
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);
    }

    protected override void SetTitleText(string value)
    {
        if (titleText != null)
            titleText.text = value;
    }

    // Naninovel에서 요구하는 프로퍼티들 오버라이드
    protected override RawImage ThumbnailImage => thumbnailImage;
    protected override Button DeleteButton => deleteButton;
}