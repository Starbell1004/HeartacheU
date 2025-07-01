using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ItemTooltip : MonoBehaviour
{
    [Header("���� UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private Vector2 offset = new Vector2(50, 50);

    private RectTransform tooltipRect;
    private Canvas parentCanvas;
    private Camera uiCamera;

    private static ItemTooltip _instance;
    public static ItemTooltip Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            canvasGroup.blocksRaycasts = false;
            // ������Ʈ �ڵ� ã��
            if (tooltipPanel == null)
                tooltipPanel = gameObject;

            if (tooltipText == null)
                tooltipText = GetComponentInChildren<TextMeshProUGUI>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                uiCamera = parentCanvas.worldCamera;
            }

            // �ʱ� ���� - ����
            HideTooltip();
            Debug.Log("[�����] ItemTooltip �̱��� ���� �Ϸ�!"); // �� �� �߰�
            Debug.Log("[ItemTooltip] �ʱ�ȭ �Ϸ�");
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowTooltip(string itemName, string description, Vector3 worldPosition)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("[ItemTooltip] ������ �̸��� ����ֽ��ϴ�.");
            return;
        }

        Debug.Log($"[ItemTooltip] ���� ǥ��: {itemName}");

        // �ؽ�Ʈ ���� (TextMeshPro ��ġ �ؽ�Ʈ �±� ���)
        string tooltipContent = $"<b>{itemName}</b>";
        if (!string.IsNullOrEmpty(description))
        {
            tooltipContent += $"\n<color=#CCCCCC>{description}</color>";
        }

        if (tooltipText != null)
        {
            tooltipText.text = tooltipContent;

        }
        else
        {
            Debug.LogError("[ItemTooltip] tooltipText�� null�Դϴ�!");
            return;
        }

        // ��ġ ����
        UpdateTooltipPosition(worldPosition);

        // ǥ��
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void HideTooltip()
    {
        Debug.Log("[ItemTooltip] ���� ����");

        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private void UpdateTooltipPosition(Vector3 worldPosition)
    {
        if (tooltipRect == null || parentCanvas == null)
        {
            Debug.LogWarning("[ItemTooltip] �ʼ� ������Ʈ�� �����ϴ�.");
            return;
        }

        Vector2 screenPoint;
        if (uiCamera != null)
        {
            screenPoint = uiCamera.WorldToScreenPoint(worldPosition);
        }
        else
        {
            screenPoint = worldPosition;
        }

        // ������ �������� ū ������ (�������� ������ �ʰ�)
        screenPoint += new Vector2(150f, -50f); // �������� 100px �ø���

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPoint,
            uiCamera,
            out localPoint);

        var canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // ȭ�� ��� üũ
        if (localPoint.x + tooltipRect.rect.width > canvasWidth / 2)
        {
            localPoint.x = canvasWidth / 2 - tooltipRect.rect.width - 10f;
        }
        if (localPoint.x < -canvasWidth / 2)
        {
            localPoint.x = -canvasWidth / 2 + 10f;
        }
        if (localPoint.y + tooltipRect.rect.height > canvasHeight / 2)
        {
            localPoint.y = canvasHeight / 2 - tooltipRect.rect.height - 10f;
        }
        if (localPoint.y < -canvasHeight / 2)
        {
            localPoint.y = -canvasHeight / 2 + 10f;
        }

        tooltipRect.localPosition = localPoint;
        Debug.Log($"[ItemTooltip] ������ ���� ��ġ ����: {localPoint}");
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;
        float startAlpha = canvasGroup.alpha;

        while (timer < 1f / fadeSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer * fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        Debug.Log("[ItemTooltip] ���̵� �� �Ϸ�");
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;
        float startAlpha = canvasGroup.alpha;

        while (timer < 1f / fadeSpeed)
        {
            timer += Time.deltaTime;
            float progress = timer * fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        Debug.Log("[ItemTooltip] ���̵� �ƿ� �Ϸ�");
    }

    // ����׿� �޼���
    [ContextMenu("Test Tooltip")]
    public void TestTooltip()
    {
        ShowTooltip("�׽�Ʈ ������", "�̰��� �׽�Ʈ�� �������Դϴ�.", transform.position);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}