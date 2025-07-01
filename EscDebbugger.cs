using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class ESCDebugger : MonoBehaviour
{
    void Update()
    {
        // 1. Unity Input System에서 ESC 키 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[ESCDebugger] ESC 키 감지됨! Time: {Time.time}, TimeScale: {Time.timeScale}");

            // 2. Naninovel InputManager 상태 확인
            var inputManager = Engine.GetService<IInputManager>();
            if (inputManager != null)
            {
                Debug.Log($"[ESCDebugger] InputManager 존재함");

                // 3. InputManager가 활성화되어 있는지 확인
                Debug.Log($"[ESCDebugger] InputManager 타입: {inputManager.GetType().Name}");
            }
            else
            {
                Debug.LogError("[ESCDebugger] InputManager를 찾을 수 없음!");
            }

            // 4. 현재 활성화된 UI 확인
            var uiManager = Engine.GetService<IUIManager>();
            if (uiManager != null)
            {
                var managedUIs = new System.Collections.Generic.List<IManagedUI>();
                uiManager.GetManagedUIs(managedUIs);

                Debug.Log($"[ESCDebugger] 총 관리되는 UI 수: {managedUIs.Count}");
                foreach (var ui in managedUIs)
                {
                    if (ui.Visible)
                    {
                        Debug.Log($"[ESCDebugger] 활성 UI: {ui.GetType().Name}");

                        // PauseUI인지 확인
                        if (ui is PauseUI)
                        {
                            Debug.Log($"[ESCDebugger] PauseUI 발견! Visible: {ui.Visible}");
                        }
                    }
                }

                // 5. CustomPauseUI 직접 찾기
                var pauseUI = uiManager.GetUI<PauseUI>();
                if (pauseUI != null)
                {
                    Debug.Log($"[ESCDebugger] PauseUI 상태 - Visible: {pauseUI.Visible}, Type: {pauseUI.GetType().Name}");
                }
                else
                {
                    Debug.Log("[ESCDebugger] PauseUI를 찾을 수 없음");
                }
            }

            // 6. 다른 스크립트가 ESC를 가로채는지 확인
            Debug.Log($"[ESCDebugger] 현재 포커스된 GameObject: {UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.name ?? "없음"}");
        }
    }
}