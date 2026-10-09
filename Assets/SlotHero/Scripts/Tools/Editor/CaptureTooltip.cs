using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SlotHero.TopBar;
using SlotHero.TopBar.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 호버 풍선이 뜬 모습을 그림으로 뽑는다.
    /// 메뉴 Slot Hero / 호버 풍선 찍기 에서 실행한다.
    ///
    /// 풍선은 마우스를 올려야 뜨는 것이라 다른 그림에는 한 번도 안 나온다.
    /// 자리와 크기가 버튼을 가리지 않는지는 눈으로 봐야 알 수 있어 따로 뽑는다.
    /// </summary>
    public static class CaptureTooltip
    {
        [MenuItem("Slot Hero/호버 풍선 찍기", priority = 5)]
        public static void Capture()
        {
            string path = UiSceneBuilder.SceneFolder + "/03 상단 표시줄.unity";

            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("상단 표시줄 확인용 씬이 없다: " + path);
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            TopBarTooltip tooltip = Object.FindAnyObjectByType<TopBarTooltip>(
                FindObjectsInactive.Include);

            if (tooltip == null)
            {
                Debug.LogError("호버 풍선을 찾지 못했다.");
                return;
            }

            TopBarButton target = null;
            TopBarButton[] buttons = Object.FindObjectsByType<TopBarButton>(FindObjectsInactive.Include);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].Kind == TopBarButtonKind.Map)
                {
                    target = buttons[i];
                    break;
                }
            }

            if (target == null)
            {
                Debug.LogError("지도 버튼을 찾지 못했다.");
                return;
            }

            // 배치 모드에서는 Awake 가 돌지 않아 색과 커서 설정이 아직 안 들어갔다.
            tooltip.ApplyStyle();
            tooltip.Show(target.RectTransform, "지도");

            CapturePreviewScenes.CaptureOpenScene("03 상단 표시줄 호버 풍선");
        }
    }
}
