using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Hud
{
    /// <summary>
    /// 런 진행 중 화면에서 자리가 고정되는 요소의 수치.
    /// 기본값은 인게임 화면 기획서 v0.2 / 04 화면 공통 규칙 의
    /// "런 진행 중 고정되는 자리"를 그대로 옮긴 것이다.
    ///
    /// 상단 표시줄은 상단 UI 바 기획서가 따로 맡으므로 크기만 적어 두고 건드리지 않는다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "HudLayoutConfig",
        menuName = "Slot Hero/화면 공통/고정 자리 설정",
        order = 0)]
    public class HudLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD 전체화면.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("현재 빌드 버튼")]
        [Tooltip("버튼 한 변의 크기. 기획서 기준 200 × 200 원형.")]
        [Min(1f)]
        [FormerlySerializedAs("ButtonSize")]
        [SerializeField] private float _buttonSize = UiScale.Px(200f);
        public float ButtonSize { get { return _buttonSize; } set { _buttonSize = value; } }

        [Tooltip("화면 우측 하단 모서리에서 가로 세로로 떨어뜨리는 거리. 기획서 기준 70.")]
        [Min(0f)]
        [FormerlySerializedAs("ButtonCornerMargin")]
        [SerializeField] private float _buttonCornerMargin = UiScale.Px(70f);
        public float ButtonCornerMargin { get { return _buttonCornerMargin; } set { _buttonCornerMargin = value; } }

        [Tooltip("버튼 안에 놓는 아이콘의 크기. 와이어프레임 기준 150 × 150.")]
        [Min(1f)]
        [FormerlySerializedAs("IconSize")]
        [SerializeField] private float _iconSize = UiScale.Px(150f);
        public float IconSize { get { return _iconSize; } set { _iconSize = value; } }

        [Tooltip(
            "현재 빌드 버튼을 화면 왼쪽 아래에 둘지. 끄면 오른쪽 아래다.\n" +
            "**이 버튼의 자리는 아직 정해지지 않았다.** 상단 표시줄에 자리가 없어 구석에 따로 둔 것이다.\n" +
            "기획서와 와이어프레임은 우측 하단이라 그쪽을 기본으로 둔다.")]
        [FormerlySerializedAs("OnLeftCorner")]
        [SerializeField] private bool _onLeftCorner;
        public bool OnLeftCorner { get { return _onLeftCorner; } set { _onLeftCorner = value; } }

        [Header("자동 저장 표시")]
        [Tooltip("자동 저장 표시의 크기. 기획서 기준 200 × 40.")]
        [FormerlySerializedAs("AutoSaveNoticeSize")]
        [SerializeField] private Vector2 _autoSaveNoticeSize = UiScale.V(200f, 40f);
        public Vector2 AutoSaveNoticeSize { get { return _autoSaveNoticeSize; } set { _autoSaveNoticeSize = value; } }

        [Tooltip("현재 빌드 버튼과 표시 사이, 표시와 화면 아래 모서리 사이의 간격. 기획서 기준 15.")]
        [Min(0f)]
        [FormerlySerializedAs("AutoSaveNoticeGap")]
        [SerializeField] private float _autoSaveNoticeGap = UiScale.Px(15f);
        public float AutoSaveNoticeGap { get { return _autoSaveNoticeGap; } set { _autoSaveNoticeGap = value; } }

        /// <summary>
        /// 현재 빌드 버튼의 자리. 화면 우측 하단을 기준점으로 잡은 값이다.
        /// 기준점과 피벗을 모두 우측 하단에 두고 이 값을 넣으면 기획서 자리가 나온다.
        /// </summary>
        public Vector2 GetButtonPosition()
        {
            return new Vector2(SideSign() * ButtonCornerMargin, ButtonCornerMargin);
        }

        /// <summary>
        /// 버튼을 놓는 아래쪽 구석. 왼쪽이면 -1, 오른쪽이면 1 을 곱한다.
        /// 기준점과 피벗도 같은 쪽에 둔다.
        /// </summary>
        public float SideSign()
        {
            return OnLeftCorner ? 1f : -1f;
        }

        /// <summary>
        /// 자동 저장 표시의 자리. 버튼과 같은 기준점을 쓴다.
        /// 버튼 아래 간격만큼 띄우고, 표시 아래로도 같은 간격이 남는다.
        /// </summary>
        public Vector2 GetAutoSaveNoticePosition()
        {
            float bottom = ButtonCornerMargin - AutoSaveNoticeGap - AutoSaveNoticeSize.y;
            return new Vector2(SideSign() * ButtonCornerMargin, bottom);
        }

        /// <summary>
        /// 버튼 아래에 자동 저장 표시가 들어갈 자리가 남는지.
        /// 기획서 수치인 70 = 15 + 40 + 15 에서 하나라도 어긋나면 알 수 있다.
        /// </summary>
        public bool AutoSaveNoticeFitsUnderButton()
        {
            return ButtonCornerMargin >= AutoSaveNoticeGap * 2f + AutoSaveNoticeSize.y;
        }

        private void OnValidate()
        {
            _buttonSize = Mathf.Max(1f, _buttonSize);
            _buttonCornerMargin = Mathf.Max(0f, _buttonCornerMargin);
            _iconSize = Mathf.Clamp(_iconSize, 1f, _buttonSize);
            _autoSaveNoticeSize.x = Mathf.Max(0f, _autoSaveNoticeSize.x);
            _autoSaveNoticeSize.y = Mathf.Max(0f, _autoSaveNoticeSize.y);
            _autoSaveNoticeGap = Mathf.Max(0f, _autoSaveNoticeGap);
        }
    }
}
