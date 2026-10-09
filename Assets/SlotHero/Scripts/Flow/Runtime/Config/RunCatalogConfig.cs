using System;
using System.Collections.Generic;
using UnityEngine;
using SlotHero.Combat;
using SlotHero.Sanctum;

namespace SlotHero.Flow
{
    /// <summary>
    /// 런에 나오는 문양과 코인과 유물의 목록.
    ///
    /// 문양 서른여섯은 저장소의 `SymbolTagAutoAssigner` 에 적힌 것을 그대로 옮겼다.
    /// 아홉 행성에서 둘을 고르는 모든 짝이 한 번씩 나온다. 9 × 8 ÷ 2 = 36 이다.
    /// 그래서 문양은 늘 서로 다른 태그를 둘 가진다.
    ///
    /// 유물과 코인은 그림이 있는 것만 담았다.
    /// 저장소의 `RelicData` 와 `BetCoinData` 에셋이 오면 그쪽 이름과 등급으로 맞춘다.
    /// </summary>
    [CreateAssetMenu(fileName = "RunCatalogConfig", menuName = "Slot Hero/흐름/물건 목록")]
    public class RunCatalogConfig : ScriptableObject
    {
        [Header("문양")]
        public List<SymbolDefinition> Symbols = new List<SymbolDefinition>();

        [Header("코인")]
        public List<ItemDefinition> Coins = new List<ItemDefinition>();

        [Header("유물")]
        public List<ItemDefinition> Relics = new List<ItemDefinition>();

        [Header("태그 가치")]
        [Tooltip("태그 하나의 값. 문양 값은 가진 태그 둘의 값을 더해 매긴다. " +
            "저장소 SymbolTagData 의 value 에 해당한다. " +
            "전투 시스템 기획서 10c / 04 문양·태그 표 대로 수성·금성 1, 지구·화성 2, 목성·토성 3, 천왕성·해왕성 4, 명왕성 5 다(SpecTagValues).")]
        public List<TagValue> TagValues = new List<TagValue>();

        [Header("코인 매각")]
        [Tooltip("23 코인 매각 에서 등급마다 받는 골드. 차례가 등급이다(일반, 고급, 희귀, 특급, 전설). " +
            "이벤트 기획서는 \"희귀도에 따른 골드\" 라고만 적어, 성소 기획서 07 가격 의 코인 기준가 15 / 25 / 35 / 45 / 60 의 절반으로 두었다. " +
            "예전에는 이벤트 코드에 숫자로 박혀 있었다.")]
        public List<int> CoinSellPrices = new List<int> { 8, 13, 18, 23, 30 };

        [Header("런 시작에 주는 것")]
        [Tooltip("런을 시작할 때 넣어 주는 문양. 같은 것을 여러 번 적으면 그만큼 들어간다.")]
        public List<string> StartingSymbolIds = new List<string>();

        [Tooltip("런을 시작할 때 넣어 주는 코인.")]
        public List<string> StartingCoinIds = new List<string>();

        [Tooltip("런을 시작할 때 넣어 주는 유물.")]
        public List<string> StartingRelicIds = new List<string>();

        [Tooltip("런을 시작할 때 주는 골드. 성소 기획서 06 재화 의 \"시작 수치 0\" 이다. " +
            "예전에는 100 이었다. 2026년 10월 8일 외부 검토가 기획서와 다르다고 짚어 0 으로 맞췄다.")]
        public int StartingGold = 0;

        [Header("소지 한도")]
        [Tooltip("런을 시작할 때 코인을 최대 몇 개까지 가질 수 있는지. 전투 UI 기획서 stt 05 의 \"코인은 최대 5개까지 소유 가능\", " +
            "인게임 화면 기획서 13 의 \"코인 최대 5개\". 런 중에 조건에 따라 바뀔 수 있어 런 데이터에 따로 저장한다.")]
        [Min(1)]
        public int StartingCoinCapacity = 5;

        [Tooltip("런을 시작할 때 유물을 최대 몇 개까지 가질 수 있는지. 전투 UI 기획서 stt 의 \"유물 목록(최대 6개)\", " +
            "인게임 화면 기획서 13 의 \"슬롯은 6개\". 런 중에 조건에 따라 바뀔 수 있어 런 데이터에 따로 저장한다.")]
        [Min(1)]
        public int StartingRelicCapacity = 6;

        [Header("덱")]
        [Tooltip("문양 덱이 가져야 하는 가장 적은 장수. 문양을 잃는 선택지는 이 아래로 줄이지 못한다. " +
            "전투 시스템 기획서 04 가 \"문양 풀이 슬롯 릴 갯수보다 적은 경우는 존재하지 않는다\" 고 적었다(릴 5 ~ 7개). " +
            "2026년 10월 9일 원재가 6 으로 정했다.")]
        [Min(1)]
        public int MinimumDeckSize = 6;

        /// <summary>
        /// 전투 시스템 기획서 10c / 04 문양·태그 표 의 태그 가치.
        /// 수성 1, 금성 1, 지구 2, 화성 2, 목성 3, 토성 3, 천왕성 4, 해왕성 4, 명왕성 5 다.
        /// 성소 기획서 07 가격 의 예시(수은 30, 연꽃 40, 까마귀 55, 밤하늘 65)가 이 값으로 맞아떨어진다.
        /// 예전에는 태양계 차례대로 1 ~ 9 를 넣어 문양 값이 기획보다 비쌌다(밤하늘 100). 2026년 10월 8일에 고쳤다.
        /// </summary>
        public static List<TagValue> SpecTagValues()
        {
            List<TagValue> values = new List<TagValue>();
            values.Add(new TagValue(SymbolTagType.Mercury, 1));
            values.Add(new TagValue(SymbolTagType.Venus, 1));
            values.Add(new TagValue(SymbolTagType.Earth, 2));
            values.Add(new TagValue(SymbolTagType.Mars, 2));
            values.Add(new TagValue(SymbolTagType.Jupiter, 3));
            values.Add(new TagValue(SymbolTagType.Saturn, 3));
            values.Add(new TagValue(SymbolTagType.Uranus, 4));
            values.Add(new TagValue(SymbolTagType.Neptune, 4));
            values.Add(new TagValue(SymbolTagType.Pluto, 5));
            return values;
        }

        /// <summary>23 코인 매각 에서 그 등급의 코인을 팔면 받는 골드. 목록을 넘으면 마지막 값이다.</summary>
        public int GetCoinSellPrice(ItemRarity rarity)
        {
            if (CoinSellPrices == null || CoinSellPrices.Count == 0)
            {
                return 0;
            }

            int index = (int)rarity;
            if (index < 0)
            {
                index = 0;
            }

            if (index >= CoinSellPrices.Count)
            {
                index = CoinSellPrices.Count - 1;
            }

            return CoinSellPrices[index];
        }

        // ---- 실행 중 조회 색인 ----
        //
        // 에디터가 고치는 것은 위의 목록이고, 실행 중 이름·그림·태그·등급을 찾을 때는 이 색인을 쓴다.
        // 예전에는 찾을 때마다 목록을 처음부터 훑었다. 콘텐츠가 늘면 느려진다. 2026년 10월 9일 외부 검토가 권했다.
        // 목록 길이가 바뀌거나 에디터에서 값을 고치면(OnValidate) 다시 만든다. 찾은 자리의 식별자가 다르면 그 자리에서 다시 만든다.
        // 같은 식별자가 여럿이면 예전과 같이 문양, 코인, 유물 차례로 먼저 나온 것을 쓴다.

        /// 목록의 종류. 색인 값에 담는다.
        private const int SymbolKind = 0;
        private const int CoinKind = 1;
        private const int RelicKind = 2;

        /// 식별자 → 종류 × 목록 자리. 값은 종류 * KindStride + 자리다.
        [NonSerialized] private Dictionary<string, int> _index;
        [NonSerialized] private int _indexedSymbols = -1;
        [NonSerialized] private int _indexedCoins = -1;
        [NonSerialized] private int _indexedRelics = -1;
        private const int KindStride = 1 << 24;

        /// <summary>색인을 버린다. 코드에서 목록 안의 식별자를 바꿨을 때 부른다. 다음 조회 때 다시 만든다.</summary>
        public void InvalidateIndex()
        {
            _index = null;
        }

        private void OnValidate()
        {
            InvalidateIndex();
        }

        private void RebuildIndex()
        {
            _index = new Dictionary<string, int>();
            AddToIndex(SymbolKind, Symbols.Count, i => Symbols[i].Id);
            AddToIndex(CoinKind, Coins.Count, i => Coins[i].Id);
            AddToIndex(RelicKind, Relics.Count, i => Relics[i].Id);
            _indexedSymbols = Symbols.Count;
            _indexedCoins = Coins.Count;
            _indexedRelics = Relics.Count;
        }

        private void AddToIndex(int kind, int count, Func<int, string> idAt)
        {
            for (int i = 0; i < count; i++)
            {
                string id = idAt(i);
                if (id != null && !_index.ContainsKey(id))
                {
                    _index.Add(id, kind * KindStride + i);
                }
            }
        }

        /// <summary>식별자의 종류와 목록 자리. 없으면 false 다.</summary>
        private bool TryFind(string id, out int kind, out int at)
        {
            kind = -1;
            at = -1;

            if (id == null)
            {
                return false;
            }

            if (_index == null || _indexedSymbols != Symbols.Count || _indexedCoins != Coins.Count || _indexedRelics != Relics.Count)
            {
                RebuildIndex();
            }

            int value;
            if (!_index.TryGetValue(id, out value))
            {
                return false;
            }

            kind = value / KindStride;
            at = value % KindStride;

            // 목록 안의 식별자가 바뀌었으면 다시 만들고 한 번 더 찾는다.
            if (IdAt(kind, at) != id)
            {
                RebuildIndex();
                if (!_index.TryGetValue(id, out value))
                {
                    kind = -1;
                    at = -1;
                    return false;
                }

                kind = value / KindStride;
                at = value % KindStride;
            }

            return true;
        }

        private string IdAt(int kind, int at)
        {
            switch (kind)
            {
                case SymbolKind:
                    return at < Symbols.Count ? Symbols[at].Id : null;
                case CoinKind:
                    return at < Coins.Count ? Coins[at].Id : null;
                case RelicKind:
                    return at < Relics.Count ? Relics[at].Id : null;
                default:
                    return null;
            }
        }

        /// <summary>그 식별자의 이름. 없으면 식별자를 그대로 돌려준다.</summary>
        public string GetDisplayName(string id)
        {
            int kind;
            int at;
            if (!TryFind(id, out kind, out at))
            {
                return id;
            }

            switch (kind)
            {
                case SymbolKind:
                    return Symbols[at].DisplayName;
                case CoinKind:
                    return Coins[at].DisplayName;
                default:
                    return Relics[at].DisplayName;
            }
        }

        /// <summary>그 식별자의 그림. 없으면 null 이다.</summary>
        public Sprite GetIcon(string id)
        {
            int kind;
            int at;
            if (!TryFind(id, out kind, out at))
            {
                return null;
            }

            switch (kind)
            {
                case SymbolKind:
                    return Symbols[at].Icon;
                case CoinKind:
                    return Coins[at].Icon;
                default:
                    return Relics[at].Icon;
            }
        }

        /// <summary>그 문양의 태그 둘. 없으면 false 다.</summary>
        public bool TryGetSymbolTags(
            string id, out SymbolTagType firstTag, out SymbolTagType secondTag)
        {
            int kind;
            int at;
            if (TryFind(id, out kind, out at) && kind == SymbolKind)
            {
                firstTag = Symbols[at].FirstTag;
                secondTag = Symbols[at].SecondTag;
                return true;
            }

            firstTag = SymbolTagType.Mercury;
            secondTag = SymbolTagType.Mercury;
            return false;
        }

        /// <summary>
        /// 그 코인이나 유물의 정의. 문양이거나 없으면 false 다.
        /// 등급을 찾을 때 쓴다. 이벤트가 후보의 등급을 볼 때마다 목록을 훑지 않게 하려는 것이다.
        /// </summary>
        public bool TryGetItem(string id, out ItemDefinition item)
        {
            int kind;
            int at;
            if (TryFind(id, out kind, out at))
            {
                if (kind == CoinKind)
                {
                    item = Coins[at];
                    return true;
                }

                if (kind == RelicKind)
                {
                    item = Relics[at];
                    return true;
                }
            }

            item = new ItemDefinition();
            return false;
        }

        /// <summary>태그 하나의 값. 적어 두지 않은 태그는 1 로 본다.</summary>
        public int GetTagValue(SymbolTagType tag)
        {
            for (int i = 0; i < TagValues.Count; i++)
            {
                if (TagValues[i].Tag == tag)
                {
                    return TagValues[i].Value;
                }
            }

            return 1;
        }

        /// <summary>
        /// 그 문양의 태그 가치 합. 행상이 값을 매기는 데 쓴다.
        /// 저장소가 태그마다 값을 두고 문양이 그 둘을 물고 있는 구조라 여기서도 더해서 낸다.
        /// </summary>
        public int GetTagValueSum(SymbolDefinition symbol)
        {
            return GetTagValue(symbol.FirstTag) + GetTagValue(symbol.SecondTag);
        }

        /// <summary>문양 하나를 행상이 쓰는 꼴로 바꾼다.</summary>
        public MerchantItem ToMerchantItem(SymbolDefinition symbol)
        {
            return MerchantItem.Symbol(symbol.Id, symbol.DisplayName, GetTagValueSum(symbol));
        }

        /// <summary>
        /// 모든 문양이 서로 다른 태그를 둘 가지는지.
        /// 저장소의 문양 규칙을 지키는지 보는 것이다.
        /// </summary>
        public bool AllSymbolsHaveTwoDifferentTags()
        {
            for (int i = 0; i < Symbols.Count; i++)
            {
                if (Symbols[i].FirstTag == Symbols[i].SecondTag)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 태그 짝이 겹치지 않는지.
        /// 저장소는 아홉 행성에서 둘을 고르는 모든 짝을 한 번씩만 쓴다.
        /// 차례를 뒤집은 것도 같은 짝으로 본다.
        /// </summary>
        public bool AllTagPairsAreUnique()
        {
            List<int> seen = new List<int>();

            for (int i = 0; i < Symbols.Count; i++)
            {
                int low = (int)Symbols[i].FirstTag;
                int high = (int)Symbols[i].SecondTag;

                if (low > high)
                {
                    int swap = low;
                    low = high;
                    high = swap;
                }

                int key = low * 100 + high;

                if (seen.Contains(key))
                {
                    return false;
                }

                seen.Add(key);
            }

            return true;
        }

        /// <summary>식별자가 겹치지 않는지.</summary>
        public bool AllIdsAreUnique()
        {
            List<string> seen = new List<string>();

            for (int i = 0; i < Symbols.Count; i++)
            {
                if (seen.Contains(Symbols[i].Id))
                {
                    return false;
                }

                seen.Add(Symbols[i].Id);
            }

            for (int i = 0; i < Coins.Count; i++)
            {
                if (seen.Contains(Coins[i].Id))
                {
                    return false;
                }

                seen.Add(Coins[i].Id);
            }

            for (int i = 0; i < Relics.Count; i++)
            {
                if (seen.Contains(Relics[i].Id))
                {
                    return false;
                }

                seen.Add(Relics[i].Id);
            }

            return true;
        }

        /// <summary>런 시작에 주는 것이 모두 목록에 있는지.</summary>
        public bool StartingItemsExist()
        {
            for (int i = 0; i < StartingSymbolIds.Count; i++)
            {
                SymbolTagType first;
                SymbolTagType second;

                if (!TryGetSymbolTags(StartingSymbolIds[i], out first, out second))
                {
                    return false;
                }
            }

            for (int i = 0; i < StartingCoinIds.Count; i++)
            {
                if (!Has(Coins, StartingCoinIds[i]))
                {
                    return false;
                }
            }

            for (int i = 0; i < StartingRelicIds.Count; i++)
            {
                if (!Has(Relics, StartingRelicIds[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Has(List<ItemDefinition> list, string id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>문양 하나의 정의.</summary>
    [Serializable]
    public struct SymbolDefinition
    {
        [Tooltip("저장소의 그림 파일 이름과 같게 둔다.")]
        public string Id;

        public string DisplayName;

        [Tooltip("첫 태그와 둘째 태그는 서로 달라야 한다.")]
        public SymbolTagType FirstTag;

        public SymbolTagType SecondTag;

        public Sprite Icon;

        [Tooltip("보상과 행상에 나오기 시작하는 스테이지. 0 이하면 첫 스테이지부터. 기획서에 수치가 없어 비워 둔다.")]
        public int MinStage;

        [Tooltip("보상과 행상에 마지막으로 나오는 스테이지. 0 이하면 끝까지.")]
        public int MaxStage;

        /// <summary>그 스테이지의 보상과 행상에 나올 수 있는지. 0 이하는 따지지 않는다(`StageRange`).</summary>
        public bool AppearsIn(int stage)
        {
            return StageRange.Covers(MinStage, MaxStage, stage);
        }
    }

    /// <summary>코인이나 유물 하나의 정의.</summary>
    [Serializable]
    public struct ItemDefinition
    {
        public string Id;
        public string DisplayName;
        public ItemRarity Rarity;
        public Sprite Icon;

        [Tooltip("보상과 행상에 나오기 시작하는 스테이지. 0 이하면 첫 스테이지부터. 기획서에 수치가 없어 비워 둔다.")]
        public int MinStage;

        [Tooltip("보상과 행상에 마지막으로 나오는 스테이지. 0 이하면 끝까지.")]
        public int MaxStage;

        [Tooltip("이 경로로는 나오지 않는다. 없음이면 모든 경로에서 나온다.\n" +
            "행상에서 미등장: 행상\n행상에서만 등장: 보상 + 이벤트\n특정 이벤트에서만 등장: 보상 + 행상, 아래 이벤트 목록에 그 이벤트")]
        public ItemSource ExcludedFrom;

        [Tooltip("비어 있지 않으면 이벤트로는 여기 적은 이벤트에서만 나온다. 이벤트 식별자를 적는다. 비우면 모든 이벤트다.")]
        public List<string> OnlyInEvents;

        /// <summary>그 스테이지의 보상과 행상에 나올 수 있는지. 0 이하는 따지지 않는다(`StageRange`).</summary>
        public bool AppearsIn(int stage)
        {
            return StageRange.Covers(MinStage, MaxStage, stage);
        }

        /// <summary>
        /// 그 경로로 나올 수 있는지. 이벤트면 eventId 가 그 이벤트다.
        /// 2026년 10월 9일 원재가 획득 경로를 구분할 장치를 넣으라고 했다(`ItemSource`).
        /// </summary>
        public bool CanComeFrom(ItemSource source, string eventId)
        {
            if ((ExcludedFrom & source) != 0)
            {
                return false;
            }

            if (source == ItemSource.Event && OnlyInEvents != null && OnlyInEvents.Count > 0)
            {
                return !string.IsNullOrEmpty(eventId) && OnlyInEvents.Contains(eventId);
            }

            return true;
        }
    }

    /// <summary>태그 하나의 값.</summary>
    [Serializable]
    public struct TagValue
    {
        public SymbolTagType Tag;
        public int Value;

        public TagValue(SymbolTagType tag, int value)
        {
            Tag = tag;
            Value = value;
        }
    }
}
