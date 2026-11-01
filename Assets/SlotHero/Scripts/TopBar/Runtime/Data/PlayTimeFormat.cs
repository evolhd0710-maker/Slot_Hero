using System.Globalization;

namespace SlotHero.TopBar
{
    /// <summary>
    /// 플레이 시간 표기.
    /// 상단 UI 바 기획서 v0.2 / 04 런 요약 표기 상세 의
    /// "분, 초 단위만 표기하고 60분 이상도 시간 단위는 표기하지 않는다"를 따른다.
    /// 기획서 표기 예시는 123:45 이고 화면 예시는 59:40 이다.
    /// </summary>
    public static class PlayTimeFormat
    {
        /// <summary>
        /// 경과 초를 분과 초로 적는다.
        /// 60분을 넘겨도 시간으로 접지 않고 분을 그대로 늘린다.
        /// </summary>
        public static string Format(float elapsedSeconds, bool padMinutes)
        {
            if (elapsedSeconds < 0f)
            {
                elapsedSeconds = 0f;
            }

            int total = (int)elapsedSeconds;
            int minutes = total / 60;
            int seconds = total - minutes * 60;

            string minuteText = padMinutes
                ? minutes.ToString("00", CultureInfo.InvariantCulture)
                : minutes.ToString(CultureInfo.InvariantCulture);

            return minuteText + ":" + seconds.ToString("00", CultureInfo.InvariantCulture);
        }
    }
}
