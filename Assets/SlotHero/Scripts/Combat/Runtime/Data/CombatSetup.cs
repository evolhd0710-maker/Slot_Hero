using System.Collections.Generic;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투 하나를 여는 데 필요한 것. 흐름이 런 데이터에서 만들어 넘긴다(`CombatEntry`).
    ///
    /// 전투는 런 데이터를 직접 만지지 않는다.
    /// 체력이 깎이면 `CombatScreenController.PlayerHealthLost` 로 알리고 흐름이 런에 적는다.
    /// </summary>
    public class CombatSetup
    {
        /// <summary>문양 덱. 같은 문양을 여러 장 가지면 그만큼 들어 있다.</summary>
        public List<ReelSymbol> Deck = new List<ReelSymbol>();

        /// <summary>
        /// 게임에 있는 모든 문양. 망치와 끌이 "그 태그의 랜덤 문양" 을 고를 때 쓴다(시트의 `CardPool`).
        /// 비어 있으면 덱에서 고른다.
        /// </summary>
        public List<ReelSymbol> SymbolPool = new List<ReelSymbol>();

        /// <summary>가진 코인. 같은 코인을 여러 개 가지면 그만큼 들어 있다.</summary>
        public List<CombatCoin> Coins = new List<CombatCoin>();

        /// <summary>가진 유물. 왼쪽 유물 목록에 차례대로 놓이고 효과가 전투에 걸린다.</summary>
        public List<CombatRelic> Relics = new List<CombatRelic>();

        /// <summary>유물 칸 수. 전투 UI 초안의 "유물 목록(최대 6개)" 다.</summary>
        public int RelicCapacity = 6;

        public CombatEnemy Enemy;

        public int PlayerHealth;
        public int PlayerMaxHealth;

        /// <summary>
        /// 이 전투의 난수 시드. 같은 시드와 같은 선택이면 어느 기기에서든 같은 릴이 나온다.
        /// 흐름이 런 시드와 스테이지와 방 번호를 섞어 만든다.
        /// </summary>
        public int Seed;
    }
}
