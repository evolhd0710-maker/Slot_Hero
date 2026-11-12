using SlotHero.CurrentBuild;
using SlotHero.Events;
using SlotHero.Events.UI;
using SlotHero.Flow;
using SlotHero.Flow.UI;
using SlotHero.Tools;
using SlotHero.Ui;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 이벤트 하나만 굴려 보는 시험 씬. 지금은 07 기록 말살 이다(2026년 11월 13일 원재).
    /// 그 전의 36 혼돈 시험 씬은 같은 날 원재가 지우라고 해 지웠다.
    ///
    /// 이벤트 화면은 이벤트 확인용 씬과 같은 코드(`BuildEventScreen`)로, 태그 선택 창은 게임 씬과 같은 코드(`BuildGameScene.BuildEventTagPicker`)로 짓는다.
    /// 왼쪽 빈자리에 시험 칸을 두어 고르기 전과 뒤에 바뀐 코인과 문양을 적고, 아래에 다시 하기 버튼 둘을 둔다.
    /// 플레이를 눌러야 움직인다. 굴리는 일은 `EventTestDriver` 가 한다.
    /// </summary>
    public static partial class BuildPreviewScenes
    {
        /// 07 기록 말살 시험 씬 이름.
        public const string RecordErasureTestSceneName = "시험 07 기록 말살";

        [MenuItem("Slot Hero/확인용 씬 만들기/07 기록 말살 시험")]
        public static void BuildRecordErasureTest()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("EventTestCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            EventScreenView view;
            EventScreenController controller = BuildEventScreen(canvas, out view);

            EventVisualConfig visual = UiSceneBuilder.LoadConfig<EventVisualConfig>("Events", "EventVisualConfig");
            float pad = UiScale.Px(16f);

            // 시험 칸. 삽화(가운데 1200 폭) 왼쪽 빈자리에 둔다.
            // 태그 하나를 지우면 바뀌는 문양 가짓수가 많아 글이 넘치면 줄여 넣는다.
            Image panel = UiSceneBuilder.NewImage("TestPanel", canvas.transform, visual.PanelColor);
            UiSceneBuilder.PlaceTopLeft(panel.rectTransform, UiScale.V(40f, 100f), UiScale.V(300f, 720f));
            panel.raycastTarget = false;

            TMP_Text info = UiSceneBuilder.NewText(
                "InfoLabel", panel.transform, "플레이를 누르면 시작한다.", visual.BodyFontSize * 0.8f, visual.TextColor,
                TextAlignmentOptions.TopLeft);
            UiSceneBuilder.Stretch(info.rectTransform);
            info.rectTransform.offsetMin = new Vector2(pad, pad);
            info.rectTransform.offsetMax = new Vector2(-pad, -pad);
            UiSceneBuilder.ShrinkToFit(info, 0.5f, UiScale.Px(4f));

            Button restart = NewTestButton(canvas.transform, "RestartButton", "다시 하기", UiScale.V(40f, 840f), visual);
            Button noLegend = NewTestButton(canvas.transform, "NoLegendButton", "전설 코인 없이", UiScale.V(40f, 920f), visual);

            // 태그 선택 창은 맨 위에 둔다. 처음에는 닫혀 있다.
            EventTagPicker tagPicker = BuildGameScene.BuildEventTagPicker(canvas);
            if (tagPicker != null)
            {
                tagPicker.transform.SetAsLastSibling();
                tagPicker.gameObject.SetActive(false);
            }

            GameObject driverGo = new GameObject("EventTestDriver");
            SceneManager.MoveGameObjectToScene(driverGo, scene);
            EventTestDriver driver = driverGo.AddComponent<EventTestDriver>();
            UiSceneBuilder.SetField(driver, "_screen", controller);
            UiSceneBuilder.SetField(driver, "_infoLabel", info);
            UiSceneBuilder.SetField(driver, "_restartButton", restart);
            UiSceneBuilder.SetField(driver, "_noLegendButton", noLegend);
            UiSceneBuilder.SetField(driver, "_tagPickerBehaviour", tagPicker);
            UiSceneBuilder.SetField(driver, "_catalog", UiSceneBuilder.LoadConfig<RunCatalogConfig>("Flow", "RunCatalogConfig"));
            UiSceneBuilder.SetField(driver, "_events", UiSceneBuilder.LoadConfig<RunEventConfig>("Flow", "RunEventConfig"));
            UiSceneBuilder.SetField(driver, "_illustrations",
                UiSceneBuilder.LoadConfig<EventIllustrationConfig>("Events", "EventIllustrationConfig"));
            UiSceneBuilder.SetField(driver, "_buildVisual",
                UiSceneBuilder.LoadConfig<CurrentBuildVisualConfig>("CurrentBuild", "CurrentBuildVisualConfig"));

            // 플레이 전에도 무엇을 시험하는 씬인지 보이게 첫 화면 삽화를 얹어 둔다.
            view.ShowIllustration(ImportArt.LoadEventIllustration(EventScripts.IllustrationIdOf("record_erasure", "1")));

            SaveScene(scene, RecordErasureTestSceneName);
        }

        private static Button NewTestButton(Transform parent, string name, string label, Vector2 position, EventVisualConfig visual)
        {
            Image image = UiSceneBuilder.NewButtonImage(name, parent, visual.PanelColor);
            UiSceneBuilder.PlaceTopLeft(image.rectTransform, position, UiScale.V(300f, 60f));
            Button button = UiSceneBuilder.AddButton(image.gameObject, image);

            TMP_Text text = UiSceneBuilder.NewText(
                "Label", image.transform, label, visual.BodyFontSize, visual.ChoiceTextColor, TextAlignmentOptions.Center, true, true);
            UiSceneBuilder.Stretch(text.rectTransform);
            return button;
        }
    }
}
