using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 행상에 무엇을 올릴 수 있는지 알려 주고, 산 것을 실제로 넣어 주는 쪽.
    /// 성소 기획서 v0.2 는 판매 아이템의 정의를 문양, 코인, 유물 기획서로 넘겼으므로
    /// 성소 코드는 그 목록을 직접 갖지 않고 이 창구로만 주고받는다.
    ///
    /// 보유 여부를 아는 쪽도 이 구현체다.
    /// 05 행상 구성 의 "보유하지 않은 코인 2개", "보유하지 않은 유물 3개"를 지키려면
    /// 후보를 모을 때 이미 가진 것을 빼고 넘겨야 한다.
    /// </summary>
    public interface IMerchantCatalog
    {
        /// <summary>진열에 올릴 수 있는 문양 후보를 모은다. 문양은 보유 여부를 따지지 않는다.</summary>
        void CollectSymbolCandidates(List<MerchantItem> into);

        /// <summary>진열에 올릴 수 있는 코인 후보를 모은다. 보유한 코인은 빼고 넘긴다.</summary>
        void CollectCoinCandidates(List<MerchantItem> into);

        /// <summary>진열에 올릴 수 있는 유물 후보를 모은다. 보유한 유물은 빼고 넘긴다.</summary>
        void CollectRelicCandidates(List<MerchantItem> into);

        /// <summary>
        /// 이 게임에 있는 유물을 전부 모은다. 보유 여부를 따지지 않는다.
        /// 런을 시작할 때 유물이 나올 순서를 미리 뽑아 두는 데 쓴다.
        /// </summary>
        void CollectAllRelics(List<MerchantItem> into);

        /// <summary>지금 가지고 있는 문양을 모은다. 문양 변경으로 바꿀 대상을 고르는 데 쓴다.</summary>
        void CollectOwnedSymbols(List<MerchantItem> into);

        /// <summary>
        /// 그 물건을 더 가질 자리가 있는지. 코인과 유물은 소지 한도가 있다.
        /// 자리가 없으면 행상이 사기 전에 버릴 것을 고르게 한다(`PurchaseResult.NoRoom`). 2026년 10월 9일 원재가 정했다.
        /// </summary>
        bool HasRoomFor(MerchantItem item);

        /// <summary>산 물건을 실제로 넣어 준다.</summary>
        void Acquire(MerchantItem item);

        /// <summary>
        /// 지금 스테이지. 1 부터 센다. 행상이 스테이지별 희귀도 분포(`SanctumConfig.RarityByStage`)를 고르는 데 쓴다.
        /// 후보를 모을 때 그 스테이지에 나올 수 없는 물건을 빼는 것은 구현체가 한다.
        /// </summary>
        int Stage { get; }

        /// <summary>
        /// 가지고 있던 문양 하나를 다른 문양으로 바꾼다.
        /// 어떤 문양으로 바꿀지는 성소 쪽이 시드 난수로 골라 넘긴다.
        /// </summary>
        bool TryReplaceSymbol(string ownedSymbolId, MerchantItem newSymbol);
    }
}
