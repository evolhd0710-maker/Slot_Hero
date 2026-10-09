using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Profile
{
    /// <summary>
    /// 프로필 선택 화면과 현재 프로필 버튼의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기는
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "ProfileVisualConfig",
        menuName = "Slot Hero/프로필/프로필 화면 표시 설정",
        order = 2)]
    public class ProfileVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("제목 칸과 뒤로 버튼. 와이어프레임 기준 D9D9D9.")]
        public Color PanelColor = new Color(0.851f, 0.851f, 0.851f, 1f);

        [Tooltip("화면 바탕. 배경 그림이 생기면 그 위에 깐다.")]
        public Color BackgroundColor = new Color(0.482f, 0.482f, 0.482f, 1f);

        [Header("프로필 카드")]
        [Tooltip("자리마다의 카드 색. 기획서가 카드마다 다른 배경색을 쓰게 한다. 와이어프레임에서 재 왔다.")]
        public List<Color> CardColors = new List<Color>
        {
            new Color(0.753f, 0.224f, 0.169f, 1f),
            new Color(0.153f, 0.522f, 0.235f, 1f),
            new Color(0.180f, 0.373f, 0.749f, 1f),
        };

        [Tooltip("프로필명. 기획서가 검은색으로 적게 한다.")]
        public Color NameColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("플레이 시간과 마지막 플레이 날짜. 기획서가 흰색으로 적게 한다.")]
        public Color InfoColor = Color.white;

        [Tooltip("비어 있음. 04 화면 공통 규칙 의 버튼 텍스트 색상 2 흰색이다.")]
        public Color EmptyTextColor = Color.white;

        [Header("수정과 삭제")]
        [Tooltip("버튼 바탕. 와이어프레임에서 카드 색이 살짝 비쳐 흰색 반투명으로 두었다.")]
        public Color ActionButtonColor = new Color(1f, 1f, 1f, 0.9f);

        [Tooltip("수정 글자. 04 화면 공통 규칙 의 1F1B16.")]
        public Color EditTextColor = new Color(0.122f, 0.106f, 0.086f, 1f);

        [Tooltip("삭제 글자. 되돌릴 수 없는 조작이라 04 화면 공통 규칙 의 강조색 9E2B25 를 쓴다.")]
        public Color DeleteTextColor = new Color(0.620f, 0.169f, 0.145f, 1f);

        [Header("현재 프로필 버튼")]
        [Tooltip("테두리 색. 와이어프레임에서 배경 위에 흰 테두리만 있다.")]
        public Color CurrentButtonBorderColor = new Color(1f, 1f, 1f, 0.7f);

        [Tooltip("테두리 안쪽 바탕. 와이어프레임에서 배경이 그대로 비친다.")]
        public Color CurrentButtonFillColor = new Color(0f, 0f, 0f, 0f);

        [Tooltip("현재 프로필명 글자 색.")]
        public Color CurrentButtonTextColor = Color.white;

        [Header("글자 크기")]
        [Tooltip("제목. 04 화면 공통 규칙 의 화면 주요 텍스트 40픽셀 굵게.")]
        [Min(1f)]
        public float TitleFontSize = UiScale.Px(40f);

        [Tooltip("프로필명. 04 화면 공통 규칙 의 주요 텍스트 32픽셀 굵게.")]
        [Min(1f)]
        public float NameFontSize = UiScale.Px(32f);

        [Tooltip("플레이 정보. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        public float InfoFontSize = UiScale.Px(24f);

        [Tooltip("비어 있음. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        public float EmptyFontSize = UiScale.Px(30f);

        [Tooltip("수정과 삭제. 04 화면 공통 규칙 의 보조 텍스트 20픽셀.")]
        [Min(1f)]
        public float ActionFontSize = UiScale.Px(20f);

        [Tooltip("뒤로. 04 화면 공통 규칙 의 버튼 텍스트 30픽셀 굵게.")]
        [Min(1f)]
        public float BackFontSize = UiScale.Px(30f);

        [Tooltip("현재 프로필명. 와이어프레임에서 잰 26픽셀 굵게.")]
        [Min(1f)]
        public float CurrentButtonFontSize = UiScale.Px(26f);

        [Header("상태")]
        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

        [Tooltip("카드에 마우스를 올렸을 때의 크기. 04 화면 공통 규칙 의 카드 호버 5퍼센트 확대.")]
        [Range(1f, 1.5f)]
        public float CardHoverScale = 1.05f;

        [Tooltip("읽을 수 없는 카드에 남기는 채도. 04 화면 공통 규칙 의 채도 제거라 0 이다.")]
        [Range(0f, 1f)]
        public float BrokenSaturation = 0f;

        [Header("문구")]
        [Tooltip("화면 제목.")]
        public string TitleText = "프로필 선택";

        [Tooltip("빈 자리에 적는 글.")]
        public string EmptyText = "비어 있음";

        [Tooltip("수정 버튼에 적는 글.")]
        public string EditText = "수정";

        [Tooltip("삭제 버튼에 적는 글.")]
        public string DeleteText = "삭제";

        [Tooltip("뒤로 버튼에 적는 글.")]
        public string BackText = "뒤로";

        [Tooltip("플레이 시간. {0} 시간, {1} 분.")]
        public string PlayTimeFormat = "플레이 시간 {0}시간 {1}분";

        [Tooltip("마지막 플레이. {0} 자리에 날짜가 들어간다.")]
        public string LastPlayedFormat = "마지막 플레이 {0}";

        [Tooltip("아직 논 적이 없을 때 날짜 자리에 넣는 글.")]
        public string LastPlayedNoneText = "없음";

        [Tooltip("읽을 수 없는 자리에 적는 글.")]
        public string BrokenText = "불러올 수 없음";

        [Tooltip("고른 프로필이 없을 때 현재 프로필 버튼에 적는 글.")]
        public string NoProfileText = "프로필 없음";

        /// <summary>그 자리의 카드 색. 자리가 색보다 많으면 처음부터 다시 쓴다.</summary>
        public Color GetCardColor(int index)
        {
            if (CardColors.Count == 0)
            {
                return PanelColor;
            }

            int wrapped = index % CardColors.Count;
            if (wrapped < 0)
            {
                wrapped += CardColors.Count;
            }

            return CardColors[wrapped];
        }

        /// <summary>상태까지 더한 카드 색. 읽을 수 없는 자리는 채도를 뺀다.</summary>
        public Color GetCardColor(int index, bool hovering, bool pressed, bool broken)
        {
            Color baseColor = GetCardColor(index);

            if (broken)
            {
                baseColor = Desaturate(baseColor, BrokenSaturation);
            }

            return ApplyBrightness(baseColor, hovering, pressed);
        }

        /// <summary>수정과 삭제 버튼의 바탕색.</summary>
        public Color GetActionButtonColor(bool hovering, bool pressed)
        {
            return ApplyBrightness(ActionButtonColor, hovering, pressed);
        }

        /// <summary>제목 칸과 뒤로 버튼의 색.</summary>
        public Color GetPanelColor(bool hovering, bool pressed)
        {
            return ApplyBrightness(PanelColor, hovering, pressed);
        }

        /// <summary>카드에 마우스를 올렸을 때의 크기 배율.</summary>
        public float GetCardScale(bool hovering)
        {
            return hovering ? CardHoverScale : 1f;
        }

        /// <summary>채도를 뺀 색. 남길 채도를 0 으로 주면 회색이 된다.</summary>
        public static Color Desaturate(Color color, float keep)
        {
            float gray = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            return new Color(
                gray + (color.r - gray) * keep,
                gray + (color.g - gray) * keep,
                gray + (color.b - gray) * keep,
                color.a);
        }

        private Color ApplyBrightness(Color baseColor, bool hovering, bool pressed)
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
                baseColor.r * brightness,
                baseColor.g * brightness,
                baseColor.b * brightness,
                baseColor.a);
        }
    }
}
