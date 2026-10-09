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
    }
}
