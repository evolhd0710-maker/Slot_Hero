using System;
using UnityEngine;

namespace SlotHero.Flow
{
    /// <summary>
    /// 코인과 유물을 얻는 경로. 물건마다 어느 경로로 나오지 않을지 고른다(`ItemDefinition.ExcludedFrom`).
    ///
    /// 2026년 10월 9일 원재가 "특정 이벤트에서만 등장, 행상에서만 등장, 행상에서 미등장 등을 구분할 장치를 넣어 달라.
    /// 다른 경우도 추가될 수 있다" 고 했다. 경로가 늘면 여기에 값을 더하고, 그 경로가 후보를 모으는 자리에서 거른다.
    /// 값은 비트라 둘 이상을 함께 고를 수 있다. 시작 아이템은 여기에 들지 않는다.
    /// </summary>
    [Flags]
    public enum ItemSource
    {
        /// <summary>어느 경로도 고르지 않았다.</summary>
        [InspectorName("없음")]
        None = 0,

        /// <summary>방을 깨고 받는 보상 카드. 이벤트가 연 전투의 보상 카드도 여기다.</summary>
        [InspectorName("보상")]
        Reward = 1,

        /// <summary>성소의 행상 진열. 유물 새로고침도 여기다.</summary>
        [InspectorName("행상")]
        Merchant = 2,

        /// <summary>이벤트가 무작위로 주거나 바꿔 주는 것. 받을 이벤트를 좁히려면 `ItemDefinition.OnlyInEvents` 를 쓴다.</summary>
        [InspectorName("이벤트")]
        Event = 4,
    }
}
