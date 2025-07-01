using UnityEngine;
using UnityEngine.UI;
using Naninovel;

public class HomeGalleryPanel : MonoBehaviour
{
    [System.Serializable]
    public class CGItem
    {
        [Header("CG 이미지들")]
        public Sprite cgImage1;       // 첫 번째 CG 이미지 (기본 표정) - 썸네일로도 사용
        public Sprite cgImage2;       // 두 번째 CG 이미지 (다른 표정)

        [Header("설정")]
        public string unlockVariable; // 해제 변수명 (예: "cg01_unlocked")
        public string title = "";     // CG 제목 (선택사항)

        // 현재 어떤 이미지가 표시되고 있는지 추적
        [System.NonSerialized]
        public bool isShowingImage1 = true;

        // 현재 표시할 이미지 반환
        public Sprite GetCurrentImage()
        {
            return isShowingImage1 ? cgImage1 : cgImage2;
        }

        // 이미지 전환
        public void ToggleImage()
        {
            isShowingImage1 = !isShowingImage1;
        }

        // 썸네일 이미지 반환 (갤러리용) - 항상 첫 번째 이미지 사용
        public Sprite GetThumbnailImage()
        {
            return cgImage1;
        }
    }

    [System.Serializable]
    public class CGSlot
    {
        public Button button;         // CG 버튼
        public Image thumbnailImage;  // 썸네일 이미지
        public int cgIndex = -1;      // cgItems 배열의 인덱스 (-1이면 빈 슬롯)
    }

    [Header("CG 데이터")]
    public CGItem[] cgItems; // CG 정보들

    [Header("고정 CG 슬롯들")]
    public CGSlot[] cgSlots; // CGSlot1~6을 여기에 연결

    [Header("풀 이미지 패널")]
    public GameObject fullImagePanel;
    public Image fullImageDisplay;
    public Button closeButton;

    [Header("이미지 전환 버튼")]
    public Button toggleImageButton; // 표정 전환 버튼
    public Text toggleButtonText;    // 버튼 텍스트 (선택사항)

    [Header("잠금 CG 설정")]
    public Sprite lockedSprite; // 잠금 CG 표시용 스프라이트 (선택사항)
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    // 현재 풀스크린으로 보고 있는 CG 인덱스
    private int currentViewingCGIndex = -1;

    private void Awake()
    {
        Debug.Log("[HomeGalleryPanel] Awake 호출");

        SetupCloseButton();
        SetupToggleButton();
        SetupCGSlots();

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(false);
        }
    }

    private void Start()
    {
        Debug.Log("[HomeGalleryPanel] Start 호출");
        RefreshGallery();
    }

    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseFullImage);
            Debug.Log("[HomeGalleryPanel] 닫기 버튼 이벤트 등록");
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] closeButton이 null입니다!");
        }
    }

    private void SetupToggleButton()
    {
        if (toggleImageButton != null)
        {
            toggleImageButton.onClick.RemoveAllListeners();
            toggleImageButton.onClick.AddListener(ToggleCurrentImage);
            Debug.Log("[HomeGalleryPanel] 이미지 전환 버튼 이벤트 등록");

            // 초기에는 버튼 비활성화
            toggleImageButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] toggleImageButton이 null입니다!");
        }
    }

    private void SetupCGSlots()
    {
        Debug.Log($"[HomeGalleryPanel] CG 슬롯 설정 - 슬롯 수: {cgSlots?.Length ?? 0}");

        if (cgSlots == null || cgSlots.Length == 0)
        {
            Debug.LogError("[HomeGalleryPanel] cgSlots가 설정되지 않았습니다!");
            return;
        }

        // 각 슬롯 버튼에 이벤트 등록
        for (int i = 0; i < cgSlots.Length; i++)
        {
            int slotIndex = i; // 클로저 문제 해결
            CGSlot slot = cgSlots[i];

            if (slot.button != null)
            {
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => OnCGSlotClicked(slotIndex));
                Debug.Log($"[HomeGalleryPanel] 슬롯 {i} 버튼 이벤트 등록");
            }
            else
            {
                Debug.LogWarning($"[HomeGalleryPanel] cgSlots[{i}].button이 null입니다!");
            }
        }
    }

    public void Show()
    {
        Debug.Log("[HomeGalleryPanel] Show 호출");
        gameObject.SetActive(true);
        RefreshGallery();
    }

    public void Hide()
    {
        Debug.Log("[HomeGalleryPanel] Hide 호출");
        gameObject.SetActive(false);
    }

    public void RefreshGallery()
    {
        Debug.Log("[HomeGalleryPanel] 갤러리 새로고침 시작");

        if (cgSlots == null || cgSlots.Length == 0)
        {
            Debug.LogError("[HomeGalleryPanel] cgSlots가 설정되지 않았습니다!");
            return;
        }

        // 각 슬롯 업데이트
        for (int i = 0; i < cgSlots.Length; i++)
        {
            UpdateCGSlot(i);
        }

        Debug.Log("[HomeGalleryPanel] 갤러리 새로고침 완료");
    }

    private void UpdateCGSlot(int slotIndex)
    {
        if (slotIndex >= cgSlots.Length)
        {
            Debug.LogWarning($"[HomeGalleryPanel] 잘못된 슬롯 인덱스: {slotIndex}");
            return;
        }

        CGSlot slot = cgSlots[slotIndex];

        if (slot.button == null || slot.thumbnailImage == null)
        {
            Debug.LogWarning($"[HomeGalleryPanel] 슬롯 {slotIndex}의 컴포넌트가 null입니다!");
            return;
        }

        // cgIndex가 유효한 범위인지 확인
        if (slot.cgIndex >= 0 && slot.cgIndex < cgItems.Length)
        {
            CGItem cgItem = cgItems[slot.cgIndex];
            bool isUnlocked = IsCGUnlocked(cgItem.unlockVariable);

            Debug.Log($"[HomeGalleryPanel] 슬롯 {slotIndex} (CG {slot.cgIndex}) 업데이트 - 해제: {isUnlocked}");

            if (isUnlocked)
            {
                // 해제된 CG - 썸네일 이미지 사용
                slot.thumbnailImage.sprite = cgItem.GetThumbnailImage();
                slot.thumbnailImage.color = unlockedColor;
                slot.button.interactable = true;
            }
            else
            {
                // 잠금 CG
                slot.thumbnailImage.sprite = lockedSprite;
                slot.thumbnailImage.color = lockedColor;
                slot.button.interactable = false;
            }
        }
        else
        {
            // 빈 슬롯 또는 잘못된 인덱스
            Debug.Log($"[HomeGalleryPanel] 슬롯 {slotIndex}는 빈 슬롯입니다 (cgIndex: {slot.cgIndex})");
            slot.thumbnailImage.sprite = null;
            slot.thumbnailImage.color = lockedColor;
            slot.button.interactable = false;
        }
    }

    private void OnCGSlotClicked(int slotIndex)
    {
        Debug.Log($"[HomeGalleryPanel] CG 슬롯 {slotIndex} 클릭됨");

        if (slotIndex >= cgSlots.Length) return;

        CGSlot slot = cgSlots[slotIndex];

        // 유효한 CG이고 해제되어 있는지 확인
        if (slot.cgIndex >= 0 && slot.cgIndex < cgItems.Length)
        {
            CGItem cgItem = cgItems[slot.cgIndex];

            if (IsCGUnlocked(cgItem.unlockVariable))
            {
                // CG 이미지 상태 초기화 (항상 첫 번째 이미지부터 시작)
                cgItem.isShowingImage1 = true;
                currentViewingCGIndex = slot.cgIndex;
                ShowFullImage(cgItem.GetCurrentImage());
            }
            else
            {
                Debug.Log($"[HomeGalleryPanel] CG {slot.cgIndex}는 아직 잠겨있습니다");
            }
        }
    }

    private void ToggleCurrentImage()
    {
        if (currentViewingCGIndex >= 0 && currentViewingCGIndex < cgItems.Length)
        {
            CGItem cgItem = cgItems[currentViewingCGIndex];

            // 이미지 전환
            cgItem.ToggleImage();

            // 새로운 이미지로 업데이트
            if (fullImageDisplay != null)
            {
                fullImageDisplay.sprite = cgItem.GetCurrentImage();
            }

            // 버튼 텍스트 업데이트 (선택사항)
            if (toggleButtonText != null)
            {
                toggleButtonText.text = cgItem.isShowingImage1 ? "표정 2" : "표정 1";
            }

            Debug.Log($"[HomeGalleryPanel] CG {currentViewingCGIndex} 이미지 전환 - 현재: {(cgItem.isShowingImage1 ? "Image1" : "Image2")}");
        }
    }

    private bool IsCGUnlocked(string unlockVariable)
    {
        if (string.IsNullOrEmpty(unlockVariable))
        {
            Debug.LogWarning("[HomeGalleryPanel] 빈 unlockVariable입니다!");
            return false;
        }

        try
        {
            bool isUnlocked = NaninovelVariableHelper.GetVariableAsBool(unlockVariable, false);
            Debug.Log($"[HomeGalleryPanel] 변수 '{unlockVariable}' 확인: {isUnlocked}");
            return isUnlocked;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeGalleryPanel] 변수 확인 오류 '{unlockVariable}': {ex.Message}");
            return false;
        }
    }

    private void ShowFullImage(Sprite fullImg)
    {
        Debug.Log("[HomeGalleryPanel] 풀 이미지 표시");

        if (fullImageDisplay != null && fullImg != null)
        {
            fullImageDisplay.sprite = fullImg;
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] fullImageDisplay 또는 fullImg가 null입니다!");
        }

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] fullImagePanel이 null입니다!");
        }

        // 이미지 전환 버튼 활성화
        if (toggleImageButton != null)
        {
            toggleImageButton.gameObject.SetActive(true);

            // 버튼 텍스트 초기화
            if (toggleButtonText != null)
            {
                toggleButtonText.text = "표정 2";
            }
        }
    }

    public void CloseFullImage()
    {
        Debug.Log("[HomeGalleryPanel] 풀 이미지 닫기");

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(false);
        }

        // 이미지 전환 버튼 비활성화
        if (toggleImageButton != null)
        {
            toggleImageButton.gameObject.SetActive(false);
        }

        // 현재 보고 있는 CG 인덱스 초기화
        currentViewingCGIndex = -1;
    }

    // 외부에서 특정 CG 해제 (테스트용)
    public void UnlockCG(string unlockVariable)
    {
        try
        {
            var customVars = Engine.GetService<ICustomVariableManager>();
            if (customVars != null)
            {
                customVars.SetVariableValue(unlockVariable, new CustomVariableValue(true));
                Debug.Log($"[HomeGalleryPanel] CG 해제: {unlockVariable}");
                RefreshGallery(); // 갤러리 새로고침
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeGalleryPanel] CG 해제 오류: {ex.Message}");
        }
    }

    // 테스트용: 모든 CG 해제
    [ContextMenu("Test: Unlock All CGs")]
    public void UnlockAllCGs()
    {
        Debug.Log("[HomeGalleryPanel] 모든 CG 해제 (테스트)");

        foreach (var cgItem in cgItems)
        {
            if (!string.IsNullOrEmpty(cgItem.unlockVariable))
            {
                UnlockCG(cgItem.unlockVariable);
            }
        }
    }

    // 테스트용: 모든 CG 잠금
    [ContextMenu("Test: Lock All CGs")]
    public void LockAllCGs()
    {
        Debug.Log("[HomeGalleryPanel] 모든 CG 잠금 (테스트)");

        try
        {
            var customVars = Engine.GetService<ICustomVariableManager>();
            if (customVars != null)
            {
                foreach (var cgItem in cgItems)
                {
                    if (!string.IsNullOrEmpty(cgItem.unlockVariable))
                    {
                        customVars.SetVariableValue(cgItem.unlockVariable, new CustomVariableValue(false));
                    }
                }
                RefreshGallery();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeGalleryPanel] CG 잠금 오류: {ex.Message}");
        }
    }
}