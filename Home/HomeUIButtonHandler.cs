using UnityEngine;
using Naninovel;
using Naninovel.UI;
using System;
using System.Linq;

public class HomeUIButtonHandler : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private HomeSaveLoadMenu saveLoadMenu; // 기존 saveLoadPanel 대신
    [SerializeField] private HomeGalleryPanel galleryPanel;
    [SerializeField] private HomeSettingsPanel settingsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("New Game 설정")]
    [Tooltip("Services to exclude from state reset when starting a new game.")]
    [SerializeField] private string[] excludeFromReset = new string[0];

    // TitleNewGameButton에서 가져온 필드들
    private string startScriptPath;
    private IScriptPlayer player;
    private IStateManager state;
    private IScriptManager scripts;

    private async void Start()
    {
        // TitleNewGameButton 초기화 로직
        if (Engine.Initialized)
        {
            await InitializeNewGameLogic();
        }
        else
        {
            Engine.OnInitializationFinished += async () => await InitializeNewGameLogic();
        }

        // 기존 초기화도 유지
        if (!Engine.Initialized)
        {
            Engine.OnInitializationFinished += OnEngineInitialized;
        }
    }

    private async UniTask InitializeNewGameLogic()
    {
        scripts = Engine.GetServiceOrErr<IScriptManager>();
        startScriptPath = await ResolveStartScriptPath(scripts);
        player = Engine.GetServiceOrErr<IScriptPlayer>();
        state = Engine.GetServiceOrErr<IStateManager>();
    }

    // === 게임 시작/로드 관련 메서드들 ===

    public async void StartGame()
    {
        try
        {
            Debug.Log("게임 시작 버튼 클릭");

            if (string.IsNullOrEmpty(startScriptPath))
            {
                Engine.Err("Can't start new game: specify start script in scripts configuration.");
                return;
            }

            await PlayTitleNewGame();
            HideHomeUI();

            using (await LoadingScreen.Show())
            {
                await state.ResetState(excludeFromReset,
                    () => player.LoadAndPlay(startScriptPath));
            }

            Debug.Log($"게임 시작 완료: {startScriptPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 시작 실패: {ex.Message}");
        }
    }

    // LoadGame 메서드 - 타이틀에서 사용
    public void LoadGame()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] LoadGame 호출됨");

            if (saveLoadMenu != null)
            {
                saveLoadMenu.PresentationMode = SaveLoadUIPresentationMode.Load;
                saveLoadMenu.Show(); // ← 이 줄이 있는지 확인!
                Debug.Log("Load 모드로 SaveLoadMenu 표시");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu가 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"로드 메뉴 열기 중 오류: {ex.Message}");
        }
    }

    // SaveGame 메서드 - 인게임에서 사용
    public void SaveGame()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] SaveGame 호출됨");

            if (saveLoadMenu != null)
            {
                saveLoadMenu.PresentationMode = SaveLoadUIPresentationMode.Save;
                saveLoadMenu.Show();
                Debug.Log("Save 모드로 SaveLoadMenu 표시");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu가 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"저장 메뉴 열기 중 오류: {ex.Message}");
        }
    }

    // QuickLoad 메서드 - 추가 기능
    public void QuickLoad()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] QuickLoad 호출됨");

            if (saveLoadMenu != null)
            {
                saveLoadMenu.PresentationMode = SaveLoadUIPresentationMode.QuickLoad;
                saveLoadMenu.Show();
                Debug.Log("QuickLoad 모드로 SaveLoadMenu 표시");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu가 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"퀵로드 메뉴 열기 중 오류: {ex.Message}");
        }
    }

    // 통합 SaveLoad 메뉴 - 범용
    public void OpenSaveLoadMenu()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] OpenSaveLoadMenu 호출됨");

            if (saveLoadMenu != null)
            {
                // 현재 게임 상태에 따라 기본 모드 결정
                bool inGame = Engine.GetService<IScriptPlayer>()?.Playing ?? false;
                saveLoadMenu.PresentationMode = inGame ?
                    SaveLoadUIPresentationMode.Save :
                    SaveLoadUIPresentationMode.Load;

                saveLoadMenu.Show();
                Debug.Log("SaveLoadMenu 표시");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu가 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveLoad 메뉴 열기 중 오류: {ex.Message}");
        }
    }

    // === 기타 UI 메서드들 ===

    public void OpenGallery()
    {
        try
        {
            if (galleryPanel != null)
            {
                galleryPanel.Show();
                Debug.Log("갤러리 패널 표시");
            }
            else
            {
                Debug.LogWarning("HomeGalleryPanel이 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"갤러리 패널 열기 중 오류: {ex.Message}");
        }
    }

    public void OpenSettings()
    {
        try
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
                Debug.Log("설정 패널 표시");
            }
            else
            {
                Debug.LogWarning("HomeSettingsPanel이 연결되지 않았습니다!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"설정 패널 열기 중 오류: {ex.Message}");
        }
    }

    public void QuitGame()
    {
        try
        {
            if (quitConfirmPanel != null)
            {
                quitConfirmPanel.SetActive(true);
                Debug.Log("종료 확인 패널 표시");
            }
            else
            {
                ConfirmQuit();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 종료 중 오류: {ex.Message}");
        }
    }

    public void ConfirmQuit()
    {
        try
        {
            Debug.Log("게임 종료");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 종료 중 오류: {ex.Message}");
        }
    }

    public void CancelQuit()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
            Debug.Log("게임 종료 취소");
        }
    }

    // === 내부 헬퍼 메서드들 ===

    private async UniTask PlayTitleNewGame()
    {
        const string label = "OnNewGame";

        var scriptPath = scripts.Configuration.TitleScript;
        if (string.IsNullOrEmpty(scriptPath)) return;
        var script = (Script)await scripts.ScriptLoader.LoadOrErr(scripts.Configuration.TitleScript);
        if (!script.LabelExists(label)) return;

        player.ResetService();
        await player.LoadAndPlayAtLabel(scriptPath, label);
        await UniTask.WaitWhile(() => player.Playing);
    }

    private async UniTask<string> ResolveStartScriptPath(IScriptManager scripts)
    {
        if (!string.IsNullOrEmpty(scripts.Configuration.StartGameScript))
            return scripts.Configuration.StartGameScript;
        if (!Application.isEditor)
            Engine.Warn("Please specify 'Start Game Script' in the scripts configuration. " +
                        "When not specified, Naninovel will pick first available script, " +
                        "which may differ between the editor and build environments.");
        return (await scripts.ScriptLoader.Locate()).OrderBy(p => p).FirstOrDefault();
    }

    private void HideHomeUI()
    {
        try
        {
            Debug.Log("=== Home UI 숨김 시작 ===");

            var homeUIComponent = GetComponent<HomeUI>();
            if (homeUIComponent != null)
            {
                homeUIComponent.Hide();
                Debug.Log("HomeUI Hide() 메서드 호출 완료");
            }

            gameObject.SetActive(false);
            Debug.Log("HomeUI GameObject 비활성화 완료");

        }
        catch (Exception ex)
        {
            Debug.LogError($"Home UI 숨김 실패: {ex.Message}");
        }
    }

    private void OnEngineInitialized()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
        Debug.Log("Naninovel Engine 초기화 완료 - UI 버튼 사용 가능");
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
    }
}