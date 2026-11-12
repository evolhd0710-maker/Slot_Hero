using System.Collections.Generic;
using System.IO;
using SlotHero.Events;
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
    /// 효과음 후보 듣기 씬(2026년 11월 13일 원재).
    /// `효과음과 이펙트 목록.md` 에서 후보만 정하고 게임에 붙이지 않은 효과음을 소리마다 한 줄로 늘어놓는다.
    /// 줄 왼쪽에 소리 번호와 이름, 그 아래에 받지 않아 넣지 못한 후보를 적고, 오른쪽에 받아 둔 후보 파일마다 칸을 둔다.
    /// 후보 파일은 `ImportSounds` 가 `Art/Audio/후보/<소리>` 로 옮겨 둔다. 굴리는 일은 `SoundAuditionDriver` 가 한다.
    /// </summary>
    public static partial class BuildPreviewScenes
    {
        /// 효과음 후보 듣기 씬 이름.
        public const string SoundTestSceneName = "시험 효과음 후보";

        /// <summary>
        /// 소리마다 받지 않아 넣지 못한 후보. 목록 6.3 · 6.9 의 후보 가운데 아직 받지 않은 것이다.
        /// 2026년 11월 13일 씬을 정리해 아직 고를 소리만 남았다.
        /// </summary>
        private static readonly Dictionary<string, string> SoundNotReceived = new Dictionary<string, string>
        {
            { "S08-1", "Freesound Parchment - Unroll (첫 후보)" },
        };

        [MenuItem("Slot Hero/확인용 씬 만들기/효과음 후보 듣기")]
        public static void BuildSoundTest()
        {
            Scene scene = NewScene();
            Canvas canvas = UiSceneBuilder.CreateCanvas("SoundTestCanvas");
            UiSceneBuilder.AttachCanvasToCamera(canvas, _camera);

            EventVisualConfig visual = UiSceneBuilder.LoadConfig<EventVisualConfig>("Events", "EventVisualConfig");
            Color panel = visual != null ? visual.PanelColor : new Color(0.2f, 0.18f, 0.16f, 1f);
            Color text = visual != null ? visual.TextColor : Color.white;
            // 밝은 칸 그림 위라 글자를 짙은 갈색으로 적는다. 선택지 글자색은 이 칸에서 흐리게 보였다.
            Color buttonText = new Color(0.16f, 0.11f, 0.07f, 1f);

            Image behind = UiSceneBuilder.NewFlatImage("BehindScreen", canvas.transform, DarkBackground);
            UiSceneBuilder.Stretch(behind.rectTransform);
            behind.raycastTarget = false;

            TMP_Text title = UiSceneBuilder.NewText(
                "TitleLabel", canvas.transform, "효과음 후보 듣기", UiScale.Px(30f), text, TextAlignmentOptions.Left, true, true);
            UiSceneBuilder.PlaceTopLeft(title.rectTransform, UiScale.V(40f, 30f), UiScale.V(400f, 50f));

            TMP_Text info = UiSceneBuilder.NewText(
                "InfoLabel", canvas.transform, "플레이를 누른 뒤 칸을 누르면 그 소리를 낸다.", UiScale.Px(20f), text,
                TextAlignmentOptions.Left, false, true);
            UiSceneBuilder.PlaceTopLeft(info.rectTransform, UiScale.V(460f, 30f), UiScale.V(1200f, 50f));
            UiSceneBuilder.ShrinkToFit(info, 0.6f);

            Button stop = NewSoundButton(canvas.transform, "StopButton", "멈춤", UiScale.V(1700f, 30f), UiScale.V(180f, 50f),
                panel, buttonText, UiScale.Px(20f));

            List<AudioClip> clips = new List<AudioClip>();
            List<Button> buttons = new List<Button>();
            List<string> titles = new List<string>();

            string[] rows = AssetDatabase.IsValidFolder(ImportSounds.CandidateFolder)
                ? AssetDatabase.GetSubFolders(ImportSounds.CandidateFolder)
                : new string[0];
            System.Array.Sort(rows, System.StringComparer.Ordinal);

            float rowHeight = UiScale.Px(44f);
            float top = UiScale.Px(100f);
            for (int r = 0; r < rows.Length; r++)
            {
                string rowName = Path.GetFileName(rows[r]);
                string number = rowName.Split(' ')[0];
                float y = top + r * rowHeight;

                string missing;
                string labelText = "<b>" + rowName + "</b>";
                if (SoundNotReceived.TryGetValue(number, out missing))
                {
                    labelText += "\n<size=70%>안 받음: " + missing + "</size>";
                }

                TMP_Text label = UiSceneBuilder.NewText(
                    "Row_" + number, canvas.transform, labelText, UiScale.Px(17f), text, TextAlignmentOptions.Left, false, true);
                UiSceneBuilder.PlaceTopLeft(label.rectTransform, new Vector2(UiScale.Px(40f), y), new Vector2(UiScale.Px(330f), rowHeight));
                UiSceneBuilder.ShrinkToFit(label, 0.6f);

                string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { rows[r] });
                List<string> paths = new List<string>();
                for (int g = 0; g < guids.Length; g++)
                {
                    paths.Add(AssetDatabase.GUIDToAssetPath(guids[g]));
                }

                paths.Sort(System.StringComparer.Ordinal);
                for (int c = 0; c < paths.Count; c++)
                {
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(paths[c]);
                    string file = Path.GetFileNameWithoutExtension(paths[c]);
                    Vector2 at = new Vector2(UiScale.Px(380f) + c * UiScale.Px(212f), y + UiScale.Px(3f));
                    Button button = NewSoundButton(canvas.transform, number + "_" + c, ShortSoundName(file), at,
                        new Vector2(UiScale.Px(204f), rowHeight - UiScale.Px(6f)), panel, buttonText, UiScale.Px(16f));

                    clips.Add(clip);
                    buttons.Add(button);
                    titles.Add(rowName + " · " + file);
                }
            }

            GameObject driverGo = new GameObject("SoundAuditionDriver");
            SceneManager.MoveGameObjectToScene(driverGo, scene);
            AudioSource audio = driverGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            SoundAuditionDriver driver = driverGo.AddComponent<SoundAuditionDriver>();
            UiSceneBuilder.SetField(driver, "_audio", audio);
            UiSceneBuilder.SetField(driver, "_infoLabel", info);
            UiSceneBuilder.SetField(driver, "_stopButton", stop);

            SerializedObject serialized = new SerializedObject(driver);
            FillList(serialized.FindProperty("_clips"), clips);
            FillList(serialized.FindProperty("_buttons"), buttons);
            SerializedProperty titleList = serialized.FindProperty("_titles");
            titleList.arraySize = titles.Count;
            for (int i = 0; i < titles.Count; i++)
            {
                titleList.GetArrayElementAtIndex(i).stringValue = titles[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(scene, SoundTestSceneName);
        }

        /// <summary>칸에 적을 짧은 이름. 앞의 차례 번호와 묶음 이름은 첫 줄, 원래 파일 이름은 둘째 줄이다.</summary>
        private static string ShortSoundName(string file)
        {
            string[] parts = file.Split(new[] { '_' }, 3);
            if (parts.Length < 3)
            {
                return file;
            }

            string rest = parts[2];
            if (rest.Length > 26)
            {
                rest = rest.Substring(0, 26) + "…";
            }

            return parts[0] + " · " + parts[1] + "\n<size=80%>" + rest + "</size>";
        }

        private static Button NewSoundButton(
            Transform parent, string name, string label, Vector2 position, Vector2 size, Color color, Color textColor, float fontSize)
        {
            Image image = UiSceneBuilder.NewButtonImage(name, parent, color);
            UiSceneBuilder.PlaceTopLeft(image.rectTransform, position, size);
            Button button = UiSceneBuilder.AddButton(image.gameObject, image);

            TMP_Text text = UiSceneBuilder.NewText("Label", image.transform, label, fontSize, textColor, TextAlignmentOptions.Center, true, true);
            UiSceneBuilder.Stretch(text.rectTransform);
            UiSceneBuilder.ShrinkToFit(text, 0.6f, UiScale.Px(6f));
            return button;
        }

        private static void FillList<T>(SerializedProperty list, List<T> items) where T : Object
        {
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }
    }
}
