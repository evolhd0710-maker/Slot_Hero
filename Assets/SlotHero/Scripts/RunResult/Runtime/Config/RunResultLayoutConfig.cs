using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.RunResult
{
    /// <summary>
    /// 런 종료 결과 화면의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 과 그 와이어프레임에서 잰 값이다.
    ///
    /// 기획서가 정한 것은 결과를 화면 위쪽에 크게 두고
    /// 타이틀로를 우측 하단에 둔다는 것까지다.
    /// 나머지 네 칸의 자리는 와이어프레임에서 재 왔다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RunResultLayoutConfig",
        menuName = "Slot Hero/런 종료 결과/런 종료 결과 화면 자리 설정",
        order = 1)]
    public class RunResultLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("결과")]
        [Tooltip("결과 칸의 자리. 칸 폭을 빼고 좌우로 똑같이 남겨 화면 가운데에 둔 값이다.")]
        public Vector2 OutcomePosition = UiScale.V(580f, 80f);

        [Tooltip("결과 칸의 크기. 와이어프레임 실측 768 × 119 를 10 단위로 옮긴 값이다.")]
        public Vector2 OutcomeSize = UiScale.V(760f, 120f);

        [Header("도달 지점")]
        [Tooltip("도달 지점 칸의 자리. 와이어프레임 실측 x 134 · y 248 을 10 단위로 옮긴 값이다.")]
        public Vector2 LocationPosition = UiScale.V(130f, 250f);

        [Tooltip("도달 지점 칸의 크기. 와이어프레임 실측 787 × 108 을 10 단위로 옮긴 값이다.")]
        public Vector2 LocationSize = UiScale.V(790f, 110f);

        [Header("최종 구성")]
        [Tooltip("최종 구성 칸의 자리. 도달 지점 칸 아래로 30 띄운 값이다.")]
        public Vector2 FinalBuildPosition = UiScale.V(130f, 390f);

        [Tooltip("최종 구성 칸의 크기. 아래 끝이 오른쪽 열과 같은 890 이 되게 잡은 값이다.")]
        public Vector2 FinalBuildSize = UiScale.V(790f, 500f);

        [Header("이번 런 기록")]
        [Tooltip("이번 런 기록 칸의 자리. 왼쪽 열과 80 띄우고 오른쪽 여백을 130 으로 맞춘 값이다.")]
        public Vector2 RunRecordPosition = UiScale.V(1000f, 250f);

        [Tooltip("이번 런 기록 칸의 크기. 와이어프레임 실측 787 × 367 을 10 단위로 옮긴 값이다.")]
        public Vector2 RunRecordSize = UiScale.V(790f, 370f);

        [Header("해금 및 도전과제")]
        [Tooltip("해금 칸의 자리. 이번 런 기록 칸 아래로 30 띄운 값이다.")]
        public Vector2 UnlockPosition = UiScale.V(1000f, 650f);

        [Tooltip("해금 칸의 크기. 와이어프레임 실측 787 × 238 을 10 단위로 옮긴 값이다.")]
        public Vector2 UnlockSize = UiScale.V(790f, 240f);

        [Header("타이틀로")]
        [Tooltip("타이틀로 버튼의 자리. 오른쪽 끝을 오른쪽 열의 오른쪽 끝 1790 에 맞춘 값이다.")]
        public Vector2 TitleButtonPosition = UiScale.V(1350f, 930f);

        [Tooltip("타이틀로 버튼의 크기. 와이어프레임 실측 442 × 97 을 10 단위로 옮긴 값이다.")]
        public Vector2 TitleButtonSize = UiScale.V(440f, 100f);

        [Header("칸 안쪽")]
        [Tooltip("칸 안쪽 좌우 여백. 와이어프레임 실측 25 를 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        public float PanelPadding = UiScale.Px(20f);

        [Tooltip("칸 안쪽 위아래 여백. 와이어프레임 실측 26 을 10 단위로 옮긴 값이다.")]
        [Min(0f)]
        public float PanelTopPadding = UiScale.Px(30f);

        [Tooltip("줄 하나에서 다음 줄까지의 거리. 와이어프레임 실측 31 을 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        public float LineStep = UiScale.Px(30f);

        /// <summary>칸 안에서 줄 하나가 차지하는 자리. 화면 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetLinePosition(Vector2 panelPosition, int index)
        {
            float x = panelPosition.x + PanelPadding;
            float y = panelPosition.y + PanelTopPadding + index * LineStep;
            return new Vector2(x, y);
        }

        /// <summary>칸 안에서 줄 하나가 쓸 수 있는 크기.</summary>
        public Vector2 GetLineSize(Vector2 panelSize)
        {
            return new Vector2(panelSize.x - PanelPadding * 2f, LineStep);
        }

        /// <summary>그 칸에 스크롤 없이 들어가는 줄 수.</summary>
        public int GetLinesInPanel(Vector2 panelSize)
        {
            if (LineStep <= 0f)
            {
                return 0;
            }

            float available = panelSize.y - PanelTopPadding * 2f;
            if (available <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt(available / LineStep);
        }

        /// <summary>줄 수에 맞춘 칸 내용의 높이. 스크롤 안에 담긴다.</summary>
        public float GetContentHeight(Vector2 panelSize, int lineCount)
        {
            if (lineCount <= 0)
            {
                return panelSize.y;
            }

            float height = PanelTopPadding * 2f + lineCount * LineStep;
            return height < panelSize.y ? panelSize.y : height;
        }

        /// <summary>그만큼의 줄을 보여 주려면 스크롤이 필요한지.</summary>
        public bool NeedsScroll(Vector2 panelSize, int lineCount)
        {
            return lineCount > GetLinesInPanel(panelSize);
        }

        /// <summary>결과 칸이 화면 가로 가운데에 있는지.</summary>
        public bool OutcomeIsCentered()
        {
            float left = OutcomePosition.x;
            float right = ReferenceResolution.x - (OutcomePosition.x + OutcomeSize.x);
            return Mathf.Abs(left - right) < 1f;
        }

        /// <summary>왼쪽 칸과 오른쪽 칸의 바깥 여백이 같은지. 와이어프레임 기준 둘 다 134다.</summary>
        public bool ColumnsAreSymmetric()
        {
            float left = FinalBuildPosition.x;
            float right = ReferenceResolution.x - (RunRecordPosition.x + RunRecordSize.x);
            return Mathf.Abs(left - right) < 1f;
        }

        /// <summary>왼쪽 열과 오른쪽 열의 아래 끝이 같은지.</summary>
        public bool ColumnsEndTogether()
        {
            float left = FinalBuildPosition.y + FinalBuildSize.y;
            float right = UnlockPosition.y + UnlockSize.y;
            return Mathf.Abs(left - right) < 1f;
        }

        /// <summary>왼쪽 칸과 오른쪽 칸이 겹치지 않는지.</summary>
        public bool ColumnsDoNotOverlap()
        {
            return FinalBuildPosition.x + FinalBuildSize.x <= RunRecordPosition.x;
        }

        private void OnValidate()
        {
            OutcomeSize.x = Mathf.Max(1f, OutcomeSize.x);
            OutcomeSize.y = Mathf.Max(1f, OutcomeSize.y);
            LocationSize.x = Mathf.Max(1f, LocationSize.x);
            LocationSize.y = Mathf.Max(1f, LocationSize.y);
            FinalBuildSize.x = Mathf.Max(1f, FinalBuildSize.x);
            FinalBuildSize.y = Mathf.Max(1f, FinalBuildSize.y);
            RunRecordSize.x = Mathf.Max(1f, RunRecordSize.x);
            RunRecordSize.y = Mathf.Max(1f, RunRecordSize.y);
            UnlockSize.x = Mathf.Max(1f, UnlockSize.x);
            UnlockSize.y = Mathf.Max(1f, UnlockSize.y);
            TitleButtonSize.x = Mathf.Max(1f, TitleButtonSize.x);
            TitleButtonSize.y = Mathf.Max(1f, TitleButtonSize.y);
            LineStep = Mathf.Max(1f, LineStep);
            PanelPadding = Mathf.Max(0f, PanelPadding);
            PanelTopPadding = Mathf.Max(0f, PanelTopPadding);
        }
    }
}
