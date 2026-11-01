using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 검증용 임시 카탈로그.
    /// 문양, 코인, 유물 기획서의 코드가 아직 없어 행상을 돌려 볼 수 없으므로
    /// 진열과 가격이 제대로 나오는지 보기 위해 만든 것이다.
    /// 실제 아이템 코드가 생기면 그쪽을 <see cref="IMerchantCatalog"/>로 감싸고 이 파일은 지운다.
    ///
    /// 문양 네 개는 성소 기획서 v0.2 / 07 가격 의 예시 그대로라 가격을 검산할 수 있다.
    /// 나머지 항목의 이름과 수치는 검증용으로 내가 임의로 채운 값이다.
    /// </summary>
    public class SampleMerchantCatalog : IMerchantCatalog
    {
        private readonly List<MerchantItem> _symbols = new List<MerchantItem>
        {
            // 07 가격 의 예시. 태그 가치 합 → 가격은 각각 30, 40, 55, 65 가 나와야 한다.
            MerchantItem.Symbol("symbol_mercury", "수은", 2),
            MerchantItem.Symbol("symbol_lotus", "연꽃", 4),
            MerchantItem.Symbol("symbol_crow", "까마귀", 7),
            MerchantItem.Symbol("symbol_nightsky", "밤하늘", 9),

            // 아래는 검증용 임의 값이다.
            MerchantItem.Symbol("symbol_sword", "검", 3),
            MerchantItem.Symbol("symbol_cloud", "구름", 5),
            MerchantItem.Symbol("symbol_clock", "시계", 6),
            MerchantItem.Symbol("symbol_skull", "해골", 8),
        };

        private readonly List<MerchantItem> _coins = new List<MerchantItem>
        {
            MerchantItem.Coin("coin_wave", "파도 은화", ItemRarity.Common),
            MerchantItem.Coin("coin_ember", "잉걸 은화", ItemRarity.Common),
            MerchantItem.Coin("coin_frost", "서리 은화", ItemRarity.Uncommon),
            MerchantItem.Coin("coin_gale", "돌풍 은화", ItemRarity.Uncommon),
            MerchantItem.Coin("coin_bolt", "번개 금화", ItemRarity.Rare),
            MerchantItem.Coin("coin_tide", "해일 금화", ItemRarity.Rare),
            MerchantItem.Coin("coin_eclipse", "월식 금화", ItemRarity.Epic),
            MerchantItem.Coin("coin_comet", "혜성 금화", ItemRarity.Epic),
            MerchantItem.Coin("coin_origin", "태초 금화", ItemRarity.Legend),
        };

        private readonly List<MerchantItem> _relics = new List<MerchantItem>
        {
            MerchantItem.Relic("relic_pickaxe", "곡괭이", ItemRarity.Common),
            MerchantItem.Relic("relic_rope", "밧줄", ItemRarity.Common),
            MerchantItem.Relic("relic_book", "책", ItemRarity.Uncommon),
            MerchantItem.Relic("relic_lantern", "등불", ItemRarity.Uncommon),
            MerchantItem.Relic("relic_wing", "날개", ItemRarity.Rare),
            MerchantItem.Relic("relic_mirror", "거울", ItemRarity.Rare),
            MerchantItem.Relic("relic_crown", "왕관", ItemRarity.Epic),
            MerchantItem.Relic("relic_hourglass", "모래시계", ItemRarity.Epic),
            MerchantItem.Relic("relic_worldtree", "세계수 가지", ItemRarity.Legend),
        };

        /// <summary>지금 스테이지. 검사가 스테이지별 희귀도 분포를 볼 때 바꾼다.</summary>
        public int StageIndex = 1;

        /// <inheritdoc />
        public int Stage
        {
            get { return StageIndex; }
        }

        private readonly List<MerchantItem> _ownedSymbols = new List<MerchantItem>();
        private readonly List<string> _ownedCoinIds = new List<string>();
        private readonly List<string> _ownedRelicIds = new List<string>();

        /// <summary>지금 가지고 있는 문양. 문양 변경의 대상이 된다.</summary>
        public IReadOnlyList<MerchantItem> OwnedSymbols
        {
            get { return _ownedSymbols; }
        }

        /// <summary>지금 가지고 있는 코인 식별자.</summary>
        public IReadOnlyList<string> OwnedCoinIds
        {
            get { return _ownedCoinIds; }
        }

        /// <summary>지금 가지고 있는 유물 식별자.</summary>
        public IReadOnlyList<string> OwnedRelicIds
        {
            get { return _ownedRelicIds; }
        }

        /// <summary>시작 덱에 문양을 넣어 둔다.</summary>
        public void AddStartingSymbol(string symbolId)
        {
            for (int i = 0; i < _symbols.Count; i++)
            {
                if (_symbols[i].Id == symbolId)
                {
                    _ownedSymbols.Add(_symbols[i]);
                    return;
                }
            }
        }

        public void CollectSymbolCandidates(List<MerchantItem> into)
        {
            if (into == null)
            {
                return;
            }

            into.AddRange(_symbols);
        }

        public void CollectCoinCandidates(List<MerchantItem> into)
        {
            CollectUnowned(_coins, _ownedCoinIds, into);
        }

        public void CollectRelicCandidates(List<MerchantItem> into)
        {
            CollectUnowned(_relics, _ownedRelicIds, into);
        }

        public void CollectAllRelics(List<MerchantItem> into)
        {
            if (into == null)
            {
                return;
            }

            into.AddRange(_relics);
        }

        public void CollectOwnedSymbols(List<MerchantItem> into)
        {
            if (into == null)
            {
                return;
            }

            into.AddRange(_ownedSymbols);
        }

        /// <summary>참이면 코인과 유물을 더 가질 자리가 없는 것으로 친다. 가득 찬 상태를 검사할 때 쓴다.</summary>
        public bool Full;

        public bool HasRoomFor(MerchantItem item)
        {
            return !Full || item.Kind == SanctumItemKind.Symbol;
        }

        public void Acquire(MerchantItem item)
        {
            switch (item.Kind)
            {
                case SanctumItemKind.Symbol:
                    _ownedSymbols.Add(item);
                    break;
                case SanctumItemKind.Coin:
                    _ownedCoinIds.Add(item.Id);
                    break;
                case SanctumItemKind.Relic:
                    _ownedRelicIds.Add(item.Id);
                    break;
            }
        }

        public bool TryReplaceSymbol(string ownedSymbolId, MerchantItem newSymbol)
        {
            for (int i = 0; i < _ownedSymbols.Count; i++)
            {
                if (_ownedSymbols[i].Id == ownedSymbolId)
                {
                    _ownedSymbols[i] = newSymbol;
                    return true;
                }
            }

            return false;
        }

        private static void CollectUnowned(List<MerchantItem> source, List<string> ownedIds, List<MerchantItem> into)
        {
            if (into == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (!ownedIds.Contains(source[i].Id))
                {
                    into.Add(source[i]);
                }
            }
        }
    }

    /// <summary>
    /// 검증용 임시 체력.
    /// 전투 쪽 코드가 생기면 그쪽을 <see cref="IPlayerVitals"/>로 감싸고 이 클래스는 지운다.
    /// </summary>
    public class SamplePlayerVitals : IPlayerVitals
    {
        private int _health;
        private int _maxHealth;

        public SamplePlayerVitals(int health, int maxHealth)
        {
            _maxHealth = maxHealth;
            _health = health;
        }

        public int Health
        {
            get { return _health; }
        }

        public int MaxHealth
        {
            get { return _maxHealth; }
        }

        public void Heal(int amount)
        {
            _health += amount;
            if (_health > _maxHealth)
            {
                _health = _maxHealth;
            }
        }
    }
}
