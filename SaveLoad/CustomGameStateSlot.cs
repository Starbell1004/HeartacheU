using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Naninovel;
using Naninovel.UI;

/// <summary>
/// �ΰ��ӿ� GameStateSlot - Naninovel.UI.GameStateSlot�� ��ӹ���
/// </summary>
public class CustomGameStateSlot : GameStateSlot
{
    [Header("Ŀ���� UI ������Ʈ")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RawImage thumbnailImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject emptyOverlay;

    [Header("��Ÿ�� ����")]
    [SerializeField] private Color emptySlotColor = Color.gray;
    [SerializeField] private Color normalSlotColor = Color.white;

    protected override void Awake()
    {
        base.Awake();

        // UI ������Ʈ �ڵ� ã��
        FindUIComponents();

        Debug.Log($"[CustomGameStateSlot] Awake - ������Ʈ ���� �Ϸ�");
    }

    protected override void SetEmptyState(int slotNumber)
    {
        base.SetEmptyState(slotNumber);

        Debug.Log($"[CustomGameStateSlot] SetEmptyState - ���� {slotNumber}");

        // Ŀ���� �� ���� UI ����
        SetCustomEmptyState();
    }

    protected override void SetNonEmptyState(int slotNumber, GameStateMap state)
    {
        base.SetNonEmptyState(slotNumber, state);

        Debug.Log($"[CustomGameStateSlot] SetNonEmptyState - ���� {slotNumber}");

        // Ŀ���� ������ �ִ� ���� UI ����
        SetCustomNonEmptyState(state);
    }

    private void FindUIComponents()
    {
        // ������ ������ �°� ������Ʈ ã��

        // ��� �̹��� (��Ʈ�� Image ������Ʈ)
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // ����� �̹��� (Screenshot�� RawImage)
        if (thumbnailImage == null)
        {
            var screenshotTransform = transform.Find("Screenshot");
            if (screenshotTransform != null)
                thumbnailImage = screenshotTransform.GetComponent<RawImage>();
        }

        // ���� ��ư (��Ʈ�� Button ������Ʈ)
        if (mainButton == null)
            mainButton = GetComponent<Button>();

        // ���� ��ư (DeleteButton�� Button)
        if (deleteButton == null)
        {
            var deleteButtonTransform = transform.Find("DeleteButton");
            if (deleteButtonTransform != null)
                deleteButton = deleteButtonTransform.GetComponent<Button>();
        }

        // �ؽ�Ʈ �����̳ʿ��� �ؽ�Ʈ�� ã��
        var textContainer = transform.Find("TextContainer");
        if (textContainer != null)
        {
            // ���� �ð� �ؽ�Ʈ
            if (titleText == null)
            {
                var gameTimeTransform = textContainer.Find("GameTimeText");
                if (gameTimeTransform != null)
                    titleText = gameTimeTransform.GetComponent<TextMeshProUGUI>();
            }

            // �ǽð� �ؽ�Ʈ
            if (dateText == null)
            {
                var realTimeTransform = textContainer.Find("RealTimeText");
                if (realTimeTransform != null)
                    dateText = realTimeTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        // �� ���� ��������
        if (emptyOverlay == null)
        {
            var emptyOverlayTransform = transform.Find("EmptyOverlay");
            if (emptyOverlayTransform != null)
                emptyOverlay = emptyOverlayTransform.gameObject;
        }

        // ������Ʈ ã�� ��� �α�
        LogComponentStatus();
    }

    private void LogComponentStatus()
    {
        Debug.Log($"[CustomGameStateSlot] ������Ʈ ����:");
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
        // �� ���� �������� ǥ��
        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);

        // ���� ��ư �����
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);

        // ��� ����
        if (backgroundImage != null)
            backgroundImage.color = emptySlotColor;

        // ����� �����
        if (thumbnailImage != null)
        {
            thumbnailImage.texture = null;
            thumbnailImage.color = emptySlotColor;
        }

        // �ؽ�Ʈ ����
        if (titleText != null)
            titleText.text = $"���� {SlotNumber:D2}";

        if (dateText != null)
            dateText.text = "�� ����";
    }

    private void SetCustomNonEmptyState(GameStateMap state)
    {
        // �� ���� �������� �����
        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);

        // ��� ����
        if (backgroundImage != null)
            backgroundImage.color = normalSlotColor;

        // ����� ����
        if (thumbnailImage != null && state?.Thumbnail != null)
        {
            thumbnailImage.texture = state.Thumbnail;
            thumbnailImage.color = Color.white;
        }

        // �ؽ�Ʈ ����
        if (titleText != null)
        {
            // ���� �� �ð��̳� ���൵ ǥ�� (�ʿ信 ���� ����)
            titleText.text = $"���� {SlotNumber:D2}";
        }

        if (dateText != null && state != null)
        {
            // ���� ���� �ð� ǥ��
            dateText.text = state.SaveDateTime.ToString("MM/dd HH:mm");
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // ���� ��ư ǥ�� (�����Ͱ� ���� ����)
        if (deleteButton != null && !Empty)
            deleteButton.gameObject.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // ���� ��ư �����
        if (deleteButton != null)
            deleteButton.gameObject.SetActive(false);
    }

    protected override void SetTitleText(string value)
    {
        if (titleText != null)
            titleText.text = value;
    }

    // Naninovel���� �䱸�ϴ� ������Ƽ�� �������̵�
    protected override RawImage ThumbnailImage => thumbnailImage;
    protected override Button DeleteButton => deleteButton;
}