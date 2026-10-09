using UnityEngine;
using SlotHero.Ui;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상 화면의 자리와 크기.
    ///
    /// 성소 기획서는 자리를 정하지 않고 "1920 × 1080 고정이라 프리팹에서 맞춘다"로 넘겼다.
    /// 그래서 값은 전부 `행상 예시.psd` 의 레이어 경계에서 잰 것이다.
    /// 괄호 안이 PSD 실측값이고, 화면 값은 10 단위로 맞췄다.
    ///
    /// 진열은 왼쪽 돗자리에 문양 넷과 코인 둘, 오른쪽 양탄자에 문양 변경과 유물 셋이다.
    /// 자리 수는 `SanctumConfig` 가 쥐고 있고 여기는 그 자리를 어디에 놓을지만 정한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "MerchantLayoutConfig",
        menuName = "Slot Hero/성소/행상 화면 자리 설정",
        order = 4)]
    public class MerchantLayoutConfig : ScriptableObject
    {
        [Header("기준")]
        [Tooltip("모든 규격의 기준 해상도. 1920 × 1080 FHD.")]
        public Vector2 ReferenceResolution = UiScale.V(1920f, 1080f);

        [Header("깔개")]
        [Tooltip("문양과 코인을 올려 둔 돗자리의 왼쪽 위. (197, 108)")]
        public Vector2 LeftMatPosition = UiScale.V(200f, 110f);

        [Tooltip("돗자리 크기. (706 × 793)")]
        public Vector2 LeftMatSize = UiScale.V(710f, 790f);

        [Tooltip("문양 변경과 유물을 올려 둔 양탄자의 왼쪽 위. (1020, 117)")]
        public Vector2 RightMatPosition = UiScale.V(1020f, 120f);

        [Tooltip("양탄자 크기. (694 × 774)")]
        public Vector2 RightMatSize = UiScale.V(690f, 770f);

        [Header("문양 자리 · 2 × 2")]
        [Tooltip("첫 문양 자리의 왼쪽 위. (353, 207)")]
        public Vector2 SymbolFirstPosition = UiScale.V(350f, 210f);

        [Tooltip("문양 자리 하나의 크기. 바닥 그림 크기다. (169 × 176)")]
        public Vector2 SymbolSlotSize = UiScale.V(170f, 180f);

        [Tooltip("문양 자리 사이의 가로와 세로 간격. (실측 213, 209)")]
        public Vector2 SymbolSpacing = UiScale.V(210f, 210f);

        [Tooltip("한 줄에 놓는 문양 수.")]
        [Min(1)]
        public int SymbolColumns = 2;

        [Header("코인 자리 · 가로")]
        [Tooltip("첫 코인 자리의 왼쪽 위. (364, 634)")]
        public Vector2 CoinFirstPosition = UiScale.V(360f, 630f);

        [Tooltip("코인 자리 하나의 크기. 받침 그림 크기다. (145 × 145)")]
        public Vector2 CoinSlotSize = UiScale.V(150f, 150f);

        [Tooltip("코인 자리 사이의 가로 간격. (실측 195)")]
        public float CoinSpacing = UiScale.Px(200f);

        [Header("유물 자리 · 가로")]
        [Tooltip("첫 유물 자리의 왼쪽 위. (1172, 579)")]
        public Vector2 RelicFirstPosition = UiScale.V(1170f, 580f);

        [Tooltip("유물 자리 하나의 크기. 양탄자에 그려진 칸 안쪽이다. (약 92 × 115)")]
        public Vector2 RelicSlotSize = UiScale.V(100f, 120f);

        [Tooltip("유물 자리 사이의 가로 간격. 실측 134 와 137 의 가운데라 10 으로 떨어지지 않는다.")]
        public float RelicSpacing = UiScale.Px(135f);

        [Header("문양 변경")]
        [Tooltip("보라 소용돌이 칸의 왼쪽 위. (1235, 150)")]
        public Vector2 SymbolChangePosition = UiScale.V(1240f, 150f);

        [Tooltip("보라 소용돌이 칸의 크기. (약 245 × 252)")]
        public Vector2 SymbolChangeSize = UiScale.V(240f, 250f);

        [Header("유물 새로고침")]
        [Tooltip("구슬 위의 새로고침 표시 자리. (1333, 447)")]
        public Vector2 RefreshPosition = UiScale.V(1330f, 450f);

        [Tooltip("새로고침 표시 크기. (44 × 46)")]
        public Vector2 RefreshSize = UiScale.V(50f, 50f);

        [Header("가격")]
        [Tooltip("가격 한 줄이 차지하는 칸. 골드 그림과 숫자가 가운데 맞춤으로 들어간다.")]
        public Vector2 PriceSize = UiScale.V(150f, 30f);

        [Tooltip("가격의 골드 그림 크기. (31 × 28)")]
        public Vector2 PriceIconSize = UiScale.V(30f, 30f);

        [Tooltip("골드 그림과 숫자 사이 간격.")]
        public float PriceIconGap = UiScale.Px(10f);

        [Tooltip("가격 글자 크기.")]
        public float PriceFontSize = UiScale.Px(28f);

        [Tooltip("문양 가격 줄의 위쪽이 문양 자리 아래 끝에서 떨어진 거리. 음수면 자리에 걸친다. (실측 -12)")]
        public float SymbolPriceOffsetY = UiScale.Px(-10f);

        [Tooltip("코인 가격 줄. 받침이 둥글어 문양보다 아래에 있다. (실측 8)")]
        public float CoinPriceOffsetY = UiScale.Px(10f);

        [Tooltip("유물 가격 줄. (실측 13)")]
        public float RelicPriceOffsetY = UiScale.Px(10f);

        [Tooltip("새로고침 가격 줄. 구슬 아래다. (실측 535 에서 새로고침 아래 500 을 뺀 값)")]
        public float RefreshPriceOffsetY = UiScale.Px(30f);

        [Header("나가기")]
        [Tooltip("아래를 가리키는 화살표 버튼의 왼쪽 위. (756, 917)")]
        public Vector2 BackPosition = UiScale.V(760f, 920f);

        [Tooltip("화살표 버튼의 크기. (375 × 133)")]
        public Vector2 BackSize = UiScale.V(370f, 130f);

        [Tooltip("화살표 안에 적는 글자 크기.")]
        public float BackFontSize = UiScale.Px(44f);

        [Tooltip("글자를 화살표 머리 쪽으로 밀지 않으려고 아래에 비워 두는 높이. (화살표 머리 약 45)")]
        public float BackArrowHeadHeight = UiScale.Px(50f);

        /// <summary>index 번째 문양 자리의 왼쪽 위. 왼쪽에서 오른쪽, 위에서 아래로 센다.</summary>
        public Vector2 GetSymbolPosition(int index)
        {
            int column = SymbolColumns <= 0 ? 0 : index % SymbolColumns;
            int row = SymbolColumns <= 0 ? index : index / SymbolColumns;

            return new Vector2(
                SymbolFirstPosition.x + column * SymbolSpacing.x,
                SymbolFirstPosition.y + row * SymbolSpacing.y);
        }

        /// <summary>index 번째 코인 자리의 왼쪽 위.</summary>
        public Vector2 GetCoinPosition(int index)
        {
            return new Vector2(CoinFirstPosition.x + index * CoinSpacing, CoinFirstPosition.y);
        }

        /// <summary>index 번째 유물 자리의 왼쪽 위.</summary>
        public Vector2 GetRelicPosition(int index)
        {
            return new Vector2(RelicFirstPosition.x + index * RelicSpacing, RelicFirstPosition.y);
        }

        /// <summary>자리 아래에 붙는 가격 줄의 왼쪽 위. 자리 가운데에 맞춘다.</summary>
        public Vector2 GetPricePosition(Vector2 slotPosition, Vector2 slotSize, float offsetY)
        {
            return new Vector2(
                slotPosition.x + slotSize.x * 0.5f - PriceSize.x * 0.5f,
                slotPosition.y + slotSize.y + offsetY);
        }

        /// <summary>문양 변경 가격은 소용돌이 한가운데에 적는다.</summary>
        public Vector2 GetSymbolChangePricePosition()
        {
            return new Vector2(
                SymbolChangePosition.x + SymbolChangeSize.x * 0.5f - PriceSize.x * 0.5f,
                SymbolChangePosition.y + SymbolChangeSize.y * 0.5f - PriceSize.y * 0.5f);
        }

        /// <summary>문양 자리 넷이 모두 왼쪽 돗자리 안에 들어가는지.</summary>
        public bool SymbolsFitLeftMat(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetSymbolPosition(i), SymbolSlotSize, LeftMatPosition, LeftMatSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>코인 자리가 모두 왼쪽 돗자리 안에 들어가는지.</summary>
        public bool CoinsFitLeftMat(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetCoinPosition(i), CoinSlotSize, LeftMatPosition, LeftMatSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>유물 자리가 모두 오른쪽 양탄자 안에 들어가는지.</summary>
        public bool RelicsFitRightMat(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!Inside(GetRelicPosition(i), RelicSlotSize, RightMatPosition, RightMatSize))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>문양 자리와 코인 자리가 위아래로 겹치지 않는지.</summary>
        public bool SymbolsAndCoinsDoNotOverlap(int symbolCount)
        {
            float bottom = 0f;
            for (int i = 0; i < symbolCount; i++)
            {
                float y = GetSymbolPosition(i).y + SymbolSlotSize.y;
                if (y > bottom)
                {
                    bottom = y;
                }
            }

            return bottom <= CoinFirstPosition.y;
        }

        /// <summary>두 깔개가 서로 겹치지 않는지.</summary>
        public bool MatsDoNotOverlap()
        {
            return LeftMatPosition.x + LeftMatSize.x <= RightMatPosition.x;
        }

        /// <summary>나가기가 두 깔개 사이 아래에 있는지.</summary>
        public bool BackIsBelowMats()
        {
            return BackPosition.y >= LeftMatPosition.y + LeftMatSize.y
                || BackPosition.y >= RightMatPosition.y + RightMatSize.y;
        }

        /// <summary>나가기 글자가 들어갈 칸. 화살표 머리를 뺀 위쪽이다.</summary>
        public Vector2 GetBackLabelSize()
        {
            return new Vector2(BackSize.x, BackSize.y - BackArrowHeadHeight);
        }

        private static bool Inside(Vector2 position, Vector2 size, Vector2 boxPosition, Vector2 boxSize)
        {
            return position.x >= boxPosition.x
                && position.y >= boxPosition.y
                && position.x + size.x <= boxPosition.x + boxSize.x
                && position.y + size.y <= boxPosition.y + boxSize.y;
        }

        private void OnValidate()
        {
            SymbolColumns = Mathf.Max(1, SymbolColumns);
            CoinSpacing = Mathf.Max(0f, CoinSpacing);
            RelicSpacing = Mathf.Max(0f, RelicSpacing);
            PriceIconGap = Mathf.Max(0f, PriceIconGap);
            BackArrowHeadHeight = Mathf.Clamp(BackArrowHeadHeight, 0f, BackSize.y - 1f);
        }
    }
}
