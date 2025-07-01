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

    //  �ٽ�: Show/Hide�� �ùٸ��� ����
    public override void Show()
    {
        base.Show();
        gameObject.SetActive(true);
        Debug.Log("HomeUI ǥ�õ�");
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
        base.Hide();
        Debug.Log("HomeUI ������ ������");
    }

    // ITitleUI �������̽� - ���ϳ뺧�� �ڵ����� ȣ���ϴ� �޼����
    public void HandleNewGameButtonClicked()
    {
        Debug.Log("ITitleUI.HandleNewGameButtonClicked ȣ��");
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
        // Continue ����� �ʿ��ϸ� ����
    }

    public void HandleQuitButtonClicked()
    {
        if (buttonHandler != null)
        {
            buttonHandler.QuitGame();
        }
    }
}