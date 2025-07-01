using System.Threading;
using UnityEngine;
using TMPro;
using Naninovel;
using Naninovel.UI;

public class HomeTextSpeedPreview : MonoBehaviour
{
    [Header("�̸����� ����")]
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField]
    private string[] previewMessages = {
        "�ؽ�Ʈ ��� �ӵ��� �̸� Ȯ���غ�����.",
        "�� �ӵ��� ���� ��ȭ�� ��µ˴ϴ�.",
        "�����̴��� �����Ͽ� ���ϴ� �ӵ��� �����ϼ���.",
        "���� �ӵ��� �����ϸ� ��ȭ�� ������ ���� �� �־��."
    };

    private ITextPrinterManager printerManager;
    private CancellationTokenSource revealCTS;
    private int currentMessageIndex = 0;
    private bool isActive = false;

    // ����� �α� ����
    [Header("����� ����")]
    [SerializeField] private bool enableDebugLogs = false;

    private void Awake()
    {
        // ���ϳ뺧 ���� ��������
        if (Engine.Initialized)
        {
            InitializeServices();
        }
        else
        {
            Engine.OnInitializationFinished += InitializeServices;
        }
    }

    private void InitializeServices()
    {
        try
        {
            printerManager = Engine.GetService<ITextPrinterManager>();
            if (enableDebugLogs)
                Debug.Log("[HomeTextSpeedPreview] ���ϳ뺧 ���� �ʱ�ȭ �Ϸ�");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedPreview] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    public void Show()
    {
        if (enableDebugLogs)
            Debug.Log("[HomeTextSpeedPreview] Show ȣ��");
        isActive = true;
        StartPreview();
    }

    public void Hide()
    {
        if (enableDebugLogs)
            Debug.Log("[HomeTextSpeedPreview] Hide ȣ��");
        isActive = false;
        StopPreview();
    }

    public void StartPreview()
    {
        if (!isActive || previewText == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[HomeTextSpeedPreview] StartPreview ���� ����!");
            return;
        }

        // ���� �ִϸ��̼� ����
        StopPreview();

        // ���� �޽��� ��������
        string message = GetCurrentMessage();

        // �ؽ�Ʈ ����
        previewText.text = "";
        previewText.maxVisibleCharacters = 0;

        // ���ο� �ִϸ��̼� ����
        revealCTS = new CancellationTokenSource();
        RevealTextAsync(message, revealCTS.Token).Forget();
    }

    private void StopPreview()
    {
        revealCTS?.Cancel();
        revealCTS?.Dispose();
        revealCTS = null;
    }

    private string GetCurrentMessage()
    {
        if (previewMessages == null || previewMessages.Length == 0)
        {
            return "�ؽ�Ʈ �ӵ� �̸������Դϴ�.";
        }

        string message = previewMessages[currentMessageIndex];
        currentMessageIndex = (currentMessageIndex + 1) % previewMessages.Length;

        return message;
    }

    private async UniTask RevealTextAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (previewText == null)
            {
                Debug.LogError("[HomeTextSpeedPreview] previewText�� null�Դϴ�!");
                return;
            }

            // �ؽ�Ʈ ����
            previewText.text = message;
            previewText.maxVisibleCharacters = 0;

            // ���� �ؽ�Ʈ �ӵ� ��������
            float revealSpeed = 1f;
            if (printerManager != null)
            {
                revealSpeed = printerManager.BaseRevealSpeed;
            }

            // �ӵ��� ���� ������ ��� (���ϳ뺧 ���)
            float revealDelay = Mathf.Lerp(0.5f, 0.01f, revealSpeed);

            // ��� ��� (�ӵ��� �ִ��� ��)
            if (revealSpeed >= 2.9f)
            {
                previewText.maxVisibleCharacters = message.Length;
                await WaitAndRestart(cancellationToken);
                return;
            }

            // ���ں��� ��� (�α� ����)
            for (int i = 0; i <= message.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                previewText.maxVisibleCharacters = i;

                if (i < message.Length)
                {
                    await UniTask.Delay(Mathf.RoundToInt(revealDelay * 1000), cancellationToken: cancellationToken);
                }
            }

            // �Ϸ� �� ��� ����ϰ� ���� �޽���
            await WaitAndRestart(cancellationToken);
        }
        catch (System.OperationCanceledException)
        {
            // �������� ��� - �α� ����
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedPreview] �ؽ�Ʈ ��� ����: {ex.Message}");
        }
    }

    private async UniTask WaitAndRestart(CancellationToken cancellationToken)
    {
        // �Ϸ� �� 2�� ���
        await UniTask.Delay(2000, cancellationToken: cancellationToken);

        if (!cancellationToken.IsCancellationRequested && isActive)
        {
            StartPreview(); // ���� �޽����� �ڵ� �����
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;
        StopPreview();
    }
}