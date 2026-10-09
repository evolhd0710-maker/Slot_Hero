using System.Collections.Generic;
using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Events
{
    /// <summary>
    /// 이벤트 화면의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 11 이벤트 화면 과
    /// 그 와이어프레임에서 잰 값이다.
    ///
    /// 기획서가 정한 것은 삽화 1200 × 500, 본문은 삽화 아래 왼쪽,
    /// 선택지는 삽화 아래 오른쪽 세로 배치,
    /// 본문과 선택지는 화면 가운데를 기준으로 좌우 대칭이다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EventLayoutConfig",
        menuName = "Slot Hero/이벤트/이벤트 화면 자리 설정",
        order = 0)]
    public class EventLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("삽화")]
        [Tooltip("삽화 크기. 기획서 기준 1200 × 500.")]
        public Vector2 IllustrationSize = UiScale.V(1200f, 500f);

        [Tooltip("화면 위에서 삽화까지의 거리. 와이어프레임 기준 100.")]
        public float IllustrationTop = UiScale.Px(100f);

        [Header("본문과 선택지")]
        [Tooltip("삽화 아래에서 본문과 선택지가 시작되는 y. 와이어프레임 기준 640.")]
        public float ContentTop = UiScale.Px(640f);

        [Tooltip("본문 칸의 크기. 와이어프레임 실측 580 × 348 을 10 단위로 옮긴 값이다.")]
        public Vector2 BodySize = UiScale.V(580f, 350f);

        [Tooltip("선택지 칸 하나의 크기. 와이어프레임 실측 580 × 36 을 10 단위로 옮긴 값이다. " +
                 "글이 한 줄에 안 들어가면 세로로만 늘어난다. 가로는 이 값을 지킨다.")]
        public Vector2 ChoiceSize = UiScale.V(580f, 40f);

        [Tooltip("선택지 칸 안쪽 위아래 여백. 글이 여러 줄이 될 때 칸이 이만큼 더 커진다.")]
        [Min(0f)]
        public float ChoiceVerticalPadding = UiScale.Px(10f);

        [Tooltip("선택지 칸 사이 간격. 와이어프레임 기준 10.")]
        [Min(0f)]
        public float ChoiceSpacing = UiScale.Px(10f);

        [Tooltip("본문 칸과 선택지 칸이 화면 가운데에서 떨어지는 거리. 와이어프레임 기준 20.")]
        public float CenterGap = UiScale.Px(20f);

        [Tooltip("한 화면에 놓을 수 있는 선택지 칸의 최대 수. 본문 칸 높이 안에 들어가는 만큼인 7이다. " +
                 "고른 이력과 새 선택지가 함께 놓인다. 넘으면 고른 이력만 가장 오래 전 것부터 지운다. " +
                 "각본 화면 하나의 선택지 수는 이벤트 목록의 화면당 최대 선택지 수(5)가 따로 막는다.")]
        [Min(1)]
        public int MaxChoiceCount = 7;

        /// <summary>삽화의 자리. 화면 가운데 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetIllustrationPosition()
        {
            return new Vector2(0f, -IllustrationTop);
        }

        /// <summary>본문 칸의 자리. 화면 가운데 위를 기준으로 잡고 피벗을 오른쪽 위에 둔 값이다.</summary>
        public Vector2 GetBodyPosition()
        {
            return new Vector2(-CenterGap, -ContentTop);
        }

        /// <summary>
        /// 선택지 칸의 자리. 화면 가운데 위를 기준으로 잡고 피벗을 왼쪽 위에 둔 값이다.
        /// index 가 커질수록 아래로 내려간다.
        ///
        /// 칸이 전부 한 줄일 때의 값이다. 글이 길어 칸이 늘어나면 아래 목록을 받는 쪽을 쓴다.
        /// </summary>
        public Vector2 GetChoicePosition(int index)
        {
            float y = ContentTop + index * (ChoiceSize.y + ChoiceSpacing);
            return new Vector2(CenterGap, -y);
        }

        /// <summary>
        /// 글 높이에 맞춘 선택지 칸의 높이.
        ///
        /// 글이 한 줄이면 와이어프레임의 40 을 그대로 쓰고,
        /// 두 줄 넘게 늘어나면 글 높이에 위아래 여백을 더한 만큼 커진다.
        /// 칸을 넘겨 글을 자르지 않으려는 것이다.
        /// </summary>
        public float GetChoiceHeight(float textHeight)
        {
            float needed = textHeight + ChoiceVerticalPadding * 2f;
            return needed > ChoiceSize.y ? needed : ChoiceSize.y;
        }

        /// <summary>선택지 글이 쓸 수 있는 가로 폭. 칸 폭에서 좌우 안쪽 여백을 뺀 값이다.</summary>
        public float GetChoiceTextWidth(float horizontalPadding)
        {
            float width = ChoiceSize.x - horizontalPadding * 2f;
            return width > 1f ? width : 1f;
        }

        /// <summary>
        /// index 번째 선택지 칸의 위쪽 y. 앞선 칸들이 실제로 차지한 높이를 더해 내려간다.
        /// heights 는 각 칸의 높이다. <see cref="GetChoiceHeight"/> 로 구한 값을 넣는다.
        /// </summary>
        public float GetChoiceTop(IList<float> heights, int index)
        {
            float y = ContentTop;

            if (heights == null)
            {
                return y + index * (ChoiceSize.y + ChoiceSpacing);
            }

            int last = index < heights.Count ? index : heights.Count;
            for (int i = 0; i < last; i++)
            {
                y += heights[i] + ChoiceSpacing;
            }

            return y;
        }

        /// <summary>글 높이를 반영한 선택지 칸의 자리.</summary>
        public Vector2 GetChoicePosition(IList<float> heights, int index)
        {
            return new Vector2(CenterGap, -GetChoiceTop(heights, index));
        }

        /// <summary>선택지 칸들이 위아래로 차지하는 전체 높이. 칸 사이 간격까지 센다.</summary>
        public float GetChoicesTotalHeight(IList<float> heights)
        {
            if (heights == null || heights.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            for (int i = 0; i < heights.Count; i++)
            {
                total += heights[i];
            }

            return total + (heights.Count - 1) * ChoiceSpacing;
        }

        /// <summary>선택지 칸들이 본문 칸 높이 안에 들어가는지.</summary>
        public bool ChoicesFitBody(IList<float> heights)
        {
            return GetChoicesTotalHeight(heights) <= BodySize.y;
        }

        /// <summary>본문 칸 높이 안에 들어가는 선택지 수. 전부 한 줄일 때의 값이다.</summary>
        public int GetChoiceCountInBody()
        {
            float step = ChoiceSize.y + ChoiceSpacing;
            if (step <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt((BodySize.y + ChoiceSpacing) / step);
        }

        /// <summary>본문과 선택지가 화면 가운데를 기준으로 좌우 대칭인지.</summary>
        public bool IsSymmetric()
        {
            return Mathf.Approximately(BodySize.x, ChoiceSize.x);
        }

        /// <summary>최대 선택지 수가 본문 칸 높이 안에 들어가는지.</summary>
        public bool MaxChoiceCountFitsBody()
        {
            return MaxChoiceCount <= GetChoiceCountInBody();
        }

        private void OnValidate()
        {
            IllustrationSize.x = Mathf.Max(1f, IllustrationSize.x);
            IllustrationSize.y = Mathf.Max(1f, IllustrationSize.y);
            BodySize.x = Mathf.Max(1f, BodySize.x);
            BodySize.y = Mathf.Max(1f, BodySize.y);
            ChoiceSize.x = Mathf.Max(1f, ChoiceSize.x);
            ChoiceSize.y = Mathf.Max(1f, ChoiceSize.y);
            ChoiceVerticalPadding = Mathf.Max(0f, ChoiceVerticalPadding);
            ChoiceSpacing = Mathf.Max(0f, ChoiceSpacing);
            MaxChoiceCount = Mathf.Max(1, MaxChoiceCount);
        }
    }
}
