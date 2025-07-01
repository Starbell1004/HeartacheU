using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeBGMVolumeSettings : MonoBehaviour
{
    [Header("BGM 볼륨 설정")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    private IAudioManager audioManager;

    private void Awake()
    {
        Debug.Log("[HomeBGMVolumeSettings] Awake 호출");

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
        Debug.Log("[HomeBGMVolumeSettings] Start 호출");
        SetupVolumeSlider();
    }

    private void InitializeServices()
    {
        try
        {
            audioManager = Engine.GetService<IAudioManager>();
            Debug.Log("[HomeBGMVolumeSettings] 나니노벨 오디오 서비스 초기화 완료");

            // 서비스 초기화 후 슬라이더 값 설정
            RefreshVolumeFromNaninovel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeBGMVolumeSettings] 서비스 초기화 오류: {ex.Message}");
        }
    }

    private void SetupVolumeSlider()
    {
        Debug.Log("[HomeBGMVolumeSettings] SetupVolumeSlider 시작");

        if (volumeSlider != null)
        {
            Debug.Log("[HomeBGMVolumeSettings] BGM 슬라이더 설정 중...");
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // 이벤트 리스너 등록
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            Debug.Log("[HomeBGMVolumeSettings] BGM 슬라이더 이벤트 등록 완료");

            // 초기값 설정
            RefreshVolumeFromNaninovel();
        }
        else
        {
            Debug.LogError("[HomeBGMVolumeSettings] volumeSlider가 null입니다!");
        }
    }

    private void RefreshVolumeFromNaninovel()
    {
        if (volumeSlider == null) return;

        float currentVolume = 1f;

        if (audioManager != null)
        {
            currentVolume = audioManager.BgmVolume;
            Debug.Log($"[HomeBGMVolumeSettings] 나니노벨에서 BGM 볼륨 로드: {currentVolume}");
        }
        else
        {
            currentVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            Debug.Log($"[HomeBGMVolumeSettings] PlayerPrefs에서 BGM 볼륨 로드: {currentVolume}");
        }

        volumeSlider.value = currentVolume;
        UpdateVolumeDisplay(currentVolume);
    }

    private void OnVolumeChanged(float volume)
    {
        Debug.Log($"[HomeBGMVolumeSettings] BGM 볼륨 변경됨: {volume}");

        // 나니노벨에 적용
        if (audioManager != null)
        {
            audioManager.BgmVolume = volume;
            Debug.Log($"[HomeBGMVolumeSettings] 나니노벨 BGM 볼륨 설정: {volume}");
        }
        else
        {
            Debug.LogWarning("[HomeBGMVolumeSettings] audioManager가 null! PlayerPrefs에 저장");
            PlayerPrefs.SetFloat("BGMVolume", volume);
        }

        // UI 업데이트
        UpdateVolumeDisplay(volume);
    }

    private void UpdateVolumeDisplay(float volume)
    {
        Debug.Log($"[HomeBGMVolumeSettings] BGM 볼륨 텍스트 업데이트: {volume:F1}");

        if (volumeText != null)
        {
            volumeText.text = $"{volume:F1}";
            Debug.Log($"[HomeBGMVolumeSettings] BGM 텍스트 설정 완료: {volumeText.text}");
        }
        else
        {
            Debug.LogError("[HomeBGMVolumeSettings] volumeText가 null!");
        }
    }

    // 외부에서 호출용
    public void RefreshSettings()
    {
        RefreshVolumeFromNaninovel();
    }

    // 테스트용 함수
    [ContextMenu("Test Volume Change")]
    public void TestVolumeChange()
    {
        Debug.Log("[HomeBGMVolumeSettings] 테스트: BGM 볼륨을 0.5로 설정");
        if (volumeSlider != null)
        {
            volumeSlider.value = 0.5f;
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
        }
    }
}