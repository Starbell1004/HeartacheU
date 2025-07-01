using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class ForcePauseUI : MonoBehaviour
{
    private IUIManager uiManager;
    private IPauseUI pauseUI;
    private bool initialized = false;

    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    System.Collections.IEnumerator DelayedInit()
    {
        // 엔진이 완전히 초기화될 때까지 대기
        yield return new WaitForSeconds(1f);

        uiManager = Engine.GetService<IUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[ForcePauseUI] UIManager를 찾을 수 없음!");
            yield break;
        }

        pauseUI = uiManager.GetUI<IPauseUI>();
        if (pauseUI == null)
        {
            Debug.LogError("[ForcePauseUI] PauseUI를 찾을 수 없음!");
        }
        else
        {
            initialized = true;
        }
    }

    void Update()
    {
        if (!initialized || pauseUI == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseUI.Visible)
                pauseUI.Hide();
            else
                pauseUI.Show();
        }
    }
}