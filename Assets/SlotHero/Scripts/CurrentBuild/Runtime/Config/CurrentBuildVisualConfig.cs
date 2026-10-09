using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;
using SlotHero.Combat;

namespace SlotHero.CurrentBuild
{
    /// <summary>태그 하나의 표시용 자료.</summary>
    [Serializable]
    public struct BuildTagVisual
    {
        public SymbolTagType Tag;

        [Tooltip("화면에 적는 이름.")]
        public string DisplayName;

        [Tooltip("이름 왼쪽에 놓는 기호. 행성 기호를 글자로 적는다. 그림을 넣으면 그림이 먼저다.")]
        public string Symbol;

        [Tooltip("이름 왼쪽에 놓는 기호 그림. 넣어 두면 글자 대신 이것을 쓴다.")]
        public Sprite Icon;

        [Tooltip("막대 색. 기획서는 태그마다 고유한 색을 쓰게 한다.")]
        public Color BarColor;
    }

    /// <summary>
    /// 현재 빌드 화면의 색과 글자.
    /// 색은 와이어프레임에서 재 왔고 글자 크기는
    /// 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CurrentBuildVisualConfig",
        menuName = "Slot Hero/현재 빌드/현재 빌드 화면 표시 설정",
        order = 1)]
    public class CurrentBuildVisualConfig : ScriptableObject
    {
        [Header("바탕")]
        [Tooltip("화면 전체를 덮는 어두운 막. 와이어프레임 기준 검정 65퍼센트.")]
        public Color DimColor = new Color(0f, 0f, 0f, 0.65f);

        [Tooltip("오른쪽 칸과 닫기 칸의 배경.")]
        public Color PanelColor = new Color(0.098f, 0.082f, 0.055f, 1f);

        [Tooltip("물건을 놓는 칸의 배경.")]
        public Color SlotColor = new Color(0.067f, 0.059f, 0.051f, 1f);

        [Tooltip("개수 배지의 배경.")]
        public Color CountBadgeColor = new Color(0.055f, 0.043f, 0.035f, 1f);

        [Tooltip("태그 막대의 바닥.")]
        public Color TagBarBackColor = new Color(0.157f, 0.133f, 0.106f, 1f);

        [Header("글자")]
        [Tooltip("줄 이름 글자 크기. 04 화면 공통 규칙 의 항목 텍스트 26픽셀.")]
        [Min(1f)]
        public float SectionFontSize = UiScale.Px(26f);

        [Tooltip("오른쪽 칸 제목 글자 크기.")]
        [Min(1f)]
        public float PanelTitleFontSize = UiScale.Px(26f);

        [Tooltip("태그 이름과 장수 글자 크기. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        public float TagFontSize = UiScale.Px(24f);

        [Tooltip("태그 기호 글자 크기.")]
        [Min(1f)]
        public float TagSymbolFontSize = UiScale.Px(44f);

        [Tooltip("개수 배지 글자 크기.")]
        [Min(1f)]
        public float CountFontSize = UiScale.Px(22f);

        [Tooltip("밝은 글자 색. 04 화면 공통 규칙 의 EEE8DC.")]
        public Color TextColor = new Color(0.933f, 0.910f, 0.863f, 1f);

        [Tooltip("보조 글자 색. 태그 이름과 단축키에 쓴다.")]
        public Color SubTextColor = new Color(0.800f, 0.773f, 0.722f, 1f);

        [Tooltip("장수가 0인 태그의 글자 색.")]
        public Color EmptyTagTextColor = new Color(0.475f, 0.451f, 0.412f, 1f);

        [Header("고르기 화면")]
        [Tooltip("고른 칸의 테두리. 보상 화면의 고른 카드와 같은 금빛이다.")]
        public Color PickSelectedBorderColor = new Color(0.878f, 0.690f, 0.251f, 1f);

        [Tooltip("고른 칸의 테두리 두께.")]
        [Min(0f)]
        public float PickSelectedBorderThickness = UiScale.Px(6f);

        [Tooltip("고르기 화면의 확인 버튼 글.")]
        public string PickConfirmText = "선택";

        [Tooltip("고르기 화면의 취소 버튼 글.")]
        public string PickCancelText = "취소";

        [Tooltip("아직 아무것도 고르지 않았을 때 아래 줄에 적는 글.")]
        public string PickNoneText = "고를 칸을 누른다";

        [Tooltip("고를 수 없을 때 확인 버튼의 투명도.")]
        [Range(0f, 1f)]
        public float PickDisabledAlpha = 0.4f;

        [Header("문구")]
        [Tooltip("문양 개수 제목. {0} 자리에 장수가 들어간다.")]
        public string SymbolCountFormat = "문양 {0}장";

        [Tooltip("태그 장수 표기. {0} 자리에 장수가 들어간다.")]
        public string TagCountFormat = "{0}장";

        [Tooltip("개수 배지 표기. {0} 자리에 개수가 들어간다.")]
        public string CountBadgeFormat = "×{0}";

        [Tooltip("닫기에 함께 적는 단축키 안내.")]
        public string CloseShortcutText = "Tab / ESC";

        [Header("태그 9종")]
        [Tooltip("태그마다의 이름과 기호, 막대 색. 해왕성 색은 예시에 0장이라 내가 정했다.")]
        public List<BuildTagVisual> TagVisuals = new List<BuildTagVisual>
        {
            new BuildTagVisual
            {
                Tag = SymbolTagType.Mercury, DisplayName = "수성", Symbol = "☿",
                BarColor = new Color(0.247f, 0.663f, 0.788f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Venus, DisplayName = "금성", Symbol = "♀",
                BarColor = new Color(0.851f, 0.643f, 0.255f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Earth, DisplayName = "지구", Symbol = "⊕",
                BarColor = new Color(0.545f, 0.420f, 0.290f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Mars, DisplayName = "화성", Symbol = "♂",
                BarColor = new Color(0.776f, 0.337f, 0.275f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Jupiter, DisplayName = "목성", Symbol = "♃",
                BarColor = new Color(0.424f, 0.659f, 0.439f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Saturn, DisplayName = "토성", Symbol = "♄",
                BarColor = new Color(0.541f, 0.502f, 0.447f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Uranus, DisplayName = "천왕성", Symbol = "♅",
                BarColor = new Color(0.725f, 0.776f, 0.812f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Neptune, DisplayName = "해왕성", Symbol = "♆",
                BarColor = new Color(0.227f, 0.373f, 0.659f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Pluto, DisplayName = "명왕성", Symbol = "♇",
                BarColor = new Color(0.420f, 0.306f, 0.525f, 1f),
            },
        };

        /// <summary>그 태그의 표시 자료를 찾는다.</summary>
        public BuildTagVisual GetTagVisual(SymbolTagType tag)
        {
            for (int i = 0; i < TagVisuals.Count; i++)
            {
                if (TagVisuals[i].Tag == tag)
                {
                    return TagVisuals[i];
                }
            }

            BuildTagVisual fallback = new BuildTagVisual();
            fallback.Tag = tag;
            fallback.DisplayName = tag.ToString();
            fallback.BarColor = Color.white;
            return fallback;
        }

        /// <summary>태그 9종에 모두 이름과 색이 들어 있는지.</summary>
        public bool HasAllTagVisuals()
        {
            IReadOnlyList<SymbolTagType> all = SymbolTags.InOrder;
            for (int i = 0; i < all.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < TagVisuals.Count; j++)
                {
                    if (TagVisuals[j].Tag == all[i] && !string.IsNullOrEmpty(TagVisuals[j].DisplayName))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
