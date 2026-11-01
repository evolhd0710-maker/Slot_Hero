using System;
using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 런이 끝나도 남는 값.
    /// 저장 시스템 기획서 v0.1 / 06 메타 데이터 의 표를 옮긴 것이다.
    ///
    /// 프로필마다 하나 둔다. 런이 끝날 때 갱신하고 누적한다.
    /// 갱신할 때 직전 값을 지우지 않고 백업으로 남긴다. 백업은 최신 하나만 둔다.
    /// </summary>
    [Serializable]
    public class MetaSaveData
    {
        /// <summary>프로필 번호. 0 부터 센다.</summary>
        public int ProfileIndex;

        /// <summary>프로필 이름.</summary>
        public string ProfileName = string.Empty;

        /// <summary>누적 플레이 시간. 초 단위다.</summary>
        public int TotalPlayTimeSeconds;

        /// <summary>
        /// 마지막으로 논 날짜. `2026-09-23` 꼴이다.
        /// 런 데이터를 저장할 때마다 오늘로 맞춘다(`SaveService.WriteRun`). 런을 끝낼 때만 바꾸면
        /// 런을 하다 나간 프로필이 더 예전에 논 것으로 보여 게임을 켤 때 골라지지 않았다. 2026년 10월 9일 원재가 정했다.
        /// </summary>
        public string LastPlayedDate = string.Empty;

        /// <summary>
        /// 진행 중인 런 데이터가 있는지.
        /// 10 예외 처리 표가 이 값과 실제 런 파일이 어긋나는 경우를 따로 다룬다.
        /// </summary>
        public bool HasSavedRun;

        /// <summary>
        /// 런을 몇 번 시작했는지. **런을 시작할 때 오르고** 오른 값이 그 런의 번호다(`SaveService.StartRun`).
        /// 예전에는 런을 끝낼 때 올라, 새 게임으로 버리거나 손상으로 지운 런 다음에 시작한 런이 같은 번호를 다시 받았다.
        /// 2026년 10월 9일 원재가 "시작 시가 맞다" 고 정했다.
        /// </summary>
        public int RunCount;

        /// <summary>해금한 것들.</summary>
        public List<string> UnlockedIds = new List<string>();

        /// <summary>도전과제와 그 달성 상태.</summary>
        public List<AchievementRecord> Achievements = new List<AchievementRecord>();

        /// <summary>여러 런에 걸쳐 쌓인 통계.</summary>
        public LifetimeStatisticsData Lifetime = new LifetimeStatisticsData();

        /// <summary>지난 런 기록 요약. 최신 것이 앞에 온다.</summary>
        public List<RunRecordSummary> RunRecords = new List<RunRecordSummary>();

        /// <summary>만나거나 얻은 적이 있는 것들. 도감에 쓴다.</summary>
        public List<string> DiscoveredIds = new List<string>();

        /// <summary>
        /// 저장 형식의 판 번호.
        /// 자료 구조가 바뀌면 이 값으로 예전 파일을 가려내 올린다.
        /// </summary>
        public int FormatVersion = SaveFormat.Current;

        /// <summary>올바르게 저장되었는지 검사하는 값.</summary>
        public string Checksum = string.Empty;

        /// <summary>빈 프로필의 메타 데이터를 만든다.</summary>
        public static MetaSaveData CreateEmpty(int profileIndex, string profileName)
        {
            MetaSaveData data = new MetaSaveData();
            data.ProfileIndex = profileIndex;
            data.ProfileName = profileName ?? string.Empty;
            return data;
        }

        /// <summary>해금한다. 이미 해금했으면 아무것도 하지 않는다.</summary>
        public bool Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || UnlockedIds.Contains(id))
            {
                return false;
            }

            UnlockedIds.Add(id);
            return true;
        }

        /// <summary>해금했는지.</summary>
        public bool IsUnlocked(string id)
        {
            return UnlockedIds.Contains(id);
        }

        /// <summary>도감에 올린다. 이미 있으면 아무것도 하지 않는다.</summary>
        public bool Discover(string id)
        {
            if (string.IsNullOrEmpty(id) || DiscoveredIds.Contains(id))
            {
                return false;
            }

            DiscoveredIds.Add(id);
            return true;
        }

        /// <summary>도전과제 하나를 찾는다. 없으면 만들어 넣는다.</summary>
        public AchievementRecord GetOrCreateAchievement(string id, int goal)
        {
            for (int i = 0; i < Achievements.Count; i++)
            {
                if (Achievements[i].AchievementId == id)
                {
                    return Achievements[i];
                }
            }

            AchievementRecord record = new AchievementRecord();
            record.AchievementId = id;
            record.Goal = goal;
            Achievements.Add(record);
            return record;
        }

        /// <summary>
        /// 도전과제를 얼마나 했는지 적는다.
        /// 목표에 닿으면 달성으로 바꾸고 그날 날짜를 남긴다. 달성한 뒤에는 건드리지 않는다.
        /// </summary>
        public bool ReportAchievement(string id, int goal, int progress, string today)
        {
            AchievementRecord record = GetOrCreateAchievement(id, goal);
            if (record.Achieved)
            {
                return false;
            }

            record.Progress = progress > record.Goal ? record.Goal : progress;

            if (record.Progress < record.Goal)
            {
                return false;
            }

            record.Achieved = true;
            record.AchievedDate = today ?? string.Empty;
            return true;
        }

        /// <summary>런이 끝났을 때 누적한다. 지난 런 기록은 최신 것부터 정해진 개수만 남긴다.</summary>
        public void AccumulateRun(
            RunSaveData run, bool cleared, string today, int runRecordLimit)
        {
            if (run == null)
            {
                return;
            }

            // 런 횟수는 시작할 때 이미 셌다. 바꾸기 전에 시작한 런은 시작 때 세지 않았으므로 그 번호까지 맞춘다.
            if (RunCount < run.RunNumber)
            {
                RunCount = run.RunNumber;
            }

            LastPlayedDate = today ?? string.Empty;
            TotalPlayTimeSeconds += run.Status.ElapsedSeconds;
            Lifetime.Accumulate(run, cleared);

            RunRecordSummary summary = new RunRecordSummary();
            summary.RunNumber = run.RunNumber;
            summary.StageIndex = run.StageIndex;
            summary.RoomsCleared = run.Statistics.RoomsCleared;
            summary.PlayTimeSeconds = run.Status.ElapsedSeconds;
            summary.Cleared = cleared;
            summary.Date = today ?? string.Empty;

            RunRecords.Insert(0, summary);

            if (runRecordLimit > 0)
            {
                while (RunRecords.Count > runRecordLimit)
                {
                    RunRecords.RemoveAt(RunRecords.Count - 1);
                }
            }

            HasSavedRun = false;
        }

        /// <summary>값이 앞뒤가 맞는지.</summary>
        public bool IsConsistent()
        {
            if (ProfileIndex < 0 || RunCount < 0 || TotalPlayTimeSeconds < 0)
            {
                return false;
            }

            return Lifetime != null && Lifetime.IsConsistent();
        }
    }

    /// <summary>도전과제 하나.</summary>
    [Serializable]
    public class AchievementRecord
    {
        public string AchievementId = string.Empty;
        public int Progress;
        public int Goal;
        public bool Achieved;
        public string AchievedDate = string.Empty;
    }

    /// <summary>지난 런 하나의 요약.</summary>
    [Serializable]
    public struct RunRecordSummary
    {
        public int RunNumber;
        public int StageIndex;
        public int RoomsCleared;
        public int PlayTimeSeconds;
        public bool Cleared;
        public string Date;
    }

    /// <summary>여러 런에 걸쳐 쌓이는 통계.</summary>
    [Serializable]
    public class LifetimeStatisticsData
    {
        public int RunsPlayed;
        public int RunsCleared;
        public int TotalRoomsCleared;
        public int TotalMonstersKilled;
        public int TotalElitesKilled;
        public int BestStageReached;
        public int BiggestHit;

        /// <summary>런 하나를 누적한다.</summary>
        public void Accumulate(RunSaveData run, bool cleared)
        {
            if (run == null)
            {
                return;
            }

            RunsPlayed++;
            if (cleared)
            {
                RunsCleared++;
            }

            TotalRoomsCleared += run.Statistics.RoomsCleared;
            TotalMonstersKilled += run.Statistics.MonstersKilled;
            TotalElitesKilled += run.Statistics.ElitesKilled;

            if (run.StageIndex > BestStageReached)
            {
                BestStageReached = run.StageIndex;
            }

            if (run.Statistics.BiggestHit > BiggestHit)
            {
                BiggestHit = run.Statistics.BiggestHit;
            }
        }

        /// <summary>값이 앞뒤가 맞는지. 깬 런이 논 런보다 많을 수 없다.</summary>
        public bool IsConsistent()
        {
            if (RunsPlayed < 0 || RunsCleared < 0 || RunsCleared > RunsPlayed)
            {
                return false;
            }

            return TotalRoomsCleared >= 0
                && TotalMonstersKilled >= 0
                && TotalElitesKilled >= 0
                && BestStageReached >= 0
                && BiggestHit >= 0;
        }
    }
}
