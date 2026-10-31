using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using SlotHero.Save;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 테스트하다 저장 데이터를 처음으로 되돌린다.
    /// 메뉴 Slot Hero / 저장 데이터 에서 쓴다.
    ///
    /// 저장 파일은 프로젝트 밖에 쌓인다.
    /// `Application.persistentDataPath` 아래의 `SaveConfig.FolderName` 폴더다.
    /// 윈도에서는 `AppData\LocalLow\<회사>\<게임>\SlotHero` 가 된다.
    /// 그래서 유니티 프로젝트를 아무리 지워도 저장은 그대로 남는다.
    ///
    /// 지우면 되돌릴 수 없으므로 두 메뉴 다 묻고 나서 지운다.
    /// </summary>
    public static class ClearSaveData
    {
        [MenuItem("Slot Hero/저장 데이터/폴더 열기", priority = 20)]
        public static void OpenFolder()
        {
            string root = GetRoot();

            if (!Directory.Exists(root))
            {
                Debug.Log("저장 폴더가 아직 없다. 한 번 플레이하면 생긴다: " + root);
                return;
            }

            EditorUtility.RevealInFinder(root);
            Debug.Log("저장 폴더: " + root);
        }

        [MenuItem("Slot Hero/저장 데이터/무엇이 있는지 보기", priority = 21)]
        public static void ListFiles()
        {
            string root = GetRoot();
            List<string> files = Collect(root);

            if (files.Count == 0)
            {
                Debug.Log("저장 파일이 없다: " + root);
                return;
            }

            string text = "저장 파일 " + files.Count + "개 · " + root;

            for (int i = 0; i < files.Count; i++)
            {
                FileInfo info = new FileInfo(files[i]);
                text += "\n  " + info.Name + "  (" + info.Length + " 바이트, "
                    + info.LastWriteTime.ToString("MM-dd HH:mm") + ")";
            }

            Debug.Log(text);
        }

        /// <summary>
        /// 저장 데이터를 전부 지운다.
        /// 프로필 셋과 설정이 다 사라져 게임을 처음 켠 상태가 된다.
        /// </summary>
        [MenuItem("Slot Hero/저장 데이터/전부 지우기", priority = 22)]
        public static void ClearAll()
        {
            string root = GetRoot();
            List<string> files = Collect(root);

            if (files.Count == 0)
            {
                Debug.Log("지울 저장 파일이 없다: " + root);
                return;
            }

            if (!Confirm(
                "저장 데이터를 전부 지운다",
                "프로필 셋과 설정이 모두 사라진다. 되돌릴 수 없다.\n\n"
                + root + "\n파일 " + files.Count + "개"))
            {
                return;
            }

            Delete(files);
            Debug.Log("저장 데이터를 전부 지웠다. 파일 " + files.Count + "개 · " + root);
        }

        /// <summary>
        /// 런만 지우고 프로필과 설정은 남긴다.
        /// 같은 프로필로 런을 처음부터 다시 돌려 볼 때 쓴다.
        /// </summary>
        [MenuItem("Slot Hero/저장 데이터/진행 중인 런만 지우기", priority = 23)]
        public static void ClearRuns()
        {
            SaveConfig config = UiSceneBuilder.LoadConfig<SaveConfig>("Save", "SaveConfig");

            if (config == null)
            {
                Debug.LogWarning("저장 설정 에셋을 찾지 못했다.");
                return;
            }

            string root = GetRoot();
            List<string> runs = new List<string>();

            for (int i = 0; i < config.ProfileCount; i++)
            {
                string path = Path.Combine(root, config.GetRunFileName(i));

                if (File.Exists(path))
                {
                    runs.Add(path);
                }
            }

            if (runs.Count == 0)
            {
                Debug.Log("진행 중인 런이 없다.");
                return;
            }

            if (!Confirm(
                "진행 중인 런을 지운다",
                "프로필과 설정은 남는다. 런만 사라진다.\n\n파일 " + runs.Count + "개"))
            {
                return;
            }

            Delete(runs);

            // 메타가 "런이 있다"고 적어 둔 것도 함께 내려야 한다.
            // 안 내리면 타이틀의 이어하기가 켜진 채로 남아 손상으로 읽힌다.
            ClearRunFlags(config, root);

            Debug.Log("진행 중인 런을 지웠다. 파일 " + runs.Count + "개");
        }

        /// <summary>
        /// 지우기 전에 묻는다.
        ///
        /// 배치 모드에서는 묻지 않고 그냥 지운다.
        /// 물을 사람이 없고 `DisplayDialog` 를 부르면 경고만 남기고 false 를 돌려주기 때문이다.
        /// 명령줄에서 부른 것은 이미 지우겠다는 뜻이다.
        /// </summary>
        private static bool Confirm(string title, string body)
        {
            if (Application.isBatchMode)
            {
                Debug.Log("배치 모드라 묻지 않고 지운다: " + title);
                return true;
            }

            return EditorUtility.DisplayDialog(title, body, "지운다", "그만둔다");
        }

        /// <summary>저장 폴더 자리. 플레이 모드가 아니어도 같은 자리를 가리킨다.</summary>
        private static string GetRoot()
        {
            SaveConfig config = UiSceneBuilder.LoadConfig<SaveConfig>("Save", "SaveConfig");
            string folder = config != null ? config.FolderName : "SlotHero";

            return Path.Combine(Application.persistentDataPath, folder);
        }

        private static List<string> Collect(string root)
        {
            List<string> files = new List<string>();

            if (Directory.Exists(root))
            {
                files.AddRange(Directory.GetFiles(root));
            }

            return files;
        }

        private static void Delete(List<string> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    File.Delete(files[i]);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("지우지 못했다: " + files[i] + " · " + e.Message);
                }
            }
        }

        /// <summary>
        /// 메타의 "저장된 런이 있다" 표시를 내린다.
        ///
        /// 런 파일만 지우고 이것을 안 내리면 메타는 런이 있다는데 파일이 없어
        /// 타이틀이 **런 데이터 손상**으로 읽는다.
        /// `SaveRepository.ReconcileRunFlag` 가 게임에서 하는 일을 여기서도 한다.
        /// </summary>
        private static void ClearRunFlags(SaveConfig config, string root)
        {
            for (int i = 0; i < config.ProfileCount * 2; i++)
            {
                // 본 파일과 백업을 다 내린다.
                // 백업만 런이 있다고 남아 있으면 되살아날 때 도로 손상으로 읽힌다.
                int profile = i / 2;
                string path = Path.Combine(
                    root,
                    i % 2 == 0
                        ? config.GetMetaFileName(profile)
                        : config.GetMetaBackupFileName(profile));

                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    MetaSaveData meta = JsonUtility.FromJson<MetaSaveData>(
                        File.ReadAllText(path, System.Text.Encoding.UTF8));

                    if (meta == null || !meta.HasSavedRun)
                    {
                        continue;
                    }

                    meta.HasSavedRun = false;
                    SaveRepository.StampMeta(meta);

                    File.WriteAllText(path, JsonUtility.ToJson(meta), System.Text.Encoding.UTF8);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("메타를 고치지 못했다: " + path + " · " + e.Message);
                }
            }
        }
    }
}
