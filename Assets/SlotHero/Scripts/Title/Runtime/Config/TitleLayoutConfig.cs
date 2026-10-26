using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Title
{
    /// <summary>
    /// 타이틀 화면의 자리와 크기.
    /// 기본값은 인게임 화면 기획서 v0.2 / 05 타이틀 화면 과 그 와이어프레임에서 잰 값을
    /// 10 단위로 옮긴 것이다.
    ///
    /// 기획서가 정한 것은 로고가 화면 왼쪽 위, 메뉴가 화면 왼쪽에 세로,
    /// 항목이 늘면 간격을 조정한다는 것까지다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TitleLayoutConfig",
        menuName = "Slot Hero/타이틀/타이틀 화면 자리 설정",
        order = 0)]
    public class TitleLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Tooltip("화면 가장자리에서 남기는 여백. 04 화면 공통 규칙 의 안전 영역 40.")]
        [Min(0f)]
        [FormerlySerializedAs("SafeAreaPadding")]
        [SerializeField] private float _safeAreaPadding = UiScale.Px(40f);
        public float SafeAreaPadding { get { return _safeAreaPadding; } set { _safeAreaPadding = value; } }

        [Header("로고")]
        [Tooltip("로고의 자리. 와이어프레임 실측 x 172 · y 218 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("LogoPosition")]
        [SerializeField] private Vector2 _logoPosition = UiScale.V(170f, 220f);
        public Vector2 LogoPosition { get { return _logoPosition; } set { _logoPosition = value; } }

        [Tooltip("로고의 크기. 와이어프레임 실측 470 × 66 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("LogoSize")]
        [SerializeField] private Vector2 _logoSize = UiScale.V(470f, 70f);
        public Vector2 LogoSize { get { return _logoSize; } set { _logoSize = value; } }

        [Header("메뉴")]
        [Tooltip("첫 메뉴 항목의 자리. 와이어프레임 실측 x 176 · y 595 를 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("MenuPosition")]
        [SerializeField] private Vector2 _menuPosition = UiScale.V(170f, 600f);
        public Vector2 MenuPosition { get { return _menuPosition; } set { _menuPosition = value; } }

        [Tooltip("메뉴 항목 하나의 크기. 누르는 영역이다.")]
        [FormerlySerializedAs("MenuItemSize")]
        [SerializeField] private Vector2 _menuItemSize = UiScale.V(400f, 50f);
        public Vector2 MenuItemSize { get { return _menuItemSize; } set { _menuItemSize = value; } }

        [Tooltip("메뉴 항목 사이 간격. 와이어프레임 실측 약 114 를 10 단위로 옮긴 값이다.")]
        [Min(1f)]
        [FormerlySerializedAs("MenuSpacing")]
        [SerializeField] private float _menuSpacing = UiScale.Px(110f);
        public float MenuSpacing { get { return _menuSpacing; } set { _menuSpacing = value; } }

        [Tooltip("항목이 늘어 화면을 넘칠 때 줄일 수 있는 가장 좁은 간격.")]
        [Min(1f)]
        [FormerlySerializedAs("MinMenuSpacing")]
        [SerializeField] private float _minMenuSpacing = UiScale.Px(60f);
        public float MinMenuSpacing { get { return _minMenuSpacing; } set { _minMenuSpacing = value; } }

        [Header("버전")]
        [Tooltip("버전 글의 자리. 와이어프레임 실측 x 1537 · y 996 을 10 단위로 옮긴 값이다.")]
        [FormerlySerializedAs("VersionPosition")]
        [SerializeField] private Vector2 _versionPosition = UiScale.V(1540f, 990f);
        public Vector2 VersionPosition { get { return _versionPosition; } set { _versionPosition = value; } }

        [Tooltip("버전 글의 크기.")]
        [FormerlySerializedAs("VersionSize")]
        [SerializeField] private Vector2 _versionSize = UiScale.V(100f, 30f);
        public Vector2 VersionSize { get { return _versionSize; } set { _versionSize = value; } }

        /// <summary>
        /// 메뉴 항목 사이의 실제 간격.
        /// 기획서의 "항목이 추가될 경우 간격을 조정하여 배치한다"를 여기서 한다.
        /// 기본 간격으로 넣어 화면을 넘치지 않으면 그대로 쓰고, 넘치면 줄인다.
        /// </summary>
        public float GetMenuSpacing(int count)
        {
            if (count <= 1)
            {
                return MenuSpacing;
            }

            float available = GetMenuAvailableHeight();
            float needed = (count - 1) * MenuSpacing;

            if (needed <= available)
            {
                return MenuSpacing;
            }

            // 10 단위를 지키려고 내림으로 맞춘다.
            float fitted = available / (count - 1);
            float rounded = Mathf.Floor(fitted / 10f) * 10f;

            return rounded < MinMenuSpacing ? MinMenuSpacing : rounded;
        }

        /// <summary>메뉴 항목 하나의 자리. 화면 왼쪽 위를 기준으로 잡은 값이다.</summary>
        public Vector2 GetMenuItemPosition(int index, int count)
        {
            float y = MenuPosition.y + index * GetMenuSpacing(count);
            return new Vector2(MenuPosition.x, y);
        }

        /// <summary>메뉴가 쓸 수 있는 세로 길이. 첫 항목 위부터 안전 영역 안쪽 끝까지다.</summary>
        public float GetMenuAvailableHeight()
        {
            float bottom = ReferenceResolution.y - SafeAreaPadding;
            return bottom - MenuPosition.y - MenuItemSize.y;
        }

        /// <summary>기본 간격 그대로 화면에 들어가는 항목 수.</summary>
        public int GetMenuCountAtDefaultSpacing()
        {
            if (MenuSpacing <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt(GetMenuAvailableHeight() / MenuSpacing) + 1;
        }

        /// <summary>그만큼의 항목이 화면 안에 들어가는지.</summary>
        public bool MenuFitsScreen(int count)
        {
            if (count <= 0)
            {
                return true;
            }

            float bottom = GetMenuItemPosition(count - 1, count).y + MenuItemSize.y;
            return bottom <= ReferenceResolution.y - SafeAreaPadding;
        }

        /// <summary>로고와 메뉴의 왼쪽 끝이 맞는지. 와이어프레임에서 둘 다 같은 자리에서 시작한다.</summary>
        public bool LogoAndMenuShareLeft()
        {
            return Mathf.Abs(LogoPosition.x - MenuPosition.x) < 1f;
        }

        /// <summary>버전 글이 화면 안쪽에 들어가는지.</summary>
        public bool VersionFitsScreen()
        {
            return VersionPosition.x + VersionSize.x <= ReferenceResolution.x
                   && VersionPosition.y + VersionSize.y <= ReferenceResolution.y;
        }

        private void OnValidate()
        {
            _logoSize.x = Mathf.Max(1f, _logoSize.x);
            _logoSize.y = Mathf.Max(1f, _logoSize.y);
            _menuItemSize.x = Mathf.Max(1f, _menuItemSize.x);
            _menuItemSize.y = Mathf.Max(1f, _menuItemSize.y);
            _versionSize.x = Mathf.Max(1f, _versionSize.x);
            _versionSize.y = Mathf.Max(1f, _versionSize.y);
            _menuSpacing = Mathf.Max(1f, _menuSpacing);
            _minMenuSpacing = Mathf.Clamp(_minMenuSpacing, 1f, _menuSpacing);
            _safeAreaPadding = Mathf.Max(0f, _safeAreaPadding);
        }
    }
}
