using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomeScreenModeSettings : MonoBehaviour
{
    [Header("화면 모드 설정")]
    [SerializeField] private Button fullscreenButton;
    [SerializeField] private Button windowedButton;

    [Header("스프라이트 설정")]
    [SerializeField] private Sprite fullscreenActiveSprite;   // 전체화면 핑크
    [SerializeField] private Sprite fullscreenInactiveSprite; // 전체화면 회색
    [SerializeField] private Sprite windowActiveSprite;       // 창모드 핑크
    [SerializeField] private Sprite windowInactiveSprite;     // 창모드 회색

    [Header("창모드 크기")]
    [SerializeField] private int windowWidth = 1280;
    [SerializeField] private int windowHeight = 720;

    private Image fullscreenImg;
    private Image windowedImg;
    private bool isChangingMode = false; // 모드 전환 중 플래그
    private VideoLoopMonitor videoMonitor; // 비디오 모니터 추가

    private void Start()
    {
        Debug.Log("[HomeScreenModeSettings] Start 호출");
        InitializeComponents();
        SetupButtons();
        RefreshButtonStates();

        // VideoLoopMonitor 찾기
        videoMonitor = FindObjectOfType<VideoLoopMonitor>();
    }

    private void InitializeComponents()
    {
        if (fullscreenButton != null)
            fullscreenImg = fullscreenButton.GetComponent<Image>();
        if (windowedButton != null)
            windowedImg = windowedButton.GetComponent<Image>();

        if (fullscreenImg == null || windowedImg == null)
        {
            Debug.LogError("[HomeScreenModeSettings] 버튼 Image 컴포넌트를 찾을 수 없습니다!");
        }
    }

    private void SetupButtons()
    {
        Debug.Log("[HomeScreenModeSettings] 버튼 설정 중...");

        if (fullscreenButton != null)
        {
            fullscreenButton.onClick.RemoveAllListeners();
            fullscreenButton.onClick.AddListener(SetFullscreen);
            Debug.Log("[HomeScreenModeSettings] 전체화면 버튼 이벤트 등록");
        }
        else
        {
            Debug.LogError("[HomeScreenModeSettings] fullscreenButton이 null!");
        }

        if (windowedButton != null)
        {
            windowedButton.onClick.RemoveAllListeners();
            windowedButton.onClick.AddListener(SetWindowed);
            Debug.Log("[HomeScreenModeSettings] 창모드 버튼 이벤트 등록");
        }
        else
        {
            Debug.LogError("[HomeScreenModeSettings] windowedButton이 null!");
        }
    }

    public void SetFullscreen()
    {
        if (isChangingMode) return; // 모드 전환 중이면 무시

        Debug.Log("[HomeScreenModeSettings] 전체화면 설정");
        isChangingMode = true;

        // 즉시 UI 업데이트 (예상되는 상태로)
        UpdateButtonSprites(true);

        // 화면 모드 변경
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);

        // 영상 재시작
        if (videoMonitor != null)
            videoMonitor.ForceRestart();

        // 약간의 딜레이 후 확인
        StartCoroutine(VerifyModeChange(true));
    }

    public void SetWindowed()
    {
        if (isChangingMode) return; // 모드 전환 중이면 무시

        Debug.Log($"[HomeScreenModeSettings] 창모드 설정 ({windowWidth}x{windowHeight})");
        isChangingMode = true;

        // 즉시 UI 업데이트 (예상되는 상태로)
        UpdateButtonSprites(false);

        // 화면 모드 변경
        Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);

        // 영상 재시작
        if (videoMonitor != null)
            videoMonitor.ForceRestart();

        // 약간의 딜레이 후 확인
        StartCoroutine(VerifyModeChange(false));
    }

    private IEnumerator VerifyModeChange(bool expectedFullscreen)
    {
        // 화면 모드 변경이 완료될 때까지 대기
        yield return new WaitForSeconds(0.5f);

        // 실제 상태 확인 및 UI 업데이트
        RefreshButtonStates();
        isChangingMode = false;
    }

    private void UpdateButtonSprites(bool isFullscreen)
    {
        if (fullscreenImg == null || windowedImg == null) return;

        if (isFullscreen)
        {
            fullscreenImg.sprite = fullscreenActiveSprite;
            windowedImg.sprite = windowInactiveSprite;
        }
        else
        {
            fullscreenImg.sprite = fullscreenInactiveSprite;
            windowedImg.sprite = windowActiveSprite;
        }
    }

    public void RefreshButtonStates()
    {
        Debug.Log("[HomeScreenModeSettings] 버튼 상태 새로고침");

        if (fullscreenImg == null || windowedImg == null)
        {
            Debug.LogError("[HomeScreenModeSettings] Image 컴포넌트가 null입니다!");
            return;
        }

        bool isFullscreen = Screen.fullScreenMode == FullScreenMode.FullScreenWindow ||
                           Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ||
                           Screen.fullScreenMode == FullScreenMode.MaximizedWindow;

        UpdateButtonSprites(isFullscreen);

        Debug.Log($"[HomeScreenModeSettings] 현재 모드: {(isFullscreen ? "전체화면" : "창모드")}");
    }

    // 외부에서 호출용
    public void RefreshSettings()
    {
        RefreshButtonStates();
    }

    private void OnDestroy()
    {
        if (fullscreenButton != null)
            fullscreenButton.onClick.RemoveAllListeners();

        if (windowedButton != null)
            windowedButton.onClick.RemoveAllListeners();
    }
}