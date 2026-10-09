using SlotHero.Map;
using SlotHero.Reward;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 방 하나의 보상을 정산한다. 무엇을 받을지 정하고, 받기 전에 남겨 두고, 받으면 한 번에 넣는다.
    ///
    /// **모든 보상은 받을 때 한 번에 받는다.** 2026년 10월 6일 원재가 정했다.
    /// 전투 골드와 오버킬은 전투가 끝날 때 런 데이터에 쌓아 두었다가 보상 화면에서 함께 받는다.
    /// 방 규칙 골드와 카드는 시드에서 다시 만든다. 그래서 받기 전에 나갔다 이어 해도 같은 보상이 나온다.
    ///
    /// 2026년 10월 8일에 `GameFlowController` 에서 떼어 냈다. `RunSession` 참고.
    /// 보상 화면을 여닫는 일은 하지 않는다. 그것은 흐름 조종기에 남는다.
    /// </summary>
    public class RoomRewardFlow
    {
        private readonly RewardMaker _maker;
        private readonly SaveService _save;

        /// <summary>보상 규칙과 물건 목록, 저장 창구를 물려 만든다.</summary>
        public RoomRewardFlow(RewardRuleConfig rules, RunCatalogConfig catalog, SaveService save)
        {
            _maker = new RewardMaker(rules, catalog);
            _save = save;
        }

        /// <summary>보상을 정하는 쪽. 검사가 같은 보상이 다시 나오는지 볼 때 쓴다.</summary>
        public RewardMaker Maker
        {
            get { return _maker; }
        }

        /// <summary>
        /// 방에 들어간다. 앞 방의 전투 골드와 오버킬이 남아 있으면 이 방의 보상에 잘못 얹히므로 비운다.
        /// </summary>
        public void BeginRoom(RunSaveData run)
        {
            if (run == null)
            {
                return;
            }

            run.RewardOverkillGold = 0;
            run.RewardCombatGold = 0;
            run.RewardCardRounds = 0;
            run.RewardCardRoundsTaken = 0;
        }

        /// <summary>
        /// 전투에서 이겼다. 전투 골드와 오버킬을 보상에 쌓는다.
        ///
        /// 오버킬 골드는 전투가 정한 값이라 시드로 다시 만들 수 없다. 방 완료 저장에 함께 실어 이어해도 남게 한다.
        /// 두 값 모두 방에 들어갈 때 0 으로 비우고 전투마다 더한다. 한 방에서 전투가 여럿이어도 전부 보상에 들어간다.
        /// **전투 골드는 지금 주지 않고 보상 골드에 합친다.** 2026년 10월 6일 원재가 정했다.
        /// 이벤트 기획서 3장 도전 이벤트 공통 규칙 · 전투 골드 보상이 2배가 된다(`CombatOptions.DoubleReward`).
        /// </summary>
        public void AddCombat(RunSaveData run, CombatResult result, CombatOptions options)
        {
            if (run == null)
            {
                return;
            }

            run.RewardOverkillGold += result.OverkillGold;
            run.RewardCombatGold += options.DoubleReward ? result.GoldGained * 2 : result.GoldGained;

            // 이벤트 방은 방 규칙에 카드가 없다. 이벤트가 연 전투를 이기면 고르기를 따로 쌓는다.
            // 보상 2배면 "전투 후 보상 선택(문양, 유물 등)을 2회 진행한다"(3장).
            if (options.FromEvent)
            {
                run.RewardCardRounds += options.DoubleReward ? 2 : 1;
            }
        }

        /// <summary>
        /// 그 방의 보상. 보상을 만들 수 없으면 null 이다.
        /// **성소는 보상이 없다.** 2026년 10월 6일 원재가 정했다. 규칙 에셋에 무엇을 적든 주지 않는다.
        /// 전투 골드는 기본 보상 골드에 더하고, 오버킬은 오버킬 줄에 얹는다. 둘 다 저장해 둔 값이다.
        /// </summary>
        public RewardOffer Make(RunContext context, MapNode node)
        {
            if (_maker == null || context == null || node == null)
            {
                return null;
            }

            if (node.RoomType == RoomType.Sanctum)
            {
                return null;
            }

            RunSaveData run = context.Run;
            RewardOffer offer = _maker.Make(run.Seed, node, run);
            if (offer == null)
            {
                return null;
            }

            // 두 번째 고르기부터는 카드만 있다. 골드는 첫 고르기에서 다 받았다.
            if (run.RewardCardRoundsTaken > 0)
            {
                offer.BaseGold = 0;
            }

            offer.BaseGold += run.RewardCombatGold;
            offer.OverkillGold = run.RewardOverkillGold;

            if (run.RewardCardRoundsTaken < run.RewardCardRounds)
            {
                _maker.AddEventCombatCards(offer, run.Seed, node, run, run.RewardCardRoundsTaken);
            }

            return offer;
        }

        /// <summary>
        /// 고르기가 더 남았는지. 35 도전자 를 이기면 보상 화면을 두 번 연다.
        /// `Receive` 가 고를 때마다 하나씩 센다.
        /// </summary>
        public bool HasMoreRounds(RunContext context)
        {
            return context != null && context.Run.RewardPending
                && context.Run.RewardCardRoundsTaken > 0
                && context.Run.RewardCardRoundsTaken < context.Run.RewardCardRounds;
        }

        /// <summary>
        /// 방 완료와 함께 "보상 남음" 을 적어 둔다. 저장은 방 완료 저장이 한다.
        /// 카드를 고르기 전에 나갔다 이어하면 보상 화면을 다시 띄우려는 것이다.
        /// </summary>
        public void MarkPending(RunContext context, RewardOffer offer)
        {
            if (context != null)
            {
                context.Run.RewardPending = offer != null && !offer.IsEmpty;
            }
        }

        /// <summary>남겨 둔 보상이 없다고 적는다. 있었으면 바로 저장한다.</summary>
        public void ClearPending(RunContext context)
        {
            if (context == null
                || (!context.Run.RewardPending && context.Run.RewardOverkillGold == 0 && context.Run.RewardCombatGold == 0
                    && context.Run.RewardCardRounds == 0))
            {
                return;
            }

            Clear(context.Run);

            if (_save != null)
            {
                _save.FlushRun();
            }
        }

        /// <summary>
        /// 보상을 받았다. **골드 전부와 고른 카드를 한 번에 받고 한 번에 저장한다.**
        /// 따로 쓰면 그 사이에 꺼졌을 때 하나만 받거나 두 번 받는다.
        /// </summary>
        public void Receive(RunContext context, int gold, RewardCard card)
        {
            if (context == null)
            {
                return;
            }

            RunSaveData run = context.Run;

            if (gold > 0)
            {
                context.GainGold(gold);
            }

            if (card != null)
            {
                switch (card.Kind)
                {
                    case RewardKind.Symbol:
                        run.Owned.AddSymbol(card.Id, 1);
                        break;

                    // 소지 한도까지 찼으면 넣지 않는다. 흐름이 받기 전에 버릴 것을 고르게 하므로 여기 오면 자리가 있다.
                    // 그래도 한도를 넘기지 않게 한 번 더 막는다.
                    case RewardKind.Coin:
                        if (run.Owned.CoinRoom > 0)
                        {
                            run.Owned.AddCoin(card.Id);
                        }

                        break;

                    case RewardKind.Relic:
                        if (run.Owned.RelicRoom > 0)
                        {
                            run.Owned.AddRelic(card.Id);
                        }

                        break;
                }
            }

            // 이벤트 전투의 고르기가 더 남았으면 골드만 비우고 "보상 남음" 을 그대로 둔다.
            // 그 사이에 꺼져도 이어할 때 남은 고르기부터 다시 띄운다.
            if (run.RewardCardRoundsTaken + 1 < run.RewardCardRounds)
            {
                run.RewardCardRoundsTaken++;
                run.RewardOverkillGold = 0;
                run.RewardCombatGold = 0;
                run.RewardPending = true;
            }
            else
            {
                Clear(run);
            }

            if (_save != null)
            {
                _save.FlushRun();
            }
        }

        private static void Clear(RunSaveData run)
        {
            run.RewardPending = false;
            run.RewardOverkillGold = 0;
            run.RewardCombatGold = 0;
            run.RewardCardRounds = 0;
            run.RewardCardRoundsTaken = 0;
        }
    }
}
