using System;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투에서 쓰는 코인 하나. 슬롯머신 작동 방식 1.1.a / 코인의 변경점 을 따른다.
    ///
    ///   - 소모값만큼 체력을 내고 쓴다. 쓰는 순간 효력이 난다
    ///   - 가중치는 고른 태그의 중복 배수에 더해진다
    ///
    /// 이름은 물건 목록에서, 소모값과 가중치는 전투 규칙(`CombatRuleConfig`)에서 온다.
    /// </summary>
    [Serializable]
    public struct CombatCoin
    {
        public string Id;
        public string DisplayName;

        /// <summary>쓸 때 내는 체력.</summary>
        public int HealthCost;

        /// <summary>중복 배수에 더하는 값.</summary>
        public int Weight;

        /// <summary>코인 정보 팝업에 적는 능력 설명.</summary>
        public string Description;

        public CombatCoin(string id, string displayName, int healthCost, int weight, string description)
        {
            Id = id;
            DisplayName = displayName;
            HealthCost = healthCost;
            Weight = weight;
            Description = description;
        }
    }
}
