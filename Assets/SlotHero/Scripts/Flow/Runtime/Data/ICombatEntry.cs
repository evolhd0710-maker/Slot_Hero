using System;
using SlotHero.Map;

namespace SlotHero.Flow
{
    /// <summary>
    /// 전투로 넘기고 결과를 받아 오는 창구.
    ///
    /// 전투 코드는 아직 붙지 않았다.
    /// 흐름이 전투를 직접 부르면 나중에 전투가 올 때 흐름을 뜯어야 하므로
    /// 지금부터 이 창구로만 주고받는다.
    /// 전투가 오면 이것을 구현한 것 하나만 갈아 끼우면 된다.
    /// </summary>
    public interface ICombatEntry
    {
        /// <summary>
        /// 전투를 연다. 끝나면 `onFinished` 를 부른다.
        /// 이긴 경우와 진 경우를 모두 이쪽에서 알려 준다.
        /// 방 전투는 `CombatOptions.None` 을, 이벤트가 연 전투는 이벤트가 정한 것을 넘긴다.
        /// </summary>
        void Enter(RunContext context, MapNode node, CombatOptions options, Action<CombatResult> onFinished);

        /// <summary>전투를 닫는다. 맵으로 돌아갈 때 부른다.</summary>
        void Leave();
    }

    /// <summary>
    /// 이벤트가 연 전투에 덧붙는 조건. 이벤트 기획서 3장 도전 이벤트 공통 규칙.
    ///
    /// 부정 효과는 전투가 걸고 전투가 끝나면 푼다. 기획서가 "부정 효과는 해당 전투가 끝나면 해제된다" 고 정했다.
    /// 보상 2배는 흐름이 보상에 적용한다(`RoomRewardFlow`). 전투는 화면에 알리는 데만 쓴다.
    /// </summary>
    public struct CombatOptions
    {
        /// <summary>
        /// 이벤트가 연 전투인지. 이벤트 전투는 이긴 뒤 바로 보상을 주지 않고 이벤트 마무리 화면으로 돌아간다.
        /// 보상은 이벤트 방을 나갈 때 받는다.
        /// </summary>
        public bool FromEvent;

        /// <summary>이기면 보상이 두 배인지. 35 도전자.</summary>
        public bool DoubleReward;

        /// <summary>걸 부정 효과의 식별자(D1 ~ D6). 걸지 않으면 빈 글이다.</summary>
        public string PenaltyId;

        /// <summary>걸 부정 효과의 글.</summary>
        public string PenaltyText;

        /// <summary>부정 효과를 거는지.</summary>
        public bool HasPenalty
        {
            get { return !string.IsNullOrEmpty(PenaltyId); }
        }

        /// <summary>방 전투. 덧붙는 조건이 없다.</summary>
        public static CombatOptions None
        {
            get
            {
                CombatOptions options = new CombatOptions();
                options.PenaltyId = string.Empty;
                options.PenaltyText = string.Empty;
                return options;
            }
        }
    }

    /// <summary>전투가 끝난 결과.</summary>
    public struct CombatResult
    {
        /// <summary>이겼는지.</summary>
        public bool Won;

        /// <summary>이 전투에서 준 가장 큰 피해.</summary>
        public int BiggestHit;

        /// <summary>얻은 골드.</summary>
        public int GoldGained;

        /// <summary>
        /// 오버킬로 더 받는 골드. 보상 화면의 오버킬 줄에 적힌다.
        /// 인게임 화면 기획서 v0.2 / 10 보상 화면 의 오버킬 보상이다. 얼마가 되는지는 전투가 정한다.
        /// </summary>
        public int OverkillGold;

        /// <summary>이긴 결과를 만든다.</summary>
        public static CombatResult Victory(int biggestHit, int goldGained)
        {
            return Victory(biggestHit, goldGained, 0);
        }

        /// <summary>오버킬 골드까지 담아 이긴 결과를 만든다.</summary>
        public static CombatResult Victory(int biggestHit, int goldGained, int overkillGold)
        {
            CombatResult result = new CombatResult();
            result.Won = true;
            result.BiggestHit = biggestHit;
            result.GoldGained = goldGained;
            result.OverkillGold = overkillGold;
            return result;
        }

        /// <summary>진 결과를 만든다.</summary>
        public static CombatResult Defeat()
        {
            return new CombatResult();
        }
    }
}
