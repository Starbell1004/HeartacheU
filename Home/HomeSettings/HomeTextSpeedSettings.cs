using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeTextSpeedSettings : MonoBehaviour
{
    [Header("텍스트 속도 설정")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedDisplayText;
    [SerializeField] private HomeTextSpeedPreview textPreview;

    private ITextPrinterManager printerManager;

    private void Awake()
    {
        Debug.Log("[HomeTextSpeedSettings] Awake 호출");

        // 나니노벨 서비스 초기화
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void Start()
    {
        Debug.Log("[HomeTextSpeedSettings] Start 호출");
        SetupTextSpeed();
    }

    private void InitializeServices()
    {
        try
        {
            printerManager = Engine.GetService<ITextPrinterManager>();
            Debug.Log("[HomeTextSpeedSettings] 나니노벨 서비스 초기화 완료");

            // 서비스 초기화 후 슬라이더 값 설정
            RefreshSpeedFromNaninovel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedSettings] 서비스 초기화 오류: {ex.Message}");
        }
    }

    private void SetupTextSpeed()
    {
        Debug.Log("[HomeTextSpeedSettings] SetupTextSpeed 시작");

        if (speedSlider != null)
        {
            Debug.Log("[HomeTextSpeedSettings] 슬라이더 설정 중...");
            speedSlider.minValue = 0.5f;
            speedSlider.maxValue = 1.5f;

            // 이벤트 리스너 등록
            speedSlider.onValueChanged.RemoveAllListeners(); // 중복 방지
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);

            Debug.Log("[HomeTextSpeedSettings] 슬라이더 이벤트 등록 완료");

            // 초기값 설정
            RefreshSpeedFromNaninovel();
        }
        else
        {
            Debug.LogError("[HomeTextSpeedSettings] speedSlider가 null입니다!");
        }

        Debug.Log($"[HomeTextSpeedSettings] textPreview 연결 상태: {(textPreview != null ? "연결됨" : "null")}");

        // ★ 미리보기 활성화 (이게 빠져있었어요!)
        if (textPreview != null)
        {
            Debug.Log("[HomeTextSpeedSettings] 미리보기 활성화 시작");
            textPreview.Show();
        }
    }

    private void RefreshSpeedFromNaninovel()
    {
        if (speedSlider == null) return;

        float currentSpeed = 1f;

        if (printerManager != null)
        {
            currentSpeed = printerManager.BaseRevealSpeed;
            Debug.Log($"[HomeTextSpeedSettings] 나니노벨에서 속도 로드: {currentSpeed}");
        }
        else
        {
            currentSpeed = PlayerPrefs.GetFloat("TextSpeed", 1f);
            Debug.Log($"[HomeTextSpeedSettings] PlayerPrefs에서 속도 로드: {currentSpeed}");
        }

        speedSlider.value = currentSpeed;
        UpdateSpeedDisplay(currentSpeed);
    }

    private void OnSpeedChanged(float speed)
    {
        Debug.Log($"[HomeTextSpeedSettings] 속도 변경됨: {speed}");

        // 나니노벨에 적용
        if (printerManager != null)
        {
            printerManager.BaseRevealSpeed = speed;
            Debug.Log($"[HomeTextSpeedSettings] 나니노벨 속도 설정: {speed}");
        }
        else
        {
            Debug.LogWarning("[HomeTextSpeedSettings] printerManager가 null! PlayerPrefs에 저장");
            PlayerPrefs.SetFloat("TextSpeed", speed);
        }

        // UI 업데이트
        UpdateSpeedDisplay(speed);

        // 미리보기 시작
        if (textPreview != null)
        {
            Debug.Log("[HomeTextSpeedSettings] 미리보기 시작");
            textPreview.StartPreview();
        }
        else
        {
            Debug.LogWarning("[HomeTextSpeedSettings] textPreview가 null!");
        }
    }

    private void UpdateSpeedDisplay(float speed)
    {
        Debug.Log($"[HomeTextSpeedSettings] 속도 텍스트 업데이트: {speed:F1}x");

        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"{speed:F1}x";
            Debug.Log($"[HomeTextSpeedSettings] 텍스트 설정 완료: {speedDisplayText.text}");
        }
        else
        {
            Debug.LogError("[HomeTextSpeedSettings] speedDisplayText가 null!");
        }
    }

    // 외부에서 호출용
    public void ShowPreview()
    {
        if (textPreview != null)
        {
            textPreview.Show();
        }
    }

    public void HidePreview()
    {
        if (textPreview != null)
        {
            textPreview.Hide();
        }
    }

    public void RefreshSettings()
    {
        RefreshSpeedFromNaninovel();
    }

    // 테스트용 함수
    [ContextMenu("Test Speed Change")]
    public void TestSpeedChange()
    {
        Debug.Log("[HomeTextSpeedSettings] 테스트: 속도를 1.5로 설정");
        if (speedSlider != null)
        {
            speedSlider.value = 1.5f;
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
        }
    }
}