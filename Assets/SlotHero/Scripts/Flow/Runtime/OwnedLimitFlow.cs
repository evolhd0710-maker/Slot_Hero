using System.Collections.Generic;
using SlotHero.CurrentBuild;
using SlotHero.Reward;
using SlotHero.Sanctum;

namespace SlotHero.Flow
{
    /// <summary>
    /// 코인과 유물의 소지 한도를 다룬다.
    ///
    /// 전투 시스템 기획서가 "유물은 소지하는 갯수에 제한이 있다" 고 정했고,
    /// 전투 UI 기획서 stt 와 인게임 화면 기획서 13 이 코인 최대 5개, 유물 최대 6개로 적었다.
    /// 가득 찼을 때의 처리는 2026년 10월 9일 원재가 정했다.
    ///
    ///   - 가득 찬 상태로 새것을 얻으면 버릴 것 고르기 창을 띄운다. 고르면 그것을 버리고 새것을 받고, 닫으면 획득을 포기한다
    ///   - 행상에서는 사기 전에 버릴 것을 고른다. 포기하면 사지 않아 골드도 그대로다
    ///   - 한도가 줄어 넘치면 넘친 만큼 바로 버리게 한다. 이때는 닫을 수 없다
    ///   - 한도가 바뀌는 조건은 아직 없다. 바꾸는 창구(`RunContext.ChangeCoinCapacity` 등)만 둔다
    ///
    /// 화면은 모른다. 고르기 화면에 넘길 요청을 만들고 무엇을 몇 개 버려야 하는지만 센다.
    /// 화면을 열고 고른 것을 받는 일은 `GameFlowController` 가 한다.
    /// </summary>
    public static class OwnedLimitFlow
    {
        /// <summary>버릴 것 고르기 창의 포기 버튼 글.</summary>
        public const string GiveUpText = "포기";

        /// <summary>그만큼 새로 받으려면 몇 개를 버려야 하는지. 자리가 넉넉하면 0 이다.</summary>
        public static int DiscardsNeeded(RunContext context, ItemPickKind kind, int incoming)
        {
            if (context == null || incoming <= 0)
            {
                return 0;
            }

            int room = kind == ItemPickKind.Coin ? context.Run.Owned.CoinRoom : context.Run.Owned.RelicRoom;
            return incoming > room ? incoming - room : 0;
        }

        /// <summary>한도가 줄어 넘친 개수. 넘친 만큼 바로 버려야 한다.</summary>
        public static int Overflow(RunContext context, ItemPickKind kind)
        {
            if (context == null)
            {
                return 0;
            }

            return kind == ItemPickKind.Coin ? context.Run.Owned.CoinOverflow : context.Run.Owned.RelicOverflow;
        }

        /// <summary>보상 카드가 코인이나 유물이면 그 종류. 아니면 false 다.</summary>
        public static bool TryGetKind(RewardCard card, out ItemPickKind kind)
        {
            kind = ItemPickKind.Symbol;
            if (card == null)
            {
                return false;
            }

            switch (card.Kind)
            {
                case RewardKind.Coin:
                    kind = ItemPickKind.Coin;
                    return true;

                case RewardKind.Relic:
                    kind = ItemPickKind.Relic;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>행상의 물건이 코인이나 유물이면 그 종류. 아니면 false 다.</summary>
        public static bool TryGetKind(MerchantItem item, out ItemPickKind kind)
        {
            kind = ItemPickKind.Symbol;

            switch (item.Kind)
            {
                case SanctumItemKind.Coin:
                    kind = ItemPickKind.Coin;
                    return true;

                case SanctumItemKind.Relic:
                    kind = ItemPickKind.Relic;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 버릴 것 고르기 창의 요청. 후보는 지금 가진 코인이나 유물이다.
        /// incomingName 은 새로 받을 것의 이름이고, 비우면 한도가 줄어 버리는 경우다. 그때는 닫을 수 없다.
        /// excluded 에 든 것은 이미 버리기로 고른 것이라 후보에서 하나씩 뺀다. 여러 개를 버려야 할 때 쓴다.
        /// </summary>
        public static ItemPickRequest MakeDiscardRequest(
            RunContext context, ItemPickKind kind, string incomingName, IList<string> excluded)
        {
            CurrentBuildSnapshot build = context.BuildSnapshot();
            bool forced = string.IsNullOrEmpty(incomingName);
            string noun = kind == ItemPickKind.Coin ? "코인" : "유물";

            ItemPickRequest request = new ItemPickRequest();
            request.Kind = kind;
            request.Build = build;
            request.ShowCount = false;
            request.Title = "버릴 " + noun + " 선택";
            request.Cancellable = !forced;
            request.CancelText = GiveUpText;
            request.ConfirmText = forced ? "버리기" : "버리고 받기";
            request.ResultFormat = forced
                ? "[{0}] 을 버린다. 최대 " + noun + " 수가 줄었다"
                : "[{0}] 을 버리고 [" + incomingName + "] 을 받는다";

            List<BuildEntry> owned = kind == ItemPickKind.Coin ? build.Coins : build.Relics;
            List<string> skip = excluded != null ? new List<string>(excluded) : new List<string>();

            for (int i = 0; i < owned.Count; i++)
            {
                if (skip.Remove(owned[i].Id))
                {
                    continue;
                }

                request.Candidates.Add(owned[i]);
            }

            return request;
        }
    }
}
