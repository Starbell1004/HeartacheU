using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

[RequireComponent(typeof(LayoutElement), typeof(CanvasGroup))]
public class ItemIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private float defaultIconSize = 50f;

    public string ItemId { get; private set; }
    private ItemData itemData;

    private RectTransform _rectTransform;
    private LayoutElement _layoutElement;
    private CanvasGroup _canvasGroup;
    private float _animationDuration;
    private float _originalWidth;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _layoutElement = GetComponent<LayoutElement>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>();
        }

        var mainImage = GetComponent<Image>();
        if (mainImage != null && mainImage != iconImage)
        {
            Debug.Log($"[ItemIconUI] 메인 GameObject의 Image 컴포넌트 제거: {gameObject.name}");
            DestroyImmediate(mainImage);
        }

        if (iconImage == null)
        {
            Debug.LogWarning($"[ItemIconUI] Image 컴포넌트를 찾을 수 없어서 새로 생성합니다: {gameObject.name}");
            CreateImageComponent();
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        // 디버그: 컴포넌트 상태 확인
        Debug.Log($"[ItemIconUI] Awake 완료 - {gameObject.name}");
        Debug.Log($"  - CanvasGroup: {(_canvasGroup != null ? "OK" : "NULL")}");
        Debug.Log($"  - Image: {(iconImage != null ? "OK" : "NULL")}");
        Debug.Log($"  - RectTransform: {(_rectTransform != null ? "OK" : "NULL")}");
    }

    private void CreateImageComponent()
    {
        GameObject imageGO = new GameObject("Icon");
        imageGO.transform.SetParent(transform, false);
        iconImage = imageGO.AddComponent<Image>();

        var imageRect = iconImage.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Debug.Log($"[ItemIconUI] Image 컴포넌트 생성 완료: {gameObject.name}");
    }

    public void Setup(ItemData data, float animDuration)
    {
        if (data == null)
        {
            Debug.LogError("[ItemIconUI] ItemData가 null입니다!");
            return;
        }

        ItemId = data.itemId;
        itemData = data;
        _animationDuration = animDuration;

        Debug.Log($"[ItemIconUI] Setup 시작 - ItemId: {ItemId}");

        if (iconImage != null)
        {
            iconImage.sprite = data.iconSprite;
            iconImage.enabled = true;
            iconImage.color = Color.white;

            Debug.Log($"[ItemIconUI] 스프라이트 설정 완료 - Sprite: {(data.iconSprite != null ? data.iconSprite.name : "null")}");
        }
        else
        {
            Debug.LogError("[ItemIconUI] iconImage가 null입니다!");
        }

        SetupSize(data);

        // 디버그: 툴팁 관련 설정 확인
        Debug.Log($"[ItemIconUI] Setup 완료 - ItemId: {ItemId}");
        Debug.Log($"  - ItemData: {(itemData != null ? "OK" : "NULL")}");
        Debug.Log($"  - ItemName: {(itemData != null ? itemData.itemName : "NULL")}");
        Debug.Log($"  - Description: {(itemData != null ? itemData.description : "NULL")}");
        Debug.Log($"  - ItemTooltip.Instance: {(ItemTooltip.Instance != null ? "OK" : "NULL")}");
    }

    private void SetupSize(ItemData data)
    {

        Debug.Log($"=== [ItemIconUI] SetupSize 디버깅 ===");
        Debug.Log($"defaultIconSize 필드값: {defaultIconSize}");
        Debug.Log($"게임오브젝트 이름: {gameObject.name}");
        Debug.Log($"스프라이트 이름: {data.iconSprite?.name}");

        float targetSize = defaultIconSize;
        Debug.Log($"targetSize 설정됨: {targetSize}");

       
        // LayoutElement 완전히 비활성화
        if (_layoutElement != null)
        {
            _layoutElement.enabled = false;
        }

        if (data.iconSprite != null)
        {
            Rect spriteRect = data.iconSprite.rect;
            float aspectRatio = spriteRect.width / spriteRect.height;
            float finalWidth = targetSize * aspectRatio;
            float finalHeight = targetSize;

            // RectTransform으로만 크기 조정
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
            }

            _originalWidth = finalWidth;
            Debug.Log($"[ItemIconUI] 새 크기 적용 - {finalWidth}x{finalHeight} (targetSize: {targetSize})");
        }
        else
        {
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = new Vector2(targetSize, targetSize);
            }

            _originalWidth = targetSize;
        }
    }

    public void PlayAppearAnimation()
    {
        Debug.Log($"[ItemIconUI] 등장 애니메이션 시작 - {ItemId}");

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one * 0.8f;
        }

        StartCoroutine(AnimateAppear());
    }

    private IEnumerator AnimateAppear()
    {
        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.8f;
        Vector3 endScale = Vector3.one;

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / _animationDuration);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = progress;
            }

            if (_rectTransform != null)
            {
                _rectTransform.localScale = Vector3.Lerp(startScale, endScale, progress);
            }

            yield return null;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = Vector3.one;
        }

        Debug.Log($"[ItemIconUI] 등장 애니메이션 완료 - {ItemId}");
    }

    public void PlayDisappearAnimation(Action onComplete)
    {
        Debug.Log($"[ItemIconUI] 사라짐 애니메이션 시작 - {ItemId}");
        StartCoroutine(AnimateDisappear(onComplete));
    }

    private IEnumerator AnimateDisappear(Action onComplete)
    {
        float startWidth = _layoutElement != null && _layoutElement.preferredWidth > 0
            ? _layoutElement.preferredWidth
            : _originalWidth;

        float timer = 0f;

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / _animationDuration);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            }

            if (_layoutElement != null)
            {
                _layoutElement.preferredWidth = Mathf.Lerp(startWidth, 0f, progress);
            }

            yield return null;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

        if (_layoutElement != null)
        {
            _layoutElement.preferredWidth = 0f;
        }

        Debug.Log($"[ItemIconUI] 사라짐 애니메이션 완료 - {ItemId}");

        onComplete?.Invoke();
        Destroy(gameObject);
    }

    // 마우스 호버 이벤트 처리 - 디버그 추가
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[ItemIconUI] 마우스 진입 - {ItemId}");
        Debug.Log($"  - ItemData: {(itemData != null ? "OK" : "NULL")}");
        Debug.Log($"  - ItemTooltip.Instance: {(ItemTooltip.Instance != null ? "OK" : "NULL")}");
        Debug.Log($"  - Transform Position: {transform.position}");
        Debug.Log($"  - Mouse Position: {Input.mousePosition}");
        Debug.Log(" 마우스 이벤트 감지됨! "); // 이 줄 추가

       

        if (itemData == null)
        {
            Debug.LogError("[ItemIconUI] itemData가 null입니다! 툴팁을 표시할 수 없습니다.");
            return;
        }

        if (ItemTooltip.Instance == null)
        {
            Debug.LogError("[ItemIconUI] ItemTooltip.Instance가 null입니다! 툴팁을 표시할 수 없습니다.");
            return;
        }

        Debug.Log($"[ItemIconUI] 툴팁 표시 시도 - 아이템: {itemData.itemName}, 설명: {itemData.description}");

        ItemTooltip.Instance.ShowTooltip(
            itemData.itemName,
            itemData.description,
            transform.position
        );

        Debug.Log("[ItemIconUI] 툴팁 표시 명령 완료");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[ItemIconUI] 마우스 벗어남 - {ItemId}");

        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
            Debug.Log("[ItemIconUI] 툴팁 숨김 명령 완료");
        }
        else
        {
            Debug.LogWarning("[ItemIconUI] ItemTooltip.Instance가 null이어서 툴팁을 숨길 수 없습니다.");
        }
    }

    [ContextMenu("Debug Icon State")]
    public void DebugIconState()
    {
        Debug.Log($"[ItemIconUI] {ItemId} 상태:");
        Debug.Log($"  - GameObject 활성화: {gameObject.activeInHierarchy}");
        Debug.Log($"  - CanvasGroup 알파: {(_canvasGroup?.alpha ?? 1f)}");
        Debug.Log($"  - Image 활성화: {(iconImage?.enabled ?? false)}");
        Debug.Log($"  - Image 스프라이트: {(iconImage?.sprite?.name ?? "null")}");
        Debug.Log($"  - 로컬 스케일: {transform.localScale}");
        Debug.Log($"  - 위치: {transform.position}");
        Debug.Log($"  - RectTransform 크기: {(_rectTransform?.sizeDelta ?? Vector2.zero)}");
        Debug.Log($"  - LayoutElement 선호 크기: {(_layoutElement?.preferredWidth ?? 0)}, {(_layoutElement?.preferredHeight ?? 0)}");
        Debug.Log($"  - ItemData: {(itemData != null ? $"ID:{itemData.itemId}, Name:{itemData.itemName}" : "NULL")}");
        Debug.Log($"  - ItemTooltip.Instance 상태: {(ItemTooltip.Instance != null ? "존재함" : "NULL")}");
    }

    [ContextMenu("Test Tooltip")]
    public void TestTooltip()
    {
        Debug.Log("[ItemIconUI] 수동 툴팁 테스트 시작");
        OnPointerEnter(null);
    }
}