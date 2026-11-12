using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SlotHero.Tools;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 효과음 후보 듣기 씬을 굴려 본다(2026년 11월 13일 원재).
    /// 후보 폴더의 소리 파일마다 칸이 하나씩 있고 소리가 물려 있는지, 칸을 누르면 그 소리 이름이 위에 적히는지 본다.
    /// </summary>
    public static class CheckSoundTest
    {
        private static int _checked;
        private static int _failed;

        [MenuItem("Slot Hero/검사/효과음 후보 듣기 씬")]
        public static void Check()
        {
            _checked = 0;
            _failed = 0;

            string path = UiSceneBuilder.SceneFolder + "/" + BuildPreviewScenes.SoundTestSceneName + ".unity";
            if (!File.Exists(path))
            {
                Report("효과음 후보 듣기 씬이 있다", false, path);
                Finish();
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            CheckPreviewScenes.CallAwake(new HashSet<MonoBehaviour>());

            SoundAuditionDriver driver = Object.FindAnyObjectByType<SoundAuditionDriver>();
            Report("조종기가 있다", driver != null);
            if (driver == null)
            {
                Finish();
                return;
            }

            List<AudioClip> clips = Field<List<AudioClip>>(driver, "_clips");
            List<Button> buttons = Field<List<Button>>(driver, "_buttons");
            int files = AssetDatabase.FindAssets("t:AudioClip", new[] { ImportSounds.CandidateFolder }).Length;
            int rows = AssetDatabase.IsValidFolder(ImportSounds.CandidateFolder)
                ? AssetDatabase.GetSubFolders(ImportSounds.CandidateFolder).Length
                : 0;

            Report("아직 고를 두 줄이다(S08 펼치기, S08 접기)", rows == 2, rows.ToString());
            Report("후보 파일마다 칸이 하나다", clips != null && buttons != null && clips.Count == files && buttons.Count == files,
                (clips != null ? clips.Count : -1) + " / " + files);
            Report("칸마다 소리가 물려 있다", clips != null && !clips.Contains(null));

            Invoke(driver, "Start");
            if (buttons != null && buttons.Count > 0)
            {
                buttons[0].onClick.Invoke();
            }

            string info = Field<TMP_Text>(driver, "_infoLabel").text;
            Report("칸을 누르면 그 소리를 위에 적는다", driver.Playing == 0 && info.StartsWith("지금 · S08-1"), info);

            CapturePreviewScenes.CaptureOpenScene(BuildPreviewScenes.SoundTestSceneName + " 누른 뒤");
            Finish();
        }

        private static void Finish()
        {
            Debug.Log("효과음 후보 듣기 씬 검사 " + _checked + "건, 실패 " + _failed + "건");
        }

        private static void Report(string name, bool ok)
        {
            Report(name, ok, string.Empty);
        }

        private static void Report(string name, bool ok, string detail)
        {
            _checked++;
            if (!ok)
            {
                _failed++;
            }

            Debug.Log("  " + (ok ? "통과" : "실패") + "  " + name + (ok || string.IsNullOrEmpty(detail) ? string.Empty : "   (" + detail + ")"));
        }

        private static void Invoke(object target, string method)
        {
            MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (info != null)
            {
                info.Invoke(target, null);
            }
        }

        private static T Field<T>(object target, string name) where T : class
        {
            FieldInfo info = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return info != null ? info.GetValue(target) as T : null;
        }
    }
}
