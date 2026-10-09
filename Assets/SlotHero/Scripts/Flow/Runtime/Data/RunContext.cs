using System;
using System.Collections.Generic;
using SlotHero.Combat;
using SlotHero.CurrentBuild;
using SlotHero.Sanctum;
using SlotHero.Save;

namespace SlotHero.Flow
{
    /// <summary>
    /// 진행 중인 런 하나를 화면들이 쓰는 꼴로 바꿔 준다.
    ///
    /// 저장에 실리는 값은 `RunSaveData` 하나뿐이다.
    /// 그런데 성소는 `IPlayerVitals` 와 `RunGoldState` 를 받고
    /// 현재 빌드 화면은 `CurrentBuildSnapshot` 을 받는다.
    /// 화면마다 자료를 따로 들고 있으면 저장된 값과 어긋나므로
    /// 여기서 한쪽으로만 흐르게 묶는다. 진짜 값은 늘 `RunSaveData` 쪽이다.
    /// </summary>
    public class RunContext : IPlayerVitals
    {
        private readonly RunSaveData _run;
        private readonly RunCatalog _catalog;
        private readonly RunGoldState _gold = new RunGoldState();

        /// <summary>런과 물건 목록을 물려 만든다.</summary>
        public RunContext(RunSaveData run, RunCatalog catalog)
        {
            _run = run;
            _catalog = catalog;

            // 골드는 성소가 직접 건드리므로 바뀔 때마다 런 데이터로 되돌려 준다.
            _gold.Gold = run != null ? run.Status.Gold : 0;
            _gold.Changed = HandleGoldChanged;
        }

        /// <summary>저장에 실리는 런 데이터.</summary>
        public RunSaveData Run
        {
            get { return _run; }
        }

        /// <summary>성소가 쓰는 골드 상태. 바뀌면 런 데이터도 함께 바뀐다.</summary>
        public RunGoldState Gold
        {
            get { return _gold; }
        }

        /// <summary>런에서 쓰는 물건 목록.</summary>
        public RunCatalog Catalog
        {
            get { return _catalog; }
        }

        /// <summary>골드나 체력이 바뀌었다. 상단 표시줄이 듣는다.</summary>
        public event Action Changed;

        // ---- IPlayerVitals ----

        /// <inheritdoc />
        public int Health
        {
            get { return _run != null ? _run.Status.Health : 0; }
        }

        /// <inheritdoc />
        public int MaxHealth
        {
            get { return _run != null ? _run.Status.MaxHealth : 0; }
        }

        /// <inheritdoc />
        public void Heal(int amount)
        {
            if (_run == null)
            {
                return;
            }

            _run.Status.Heal(amount);
            Notify();
        }

        /// <summary>체력을 깎는다.</summary>
        public void TakeDamage(int amount)
        {
            if (_run == null)
            {
                return;
            }

            _run.Status.TakeDamage(amount);
            Notify();
        }

        /// <summary>
        /// 최대 체력을 영구히 올리거나 내린다. 실제로 바뀐 만큼을 돌려준다.
        ///
        /// **올릴 때는 지금 체력도 같은 만큼 오른다.** 원재가 2026년 10월 4일에 정했다.
        /// 최대만 오르면 늘어난 칸이 빈 채로 남아 얻은 느낌이 나지 않는다.
        /// 내릴 때는 지금 체력이 최대를 넘지 않게 함께 내린다. 최대 체력은 1 아래로 내려가지 않는다.
        /// </summary>
        public int ChangeMaxHealth(int amount)
        {
            if (_run == null || amount == 0)
            {
                return 0;
            }

            RunStatusData status = _run.Status;
            int before = status.MaxHealth;
            int wanted = before + amount;
            status.MaxHealth = wanted < 1 ? 1 : wanted;

            int changed = status.MaxHealth - before;

            if (changed > 0)
            {
                status.Health += changed;
            }

            if (status.Health > status.MaxHealth)
            {
                status.Health = status.MaxHealth;
            }

            Notify();
            return changed;
        }

        /// <summary>골드를 얻는다. 통계에도 함께 쌓인다.</summary>
        public void GainGold(int amount)
        {
            if (_run == null)
            {
                return;
            }

            _run.Statistics.GainGold(_run.Status, amount);
            _gold.Gold = _run.Status.Gold;
            Notify();
        }

        // ---- 소지 한도 ----

        /// <summary>
        /// 코인을 최대 몇 개까지 가질 수 있는지 바꾼다. 실제로 바뀐 만큼을 돌려준다. 0 아래로는 내려가지 않는다(2026년 10월 9일 원재).
        /// 어떤 조건에서 바뀌는지는 아직 정해지지 않았다. 유물이나 이벤트 기획서가 오면 거기서 부른다(2026년 10월 9일 원재).
        /// 줄어 넘치면 흐름이 넘친 만큼 바로 버리게 한다(`GameFlowController.ResolveOverflow`).
        /// </summary>
        public int ChangeCoinCapacity(int amount)
        {
            if (_run == null)
            {
                return 0;
            }

            int before = _run.Owned.CoinCapacity;
            _run.Owned.CoinCapacity = ClampCapacity(before + amount);
            Notify();
            return _run.Owned.CoinCapacity - before;
        }

        /// <summary>유물을 최대 몇 개까지 가질 수 있는지 바꾼다. 규칙은 `ChangeCoinCapacity` 와 같다.</summary>
        public int ChangeRelicCapacity(int amount)
        {
            if (_run == null)
            {
                return 0;
            }

            int before = _run.Owned.RelicCapacity;
            _run.Owned.RelicCapacity = ClampCapacity(before + amount);
            Notify();
            return _run.Owned.RelicCapacity - before;
        }

        /// <summary>
        /// 가진 코인이나 유물 하나를 버린다. 소지 한도가 차 새것을 받을 때와 한도가 줄어 넘칠 때 쓴다.
        /// 버린 것은 그냥 사라진다. 골드로 바꿔 주지 않는다.
        /// </summary>
        public bool Discard(ItemPickKind kind, string id)
        {
            if (_run == null || string.IsNullOrEmpty(id))
            {
                return false;
            }

            bool removed = kind == ItemPickKind.Coin
                ? _run.Owned.RemoveCoin(id)
                : kind == ItemPickKind.Relic && _run.Owned.RemoveRelic(id);

            if (removed)
            {
                Notify();
            }

            return removed;
        }

        private static int ClampCapacity(int value)
        {
            return value < 0 ? 0 : value;
        }

        // ---- 현재 빌드 화면 ----

        /// <summary>
        /// 지금 빌드를 현재 빌드 화면과 런 종료 결과 화면이 쓰는 꼴로 만든다.
        /// 문양의 태그 둘은 물건 목록에서 가져온다.
        /// </summary>
        public CurrentBuildSnapshot BuildSnapshot()
        {
            CurrentBuildSnapshot snapshot = new CurrentBuildSnapshot();

            if (_run == null)
            {
                return snapshot;
            }

            snapshot.CoinCapacity = _run.Owned.CoinCapacity;
            snapshot.RelicCapacity = _run.Owned.RelicCapacity;

            for (int i = 0; i < _run.Owned.Relics.Count; i++)
            {
                string id = _run.Owned.Relics[i].RelicId;
                snapshot.Relics.Add(BuildEntry.Item(id, GetName(id)));
            }

            for (int i = 0; i < _run.Owned.CoinIds.Count; i++)
            {
                string id = _run.Owned.CoinIds[i];
                snapshot.Coins.Add(BuildEntry.Item(id, GetName(id)));
            }

            for (int i = 0; i < _run.Owned.Symbols.Count; i++)
            {
                OwnedSymbol owned = _run.Owned.Symbols[i];

                SymbolTagType first = SymbolTagType.Mercury;
                SymbolTagType second = SymbolTagType.Mercury;

                if (_catalog != null)
                {
                    _catalog.GetSymbolTags(owned.SymbolId, out first, out second);
                }

                snapshot.Symbols.Add(BuildEntry.Symbol(
                    owned.SymbolId, GetName(owned.SymbolId), owned.Count, first, second));
            }

            return snapshot;
        }

        /// <summary>런 시작에 주는 물건들을 넣는다.</summary>
        public void GiveStartingItems(
            IList<string> symbolIds, IList<string> coinIds, IList<string> relicIds, int startingGold)
        {
            if (_run == null)
            {
                return;
            }

            if (symbolIds != null)
            {
                for (int i = 0; i < symbolIds.Count; i++)
                {
                    _run.Owned.AddSymbol(symbolIds[i], 1);
                }
            }

            if (coinIds != null)
            {
                for (int i = 0; i < coinIds.Count; i++)
                {
                    _run.Owned.AddCoin(coinIds[i]);
                }
            }

            if (relicIds != null)
            {
                for (int i = 0; i < relicIds.Count; i++)
                {
                    _run.Owned.AddRelic(relicIds[i]);
                }
            }

            _run.Statistics.GainGold(_run.Status, startingGold);
            _gold.Gold = _run.Status.Gold;
            Notify();
        }

        private string GetName(string id)
        {
            return _catalog != null ? _catalog.GetDisplayName(id) : id;
        }

        private void HandleGoldChanged(int before, int after)
        {
            if (_run == null)
            {
                return;
            }

            // 성소에서 쓴 만큼은 통계의 소모 골드로도 쌓는다.
            if (after < before)
            {
                _run.Statistics.GoldSpent += before - after;
            }
            else if (after > before)
            {
                _run.Statistics.GoldGained += after - before;
            }

            _run.Status.Gold = after;
            Notify();
        }

        private void Notify()
        {
            if (Changed != null)
            {
                Changed();
            }
        }
    }
}
