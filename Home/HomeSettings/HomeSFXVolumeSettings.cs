using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeSFXVolumeSettings : MonoBehaviour
{
    [Header("SFX 볼륨 설정")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Button testSfxButton; // 효과음 테스트 버튼

    [Header("테스트 효과음 (AudioClip 직접 지정)")]
    [SerializeField] private AudioClip testAudioClip; // AudioClip 직접 할당

    private IAudioManager audioManager;

    private void Awake()
    {
        Debug.Log("[HomeSFXVolumeSettings] Awake 호출");

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
        Debug.Log("[HomeSFXVolumeSettings] Start 호출");
        SetupVolumeSlider();
    }

    private void InitializeServices()
    {
        try
        {
            audioManager = Engine.GetService<IAudioManager>();
            Debug.Log("[HomeSFXVolumeSettings] 나니노벨 오디오 서비스 초기화 완료");

            // 서비스 초기화 후 슬라이더 값 설정 (약간 지연 후 실행)
            Invoke(nameof(RefreshVolumeFromNaninovel), 0.1f);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSFXVolumeSettings] 서비스 초기화 오류: {ex.Message}");
            // 오류 시에도 기본값으로 초기화
            Invoke(nameof(RefreshVolumeFromNaninovel), 0.1f);
        }
    }

    private void SetupVolumeSlider()
    {
        Debug.Log("[HomeSFXVolumeSettings] SetupVolumeSlider 시작");

        if (volumeSlider != null)
        {
            Debug.Log("[HomeSFXVolumeSettings] SFX 슬라이더 설정 중...");
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // 이벤트 리스너 등록
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            Debug.Log("[HomeSFXVolumeSettings] SFX 슬라이더 이벤트 등록 완료");

            // 초기값 설정 (지연 실행)
            Invoke(nameof(RefreshVolumeFromNaninovel), 0.2f);
        }
        else
        {
            Debug.LogError("[HomeSFXVolumeSettings] volumeSlider가 null입니다!");
        }

        // 테스트 버튼 설정
        SetupTestButton();
    }

    private void SetupTestButton()
    {
        if (testSfxButton != null)
        {
            Debug.Log("[HomeSFXVolumeSettings] 테스트 버튼 설정 중...");
            testSfxButton.onClick.RemoveAllListeners();
            testSfxButton.onClick.AddListener(PlayTestSfx);
            Debug.Log("[HomeSFXVolumeSettings] 테스트 버튼 이벤트 등록 완료");
        }
        else
        {
            Debug.LogWarning("[HomeSFXVolumeSettings] testSfxButton이 null입니다!");
        }
    }

    private void RefreshVolumeFromNaninovel()
    {
        if (volumeSlider == null)
        {
            Debug.LogError("[HomeSFXVolumeSettings] volumeSlider가 null! RefreshVolumeFromNaninovel 중단");
            return;
        }

        float currentVolume = 1f;

        if (audioManager != null)
        {
            currentVolume = audioManager.SfxVolume;
            Debug.Log($"[HomeSFXVolumeSettings] 나니노벨에서 SFX 볼륨 로드: {currentVolume}");
        }
        else
        {
            currentVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            Debug.Log($"[HomeSFXVolumeSettings] PlayerPrefs에서 SFX 볼륨 로드: {currentVolume}");
        }

        // 슬라이더 값 설정 (이벤트 발생 방지를 위해 임시로 리스너 제거)
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.value = currentVolume;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        UpdateVolumeDisplay(currentVolume);

        Debug.Log($"[HomeSFXVolumeSettings] 볼륨 초기화 완료 - 슬라이더: {volumeSlider.value}, 텍스트: {(volumeText != null ? volumeText.text : "null")}");
    }

    private void OnVolumeChanged(float volume)
    {
        Debug.Log($"[HomeSFXVolumeSettings] SFX 볼륨 변경됨: {volume}");

        // 나니노벨에 적용
        if (audioManager != null)
        {
            audioManager.SfxVolume = volume;
            Debug.Log($"[HomeSFXVolumeSettings] 나니노벨 SFX 볼륨 설정: {volume}");
        }
        else
        {
            Debug.LogWarning("[HomeSFXVolumeSettings] audioManager가 null! PlayerPrefs에 저장");
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }

        // UI 업데이트
        UpdateVolumeDisplay(volume);
    }

    private void UpdateVolumeDisplay(float volume)
    {
        Debug.Log($"[HomeSFXVolumeSettings] SFX 볼륨 텍스트 업데이트: {volume:F1}");

        if (volumeText != null)
        {
            volumeText.text = $"{volume:F1}";
            Debug.Log($"[HomeSFXVolumeSettings] SFX 텍스트 설정 완료: {volumeText.text}");
        }
        else
        {
            Debug.LogError("[HomeSFXVolumeSettings] volumeText가 null!");
        }
    }

    private void PlayTestSfx()
    {
        Debug.Log("[HomeSFXVolumeSettings] 테스트 효과음 재생");

        try
        {
            if (testAudioClip != null)
            {
                // AudioClip 직접 재생
                PlayAudioClipDirectly();
            }
            else
            {
                Debug.LogWarning("[HomeSFXVolumeSettings] testAudioClip이 설정되지 않았습니다! 기본 톤 재생");
                PlayDefaultTestSound();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeSFXVolumeSettings] 테스트 효과음 재생 오류: {ex.Message}");
            PlayDefaultTestSound();
        }
    }

    private void PlayAudioClipDirectly()
    {
        if (testAudioClip != null)
        {
            // AudioSource를 임시로 생성해서 재생
            GameObject tempObj = new GameObject("TempSFXPlayer");
            AudioSource tempSource = tempObj.AddComponent<AudioSource>();

            tempSource.clip = testAudioClip;
            tempSource.volume = volumeSlider != null ? volumeSlider.value : 0.5f;
            tempSource.Play();

            Debug.Log($"[HomeSFXVolumeSettings] AudioClip 직접 재생: {testAudioClip.name}, 볼륨: {tempSource.volume}");

            // 재생 완료 후 삭제
            Destroy(tempObj, testAudioClip.length + 0.1f);
        }
        else
        {
            Debug.LogWarning("[HomeSFXVolumeSettings] testAudioClip이 null입니다!");
            PlayDefaultTestSound();
        }
    }

    private void PlayDefaultTestSound()
    {
        // 기본 테스트 사운드 (440Hz 톤)
        Debug.Log("[HomeSFXVolumeSettings] 기본 테스트 사운드 재생");

        GameObject tempObj = new GameObject("DefaultTestSound");
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();

        // 440Hz 톤 생성 (라 음)
        int sampleRate = 44100;
        float frequency = 440f;
        float duration = 0.3f;
        int samples = Mathf.RoundToInt(sampleRate * duration);

        AudioClip clip = AudioClip.Create("TestTone", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.3f;
        }

        clip.SetData(data, 0);
        tempSource.clip = clip;
        tempSource.volume = volumeSlider != null ? volumeSlider.value : 0.5f;
        tempSource.Play();

        Debug.Log($"[HomeSFXVolumeSettings] 프로그래밍 방식 테스트 톤 재생, 볼륨: {tempSource.volume}");

        // 재생 완료 후 삭제
        Destroy(tempObj, duration + 0.1f);
    }

    // 외부에서 호출용
    public void RefreshSettings()
    {
        Debug.Log("[HomeSFXVolumeSettings] RefreshSettings 호출");
        RefreshVolumeFromNaninovel();
    }

    // 테스트용 함수
    [ContextMenu("Test Volume Change")]
    public void TestVolumeChange()
    {
        Debug.Log("[HomeSFXVolumeSettings] 테스트: SFX 볼륨을 0.5로 설정");
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

        if (testSfxButton != null)
        {
            testSfxButton.onClick.RemoveAllListeners();
        }
    }
}