using System.Globalization;

namespace SlotHero.Profile
{
    /// <summary>
    /// 프로필 카드에 적을 글을 만든다.
    /// 인게임 화면 기획서 v0.2 / 06 프로필 선택 화면 의 와이어프레임 예시를 그대로 낼 수 있게 맞췄다.
    /// </summary>
    public static class ProfileText
    {
        /// <summary>
        /// 플레이 시간 줄.
        /// 와이어프레임 예시는 "플레이 시간 12시간 40분"과 "플레이 시간 0시간 0분"이다.
        /// 논 적이 없어도 0 으로 적는다.
        /// </summary>
        public static string PlayTime(ProfileVisualConfig visual, ProfileSummary summary)
        {
            if (visual == null)
            {
                return string.Empty;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                visual.PlayTimeFormat,
                summary.PlayHours,
                summary.PlayMinutes);
        }

        /// <summary>
        /// 마지막 플레이 줄.
        /// 와이어프레임 예시는 "마지막 플레이 2026-09-03"과 "마지막 플레이 없음"이다.
        /// </summary>
        public static string LastPlayed(ProfileVisualConfig visual, ProfileSummary summary)
        {
            if (visual == null)
            {
                return string.Empty;
            }

            string date = summary.HasPlayed ? summary.LastPlayedDate : visual.LastPlayedNoneText;
            return string.Format(CultureInfo.InvariantCulture, visual.LastPlayedFormat, date);
        }

        /// <summary>
        /// 현재 프로필 버튼에 적을 글.
        /// 기획서의 "현재 프로필명을 표시한다"에 해당한다.
        /// 고른 프로필이 없으면 그 자리에 대신 적을 글을 낸다.
        /// </summary>
        public static string CurrentProfile(ProfileVisualConfig visual, ProfileList list)
        {
            if (visual == null)
            {
                return string.Empty;
            }

            if (list == null || string.IsNullOrEmpty(list.CurrentName))
            {
                return visual.NoProfileText;
            }

            return list.CurrentName;
        }
    }
}
