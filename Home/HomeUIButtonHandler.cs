using UnityEngine;
using Naninovel;
using Naninovel.UI;
using System;
using System.Linq;

public class HomeUIButtonHandler : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    [SerializeField] private HomeSaveLoadMenu saveLoadMenu; // ���� saveLoadPanel ���
    [SerializeField] private HomeGalleryPanel galleryPanel;
    [SerializeField] private HomeSettingsPanel settingsPanel;
    [SerializeField] private GameObject quitConfirmPanel;

    [Header("New Game ����")]
    [Tooltip("Services to exclude from state reset when starting a new game.")]
    [SerializeField] private string[] excludeFromReset = new string[0];

    // TitleNewGameButton���� ������ �ʵ��
    private string startScriptPath;
    private IScriptPlayer player;
    private IStateManager state;
    private IScriptManager scripts;

    private async void Start()
    {
        // TitleNewGameButton �ʱ�ȭ ����
        if (Engine.Initialized)
        {
            await InitializeNewGameLogic();
        }
        else
        {
            Engine.OnInitializationFinished += async () => await InitializeNewGameLogic();
        }

        // ���� �ʱ�ȭ�� ����
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

    // === ���� ����/�ε� ���� �޼���� ===

    public async void StartGame()
    {
        try
        {
            Debug.Log("���� ���� ��ư Ŭ��");

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

            Debug.Log($"���� ���� �Ϸ�: {startScriptPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� ����: {ex.Message}");
        }
    }

    // LoadGame �޼��� - Ÿ��Ʋ���� ���
    public void LoadGame()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] LoadGame ȣ���");

            if (saveLoadMenu != null)
            {
                saveLoadMenu.PresentationMode = SaveLoadUIPresentationMode.Load;
                saveLoadMenu.Show(); // �� �� ���� �ִ��� Ȯ��!
                Debug.Log("Load ���� SaveLoadMenu ǥ��");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"�ε� �޴� ���� �� ����: {ex.Message}");
        }
    }

    // SaveGame �޼��� - �ΰ��ӿ��� ���
    public void SaveGame()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] SaveGame ȣ���");

            if (saveLoadMenu != null)
            {
                saveLoadMenu.PresentationMode = SaveLoadUIPresentationMode.Save;
                saveLoadMenu.Show();
                Debug.Log("Save ���� SaveLoadMenu ǥ��");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �޴� ���� �� ����: {ex.Message}");
        }
    }

    // QuickLoad �޼��� - �߰� ���
    public void QuickLoad()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] QuickLoad ȣ���");

            if (saveLoadMenu != null)
            {
                saveLoadMenu.PresentationMode = SaveLoadUIPresentationMode.QuickLoad;
                saveLoadMenu.Show();
                Debug.Log("QuickLoad ���� SaveLoadMenu ǥ��");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���ε� �޴� ���� �� ����: {ex.Message}");
        }
    }

    // ���� SaveLoad �޴� - ����
    public void OpenSaveLoadMenu()
    {
        try
        {
            Debug.Log("[HomeUIButtonHandler] OpenSaveLoadMenu ȣ���");

            if (saveLoadMenu != null)
            {
                // ���� ���� ���¿� ���� �⺻ ��� ����
                bool inGame = Engine.GetService<IScriptPlayer>()?.Playing ?? false;
                saveLoadMenu.PresentationMode = inGame ?
                    SaveLoadUIPresentationMode.Save :
                    SaveLoadUIPresentationMode.Load;

                saveLoadMenu.Show();
                Debug.Log("SaveLoadMenu ǥ��");
            }
            else
            {
                Debug.LogError("HomeSaveLoadMenu�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveLoad �޴� ���� �� ����: {ex.Message}");
        }
    }

    // === ��Ÿ UI �޼���� ===

    public void OpenGallery()
    {
        try
        {
            if (galleryPanel != null)
            {
                galleryPanel.Show();
                Debug.Log("������ �г� ǥ��");
            }
            else
            {
                Debug.LogWarning("HomeGalleryPanel�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"������ �г� ���� �� ����: {ex.Message}");
        }
    }

    public void OpenSettings()
    {
        try
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
                Debug.Log("���� �г� ǥ��");
            }
            else
            {
                Debug.LogWarning("HomeSettingsPanel�� ������� �ʾҽ��ϴ�!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� �г� ���� �� ����: {ex.Message}");
        }
    }

    public void QuitGame()
    {
        try
        {
            if (quitConfirmPanel != null)
            {
                quitConfirmPanel.SetActive(true);
                Debug.Log("���� Ȯ�� �г� ǥ��");
            }
            else
            {
                ConfirmQuit();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ����: {ex.Message}");
        }
    }

    public void ConfirmQuit()
    {
        try
        {
            Debug.Log("���� ����");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ����: {ex.Message}");
        }
    }

    public void CancelQuit()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
            Debug.Log("���� ���� ���");
        }
    }

    // === ���� ���� �޼���� ===

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
            Debug.Log("=== Home UI ���� ���� ===");

            var homeUIComponent = GetComponent<HomeUI>();
            if (homeUIComponent != null)
            {
                homeUIComponent.Hide();
                Debug.Log("HomeUI Hide() �޼��� ȣ�� �Ϸ�");
            }

            gameObject.SetActive(false);
            Debug.Log("HomeUI GameObject ��Ȱ��ȭ �Ϸ�");

        }
        catch (Exception ex)
        {
            Debug.LogError($"Home UI ���� ����: {ex.Message}");
        }
    }

    private void OnEngineInitialized()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
        Debug.Log("Naninovel Engine �ʱ�ȭ �Ϸ� - UI ��ư ��� ����");
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= OnEngineInitialized;
    }
}