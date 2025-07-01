using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ItemTooltip : MonoBehaviour
{
    [Header("툴팁 UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("애니메이션 설정")]
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
            // 컴포넌트 자동 찾기
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

            // 초기 상태 - 숨김
            HideTooltip();
            Debug.Log("[디버그] ItemTooltip 싱글톤 생성 완료!"); // 이 줄 추가
            Debug.Log("[ItemTooltip] 초기화 완료");
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
            Debug.LogWarning("[ItemTooltip] 아이템 이름이 비어있습니다.");
            return;
        }

        Debug.Log($"[ItemTooltip] 툴팁 표시: {itemName}");

        // 텍스트 설정 (TextMeshPro 리치 텍스트 태그 사용)
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
            Debug.LogError("[ItemTooltip] tooltipText가 null입니다!");
            return;
        }

        // 위치 설정
        UpdateTooltipPosition(worldPosition);

        // 표시
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void HideTooltip()
    {
        Debug.Log("[ItemTooltip] 툴팁 숨김");

        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private void UpdateTooltipPosition(Vector3 worldPosition)
    {
        if (tooltipRect == null || parentCanvas == null)
        {
            Debug.LogWarning("[ItemTooltip] 필수 컴포넌트가 없습니다.");
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

        // 아이콘 위쪽으로 큰 오프셋 (아이콘을 가리지 않게)
        screenPoint += new Vector2(150f, -50f); // 위쪽으로 100px 올리기

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPoint,
            uiCamera,
            out localPoint);

        var canvasRect = parentCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // 화면 경계 체크
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
        Debug.Log($"[ItemTooltip] 아이콘 위쪽 위치 설정: {localPoint}");
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
        Debug.Log("[ItemTooltip] 페이드 인 완료");
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

        Debug.Log("[ItemTooltip] 페이드 아웃 완료");
    }

    // 디버그용 메서드
    [ContextMenu("Test Tooltip")]
    public void TestTooltip()
    {
        ShowTooltip("테스트 아이템", "이것은 테스트용 아이템입니다.", transform.position);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}