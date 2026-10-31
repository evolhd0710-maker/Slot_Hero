namespace SlotHero.Popup
{
    /// <summary>
    /// 팝업 버튼을 늘어놓는 규칙.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 의 "버튼 개수에 따른 배치"를 그대로 옮겼다.
    ///
    /// 하나는 넓은 버튼 하나를 가운데에,
    /// 둘은 넓은 버튼 두 개를 넓은 간격으로,
    /// 셋은 좁은 버튼 세 개를 좁은 간격으로 둔다.
    /// 폭과 간격은 `PopupLayoutConfig` 에 있다. 기획서 값은 450 · 80 과 300 · 40 이고 둘과 셋 모두 전체 폭이 980 이다.
    /// 예전에는 이 넷이 여기 박혀 있었다(2026년 10월 22일 코덱스 검토).
    /// </summary>
    public static class PopupLayoutMath
    {
        /// <summary>버튼 수에 따른 폭.</summary>
        public static float GetButtonWidth(PopupLayoutConfig layout, int count)
        {
            return count >= 3 ? layout.NarrowButtonWidth : layout.WideButtonWidth;
        }

        /// <summary>버튼 수에 따른 간격.</summary>
        public static float GetButtonGap(PopupLayoutConfig layout, int count)
        {
            return count >= 3 ? layout.NarrowButtonGap : layout.WideButtonGap;
        }

        /// <summary>버튼들이 나란히 차지하는 전체 폭.</summary>
        public static float GetTotalWidth(PopupLayoutConfig layout, int count)
        {
            if (count <= 0)
            {
                return 0f;
            }

            return count * GetButtonWidth(layout, count) + (count - 1) * GetButtonGap(layout, count);
        }

        /// <summary>
        /// 버튼 하나의 가로 자리. 버튼 줄의 가운데를 0으로 본 값이다.
        /// 왼쪽 끝을 기준으로 잡으려면 <see cref="GetTotalWidth"/>의 절반을 더하면 된다.
        /// </summary>
        public static float GetButtonCenterOffset(PopupLayoutConfig layout, int count, int index)
        {
            if (count <= 0)
            {
                return 0f;
            }

            float width = GetButtonWidth(layout, count);
            float gap = GetButtonGap(layout, count);
            float total = GetTotalWidth(layout, count);

            float left = -total * 0.5f;
            return left + index * (width + gap) + width * 0.5f;
        }

        /// <summary>버튼 줄이 이 폭 안에 들어가는지.</summary>
        public static bool FitsWidth(PopupLayoutConfig layout, int count, float available)
        {
            return GetTotalWidth(layout, count) <= available;
        }
    }
}
