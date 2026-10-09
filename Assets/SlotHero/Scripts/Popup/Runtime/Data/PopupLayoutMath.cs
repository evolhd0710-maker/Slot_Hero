using SlotHero.Ui;

namespace SlotHero.Popup
{
    /// <summary>
    /// 팝업 버튼을 늘어놓는 규칙.
    /// 인게임 화면 기획서 v0.2 / 08 화면 팝업 의 "버튼 개수에 따른 배치"를 그대로 옮겼다.
    ///
    /// 하나는 폭 450 버튼 하나를 가운데에,
    /// 둘은 폭 450 버튼 두 개를 간격 80으로,
    /// 셋은 폭 300 버튼 세 개를 간격 40으로 둔다.
    /// 둘과 셋은 모두 전체 폭이 980이 되어 같은 자리에서 시작한다.
    ///
    /// 아래 넷은 좌표 배율을 타야 해서 const 가 아니라 속성이다.
    /// const 로 두면 배율이 곱해지지 않는다. 배율이 12 였을 때 버튼이 화면에서 1/12 크기로 쪼그라들었다.
    /// </summary>
    public static class PopupLayoutMath
    {
        /// <summary>버튼이 하나일 때와 둘일 때의 폭.</summary>
        public static float WideButtonWidth { get { return UiScale.Px(450f); } }

        /// <summary>버튼이 셋일 때의 폭.</summary>
        public static float NarrowButtonWidth { get { return UiScale.Px(300f); } }

        /// <summary>버튼이 둘일 때의 간격.</summary>
        public static float WideButtonGap { get { return UiScale.Px(80f); } }

        /// <summary>버튼이 셋일 때의 간격.</summary>
        public static float NarrowButtonGap { get { return UiScale.Px(40f); } }

        /// <summary>버튼 수에 따른 폭.</summary>
        public static float GetButtonWidth(int count)
        {
            return count >= 3 ? NarrowButtonWidth : WideButtonWidth;
        }

        /// <summary>버튼 수에 따른 간격.</summary>
        public static float GetButtonGap(int count)
        {
            return count >= 3 ? NarrowButtonGap : WideButtonGap;
        }

        /// <summary>버튼들이 나란히 차지하는 전체 폭.</summary>
        public static float GetTotalWidth(int count)
        {
            if (count <= 0)
            {
                return 0f;
            }

            return count * GetButtonWidth(count) + (count - 1) * GetButtonGap(count);
        }

        /// <summary>
        /// 버튼 하나의 가로 자리. 버튼 줄의 가운데를 0으로 본 값이다.
        /// 왼쪽 끝을 기준으로 잡으려면 <see cref="GetTotalWidth"/>의 절반을 더하면 된다.
        /// </summary>
        public static float GetButtonCenterOffset(int count, int index)
        {
            if (count <= 0)
            {
                return 0f;
            }

            float width = GetButtonWidth(count);
            float gap = GetButtonGap(count);
            float total = GetTotalWidth(count);

            float left = -total * 0.5f;
            return left + index * (width + gap) + width * 0.5f;
        }

        /// <summary>버튼 줄이 이 폭 안에 들어가는지.</summary>
        public static bool FitsWidth(int count, float available)
        {
            return GetTotalWidth(count) <= available;
        }
    }
}
