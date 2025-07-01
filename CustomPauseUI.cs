using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Naninovel;
using Naninovel.UI;

namespace Naninovel.UI
{
    /// <summary>
    /// 간단한 커스텀 PauseUI - 타이틀 버튼이 HomeUI로 연결됨
    /// </summary>
    public class SimplifiedCustomPauseUI : PauseUI
    {
        [Header("Custom Settings")]
        [SerializeField] private Button titleButton; // Inspector에서 타이틀 버튼 연결
        [SerializeField] private float fadeDelay = 0.5f; // 페이드 효과 전 대기 시간

        protected override void Awake()
        {
            base.Awake();

            // 타이틀 버튼이 지정되지 않았으면 자동으로 찾기
            if (titleButton == null)
            {
                // "Title"이 포함된 버튼 찾기
                var buttons = GetComponentsInChildren<Button>(true); // 비활성화된 것도 포함
                foreach (var btn in buttons)
                {
                    // ControlPanelTitleButton 컴포넌트가 있는 버튼 찾기
                    var controlPanelButton = btn.GetComponent<ControlPanelTitleButton>();
                    if (controlPanelButton != null)
                    {
                        titleButton = btn;
                        // 기존 ControlPanelTitleButton 컴포넌트 비활성화
                        controlPanelButton.enabled = false;
                        Debug.Log("[CustomPauseUI] ControlPanelTitleButton 비활성화");
                        break;
                    }

                    // 또는 이름으로 찾기
                    if (btn.name.ToLower().Contains("title") ||
                        btn.GetComponentInChildren<Text>()?.text.Contains("타이틀") == true)
                    {
                        titleButton = btn;
                        break;
                    }
                }
            }

            // 버튼에 새로운 동작 추가
            if (titleButton != null)
            {
                // 모든 기존 리스너 제거
                titleButton.onClick.RemoveAllListeners();

                // 기존 ScriptableButton이나 ControlPanelTitleButton 비활성화
                var scriptableButtons = titleButton.GetComponents<ScriptableButton>();
                foreach (var sb in scriptableButtons)
                {
                    sb.enabled = false;
                }

                // 새로운 동작 추가
                titleButton.onClick.AddListener(OnTitleButtonClick);
                Debug.Log("[CustomPauseUI] 타이틀 버튼 연결 완료");
            }
            else
            {
                Debug.LogWarning("[CustomPauseUI] 타이틀 버튼을 찾을 수 없습니다!");
            }
        }

        public override UniTask Initialize()
        {
            Debug.Log("[CustomPauseUI] Initialize 호출됨!");

            // 기본 바인딩은 하지 않음 (OnEnable에서 처리)

            return UniTask.CompletedTask;
        }

        protected override void HandleVisibilityChanged(bool visible)
        {
            base.HandleVisibilityChanged(visible);
            Debug.Log($"[CustomPauseUI] HandleVisibilityChanged - visible: {visible}");

            // UI가 표시될 때 포커스 제거
            if (visible && gameObject.activeInHierarchy)
            {
                StartCoroutine(ClearFocusDelayed());
            }
        }

        private IEnumerator ClearFocusDelayed()
        {
            yield return null;

            // 현재 선택된 GameObject를 null로 설정하여 포커스 제거
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        public override void ToggleVisibility()
        {
            Debug.Log($"[CustomPauseUI] ToggleVisibility 호출 - 현재 상태: {Visible}, Time: {Time.unscaledTime}");
            base.ToggleVisibility();
        }

        public override void Show()
        {
            base.Show();
            Debug.Log($"[CustomPauseUI] Show - Time.timeScale: {Time.timeScale}");
        }

        public async void OnTitleButtonClick()
        {
            Debug.Log("[CustomPauseUI] 타이틀 버튼 클릭 - 부드러운 전환과 함께 HomeUI로 이동");

            // 1. 로딩 화면 표시 (페이드 효과 포함)
            using (await LoadingScreen.Show())
            {
                // Pause UI 숨기기
                Hide();

                // 페이드 효과를 위한 대기
                await UniTask.Delay(System.TimeSpan.FromSeconds(fadeDelay));

                // 진행 중인 스크립트 중지
                var scriptPlayer = Engine.GetService<IScriptPlayer>();
                if (scriptPlayer != null && scriptPlayer.Playing)
                {
                    scriptPlayer.Stop();
                }

                // 게임 상태 리셋
                var stateManager = Engine.GetService<IStateManager>();
                if (stateManager != null)
                {
                    await stateManager.ResetState();
                }

                // HomeUI 표시 전 추가 대기
                await UniTask.Delay(System.TimeSpan.FromSeconds(fadeDelay));
            }

            // 2. HomeUI 표시
            ShowHomeUI();
        }

        private void ShowHomeUI()
        {
            var uiManager = Engine.GetService<IUIManager>();

            // 방법 1: UIManager에서 찾기
            var homeUI = uiManager?.GetUI<HomeUI>();
            if (homeUI != null)
            {
                homeUI.Show();
                Debug.Log("[CustomPauseUI] UIManager에서 HomeUI 찾음");
            }
            else
            {
                // 방법 2: 비활성화된 오브젝트 포함해서 찾기
                var allHomeUIs = Resources.FindObjectsOfTypeAll<HomeUI>();
                if (allHomeUIs.Length > 0)
                {
                    homeUI = allHomeUIs[0];
                    homeUI.gameObject.SetActive(true);
                    homeUI.Show();
                    Debug.Log("[CustomPauseUI] Resources에서 HomeUI 찾음");
                }
                else
                {
                    Debug.LogError("[CustomPauseUI] HomeUI를 찾을 수 없습니다!");

                    // 대체 방안: 기본 타이틀 UI 표시
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