using UnityEngine;
using UnityEngine.Serialization;
using SlotHero.Ui;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 성소 화면의 자리와 크기.
    ///
    /// 성소 기획서는 자리를 정하지 않고 "1920 × 1080 고정이라 프리팹에서 맞춘다"로 넘겼다.
    /// 그래서 값은 전부 `성소 이미지 예시.psd` 의 레이어 경계에서 잰 것이다.
    /// 괄호 안이 PSD 실측값이고, 화면 값은 10 단위로 맞췄다.
    ///
    /// 화면은 그림 석 장이 전부다. 모닥불이 야영, 행상인이 행상, 화살표가 나가기다.
    /// 누르는 영역이 곧 그림이라 따로 칸을 두지 않는다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SanctumLayoutConfig",
        menuName = "Slot Hero/성소/성소 화면 자리 설정",
        order = 3)]
    public class SanctumLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        [FormerlySerializedAs("ReferenceResolution")]
        [SerializeField] private Vector2 _referenceResolution = UiScale.V(1920f, 1080f);
        public Vector2 ReferenceResolution { get { return _referenceResolution; } set { _referenceResolution = value; } }

        [Header("야영")]
        [Tooltip("모닥불 그림의 왼쪽 위. (35, 374)")]
        [FormerlySerializedAs("CampPosition")]
        [SerializeField] private Vector2 _campPosition = UiScale.V(30f, 370f);
        public Vector2 CampPosition { get { return _campPosition; } set { _campPosition = value; } }

        [Tooltip("모닥불 그림의 크기. (851 × 556)")]
        [FormerlySerializedAs("CampSize")]
        [SerializeField] private Vector2 _campSize = UiScale.V(850f, 560f);
        public Vector2 CampSize { get { return _campSize; } set { _campSize = value; } }

        [Header("행상")]
        [Tooltip("행상인 그림의 왼쪽 위. (1056, 316)")]
        [FormerlySerializedAs("MerchantPosition")]
        [SerializeField] private Vector2 _merchantPosition = UiScale.V(1060f, 320f);
        public Vector2 MerchantPosition { get { return _merchantPosition; } set { _merchantPosition = value; } }

        [Tooltip("행상인 그림의 크기. (777 × 531)")]
        [FormerlySerializedAs("MerchantSize")]
        [SerializeField] private Vector2 _merchantSize = UiScale.V(780f, 530f);
        public Vector2 MerchantSize { get { return _merchantSize; } set { _merchantSize = value; } }

        [Header("나가기")]
        [Tooltip(
            "오른쪽을 가리키는 화살표 버튼의 왼쪽 위.\n" +
            "PSD 실측은 (1532, 898) 인데 **현재 빌드 버튼과 겹쳐** 왼쪽으로 220 물렸다.\n" +
            "현재 빌드 버튼은 04 화면 공통 규칙 이 우측 하단 1650 부터로 정해 둔 고정 자리다.\n" +
            "화살표를 PSD 자리로 되돌리려면 현재 빌드 버튼을 왼쪽 아래로 옮겨야 한다. " +
            "`HudLayoutConfig.OnLeftCorner` 로 바꾼다.")]
        [FormerlySerializedAs("ExitPosition")]
        [SerializeField] private Vector2 _exitPosition = UiScale.V(1310f, 900f);
        public Vector2 ExitPosition { get { return _exitPosition; } set { _exitPosition = value; } }

        [Tooltip("화살표 버튼의 크기. (343 × 161)")]
        [FormerlySerializedAs("ExitSize")]
        [SerializeField] private Vector2 _exitSize = UiScale.V(340f, 160f);
        public Vector2 ExitSize { get { return _exitSize; } set { _exitSize = value; } }

        [Tooltip("화살표 안에 적는 글자 크기.")]
        [FormerlySerializedAs("ExitFontSize")]
        [SerializeField] private float _exitFontSize = UiScale.Px(44f);
        public float ExitFontSize { get { return _exitFontSize; } set { _exitFontSize = value; } }

        [Tooltip("글자를 화살표 머리 쪽으로 밀지 않으려고 오른쪽에 비워 두는 폭. (화살표 머리 약 90)")]
        [FormerlySerializedAs("ExitArrowHeadWidth")]
        [SerializeField] private float _exitArrowHeadWidth = UiScale.Px(90f);
        public float ExitArrowHeadWidth { get { return _exitArrowHeadWidth; } set { _exitArrowHeadWidth = value; } }

        /// <summary>야영과 행상 그림이 서로 겹치지 않는지.</summary>
        public bool CampAndMerchantDoNotOverlap()
        {
            return CampPosition.x + CampSize.x <= MerchantPosition.x;
        }

        /// <summary>나가기가 화면 오른쪽 아래 안전 영역 안에 있는지.</summary>
        public bool ExitFitsSafeArea()
        {
            float margin = UiScale.Px(40f);
            return ExitPosition.x + ExitSize.x <= ReferenceResolution.x - margin * 0.5f
                && ExitPosition.y + ExitSize.y <= ReferenceResolution.y - margin * 0.5f;
        }

        /// <summary>
        /// 나가기가 현재 빌드 버튼과 겹치지 않는지.
        /// 둘 다 화면 오른쪽 아래를 쓰므로 자리를 바꿀 때마다 확인해야 한다.
        /// </summary>
        public bool ExitClearsHudButton(float hudButtonLeft)
        {
            return ExitPosition.x + ExitSize.x <= hudButtonLeft;
        }

        /// <summary>나가기 글자가 들어갈 칸. 화살표 머리를 뺀 왼쪽이다.</summary>
        public Vector2 GetExitLabelSize()
        {
            return new Vector2(ExitSize.x - ExitArrowHeadWidth, ExitSize.y);
        }

        private void OnValidate()
        {
            _campSize.x = Mathf.Max(1f, _campSize.x);
            _campSize.y = Mathf.Max(1f, _campSize.y);
            _merchantSize.x = Mathf.Max(1f, _merchantSize.x);
            _merchantSize.y = Mathf.Max(1f, _merchantSize.y);
            _exitSize.x = Mathf.Max(1f, _exitSize.x);
            _exitSize.y = Mathf.Max(1f, _exitSize.y);
            _exitArrowHeadWidth = Mathf.Clamp(_exitArrowHeadWidth, 0f, _exitSize.x - 1f);
        }
    }
}
