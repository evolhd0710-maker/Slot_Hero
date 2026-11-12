using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 윈도 실행 파일을 만든다. 메뉴 Slot Hero / 게임 빌드하기 에서 실행한다.
    /// 2026년 11월 1일 원재가 "필요한 파일만 담아서" 실행 파일을 만들라고 했다.
    ///
    /// 담는 씬은 빌드 설정의 씬(게임 씬 하나)이다.
    /// 필요한 것만 담으려고 두 가지를 한다.
    /// 1. 받아 둔 묶음(Vefects 데모)의 `Resources` 폴더를 빌드하는 동안만 이름을 바꿔 뺀다.
    ///    유니티는 `Resources` 안의 것을 쓰든 안 쓰든 모두 빌드에 넣는다. 끝나면 이름을 되돌린다
    /// 2. 빌드가 함께 만드는 디버그 정보 폴더(`_BurstDebugInformation_DoNotShip`)와
    ///    IL2CPP 백업 폴더(`_BackUpThisFolder_ButDontShipItWithYourGame`)를 지운다. 이름 그대로 나눠 주지 않는 폴더다
    ///
    /// 결과는 `slot hero/빌드 날짜` 폴더에 `Slot Hero Demo.exe` 로 나온다.
    /// </summary>
    public static class BuildGame
    {
        private const string ExeName = "Slot Hero Demo";

        /// 빌드에서 뺄 Resources 를 찾지 않는 곳. 우리 것과 TextMeshPro 기본 자료는 필요하다.
        private static readonly string[] KeepResources = { "Assets/SlotHero/", "Assets/TextMesh Pro/" };

        /// 빌드하는 동안 Resources 폴더에 붙이는 이름.
        private const string HiddenName = "Resources_빌드제외";

        [MenuItem("Slot Hero/게임 빌드하기")]
        public static void Build()
        {
            string root = Path.GetDirectoryName(ImportArt.SourceRoot);
            string folder = Path.Combine(root, "빌드 " + DateTime.Now.ToString("yyyyMMdd"));
            string exe = Path.Combine(folder, ExeName + ".exe");

            string[] scenes = EnabledScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("빌드 설정에 켜진 씬이 없다.");
                return;
            }

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            List<string> hidden = HidePackResources();
            BuildReport report;
            try
            {
                BuildPlayerOptions options = new BuildPlayerOptions();
                options.scenes = scenes;
                options.locationPathName = exe;
                options.target = BuildTarget.StandaloneWindows64;
                options.targetGroup = BuildTargetGroup.Standalone;
                options.options = BuildOptions.None;
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                RestorePackResources(hidden);
            }

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("빌드에 실패했다: " + report.summary.result + ", 오류 " + report.summary.totalErrors + "개");
                return;
            }

            // 디버그 정보(_BurstDebugInformation_DoNotShip)와 IL2CPP 백업(_BackUpThisFolder_ButDontShipItWithYourGame)은 나눠 주지 않는다.
            string[] patterns = { "*_DoNotShip", "*_ButDontShipItWithYourGame" };
            for (int p = 0; p < patterns.Length; p++)
            {
                string[] extraFolders = Directory.GetDirectories(folder, patterns[p]);
                for (int i = 0; i < extraFolders.Length; i++)
                {
                    Directory.Delete(extraFolders[i], true);
                }
            }

            long bytes = 0;
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                bytes += new FileInfo(files[i]).Length;
            }

            Debug.Log("게임을 빌드했다: " + exe + " · 파일 " + files.Length + "개 · " + (bytes / (1024f * 1024f)).ToString("0.0") + "MB");
        }

        private static string[] EnabledScenes()
        {
            List<string> scenes = new List<string>();
            EditorBuildSettingsScene[] all = EditorBuildSettings.scenes;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].enabled)
                {
                    scenes.Add(all[i].path);
                }
            }

            return scenes.ToArray();
        }

        /// <summary>우리 것이 아닌 Resources 폴더의 이름을 바꿔 빌드에서 뺀다. 바꾼 폴더의 새 경로를 돌려준다.</summary>
        private static List<string> HidePackResources()
        {
            List<string> hidden = new List<string>();
            string[] folders = AssetDatabase.GetSubFolders("Assets");
            Stack<string> stack = new Stack<string>(folders);

            while (stack.Count > 0)
            {
                string path = stack.Pop();
                if (IsKept(path))
                {
                    continue;
                }

                if (Path.GetFileName(path) == "Resources")
                {
                    string error = AssetDatabase.RenameAsset(path, HiddenName);
                    if (string.IsNullOrEmpty(error))
                    {
                        string moved = Path.GetDirectoryName(path).Replace('\\', '/') + "/" + HiddenName;
                        hidden.Add(moved);
                        Debug.Log("빌드에서 뺀다: " + path);
                    }
                    else
                    {
                        Debug.LogWarning("Resources 이름을 바꾸지 못했다: " + path + " · " + error);
                    }

                    continue;
                }

                string[] children = AssetDatabase.GetSubFolders(path);
                for (int i = 0; i < children.Length; i++)
                {
                    stack.Push(children[i]);
                }
            }

            return hidden;
        }

        private static void RestorePackResources(List<string> hidden)
        {
            for (int i = 0; i < hidden.Count; i++)
            {
                string error = AssetDatabase.RenameAsset(hidden[i], "Resources");
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError("Resources 이름을 되돌리지 못했다. 손으로 되돌린다: " + hidden[i] + " · " + error);
                }
            }
        }

        private static bool IsKept(string path)
        {
            string withSlash = path + "/";
            for (int i = 0; i < KeepResources.Length; i++)
            {
                if (withSlash.StartsWith(KeepResources[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
