using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;
using SlotHero.Combat;

namespace SlotHero.CurrentBuild
{
    /// <summary>태그 하나의 표시용 자료.</summary>
    [Serializable]
    public struct BuildTagVisual
    {
        [FormerlySerializedAs("Tag")]
        [SerializeField] private SymbolTagType _tag;
        public SymbolTagType Tag { get { return _tag; } set { _tag = value; } }

        [Tooltip("화면에 적는 이름.")]
        [FormerlySerializedAs("DisplayName")]
        [SerializeField] private string _displayName;
        public string DisplayName { get { return _displayName; } set { _displayName = value; } }

        [Tooltip("이름 왼쪽에 놓는 기호. 행성 기호를 글자로 적는다. 그림을 넣으면 그림이 먼저다.")]
        [FormerlySerializedAs("Symbol")]
        [SerializeField] private string _symbol;
        public string Symbol { get { return _symbol; } set { _symbol = value; } }

        [Tooltip("이름 왼쪽에 놓는 기호 그림. 넣어 두면 글자 대신 이것을 쓴다.")]
        [FormerlySerializedAs("Icon")]
        [SerializeField] private Sprite _icon;
        public Sprite Icon { get { return _icon; } set { _icon = value; } }

        [Tooltip("막대 색. 기획서는 태그마다 고유한 색을 쓰게 한다.")]
        [FormerlySerializedAs("BarColor")]
        [SerializeField] private Color _barColor;
        public Color BarColor { get { return _barColor; } set { _barColor = value; } }

        [Tooltip("태그 이름을 글로 적을 때의 색. 막대 색과 같은 빛깔이고, 어두운 칸 위에서 읽히게 막대 색보다 밝을 수 있다.")]
        [SerializeField] private Color _textColor;
        public Color TextColor { get { return _textColor; } set { _textColor = value; } }

        [Tooltip("문양 칸 우상단에 얹는 테두리. 이 태그가 문양의 첫 태그일 때 쓴다. 500 × 500 그림의 우상단에 무늬가 있다.")]
        [FormerlySerializedAs("CornerTopRight")]
        [SerializeField] private Sprite _cornerTopRight;
        public Sprite CornerTopRight { get { return _cornerTopRight; } set { _cornerTopRight = value; } }

        [Tooltip("문양 칸 좌하단에 얹는 테두리. 이 태그가 문양의 둘째 태그일 때 쓴다. 500 × 500 그림의 좌하단에 무늬가 있다.")]
        [FormerlySerializedAs("CornerBottomLeft")]
        [SerializeField] private Sprite _cornerBottomLeft;
        public Sprite CornerBottomLeft { get { return _cornerBottomLeft; } set { _cornerBottomLeft = value; } }
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
        [Tooltip("화면 전체를 덮는 어두운 막. 와이어프레임 기준은 검정 65퍼센트다. " +
            "새 지도 양피지가 밝아 칸 사이로 지도 아이콘이 비쳐 95퍼센트로 올렸다(2026년 10월 21일). " +
            "프로젝트가 선형 색 공간에서 섞어 숫자보다 옅게 보인다. 88 이 화면에서는 62 쯤으로 보였다.")]
        [FormerlySerializedAs("DimColor")]
        [SerializeField] private Color _dimColor = new Color(0f, 0f, 0f, 0.95f);
        public Color DimColor { get { return _dimColor; } set { _dimColor = value; } }

        [Tooltip("오른쪽 칸과 정렬, 닫기 칸의 배경. 흰색이면 그린 칸 그림(어두운 갈회색, 쇠 모서리) 그대로 보인다. 이 화면은 버튼도 어두운 칸 그림을 쓴다(2026년 10월 20일 원재).")]
        [FormerlySerializedAs("PanelColor")]
        [SerializeField] private Color _panelColor = Color.white;
        public Color PanelColor { get { return _panelColor; } set { _panelColor = value; } }

        [Tooltip("물건을 가진 칸의 배경. 꽉 칠하면 보기 좋지 않아 반투명이다(2026년 10월 21일 원재). " +
            "문양 칸은 그 위에 흑요석 판을 깔아 가려진다.")]
        [FormerlySerializedAs("SlotColor")]
        [SerializeField] private Color _slotColor = new Color(0.067f, 0.059f, 0.051f, 0.4f);
        public Color SlotColor { get { return _slotColor; } set { _slotColor = value; } }

        [Tooltip("아직 갖지 않은 빈 칸의 점선 테두리 색. 빈 칸은 배경 없이 이 테두리만 남긴다(2026년 10월 21일 원재).")]
        [SerializeField] private Color _emptyOutlineColor = new Color(0.925f, 0.886f, 0.784f, 0.45f);
        public Color EmptyOutlineColor { get { return _emptyOutlineColor; } set { _emptyOutlineColor = value; } }

        [Tooltip("개수 배지의 배경.")]
        [FormerlySerializedAs("CountBadgeColor")]
        [SerializeField] private Color _countBadgeColor = new Color(0.055f, 0.043f, 0.035f, 1f);
        public Color CountBadgeColor { get { return _countBadgeColor; } set { _countBadgeColor = value; } }

        [Tooltip("태그 막대의 바닥.")]
        [FormerlySerializedAs("TagBarBackColor")]
        [SerializeField] private Color _tagBarBackColor = new Color(0.157f, 0.133f, 0.106f, 1f);
        public Color TagBarBackColor { get { return _tagBarBackColor; } set { _tagBarBackColor = value; } }

        [Header("글자")]
        [Tooltip("줄 이름 글자 크기. 04 화면 공통 규칙 의 항목 텍스트 26픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("SectionFontSize")]
        [SerializeField] private float _sectionFontSize = UiScale.Px(26f);
        public float SectionFontSize { get { return _sectionFontSize; } set { _sectionFontSize = value; } }

        [Tooltip("오른쪽 칸 제목 글자 크기.")]
        [Min(1f)]
        [FormerlySerializedAs("PanelTitleFontSize")]
        [SerializeField] private float _panelTitleFontSize = UiScale.Px(26f);
        public float PanelTitleFontSize { get { return _panelTitleFontSize; } set { _panelTitleFontSize = value; } }

        [Tooltip("태그 이름과 장수 글자 크기. 04 화면 공통 규칙 의 본문 텍스트 24픽셀.")]
        [Min(1f)]
        [FormerlySerializedAs("TagFontSize")]
        [SerializeField] private float _tagFontSize = UiScale.Px(24f);
        public float TagFontSize { get { return _tagFontSize; } set { _tagFontSize = value; } }

        [Tooltip("태그 기호 글자 크기.")]
        [Min(1f)]
        [FormerlySerializedAs("TagSymbolFontSize")]
        [SerializeField] private float _tagSymbolFontSize = UiScale.Px(44f);
        public float TagSymbolFontSize { get { return _tagSymbolFontSize; } set { _tagSymbolFontSize = value; } }

        [Tooltip("개수 배지 글자 크기.")]
        [Min(1f)]
        [FormerlySerializedAs("CountFontSize")]
        [SerializeField] private float _countFontSize = UiScale.Px(22f);
        public float CountFontSize { get { return _countFontSize; } set { _countFontSize = value; } }

        [Tooltip("밝은 글자 색. 04 화면 공통 규칙 의 EEE8DC.")]
        [FormerlySerializedAs("TextColor")]
        [SerializeField] private Color _textColor = new Color(0.933f, 0.910f, 0.863f, 1f);
        public Color TextColor { get { return _textColor; } set { _textColor = value; } }

        [Tooltip("보조 글자 색. 태그 이름과 단축키에 쓴다.")]
        [FormerlySerializedAs("SubTextColor")]
        [SerializeField] private Color _subTextColor = new Color(0.800f, 0.773f, 0.722f, 1f);
        public Color SubTextColor { get { return _subTextColor; } set { _subTextColor = value; } }

        [Tooltip("장수가 0인 태그의 글자 색.")]
        [FormerlySerializedAs("EmptyTagTextColor")]
        [SerializeField] private Color _emptyTagTextColor = new Color(0.475f, 0.451f, 0.412f, 1f);
        public Color EmptyTagTextColor { get { return _emptyTagTextColor; } set { _emptyTagTextColor = value; } }

        [Tooltip("태그 기호 그림에 곱하는 색. 기호 그림이 문양 화풍으로 태그 색을 직접 가져 흰색이면 그림 그대로다(2026년 10월 22일 원재).")]
        [SerializeField] private Color _tagIconColor = Color.white;
        public Color TagIconColor { get { return _tagIconColor; } set { _tagIconColor = value; } }

        [Tooltip("장수가 0인 태그의 기호 그림 색. 글자처럼 흐리게 둔다.")]
        [SerializeField] private Color _emptyTagIconColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        public Color EmptyTagIconColor { get { return _emptyTagIconColor; } set { _emptyTagIconColor = value; } }

        [Tooltip("아이템 상세에 적는 태그 둘의 글자 크기 배율(퍼센트). 굵게 적고 본문보다 크게 둔다. " +
            "2026년 10월 25일 원재가 120 으로 정했고 같은 날 더 크게 하라고 해 150 이다.")]
        [Min(1f)]
        [SerializeField] private float _detailTagSizePercent = 150f;
        public float DetailTagSizePercent { get { return _detailTagSizePercent; } set { _detailTagSizePercent = value; } }

        [Tooltip("현재 빌드와 고르기 화면의 태그 칸에 적는 태그 이름 글자 크기. 장수보다 크게 둔다(2026년 10월 25일 원재).")]
        [Min(1f)]
        [SerializeField] private float _tagNameFontSize = UiScale.Px(30f);
        public float TagNameFontSize { get { return _tagNameFontSize; } set { _tagNameFontSize = value; } }

        [Tooltip("태그 이름 글자에 두르는 테두리 색. 태그 칸, 보상 카드, 아이템 상세의 태그 글자가 잘 보이게 흰색으로 두른다(2026년 10월 25일 원재).")]
        [SerializeField] private Color _tagOutlineColor = Color.white;
        public Color TagOutlineColor { get { return _tagOutlineColor; } set { _tagOutlineColor = value; } }

        [Tooltip("태그 이름 글자 테두리 두께. 글자 칸(TextMeshPro)의 테두리 두께 값으로 0 ~ 1 이다. 0.2 는 30픽셀 이름에서 제 색이 묻혀 0.15 로 두었다.")]
        [Range(0f, 1f)]
        [SerializeField] private float _tagOutlineWidth = 0.15f;
        public float TagOutlineWidth { get { return _tagOutlineWidth; } set { _tagOutlineWidth = value; } }

        [Header("고르기 화면")]
        [Tooltip("고른 칸의 테두리. 문양 변경에서 고른 문양이 잘 보이게 짙은 초록으로 두껍게 두른다(2026년 10월 25일 원재).")]
        [FormerlySerializedAs("PickSelectedBorderColor")]
        [SerializeField] private Color _pickSelectedBorderColor = new Color(0.180f, 0.800f, 0.251f, 1f);
        public Color PickSelectedBorderColor { get { return _pickSelectedBorderColor; } set { _pickSelectedBorderColor = value; } }

        [Tooltip("고른 칸의 테두리 두께.")]
        [Min(0f)]
        [FormerlySerializedAs("PickSelectedBorderThickness")]
        [SerializeField] private float _pickSelectedBorderThickness = UiScale.Px(12f);
        public float PickSelectedBorderThickness { get { return _pickSelectedBorderThickness; } set { _pickSelectedBorderThickness = value; } }

        [Tooltip("고르기 화면의 확인 버튼 글.")]
        [FormerlySerializedAs("PickConfirmText")]
        [SerializeField] private string _pickConfirmText = "선택";
        public string PickConfirmText { get { return _pickConfirmText; } set { _pickConfirmText = value; } }

        [Tooltip("고르기 화면의 취소 버튼 글.")]
        [FormerlySerializedAs("PickCancelText")]
        [SerializeField] private string _pickCancelText = "취소";
        public string PickCancelText { get { return _pickCancelText; } set { _pickCancelText = value; } }

        [Tooltip("아직 아무것도 고르지 않았을 때 아래 줄에 적는 글.")]
        [FormerlySerializedAs("PickNoneText")]
        [SerializeField] private string _pickNoneText = "고를 칸을 누른다";
        public string PickNoneText { get { return _pickNoneText; } set { _pickNoneText = value; } }

        [Tooltip("고를 수 없을 때 확인 버튼의 투명도.")]
        [Range(0f, 1f)]
        [FormerlySerializedAs("PickDisabledAlpha")]
        [SerializeField] private float _pickDisabledAlpha = 0.4f;
        public float PickDisabledAlpha { get { return _pickDisabledAlpha; } set { _pickDisabledAlpha = value; } }

        [Header("문구")]
        [Tooltip("문양 개수 제목. {0} 자리에 장수가 들어간다.")]
        [FormerlySerializedAs("SymbolCountFormat")]
        [SerializeField] private string _symbolCountFormat = "문양 {0}장";
        public string SymbolCountFormat { get { return _symbolCountFormat; } set { _symbolCountFormat = value; } }

        [Tooltip("태그 장수 표기. {0} 자리에 장수가 들어간다.")]
        [FormerlySerializedAs("TagCountFormat")]
        [SerializeField] private string _tagCountFormat = "{0}장";
        public string TagCountFormat { get { return _tagCountFormat; } set { _tagCountFormat = value; } }

        [Tooltip("개수 배지 표기. {0} 자리에 개수가 들어간다.")]
        [FormerlySerializedAs("CountBadgeFormat")]
        [SerializeField] private string _countBadgeFormat = "×{0}";
        public string CountBadgeFormat { get { return _countBadgeFormat; } set { _countBadgeFormat = value; } }

        [Tooltip("닫기에 함께 적는 단축키 안내.")]
        [FormerlySerializedAs("CloseShortcutText")]
        [SerializeField] private string _closeShortcutText = "Tab / ESC";
        public string CloseShortcutText { get { return _closeShortcutText; } set { _closeShortcutText = value; } }

        [Header("태그 9종")]
        [Tooltip("태그마다의 이름과 기호, 막대 색, 문양 테두리. 2026년 10월 19일 원재가 태그 테두리 무늬에 맞춰 색을 다시 정하게 했다. " +
            "무늬는 행성의 한자 뜻(수 · 금 · 지 · 화 · 목 · 토 · 천 · 해 · 명)과 서양 신화를 섞었고, 아홉 색은 서로 구분되게 골랐다.")]
        [FormerlySerializedAs("TagVisuals")]
        [SerializeField] private List<BuildTagVisual> _tagVisuals = new List<BuildTagVisual>
        {
            new BuildTagVisual
            {
                Tag = SymbolTagType.Mercury, DisplayName = "수성", Symbol = "☿",
                BarColor = new Color(0.525f, 0.737f, 0.878f, 1f), // 물. 옅은 하늘색
                TextColor = new Color(0.525f, 0.737f, 0.878f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Venus, DisplayName = "금성", Symbol = "♀",
                BarColor = new Color(0.639f, 0.604f, 0.565f, 1f), // 쇠. 따뜻한 회색
                TextColor = new Color(0.639f, 0.604f, 0.565f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Earth, DisplayName = "지구", Symbol = "⊕",
                BarColor = new Color(0.373f, 0.627f, 0.322f, 1f), // 흙과 새싹. 잎사귀 초록
                TextColor = new Color(0.373f, 0.627f, 0.322f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Mars, DisplayName = "화성", Symbol = "♂",
                BarColor = new Color(0.824f, 0.235f, 0.180f, 1f), // 불과 전쟁. 진홍
                TextColor = new Color(0.824f, 0.235f, 0.180f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Jupiter, DisplayName = "목성", Symbol = "♃",
                BarColor = new Color(0.478f, 0.329f, 0.251f, 1f), // 나무. 나뭇가지 갈색
                TextColor = new Color(0.725f, 0.541f, 0.384f, 1f), // 어두운 칸에서 잘 안 보여 밝혔다(2026년 10월 25일 원재)
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Saturn, DisplayName = "토성", Symbol = "♄",
                BarColor = new Color(0.816f, 0.643f, 0.227f, 1f), // 흙덩이와 수확. 황토 금색
                TextColor = new Color(0.816f, 0.643f, 0.227f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Uranus, DisplayName = "천왕성", Symbol = "♅",
                BarColor = new Color(0.208f, 0.710f, 0.667f, 1f), // 하늘과 바람. 청록
                TextColor = new Color(0.208f, 0.710f, 0.667f, 1f),
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Neptune, DisplayName = "해왕성", Symbol = "♆",
                BarColor = new Color(0.176f, 0.337f, 0.702f, 1f), // 깊은 바다. 짙은 파랑
                TextColor = new Color(0.416f, 0.573f, 0.925f, 1f), // 어두운 칸에서 잘 안 보여 밝혔다(2026년 10월 25일 원재)
            },
            new BuildTagVisual
            {
                Tag = SymbolTagType.Pluto, DisplayName = "명왕성", Symbol = "♇",
                BarColor = new Color(0.482f, 0.271f, 0.651f, 1f), // 저승. 보라
                TextColor = new Color(0.655f, 0.482f, 0.839f, 1f), // 어두운 칸에서 잘 안 보여 밝혔다(2026년 10월 25일 원재)
            },
        };
        public List<BuildTagVisual> TagVisuals { get { return _tagVisuals; } set { _tagVisuals = value; } }

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
            fallback.TextColor = Color.white;
            return fallback;
        }

        /// <summary>
        /// 문양 칸에 얹을 태그 테두리 둘. 첫 태그의 우상단 그림과 둘째 태그의 좌하단 그림이다(2026년 10월 19일 원재).
        /// 그림을 물리지 않은 태그면 그쪽은 null 이다.
        /// </summary>
        public void GetTagCorners(SymbolTagType first, SymbolTagType second, out Sprite topRight, out Sprite bottomLeft)
        {
            topRight = GetTagVisual(first).CornerTopRight;
            bottomLeft = GetTagVisual(second).CornerBottomLeft;
        }

        [Tooltip("태그 테두리 장식 모서리에 얹는 행성 기호의 크기. 테두리 한 변에 대한 비율이다(2026년 10월 26일 원재).")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _cornerGlyphSizeRatio = 0.22f;
        public float CornerGlyphSizeRatio { get { return _cornerGlyphSizeRatio; } set { _cornerGlyphSizeRatio = value; } }

        [Tooltip("모서리에서 기호 가운데까지. 테두리 한 변에 대한 비율이다. 테두리 그림의 장식이 감기는 모서리 끝(12 퍼센트)에 맞췄다.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _cornerGlyphFromCorner = 0.12f;
        public float CornerGlyphFromCorner { get { return _cornerGlyphFromCorner; } set { _cornerGlyphFromCorner = value; } }

        /// <summary>
        /// 태그 테두리 장식 모서리에 얹을 행성 기호 둘. 첫 태그는 우상단, 둘째 태그는 좌하단이다(2026년 10월 26일 원재).
        /// 기호 그림을 물리지 않은 태그면 그쪽은 null 이다.
        /// </summary>
        public void GetTagGlyphs(SymbolTagType first, SymbolTagType second, out Sprite topRight, out Sprite bottomLeft)
        {
            topRight = GetTagVisual(first).Icon;
            bottomLeft = GetTagVisual(second).Icon;
        }

        [Header("문양 판")]
        [Tooltip("문양 뒤에 까는 흑요석 판. 행상 진열의 `문양_바닥` 한 장과 같다. 예전 넉 장을 2026년 10월 25일 원재가 가장 깔끔한 한 장으로 모았다. " +
            "문양이 나오는 화면은 모두 이 판 위에 문양을 올린다(2026년 10월 20일 원재).")]
        [SerializeField] private Sprite[] _symbolPlates = new Sprite[0];
        public Sprite[] SymbolPlates { get { return _symbolPlates; } set { _symbolPlates = value; } }

        [Tooltip("태그 테두리와 문양 그림을 판 가장자리에서 얼마나 안쪽에 둘지. 판 한 변에 대한 비율이다. " +
            "테두리가 판의 쇠 테두리에 걸치지 않고 판 안쪽에 들어가게 한다. 문양 그림은 테두리와 같은 넓이로 꼭 맞춘다(2026년 10월 20일 원재).")]
        [Range(0f, 0.45f)]
        [SerializeField] private float _symbolFrameInset = 0.06f;
        public float SymbolFrameInset { get { return _symbolFrameInset; } set { _symbolFrameInset = value; } }

        /// <summary>
        /// 그 문양 뒤에 깔 판. 판이 여러 장이면 식별자로 하나를 고른다.
        /// 같은 문양은 어느 화면에서든 같은 판에 놓인다. 판을 물리지 않았으면 null 이다.
        /// </summary>
        public Sprite GetSymbolPlate(string symbolId)
        {
            if (_symbolPlates == null || _symbolPlates.Length == 0)
            {
                return null;
            }

            // 글자 값을 더한 수로 고른다. 기기나 실행마다 달라지는 string.GetHashCode 는 쓰지 않는다.
            int sum = 0;
            if (symbolId != null)
            {
                for (int i = 0; i < symbolId.Length; i++)
                {
                    sum += symbolId[i];
                }
            }

            return _symbolPlates[sum % _symbolPlates.Length];
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
