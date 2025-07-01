using UnityEngine;
using Naninovel;
using Naninovel.Commands;

public class HomeUIBGMController : MonoBehaviour
{
    [Header("BGM 설정")]
    [SerializeField] private string bgmPath = "MainTheme"; // Naninovel BGM 리소스 경로
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float defaultVolume = 1f;
    [SerializeField] private bool loop = true;

    private IAudioManager audioManager;
    private bool isPlaying = false;

    private void Start()
    {
        // 기존 AudioSource가 있다면 비활성화
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.enabled = false;
            Debug.Log("[HomeUIBGMController] 기존 AudioSource 비활성화");
        }

        // Naninovel 초기화 대기
        if (Engine.Initialized)
        {
            InitializeBGM();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeBGM;
        }
    }

    private void InitializeBGM()
    {
        audioManager = Engine.GetService<IAudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("[HomeUIBGMController] AudioManager를 찾을 수 없음!");
            return;
        }

        PlayBGM();
    }

    private async void PlayBGM()
    {
        if (isPlaying) return;

        try
        {
            Debug.Log($"[HomeUIBGMController] BGM 재생 시작: {bgmPath}");

            // Naninovel의 @bgm 명령어와 동일한 방식으로 재생
            var playBgm = new PlayBgm
            {
                BgmPath = bgmPath,
                Volume = defaultVolume,
                Loop = loop,
                FadeInDuration = fadeInDuration, // Fade 대신 FadeInDuration
                Wait = false
            };

            await playBgm.Execute();
            isPlaying = true;

            Debug.Log($"[HomeUIBGMController] BGM 재생 중: {bgmPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HomeUIBGMController] BGM 재생 실패: {e.Message}");
        }
    }

    public async void StopBGM()
    {
        if (!isPlaying) return;

        try
        {
            Debug.Log("[HomeUIBGMController] BGM 정지");

            // Naninovel의 @stopBgm 명령어와 동일한 방식으로 정지
            var stopBgm = new StopBgm
            {
                FadeOutDuration = fadeInDuration, // Fade 대신 FadeOutDuration
                Wait = false
            };

            await stopBgm.Execute();
            isPlaying = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HomeUIBGMController] BGM 정지 실패: {e.Message}");
        }
    }

    // HomeUI가 숨겨질 때 BGM 정지
    private void OnDisable()
    {
        if (isPlaying)
        {
            StopBGM();
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeBGM;
    }
}