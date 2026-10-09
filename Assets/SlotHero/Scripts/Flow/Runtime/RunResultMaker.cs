using SlotHero.CurrentBuild;
using SlotHero.RunResult;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 끝난 런을 12 런 종료 결과 화면이 쓰는 꼴로 바꾼다.
    /// 2026년 10월 8일에 `GameFlowController` 에서 떼어 냈다. `RunSession` 참고.
    /// </summary>
    public static class RunResultMaker
    {
        /// <summary>결과 화면에 넘길 자료를 만든다. meta 는 누적 기록을 적는 데 쓴다. 없으면 그 줄을 비운다.</summary>
        public static RunResultData Make(
            RunSaveData run,
            CurrentBuildSnapshot finalBuild,
            bool cleared,
            string endingText,
            MetaSaveData meta,
            RunResultVisualConfig resultVisual,
            CurrentBuildVisualConfig buildVisual)
        {
            RunResultData data = new RunResultData();

            if (run == null)
            {
                return data;
            }

            data.Outcome = cleared ? RunOutcome.Clear : RunOutcome.Defeat;
            data.LocationText = RunResultText.Location(
                resultVisual, run.StageIndex, run.Statistics.RoomsCleared, endingText);
            data.FinalBuild = RunResultText.BuildFinalBuild(finalBuild, buildVisual, resultVisual);

            int minutes = run.Status.ElapsedSeconds / 60;
            int seconds = run.Status.ElapsedSeconds % 60;

            data.RunRecord.Add(ResultLine.Body("플레이 시간 " + minutes + "분 " + seconds + "초"));
            data.RunRecord.Add(ResultLine.Body("지나온 방 " + run.Statistics.RoomsCleared + "개"));
            data.RunRecord.Add(ResultLine.Body(
                "처치한 몬스터 " + run.Statistics.MonstersKilled + "마리 · 엘리트 "
                + run.Statistics.ElitesKilled + "마리"));
            data.RunRecord.Add(ResultLine.Blank());
            data.RunRecord.Add(ResultLine.Body(
                "획득한 골드 " + run.Statistics.GoldGained + " · 소모한 골드 "
                + run.Statistics.GoldSpent));
            data.RunRecord.Add(ResultLine.Body("한 번에 준 가장 큰 피해 " + run.Statistics.BiggestHit));

            if (meta != null)
            {
                data.Unlocks.Add(ResultLine.Body("누적 런 " + meta.Lifetime.RunsPlayed + "회"));
                data.Unlocks.Add(ResultLine.Body("가장 멀리 간 스테이지 " + meta.Lifetime.BestStageReached));
            }

            return data;
        }
    }
}
