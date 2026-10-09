using System;
using UnityEditor;
using UnityEngine;

namespace SlotHero.ToolsEditor
{
    /// <summary>
    /// 설정 되돌리기부터 씬 만들기, 굴려 보기, 그림 뽑기까지 한 번에 돈다.
    ///
    /// 왜 필요한가. 하나씩 `-executeMethod` 로 부르면 그때마다 에디터가 새로 열려
    /// 한 바퀴에 몇 분씩 걸린다. 이 하나로 부르면 에디터가 한 번만 열린다.
    ///
    /// 명령줄에서 쓰는 법.
    ///   unity run "C:\...\SlotHero" --timeout 1200 -- ^
    ///     -executeMethod SlotHero.ToolsEditor.RunEverything.All -logFile "C:\...\로그.txt"
    ///
    /// 메뉴에서도 부를 수 있다. Slot Hero / 한 바퀴 다 돌리기
    /// </summary>
    public static class RunEverything
    {
        [MenuItem("Slot Hero/한 바퀴 다 돌리기", priority = 100)]
        public static void All()
        {
            Step("설정 에셋 만들기", CreateDefaultConfigs.Create);
            Step("설정 에셋 기본값으로 되돌리기", CreateDefaultConfigs.ResetLayoutAndVisual);

            // 되돌리기가 설정 안의 그림 연결까지 지운다.
            // 방 종류 아이콘과 태그 그림이 비므로 바로 다시 이어 준다.
            Step("그림 가져오기", ImportArt.Import);

            Step("UI 스킨 만들기", MakeUiSkin.Make);
            Step("확인용 씬 만들기", BuildPreviewScenes.BuildAll);
            Step("게임 씬 만들기", BuildGameScene.Build);
            Step("확인용 씬 눌러 보기", CheckPreviewScenes.CheckAll);
            Step("게임 씬 굴려 보기", CheckGameScene.Check);
            Step("그림으로 뽑기", CapturePreviewScenes.CaptureAll);

            Debug.Log("=== 한 바퀴 끝 ===");
        }

        /// <summary>
        /// 한 단계를 돌린다.
        /// 도중에 터져도 다음 단계로 넘어간다. 첫 단계에서 멈추면 뒤쪽 상태를 볼 수 없다.
        /// </summary>
        private static void Step(string title, Action step)
        {
            Debug.Log("=== " + title + " ===");

            try
            {
                step();
            }
            catch (Exception error)
            {
                Debug.LogError("=== " + title + " 에서 터졌다 === " + error);
            }
        }
    }
}
