using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.RunResult
{
    /// <summary>
    /// 런 종료 결과 화면의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기는
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RunResultVisualConfig",
        menuName = "Slot Hero/런 종료 결과/런 종료 결과 화면 표시 설정",
        order = 2)]
    public class RunResultVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("배경 그림이 없을 때 깔리는 색. 배경은 배경 요청 기획서 소관이라 검정만 둔다.")]
        public Color BackgroundColor = Color.black;

        [Tooltip("여섯 칸과 타이틀로 버튼의 배경. 와이어프레임 기준 D9D9D9.")]
        public Color PanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Header("글자 색")]
        [Tooltip("칸 안의 글자. 04 화면 공통 규칙 의 1F1B16.")]
        public Color TextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("패배. 04 화면 공통 규칙 의 강조 텍스트 9E2B25. 와이어프레임도 같다.")]
        public Color DefeatColor = new Color(0.620f, 0.169f, 0.145f, 1f);

        [Tooltip("클리어. 기획서와 와이어프레임에 없어 기본 글자색을 그대로 두었다. 정해지면 바꾼다.")]
        public Color ClearColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Header("글자 크기")]
        [Tooltip("결과. 04 화면 공통 규칙 의 강조 텍스트 64픽셀 굵게. 와이어프레임 실측도 같다.")]
        [Min(1f)]
        public float OutcomeFontSize = UiScale.Px(64f);

        [Tooltip("도달 지점. 와이어프레임에서 잰 30픽셀 굵게.")]
        [Min(1f)]
        public float LocationFontSize = UiScale.Px(30f);

        [Tooltip("칸 안의 소제목. 04 화면 공통 규칙 의 항목 텍스트 26픽셀 굵게.")]
        [Min(1f)]
        public float HeadingFontSize = UiScale.Px(26f);

        [Tooltip("칸 안의 본문. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        public float BodyFontSize = UiScale.Px(24f);

        [Tooltip("타이틀로 버튼. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        public float ButtonFontSize = UiScale.Px(30f);

        [Header("상태")]
        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

        [Header("문구")]
        [Tooltip("패배로 끝났을 때 적는 글. 기획서 본문은 사망, 와이어프레임은 패배다.")]
        public string DefeatText = "패배";

        [Tooltip("클리어로 끝났을 때 적는 글.")]
        public string ClearText = "클리어";

        [Tooltip("타이틀로 버튼에 적는 글.")]
        public string TitleButtonText = "타이틀로";

        [Header("줄을 잇는 방법")]
        [Tooltip("한 줄 안에서 항목을 잇는 글자. 와이어프레임 기준 가운뎃점이다.")]
        public string Separator = " · ";

        [Tooltip("도달 지점의 스테이지. {0} 자리에 번호가 들어간다.")]
        public string StageFormat = "스테이지 {0}";

        [Tooltip("도달 지점의 단계. {0} 자리에 번호가 들어간다.")]
        public string RoomFormat = "{0}번째 방";

        [Header("최종 구성 소제목")]
        [Tooltip("문양 소제목. {0} 자리에 전체 장수가 들어간다.")]
        public string SymbolHeadingFormat = "문양 {0}장";

        [Tooltip("태그 소제목.")]
        public string TagHeadingText = "태그 정보";

        [Tooltip("유물 소제목.")]
        public string RelicHeadingText = "유물";

        [Tooltip("코인 소제목.")]
        public string CoinHeadingText = "코인";

        [Tooltip("문양 한 종류의 표기. {0} 이름, {1} 개수.")]
        public string SymbolEntryFormat = "{0} {1}";

        [Tooltip("태그 하나의 표기. {0} 이름, {1} 장수.")]
        public string TagEntryFormat = "{0} {1}";

        /// <summary>결과에 맞는 글.</summary>
        public string GetOutcomeText(RunOutcome outcome)
        {
            return outcome == RunOutcome.Clear ? ClearText : DefeatText;
        }

        /// <summary>결과에 맞는 색.</summary>
        public Color GetOutcomeColor(RunOutcome outcome)
        {
            return outcome == RunOutcome.Clear ? ClearColor : DefeatColor;
        }

        /// <summary>줄 하나의 글자 크기. 소제목은 굵고 크다.</summary>
        public float GetLineFontSize(bool strong)
        {
            return strong ? HeadingFontSize : BodyFontSize;
        }

        /// <summary>타이틀로 버튼의 색.</summary>
        public Color GetButtonColor(bool hovering, bool pressed)
        {
            float brightness = 1f;
            if (pressed)
            {
                brightness = PressedBrightness;
            }
            else if (hovering)
            {
                brightness = HoverBrightness;
            }

            return new Color(
                PanelColor.r * brightness,
                PanelColor.g * brightness,
                PanelColor.b * brightness,
                PanelColor.a);
        }
    }
}
