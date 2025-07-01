using UnityEngine;
using UnityEngine.UI;
using Naninovel;

public class HomeGalleryPanel : MonoBehaviour
{
    [System.Serializable]
    public class CGItem
    {
        [Header("CG �̹�����")]
        public Sprite cgImage1;       // ù ��° CG �̹��� (�⺻ ǥ��) - ����Ϸε� ���
        public Sprite cgImage2;       // �� ��° CG �̹��� (�ٸ� ǥ��)

        [Header("����")]
        public string unlockVariable; // ���� ������ (��: "cg01_unlocked")
        public string title = "";     // CG ���� (���û���)

        // ���� � �̹����� ǥ�õǰ� �ִ��� ����
        [System.NonSerialized]
        public bool isShowingImage1 = true;

        // ���� ǥ���� �̹��� ��ȯ
        public Sprite GetCurrentImage()
        {
            return isShowingImage1 ? cgImage1 : cgImage2;
        }

        // �̹��� ��ȯ
        public void ToggleImage()
        {
            isShowingImage1 = !isShowingImage1;
        }

        // ����� �̹��� ��ȯ (��������) - �׻� ù ��° �̹��� ���
        public Sprite GetThumbnailImage()
        {
            return cgImage1;
        }
    }

    [System.Serializable]
    public class CGSlot
    {
        public Button button;         // CG ��ư
        public Image thumbnailImage;  // ����� �̹���
        public int cgIndex = -1;      // cgItems �迭�� �ε��� (-1�̸� �� ����)
    }

    [Header("CG ������")]
    public CGItem[] cgItems; // CG ������

    [Header("���� CG ���Ե�")]
    public CGSlot[] cgSlots; // CGSlot1~6�� ���⿡ ����

    [Header("Ǯ �̹��� �г�")]
    public GameObject fullImagePanel;
    public Image fullImageDisplay;
    public Button closeButton;

    [Header("�̹��� ��ȯ ��ư")]
    public Button toggleImageButton; // ǥ�� ��ȯ ��ư
    public Text toggleButtonText;    // ��ư �ؽ�Ʈ (���û���)

    [Header("��� CG ����")]
    public Sprite lockedSprite; // ��� CG ǥ�ÿ� ��������Ʈ (���û���)
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    // ���� Ǯ��ũ������ ���� �ִ� CG �ε���
    private int currentViewingCGIndex = -1;

    private void Awake()
    {
        Debug.Log("[HomeGalleryPanel] Awake ȣ��");

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
        Debug.Log("[HomeGalleryPanel] Start ȣ��");
        RefreshGallery();
    }

    private void SetupCloseButton()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseFullImage);
            Debug.Log("[HomeGalleryPanel] �ݱ� ��ư �̺�Ʈ ���");
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] closeButton�� null�Դϴ�!");
        }
    }

    private void SetupToggleButton()
    {
        if (toggleImageButton != null)
        {
            toggleImageButton.onClick.RemoveAllListeners();
            toggleImageButton.onClick.AddListener(ToggleCurrentImage);
            Debug.Log("[HomeGalleryPanel] �̹��� ��ȯ ��ư �̺�Ʈ ���");

            // �ʱ⿡�� ��ư ��Ȱ��ȭ
            toggleImageButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] toggleImageButton�� null�Դϴ�!");
        }
    }

    private void SetupCGSlots()
    {
        Debug.Log($"[HomeGalleryPanel] CG ���� ���� - ���� ��: {cgSlots?.Length ?? 0}");

        if (cgSlots == null || cgSlots.Length == 0)
        {
            Debug.LogError("[HomeGalleryPanel] cgSlots�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // �� ���� ��ư�� �̺�Ʈ ���
        for (int i = 0; i < cgSlots.Length; i++)
        {
            int slotIndex = i; // Ŭ���� ���� �ذ�
            CGSlot slot = cgSlots[i];

            if (slot.button != null)
            {
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => OnCGSlotClicked(slotIndex));
                Debug.Log($"[HomeGalleryPanel] ���� {i} ��ư �̺�Ʈ ���");
            }
            else
            {
                Debug.LogWarning($"[HomeGalleryPanel] cgSlots[{i}].button�� null�Դϴ�!");
            }
        }
    }

    public void Show()
    {
        Debug.Log("[HomeGalleryPanel] Show ȣ��");
        gameObject.SetActive(true);
        RefreshGallery();
    }

    public void Hide()
    {
        Debug.Log("[HomeGalleryPanel] Hide ȣ��");
        gameObject.SetActive(false);
    }

    public void RefreshGallery()
    {
        Debug.Log("[HomeGalleryPanel] ������ ���ΰ�ħ ����");

        if (cgSlots == null || cgSlots.Length == 0)
        {
            Debug.LogError("[HomeGalleryPanel] cgSlots�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // �� ���� ������Ʈ
        for (int i = 0; i < cgSlots.Length; i++)
        {
            UpdateCGSlot(i);
        }

        Debug.Log("[HomeGalleryPanel] ������ ���ΰ�ħ �Ϸ�");
    }

    private void UpdateCGSlot(int slotIndex)
    {
        if (slotIndex >= cgSlots.Length)
        {
            Debug.LogWarning($"[HomeGalleryPanel] �߸��� ���� �ε���: {slotIndex}");
            return;
        }

        CGSlot slot = cgSlots[slotIndex];

        if (slot.button == null || slot.thumbnailImage == null)
        {
            Debug.LogWarning($"[HomeGalleryPanel] ���� {slotIndex}�� ������Ʈ�� null�Դϴ�!");
            return;
        }

        // cgIndex�� ��ȿ�� �������� Ȯ��
        if (slot.cgIndex >= 0 && slot.cgIndex < cgItems.Length)
        {
            CGItem cgItem = cgItems[slot.cgIndex];
            bool isUnlocked = IsCGUnlocked(cgItem.unlockVariable);

            Debug.Log($"[HomeGalleryPanel] ���� {slotIndex} (CG {slot.cgIndex}) ������Ʈ - ����: {isUnlocked}");

            if (isUnlocked)
            {
                // ������ CG - ����� �̹��� ���
                slot.thumbnailImage.sprite = cgItem.GetThumbnailImage();
                slot.thumbnailImage.color = unlockedColor;
                slot.button.interactable = true;
            }
            else
            {
                // ��� CG
                slot.thumbnailImage.sprite = lockedSprite;
                slot.thumbnailImage.color = lockedColor;
                slot.button.interactable = false;
            }
        }
        else
        {
            // �� ���� �Ǵ� �߸��� �ε���
            Debug.Log($"[HomeGalleryPanel] ���� {slotIndex}�� �� �����Դϴ� (cgIndex: {slot.cgIndex})");
            slot.thumbnailImage.sprite = null;
            slot.thumbnailImage.color = lockedColor;
            slot.button.interactable = false;
        }
    }

    private void OnCGSlotClicked(int slotIndex)
    {
        Debug.Log($"[HomeGalleryPanel] CG ���� {slotIndex} Ŭ����");

        if (slotIndex >= cgSlots.Length) return;

        CGSlot slot = cgSlots[slotIndex];

        // ��ȿ�� CG�̰� �����Ǿ� �ִ��� Ȯ��
        if (slot.cgIndex >= 0 && slot.cgIndex < cgItems.Length)
        {
            CGItem cgItem = cgItems[slot.cgIndex];

            if (IsCGUnlocked(cgItem.unlockVariable))
            {
                // CG �̹��� ���� �ʱ�ȭ (�׻� ù ��° �̹������� ����)
                cgItem.isShowingImage1 = true;
                currentViewingCGIndex = slot.cgIndex;
                ShowFullImage(cgItem.GetCurrentImage());
            }
            else
            {
                Debug.Log($"[HomeGalleryPanel] CG {slot.cgIndex}�� ���� ����ֽ��ϴ�");
            }
        }
    }

    private void ToggleCurrentImage()
    {
        if (currentViewingCGIndex >= 0 && currentViewingCGIndex < cgItems.Length)
        {
            CGItem cgItem = cgItems[currentViewingCGIndex];

            // �̹��� ��ȯ
            cgItem.ToggleImage();

            // ���ο� �̹����� ������Ʈ
            if (fullImageDisplay != null)
            {
                fullImageDisplay.sprite = cgItem.GetCurrentImage();
            }

            // ��ư �ؽ�Ʈ ������Ʈ (���û���)
            if (toggleButtonText != null)
            {
                toggleButtonText.text = cgItem.isShowingImage1 ? "ǥ�� 2" : "ǥ�� 1";
            }

            Debug.Log($"[HomeGalleryPanel] CG {currentViewingCGIndex} �̹��� ��ȯ - ����: {(cgItem.isShowingImage1 ? "Image1" : "Image2")}");
        }
    }

    private bool IsCGUnlocked(string unlockVariable)
    {
        if (string.IsNullOrEmpty(unlockVariable))
        {
            Debug.LogWarning("[HomeGalleryPanel] �� unlockVariable�Դϴ�!");
            return false;
        }

        try
        {
            bool isUnlocked = NaninovelVariableHelper.GetVariableAsBool(unlockVariable, false);
            Debug.Log($"[HomeGalleryPanel] ���� '{unlockVariable}' Ȯ��: {isUnlocked}");
            return isUnlocked;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeGalleryPanel] ���� Ȯ�� ���� '{unlockVariable}': {ex.Message}");
            return false;
        }
    }

    private void ShowFullImage(Sprite fullImg)
    {
        Debug.Log("[HomeGalleryPanel] Ǯ �̹��� ǥ��");

        if (fullImageDisplay != null && fullImg != null)
        {
            fullImageDisplay.sprite = fullImg;
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] fullImageDisplay �Ǵ� fullImg�� null�Դϴ�!");
        }

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[HomeGalleryPanel] fullImagePanel�� null�Դϴ�!");
        }

        // �̹��� ��ȯ ��ư Ȱ��ȭ
        if (toggleImageButton != null)
        {
            toggleImageButton.gameObject.SetActive(true);

            // ��ư �ؽ�Ʈ �ʱ�ȭ
            if (toggleButtonText != null)
            {
                toggleButtonText.text = "ǥ�� 2";
            }
        }
    }

    public void CloseFullImage()
    {
        Debug.Log("[HomeGalleryPanel] Ǯ �̹��� �ݱ�");

        if (fullImagePanel != null)
        {
            fullImagePanel.SetActive(false);
        }

        // �̹��� ��ȯ ��ư ��Ȱ��ȭ
        if (toggleImageButton != null)
        {
            toggleImageButton.gameObject.SetActive(false);
        }

        // ���� ���� �ִ� CG �ε��� �ʱ�ȭ
        currentViewingCGIndex = -1;
    }

    // �ܺο��� Ư�� CG ���� (�׽�Ʈ��)
    public void UnlockCG(string unlockVariable)
    {
        try
        {
            var customVars = Engine.GetService<ICustomVariableManager>();
            if (customVars != null)
            {
                customVars.SetVariableValue(unlockVariable, new CustomVariableValue(true));
                Debug.Log($"[HomeGalleryPanel] CG ����: {unlockVariable}");
                RefreshGallery(); // ������ ���ΰ�ħ
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeGalleryPanel] CG ���� ����: {ex.Message}");
        }
    }

    // �׽�Ʈ��: ��� CG ����
    [ContextMenu("Test: Unlock All CGs")]
    public void UnlockAllCGs()
    {
        Debug.Log("[HomeGalleryPanel] ��� CG ���� (�׽�Ʈ)");

        foreach (var cgItem in cgItems)
        {
            if (!string.IsNullOrEmpty(cgItem.unlockVariable))
            {
                UnlockCG(cgItem.unlockVariable);
            }
        }
    }

    // �׽�Ʈ��: ��� CG ���
    [ContextMenu("Test: Lock All CGs")]
    public void LockAllCGs()
    {
        Debug.Log("[HomeGalleryPanel] ��� CG ��� (�׽�Ʈ)");

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
            Debug.LogError($"[HomeGalleryPanel] CG ��� ����: {ex.Message}");
        }
    }
}