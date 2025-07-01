using UnityEngine;
using Naninovel;

public class EscButton : MonoBehaviour
{
    private IUIManager uiManager;

    void Start()
    {
        // UI 매니저 가져오기
        uiManager = Engine.GetService<IUIManager>();
    }

    void Update()
    {
        // ESC 키 입력 시 PauseUITest UI 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseUI();
        }
    }

    public void OnButtonClick()
    {
        // 버튼 클릭 시 ESC 효과 실행
        TogglePauseUI();
    }

    void TogglePauseUI()
    {
        var pauseUI = uiManager.GetUI("PauseUITest"); // UI 이름을 "PauseUITest"로 변경
        if (pauseUI != null)
        {
            pauseUI.Visible = !pauseUI.Visible; // 현재 상태 반전
        }
    }
}

