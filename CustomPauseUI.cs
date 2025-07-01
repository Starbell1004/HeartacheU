using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Naninovel;
using Naninovel.UI;

namespace Naninovel.UI
{
    /// <summary>
    /// ������ Ŀ���� PauseUI - Ÿ��Ʋ ��ư�� HomeUI�� �����
    /// </summary>
    public class SimplifiedCustomPauseUI : PauseUI
    {
        [Header("Custom Settings")]
        [SerializeField] private Button titleButton; // Inspector���� Ÿ��Ʋ ��ư ����
        [SerializeField] private float fadeDelay = 0.5f; // ���̵� ȿ�� �� ��� �ð�

        protected override void Awake()
        {
            base.Awake();

            // Ÿ��Ʋ ��ư�� �������� �ʾ����� �ڵ����� ã��
            if (titleButton == null)
            {
                // "Title"�� ���Ե� ��ư ã��
                var buttons = GetComponentsInChildren<Button>(true); // ��Ȱ��ȭ�� �͵� ����
                foreach (var btn in buttons)
                {
                    // ControlPanelTitleButton ������Ʈ�� �ִ� ��ư ã��
                    var controlPanelButton = btn.GetComponent<ControlPanelTitleButton>();
                    if (controlPanelButton != null)
                    {
                        titleButton = btn;
                        // ���� ControlPanelTitleButton ������Ʈ ��Ȱ��ȭ
                        controlPanelButton.enabled = false;
                        Debug.Log("[CustomPauseUI] ControlPanelTitleButton ��Ȱ��ȭ");
                        break;
                    }

                    // �Ǵ� �̸����� ã��
                    if (btn.name.ToLower().Contains("title") ||
                        btn.GetComponentInChildren<Text>()?.text.Contains("Ÿ��Ʋ") == true)
                    {
                        titleButton = btn;
                        break;
                    }
                }
            }

            // ��ư�� ���ο� ���� �߰�
            if (titleButton != null)
            {
                // ��� ���� ������ ����
                titleButton.onClick.RemoveAllListeners();

                // ���� ScriptableButton�̳� ControlPanelTitleButton ��Ȱ��ȭ
                var scriptableButtons = titleButton.GetComponents<ScriptableButton>();
                foreach (var sb in scriptableButtons)
                {
                    sb.enabled = false;
                }

                // ���ο� ���� �߰�
                titleButton.onClick.AddListener(OnTitleButtonClick);
                Debug.Log("[CustomPauseUI] Ÿ��Ʋ ��ư ���� �Ϸ�");
            }
            else
            {
                Debug.LogWarning("[CustomPauseUI] Ÿ��Ʋ ��ư�� ã�� �� �����ϴ�!");
            }
        }

        public override UniTask Initialize()
        {
            Debug.Log("[CustomPauseUI] Initialize ȣ���!");

            // �⺻ ���ε��� ���� ���� (OnEnable���� ó��)

            return UniTask.CompletedTask;
        }

        protected override void HandleVisibilityChanged(bool visible)
        {
            base.HandleVisibilityChanged(visible);
            Debug.Log($"[CustomPauseUI] HandleVisibilityChanged - visible: {visible}");

            // UI�� ǥ�õ� �� ��Ŀ�� ����
            if (visible && gameObject.activeInHierarchy)
            {
                StartCoroutine(ClearFocusDelayed());
            }
        }

        private IEnumerator ClearFocusDelayed()
        {
            yield return null;

            // ���� ���õ� GameObject�� null�� �����Ͽ� ��Ŀ�� ����
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        public override void ToggleVisibility()
        {
            Debug.Log($"[CustomPauseUI] ToggleVisibility ȣ�� - ���� ����: {Visible}, Time: {Time.unscaledTime}");
            base.ToggleVisibility();
        }

        public override void Show()
        {
            base.Show();
            Debug.Log($"[CustomPauseUI] Show - Time.timeScale: {Time.timeScale}");
        }

        public async void OnTitleButtonClick()
        {
            Debug.Log("[CustomPauseUI] Ÿ��Ʋ ��ư Ŭ�� - �ε巯�� ��ȯ�� �Բ� HomeUI�� �̵�");

            // 1. �ε� ȭ�� ǥ�� (���̵� ȿ�� ����)
            using (await LoadingScreen.Show())
            {
                // Pause UI �����
                Hide();

                // ���̵� ȿ���� ���� ���
                await UniTask.Delay(System.TimeSpan.FromSeconds(fadeDelay));

                // ���� ���� ��ũ��Ʈ ����
                var scriptPlayer = Engine.GetService<IScriptPlayer>();
                if (scriptPlayer != null && scriptPlayer.Playing)
                {
                    scriptPlayer.Stop();
                }

                // ���� ���� ����
                var stateManager = Engine.GetService<IStateManager>();
                if (stateManager != null)
                {
                    await stateManager.ResetState();
                }

                // HomeUI ǥ�� �� �߰� ���
                await UniTask.Delay(System.TimeSpan.FromSeconds(fadeDelay));
            }

            // 2. HomeUI ǥ��
            ShowHomeUI();
        }

        private void ShowHomeUI()
        {
            var uiManager = Engine.GetService<IUIManager>();

            // ��� 1: UIManager���� ã��
            var homeUI = uiManager?.GetUI<HomeUI>();
            if (homeUI != null)
            {
                homeUI.Show();
                Debug.Log("[CustomPauseUI] UIManager���� HomeUI ã��");
            }
            else
            {
                // ��� 2: ��Ȱ��ȭ�� ������Ʈ �����ؼ� ã��
                var allHomeUIs = Resources.FindObjectsOfTypeAll<HomeUI>();
                if (allHomeUIs.Length > 0)
                {
                    homeUI = allHomeUIs[0];
                    homeUI.gameObject.SetActive(true);
                    homeUI.Show();
                    Debug.Log("[CustomPauseUI] Resources���� HomeUI ã��");
                }
                else
                {
                    Debug.LogError("[CustomPauseUI] HomeUI�� ã�� �� �����ϴ�!");

                    // ��ü ���: �⺻ Ÿ��Ʋ UI ǥ��
                    var titleUI = uiManager?.GetUI<ITitleUI>();
                    if (titleUI != null)
                    {
                        titleUI.Show();
                    }
                }
            }
        }
    }
}