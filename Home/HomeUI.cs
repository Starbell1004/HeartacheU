using UnityEngine;
using Naninovel.UI;
using Naninovel;

public class HomeUI : CustomUI, ITitleUI
{
    private HomeUIButtonHandler buttonHandler;

    protected override void Awake()
    {
        base.Awake();
        buttonHandler = GetComponent<HomeUIButtonHandler>();
    }

    //  핵심: Show/Hide를 올바르게 구현
    public override void Show()
    {
        base.Show();
        gameObject.SetActive(true);
        Debug.Log("HomeUI 표시됨");
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
        base.Hide();
        Debug.Log("HomeUI 완전히 숨겨짐");
    }

    // ITitleUI 인터페이스 - 나니노벨이 자동으로 호출하는 메서드들
    public void HandleNewGameButtonClicked()
    {
        Debug.Log("ITitleUI.HandleNewGameButtonClicked 호출");
        if (buttonHandler != null)
        {
            buttonHandler.StartGame();
        }
    }

    public void HandleLoadButtonClicked()
    {
        if (buttonHandler != null)
        {
            buttonHandler.LoadGame();
        }
    }

    public void HandleContinueButtonClicked()
    {
        // Continue 기능이 필요하면 구현
    }

    public void HandleQuitButtonClicked()
    {
        if (buttonHandler != null)
        {
            buttonHandler.QuitGame();
        }
    }
}