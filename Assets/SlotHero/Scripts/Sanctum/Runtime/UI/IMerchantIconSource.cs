using UnityEngine;

namespace SlotHero.Sanctum.UI
{
    /// <summary>
    /// 상품의 그림을 내주는 쪽.
    /// <see cref="MerchantItem"/>은 저장에 그대로 실려야 해서 값만 들고 있으므로
    /// 화면에 그릴 그림은 식별자를 주고 여기서 받아 온다.
    ///
    /// 문양, 코인, 유물 기획서의 데이터가 생기면
    /// 그 데이터를 감싸는 클래스가 <see cref="IMerchantCatalog"/>와 이 창구를 함께 구현하면 된다.
    /// </summary>
    public interface IMerchantIconSource
    {
        /// <summary>상품의 그림. 없으면 null 을 돌려주면 된다.</summary>
        Sprite GetIcon(MerchantItem item);

        /// <summary>
        /// 문양 상품의 태그 테두리 둘. 첫 태그는 우상단, 둘째 태그는 좌하단 그림이다(2026년 10월 19일 원재).
        /// 성소는 태그를 모르므로 그림만 받아 온다. 문양이 아니거나 모르면 false 다.
        /// </summary>
        bool TryGetTagCorners(MerchantItem item, out Sprite topRight, out Sprite bottomLeft);

        /// <summary>
        /// 문양 상품의 태그 테두리 장식 모서리에 얹을 행성 기호 둘. 첫 태그는 우상단, 둘째 태그는 좌하단이다(2026년 10월 26일 원재).
        /// 문양이 아니거나 모르면 false 다.
        /// </summary>
        bool TryGetTagGlyphs(MerchantItem item, out Sprite topRight, out Sprite bottomLeft);

        /// <summary>기호 크기. 테두리 한 변에 대한 비율이다. 다른 화면의 문양 칸과 같은 값을 쓴다.</summary>
        float CornerGlyphSizeRatio { get; }

        /// <summary>모서리에서 기호 가운데까지. 테두리 한 변에 대한 비율이다.</summary>
        float CornerGlyphFromCorner { get; }

        /// <summary>
        /// 태그 테두리와 문양 그림을 진열 칸 가장자리에서 얼마나 안쪽에 둘지. 칸 한 변에 대한 비율이다.
        /// 다른 화면의 문양 칸과 같은 값을 쓴다(2026년 10월 20일 원재).
        /// </summary>
        float SymbolFrameInset { get; }
    }
}
