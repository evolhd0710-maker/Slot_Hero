using UnityEngine;
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
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("야영")]
        [Tooltip("모닥불 그림의 왼쪽 위. (35, 374)")]
        public Vector2 CampPosition = UiScale.V(30f, 370f);

        [Tooltip("모닥불 그림의 크기. (851 × 556)")]
        public Vector2 CampSize = UiScale.V(850f, 560f);

        [Header("행상")]
        [Tooltip("행상인 그림의 왼쪽 위. (1056, 316)")]
        public Vector2 MerchantPosition = UiScale.V(1060f, 320f);

        [Tooltip("행상인 그림의 크기. (777 × 531)")]
        public Vector2 MerchantSize = UiScale.V(780f, 530f);

        [Header("나가기")]
        [Tooltip(
            "오른쪽을 가리키는 화살표 버튼의 왼쪽 위.\n" +
            "PSD 실측은 (1532, 898) 인데 **현재 빌드 버튼과 겹쳐** 왼쪽으로 220 물렸다.\n" +
            "현재 빌드 버튼은 04 화면 공통 규칙 이 우측 하단 1650 부터로 정해 둔 고정 자리다.\n" +
            "화살표를 PSD 자리로 되돌리려면 현재 빌드 버튼을 왼쪽 아래로 옮겨야 한다. " +
            "`HudLayoutConfig.OnLeftCorner` 로 바꾼다.")]
        public Vector2 ExitPosition = UiScale.V(1310f, 900f);

        [Tooltip("화살표 버튼의 크기. (343 × 161)")]
        public Vector2 ExitSize = UiScale.V(340f, 160f);

        [Tooltip("화살표 안에 적는 글자 크기.")]
        public float ExitFontSize = UiScale.Px(44f);

        [Tooltip("글자를 화살표 머리 쪽으로 밀지 않으려고 오른쪽에 비워 두는 폭. (화살표 머리 약 90)")]
        public float ExitArrowHeadWidth = UiScale.Px(90f);

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
            CampSize.x = Mathf.Max(1f, CampSize.x);
            CampSize.y = Mathf.Max(1f, CampSize.y);
            MerchantSize.x = Mathf.Max(1f, MerchantSize.x);
            MerchantSize.y = Mathf.Max(1f, MerchantSize.y);
            ExitSize.x = Mathf.Max(1f, ExitSize.x);
            ExitSize.y = Mathf.Max(1f, ExitSize.y);
            ExitArrowHeadWidth = Mathf.Clamp(ExitArrowHeadWidth, 0f, ExitSize.x - 1f);
        }
    }
}
