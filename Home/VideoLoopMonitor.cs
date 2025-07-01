using UnityEngine;
using UnityEngine.Video;
using System.Collections;

[RequireComponent(typeof(VideoPlayer))]
public class VideoLoopMonitor : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private bool wasPlaying = false;
    private float lastPlayTime = 0f;
    private float stuckCheckInterval = 1f; // 1초마다 체크
    private double lastFrameTime = 0;

    [Header("모니터링 설정")]
    [SerializeField] private bool enableMonitoring = true;
    [SerializeField] private float restartDelay = 0.5f; // 재시작 전 대기 시간

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // 기본 루프 설정 확인
        if (!videoPlayer.isLooping)
        {
            Debug.Log("[VideoLoopMonitor] 비디오 루프 설정 활성화");
            videoPlayer.isLooping = true;
        }

        // 이벤트 리스너 추가
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void Start()
    {
        if (enableMonitoring)
        {
            StartCoroutine(MonitorVideoPlayback());
        }
    }

    private IEnumerator MonitorVideoPlayback()
    {
        while (true)
        {
            yield return new WaitForSeconds(stuckCheckInterval);

            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                // 프레임이 진행되고 있는지 체크
                double currentTime = videoPlayer.time;

                if (currentTime == lastFrameTime && currentTime > 0)
                {
                    Debug.LogWarning($"[VideoLoopMonitor] 영상이 멈춘 것 같습니다. 현재 시간: {currentTime}");
                    RestartVideo();
                }

                lastFrameTime = currentTime;
            }
            else if (videoPlayer != null && !videoPlayer.isPlaying && videoPlayer.isPrepared)
            {
                // 준비되었는데 재생 중이 아닌 경우
                Debug.LogWarning("[VideoLoopMonitor] 영상이 재생되지 않고 있습니다. 재시작합니다.");
                RestartVideo();
            }
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("[VideoLoopMonitor] 영상 끝 도달 - 루프 재생");
        // isLooping이 true면 자동으로 재시작되지만, 혹시 모르니 체크
        if (!vp.isPlaying)
        {
            StartCoroutine(DelayedRestart());
        }
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[VideoLoopMonitor] 비디오 에러: {message}");
        StartCoroutine(DelayedRestart());
    }

    private void RestartVideo()
    {
        StartCoroutine(DelayedRestart());
    }

    private IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(restartDelay);

        if (videoPlayer != null)
        {
            Debug.Log("[VideoLoopMonitor] 영상 재시작");
            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    // 화면 모드 변경이나 포커스 변경 시 호출
    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"[VideoLoopMonitor] 포커스 변경: {hasFocus}");

        if (hasFocus)
        {
            // 포커스를 다시 얻었을 때 강제 재시작
            StartCoroutine(EnsureVideoPlaying());
        }
        else
        {
            // 포커스를 잃었을 때 상태 저장
            wasPlaying = videoPlayer != null && videoPlayer.isPlaying;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[VideoLoopMonitor] 일시정지 상태: {pauseStatus}");

        if (!pauseStatus)
        {
            StartCoroutine(EnsureVideoPlaying());
        }
    }

    private IEnumerator EnsureVideoPlaying()
    {
        yield return new WaitForSeconds(0.2f); // 약간의 딜레이

        if (videoPlayer != null)
        {
            if (!videoPlayer.isPlaying)
            {
                Debug.Log("[VideoLoopMonitor] 영상 재생 강제 시작");
                videoPlayer.Play();
            }

            // 그래도 안 되면 재시작
            yield return new WaitForSeconds(0.5f);
            if (!videoPlayer.isPlaying)
            {
                Debug.LogWarning("[VideoLoopMonitor] 영상 여전히 멈춤 - 완전 재시작");
                RestartVideo();
            }
        }
    }

    // 수동으로 재시작이 필요한 경우 호출
    public void ForceRestart()
    {
        Debug.Log("[VideoLoopMonitor] 강제 재시작 요청");
        RestartVideo();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }
}