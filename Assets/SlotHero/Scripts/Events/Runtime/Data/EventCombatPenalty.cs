using System;
using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 도전 이벤트가 전투에 거는 부정 효과 하나.
    /// 이벤트 기획서 3장 도전 이벤트 공통 규칙 의 부정 효과 목록(D1 ~ D6)이다.
    ///
    /// 여기에는 식별자와 글만 둔다. 얼마나 깎고 늘리는지는 전투가 정한다.
    /// 기획서가 "부정 효과는 해당 전투가 끝나면 해제된다" 고 정해 런에 남기지 않는다.
    /// </summary>
    [Serializable]
    public struct EventCombatPenalty
    {
        /// <summary>전투가 이 효과를 알아보는 이름. 기획서 번호 D1 ~ D6 을 그대로 쓴다.</summary>
        public string Id;

        /// <summary>선택지와 전투 화면에 적는 글.</summary>
        public string Text;

        public EventCombatPenalty(string id, string text)
        {
            Id = id;
            Text = text;
        }

        /// <summary>3장 부정 효과 목록 여섯 가지. 기획서가 "예시" 로 적은 것을 그대로 옮겼다.</summary>
        public static List<EventCombatPenalty> SpecDefaults()
        {
            List<EventCombatPenalty> list = new List<EventCombatPenalty>();
            list.Add(new EventCombatPenalty("D1", "주는 피해가 감소한다"));
            list.Add(new EventCombatPenalty("D2", "적의 체력이 증가한다"));
            list.Add(new EventCombatPenalty("D3", "코인의 비용이 증가한다"));
            list.Add(new EventCombatPenalty("D4", "받는 피해가 증가한다"));
            list.Add(new EventCombatPenalty("D5", "무작위 유물이 이번 전투 동안 비활성화된다"));
            list.Add(new EventCombatPenalty("D6", "무작위 코인이 이번 전투 동안 비활성화된다"));
            return list;
        }
    }
}
