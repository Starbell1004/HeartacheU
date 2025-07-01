using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

public class HomeTextSpeedSettings : MonoBehaviour
{
    [Header("�ؽ�Ʈ �ӵ� ����")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedDisplayText;
    [SerializeField] private HomeTextSpeedPreview textPreview;

    private ITextPrinterManager printerManager;

    private void Awake()
    {
        Debug.Log("[HomeTextSpeedSettings] Awake ȣ��");

        // ���ϳ뺧 ���� �ʱ�ȭ
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
        Debug.Log("[HomeTextSpeedSettings] Start ȣ��");
        SetupTextSpeed();
    }

    private void InitializeServices()
    {
        try
        {
            printerManager = Engine.GetService<ITextPrinterManager>();
            Debug.Log("[HomeTextSpeedSettings] ���ϳ뺧 ���� �ʱ�ȭ �Ϸ�");

            // ���� �ʱ�ȭ �� �����̴� �� ����
            RefreshSpeedFromNaninovel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HomeTextSpeedSettings] ���� �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private void SetupTextSpeed()
    {
        Debug.Log("[HomeTextSpeedSettings] SetupTextSpeed ����");

        if (speedSlider != null)
        {
            Debug.Log("[HomeTextSpeedSettings] �����̴� ���� ��...");
            speedSlider.minValue = 0.5f;
            speedSlider.maxValue = 1.5f;

            // �̺�Ʈ ������ ���
            speedSlider.onValueChanged.RemoveAllListeners(); // �ߺ� ����
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);

            Debug.Log("[HomeTextSpeedSettings] �����̴� �̺�Ʈ ��� �Ϸ�");

            // �ʱⰪ ����
            RefreshSpeedFromNaninovel();
        }
        else
        {
            Debug.LogError("[HomeTextSpeedSettings] speedSlider�� null�Դϴ�!");
        }

        Debug.Log($"[HomeTextSpeedSettings] textPreview ���� ����: {(textPreview != null ? "�����" : "null")}");

        // �� �̸����� Ȱ��ȭ (�̰� �����־����!)
        if (textPreview != null)
        {
            Debug.Log("[HomeTextSpeedSettings] �̸����� Ȱ��ȭ ����");
            textPreview.Show();
        }
    }

    private void RefreshSpeedFromNaninovel()
    {
        if (speedSlider == null) return;

        float currentSpeed = 1f;

        if (printerManager != null)
        {
            currentSpeed = printerManager.BaseRevealSpeed;
            Debug.Log($"[HomeTextSpeedSettings] ���ϳ뺧���� �ӵ� �ε�: {currentSpeed}");
        }
        else
        {
            currentSpeed = PlayerPrefs.GetFloat("TextSpeed", 1f);
            Debug.Log($"[HomeTextSpeedSettings] PlayerPrefs���� �ӵ� �ε�: {currentSpeed}");
        }

        speedSlider.value = currentSpeed;
        UpdateSpeedDisplay(currentSpeed);
    }

    private void OnSpeedChanged(float speed)
    {
        Debug.Log($"[HomeTextSpeedSettings] �ӵ� �����: {speed}");

        // ���ϳ뺧�� ����
        if (printerManager != null)
        {
            printerManager.BaseRevealSpeed = speed;
            Debug.Log($"[HomeTextSpeedSettings] ���ϳ뺧 �ӵ� ����: {speed}");
        }
        else
        {
            Debug.LogWarning("[HomeTextSpeedSettings] printerManager�� null! PlayerPrefs�� ����");
            PlayerPrefs.SetFloat("TextSpeed", speed);
        }

        // UI ������Ʈ
        UpdateSpeedDisplay(speed);

        // �̸����� ����
        if (textPreview != null)
        {
            Debug.Log("[HomeTextSpeedSettings] �̸����� ����");
            textPreview.StartPreview();
        }
        else
        {
            Debug.LogWarning("[HomeTextSpeedSettings] textPreview�� null!");
        }
    }

    private void UpdateSpeedDisplay(float speed)
    {
        Debug.Log($"[HomeTextSpeedSettings] �ӵ� �ؽ�Ʈ ������Ʈ: {speed:F1}x");

        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"{speed:F1}x";
            Debug.Log($"[HomeTextSpeedSettings] �ؽ�Ʈ ���� �Ϸ�: {speedDisplayText.text}");
        }
        else
        {
            Debug.LogError("[HomeTextSpeedSettings] speedDisplayText�� null!");
        }
    }

    // �ܺο��� ȣ���
    public void ShowPreview()
    {
        if (textPreview != null)
        {
            textPreview.Show();
        }
    }

    public void HidePreview()
    {
        if (textPreview != null)
        {
            textPreview.Hide();
        }
    }

    public void RefreshSettings()
    {
        RefreshSpeedFromNaninovel();
    }

    // �׽�Ʈ�� �Լ�
    [ContextMenu("Test Speed Change")]
    public void TestSpeedChange()
    {
        Debug.Log("[HomeTextSpeedSettings] �׽�Ʈ: �ӵ��� 1.5�� ����");
        if (speedSlider != null)
        {
            speedSlider.value = 1.5f;
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= InitializeServices;

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveAllListeners();
        }
    }
}