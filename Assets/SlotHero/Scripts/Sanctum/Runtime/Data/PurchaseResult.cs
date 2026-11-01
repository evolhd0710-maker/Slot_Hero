namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상에서 값을 치르는 모든 행동의 결과.
    /// 성소 기획서 v0.2 / 06 재화 의 "보유 수치를 초과하여 소모할 수 없다"를 코드에서 지키는 자리다.
    /// </summary>
    public enum PurchaseResult
    {
        /// <summary>값을 치르고 처리까지 끝났다.</summary>
        Success = 0,

        /// <summary>골드가 모자라다. 가격을 붉은 색으로 표기한다.</summary>
        NotEnoughGold = 1,

        /// <summary>이미 팔린 자리다.</summary>
        AlreadySold = 2,

        /// <summary>상품이 없는 빈 자리다.</summary>
        EmptySlot = 3,

        /// <summary>바꿔 줄 후보가 없다. 카탈로그가 후보를 내놓지 못한 경우다.</summary>
        NoCandidate = 4,

        /// <summary>지금은 쓸 수 없는 기능이다. 야영을 이미 쓴 경우가 여기에 해당한다.</summary>
        Unavailable = 5,

        /// <summary>
        /// 코인이나 유물이 소지 한도까지 차 있다. 값은 치르지 않았다.
        /// 성소는 버릴 것을 고르게 하고(`SanctumController.RoomNeeded`) 고르면 다시 산다. 2026년 10월 9일 원재가 정했다.
        /// </summary>
        NoRoom = 6,
    }
}
