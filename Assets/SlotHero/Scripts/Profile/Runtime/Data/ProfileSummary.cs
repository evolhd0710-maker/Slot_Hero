using System;

namespace SlotHero.Profile
{
    /// <summary>
    /// 프로필 자리 하나의 요약.
    /// 인게임 화면 기획서 v0.2 / 06 프로필 선택 화면 의 프로필 카드에 적는 것이다.
    ///
    /// 카드에는 프로필명과 플레이 시간, 마지막 플레이 날짜만 적는다.
    /// 그 안에 무엇이 들어 있는지는 저장 시스템 기획서 소관이라 여기서는 받아 적기만 한다.
    /// </summary>
    [Serializable]
    public struct ProfileSummary
    {
        /// <summary>프로필명. 빈 자리면 비어 있다.</summary>
        public string Name;

        /// <summary>그 프로필로 논 시간. 초로 센다.</summary>
        public int PlayTimeSeconds;

        /// <summary>
        /// 마지막으로 논 날짜. 와이어프레임 예시는 "2026-09-03"이다.
        /// 비어 있으면 아직 논 적이 없다는 뜻이다.
        /// 저장에 그대로 실으려고 글로 둔다.
        /// </summary>
        public string LastPlayedDate;

        /// <summary>
        /// 저장 파일을 읽을 수 없는지.
        /// 기획서의 "저장 파일을 읽을 수 없는 카드 입력 시 저장 데이터 오류 팝업을 출력한다"에 쓴다.
        /// </summary>
        public bool IsBroken;

        /// <summary>
        /// 손상이 아니라 지금 잠시 읽지 못한 것인지. `IsBroken` 도 함께 켜져 들어갈 수 없다.
        /// 눌렀을 때 손상 알림 대신 "다시 시도" 를 안내한다. 2026년 10월 9일 외부 검토가 둘을 섞지 말라고 짚었다.
        /// </summary>
        public bool IsReadFailed;

        /// <summary>빈 자리인지. 이름이 없으면 빈 자리다.</summary>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Name); }
        }

        /// <summary>눌렀을 때 그 프로필로 바로 넘어갈 수 있는지.</summary>
        public bool CanEnter
        {
            get { return !IsEmpty && !IsBroken; }
        }

        /// <summary>플레이 시간의 시간 부분.</summary>
        public int PlayHours
        {
            get { return PlayTimeSeconds <= 0 ? 0 : PlayTimeSeconds / 3600; }
        }

        /// <summary>플레이 시간의 분 부분. 시간을 뺀 나머지다.</summary>
        public int PlayMinutes
        {
            get { return PlayTimeSeconds <= 0 ? 0 : PlayTimeSeconds % 3600 / 60; }
        }

        /// <summary>논 적이 있는지.</summary>
        public bool HasPlayed
        {
            get { return !string.IsNullOrEmpty(LastPlayedDate); }
        }

        /// <summary>비어 있는 자리.</summary>
        public static ProfileSummary Empty()
        {
            return new ProfileSummary();
        }

        /// <summary>이름과 기록이 있는 자리.</summary>
        public static ProfileSummary Filled(string name, int playTimeSeconds, string lastPlayedDate)
        {
            ProfileSummary summary = new ProfileSummary();
            summary.Name = name;
            summary.PlayTimeSeconds = playTimeSeconds;
            summary.LastPlayedDate = lastPlayedDate;
            return summary;
        }

        /// <summary>읽을 수 없는 자리. 이름은 남아 있을 수도 있다.</summary>
        public static ProfileSummary Broken(string name)
        {
            ProfileSummary summary = new ProfileSummary();
            summary.Name = name;
            summary.IsBroken = true;
            return summary;
        }

        /// <summary>지금 읽지 못한 자리. 이름을 몰라 기본 이름을 적는다. 다음에 목록을 다시 만들 때 다시 읽는다.</summary>
        public static ProfileSummary Unreadable(string name)
        {
            ProfileSummary summary = Broken(name);
            summary.IsReadFailed = true;
            return summary;
        }
    }
}
