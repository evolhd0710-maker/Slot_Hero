using System;

namespace SlotHero.Settings
{
    /// <summary>
    /// 일반 탭의 창 모드와 연출 속도를 화면에 걸 때 쓰는 규칙(2026년 11월 11일 원재가 설정 적용을 반영하라고 했다).
    /// 인게임 화면 기획서 v0.2 / 07 설정 화면 의 일반 탭이다. 값은 선택지 차례(0 부터)로 저장된다.
    ///
    /// 기획서는 선택지 이름만 정했다. 빠름과 스킵이 얼마나 빠른지, 창 크기는 내가 정했고 수치는 `SettingsCatalogConfig` 에 있다.
    /// </summary>
    public static class DisplaySettingRules
    {
        /// <summary>설정 목록의 창 모드 식별자. 선택지는 창, 전체 화면, 테두리 없는 창이다.</summary>
        public const string ScreenModeId = "screenMode";

        /// <summary>설정 목록의 연출 속도 식별자. 선택지는 보통, 빠름, 스킵이다.</summary>
        public const string PresentationSpeedId = "presentationSpeed";

        /// <summary>설정 목록의 자동 저장 표시 식별자.</summary>
        public const string AutoSaveNoticeId = "autoSaveNotice";

        public const int Windowed = 0;
        public const int FullScreen = 1;
        public const int Borderless = 2;

        public const int SpeedNormal = 0;
        public const int SpeedFast = 1;
        public const int SpeedSkip = 2;

        /// <summary>저장된 값을 창 모드 차례로 읽는다. 모르는 값은 전체 화면(기본값)이다.</summary>
        public static int ReadScreenMode(float raw)
        {
            int index = (int)Math.Round(raw);
            return index >= Windowed && index <= Borderless ? index : FullScreen;
        }

        /// <summary>저장된 값을 연출 속도 차례로 읽는다. 모르는 값은 보통이다.</summary>
        public static int ReadPresentationSpeed(float raw)
        {
            int index = (int)Math.Round(raw);
            return index >= SpeedNormal && index <= SpeedSkip ? index : SpeedNormal;
        }

        /// <summary>연출 속도 배율. 보통 1, 빠름과 스킵은 설정 값이다. 1 보다 작게 받으면 1 이다.</summary>
        public static float GetSpeed(int speedIndex, float fast, float skip)
        {
            switch (speedIndex)
            {
                case SpeedFast:
                    return Math.Max(1f, fast);
                case SpeedSkip:
                    return Math.Max(1f, skip);
                default:
                    return 1f;
            }
        }

        /// <summary>스킵이면 누르지 않아도 건너뛸 수 있는 연출을 바로 끝 모습으로 넘긴다.</summary>
        public static bool SkipsEffects(int speedIndex)
        {
            return speedIndex == SpeedSkip;
        }

        /// <summary>
        /// 창 모드일 때 창 크기. 모니터의 ratio 만큼 차지하는 가장 큰 16:9 크기다.
        /// 게임 화면이 16:9(1920 × 1080 기준)라 창도 16:9 로 둔다. 모니터 크기를 모르면 1280 × 720 이다.
        /// </summary>
        public static void GetWindowSize(int displayWidth, int displayHeight, float ratio, out int width, out int height)
        {
            if (displayWidth <= 0 || displayHeight <= 0)
            {
                width = 1280;
                height = 720;
                return;
            }

            float r = ratio > 0f && ratio <= 1f ? ratio : 0.8f;
            float maxWidth = displayWidth * r;
            float maxHeight = displayHeight * r;

            float w = maxWidth;
            float h = w * 9f / 16f;
            if (h > maxHeight)
            {
                h = maxHeight;
                w = h * 16f / 9f;
            }

            width = Math.Max(640, (int)Math.Round(w));
            height = Math.Max(360, (int)Math.Round(h));
        }
    }
}
