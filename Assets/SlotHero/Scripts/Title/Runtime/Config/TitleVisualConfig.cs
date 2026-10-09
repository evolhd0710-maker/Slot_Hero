using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Title
{
    /// <summary>메뉴 항목 하나의 종류와 화면에 적는 이름.</summary>
    [Serializable]
    public struct TitleMenuEntry
    {
        public TitleMenuKind Kind;

        [Tooltip("화면에 적는 이름.")]
        public string DisplayName;

        public TitleMenuEntry(TitleMenuKind kind, string displayName)
        {
            Kind = kind;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// 타이틀 화면의 색과 글자, 메뉴 목록.
    /// 글자 크기는 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의 텍스트 표를 따른다.
    ///
    /// 메뉴 차례를 목록으로 둔 것은 기획서가 "항목이 추가될 경우 간격을 조정하여 배치한다"로
    /// 항목이 늘 수 있다고 적었기 때문이다. 차례를 바꾸는 일도 여기서 한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TitleVisualConfig",
        menuName = "Slot Hero/타이틀/타이틀 화면 표시 설정",
        order = 1)]
    public class TitleVisualConfig : ScriptableObject
    {
        [Header("메뉴 목록")]
        [Tooltip("위에서부터의 차례대로 적는다. 기획서 본문과 와이어프레임이 모두 새 게임을 먼저 두었다.")]
        public List<TitleMenuEntry> MenuEntries = new List<TitleMenuEntry>
        {
            new TitleMenuEntry(TitleMenuKind.NewGame, "새 게임"),
            new TitleMenuEntry(TitleMenuKind.Continue, "이어하기"),
            new TitleMenuEntry(TitleMenuKind.Settings, "설정"),
            new TitleMenuEntry(TitleMenuKind.Quit, "종료"),
        };

        [Header("글자 색")]
        [Tooltip("메뉴 글자. 와이어프레임 기준 흰색이다.")]
        public Color MenuTextColor = Color.white;

        [Tooltip("버전 글자. 04 화면 공통 규칙 의 보조 텍스트 색을 밝은 쪽으로 쓴다.")]
        public Color VersionColor = new Color(0.933f, 0.910f, 0.863f, 0.7f);

        [Header("글자 크기")]
        [Tooltip("메뉴. 04 화면 공통 규칙 의 화면 주요 텍스트 40픽셀 굵게. 와이어프레임 실측도 같다.")]
        [Min(1f)]
        public float MenuFontSize = UiScale.Px(40f);

        [Tooltip("버전. 04 화면 공통 규칙 의 보조 텍스트 20픽셀. 와이어프레임은 이보다 작게 그렸다.")]
        [Min(1f)]
        public float VersionFontSize = UiScale.Px(20f);

        [Header("상태")]
        [Tooltip("마우스를 올렸을 때 곱하는 밝기. 04 화면 공통 규칙 의 밝기 한 단계를 10퍼센트로 본다.")]
        [Range(1f, 2f)]
        public float HoverBrightness = 1.10f;

        [Tooltip("누르고 있을 때 곱하는 밝기.")]
        [Range(0f, 1f)]
        public float PressedBrightness = 0.90f;

        [Tooltip("누르고 있을 때 아래로 내리는 거리. 항목 높이에 대한 비율. 04 화면 공통 규칙 의 2퍼센트.")]
        [Range(0f, 0.2f)]
        public float PressedOffsetRatio = 0.02f;

        [Tooltip("누를 수 없을 때의 투명도. 04 화면 공통 규칙 의 40퍼센트.")]
        [Range(0f, 1f)]
        public float DisabledAlpha = 0.4f;

        [Header("문구")]
        [Tooltip("버전 표기. {0} 자리에 빌드 번호가 들어간다.")]
        public string VersionFormat = "v{0}";

        [Tooltip("이어하기를 누를 수 없는 까닭. 04 화면 공통 규칙 이 비활성 이유를 적게 한다.")]
        public string NoSavedRunReason = "저장된 런이 없습니다";

        [Tooltip("런 데이터가 손상되어 이어하기를 누를 수 없을 때의 까닭.")]
        public string BrokenRunReason = "저장된 런을 불러올 수 없습니다";

        [Tooltip("런 파일을 지금 읽지 못해 이어하기를 누를 수 없을 때의 까닭. 손상과 달리 잠시 뒤 다시 읽는다.")]
        public string UnreadableRunReason = "저장된 런을 지금 읽을 수 없습니다. 잠시 뒤 다시 시도해 주세요";

        /// <summary>메뉴 항목 수.</summary>
        public int MenuCount
        {
            get { return MenuEntries.Count; }
        }

        /// <summary>그 자리의 메뉴 항목.</summary>
        public TitleMenuEntry GetEntry(int index)
        {
            if (index < 0 || index >= MenuEntries.Count)
            {
                return new TitleMenuEntry(TitleMenuKind.NewGame, string.Empty);
            }

            return MenuEntries[index];
        }

        /// <summary>그 종류가 몇 번째에 있는지. 없으면 -1.</summary>
        public int IndexOf(TitleMenuKind kind)
        {
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                if (MenuEntries[i].Kind == kind)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>메뉴 글자 색. 누를 수 없으면 투명도를 낮춘다.</summary>
        public Color GetMenuTextColor(bool interactable, bool hovering, bool pressed)
        {
            if (!interactable)
            {
                return new Color(
                    MenuTextColor.r,
                    MenuTextColor.g,
                    MenuTextColor.b,
                    MenuTextColor.a * DisabledAlpha);
            }

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
                MenuTextColor.r * brightness,
                MenuTextColor.g * brightness,
                MenuTextColor.b * brightness,
                MenuTextColor.a);
        }

        /// <summary>빌드 번호를 적은 글.</summary>
        public string FormatVersion(string buildNumber)
        {
            if (string.IsNullOrEmpty(buildNumber))
            {
                return string.Empty;
            }

            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture, VersionFormat, buildNumber);
        }

        /// <summary>이어하기를 누를 수 없는 까닭. 누를 수 있으면 빈 글.</summary>
        public string GetContinueDisabledReason(SavedRunState state)
        {
            if (state == SavedRunState.Broken)
            {
                return BrokenRunReason;
            }

            if (state == SavedRunState.Unreadable)
            {
                return UnreadableRunReason;
            }

            return state == SavedRunState.None ? NoSavedRunReason : string.Empty;
        }
    }
}
