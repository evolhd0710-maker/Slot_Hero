using System;

namespace SlotHero.Combat
{
    /// <summary>
    /// 릴 한 칸에 놓이는 문양 하나.
    ///
    /// 전투는 물건 목록을 모른다. 흐름이 덱을 이 꼴로 바꿔 넘긴다(`CombatEntry`).
    /// 가치는 태그 둘의 값을 더한 것이다. 슬롯머신 작동 방식 1.1.a 예시의 섬 7(해왕성 4 + 토성 3)이 그렇다.
    /// </summary>
    [Serializable]
    public struct ReelSymbol
    {
        /// <summary>물건 목록의 식별자. 그림 파일 이름과 같다.</summary>
        public string Id;

        public string DisplayName;
        public SymbolTagType FirstTag;
        public SymbolTagType SecondTag;

        /// <summary>문양의 가치. 공격력 계산의 왼쪽 값이 된다.</summary>
        public int Value;

        public ReelSymbol(string id, string displayName, SymbolTagType firstTag, SymbolTagType secondTag, int value)
        {
            Id = id;
            DisplayName = displayName;
            FirstTag = firstTag;
            SecondTag = secondTag;
            Value = value;
        }

        /// <summary>그 태그가 붙어 있는지.</summary>
        public bool Has(SymbolTagType tag)
        {
            return FirstTag == tag || SecondTag == tag;
        }
    }
}
