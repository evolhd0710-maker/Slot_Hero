namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상이 진열하는 상품의 종류.
    /// 성소 기획서 v0.2 / 05 행상 구성 의 진열 항목 중 값을 주고 사는 세 가지다.
    /// 문양 변경과 유물 새로고침은 상품이 아니라 서비스라 여기에 넣지 않는다.
    /// </summary>
    public enum SanctumItemKind
    {
        /// <summary>정해지지 않은 상태. 빈 자리에 쓴다.</summary>
        None = 0,

        /// <summary>문양. 가격은 태그 가치 합으로 산정한다.</summary>
        Symbol = 1,

        /// <summary>코인. 가격은 등급으로 산정한다.</summary>
        Coin = 2,

        /// <summary>유물. 가격은 등급으로 산정한다.</summary>
        Relic = 3,
    }
}
