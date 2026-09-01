using UnityEngine;

public enum TriggerTiming
{
    TurnStart,      // 자신의 턴 시작
    TurnEnd,        // 자신의 턴 종료
    OnDamaged,      // 데미지를 받은 직후
    OnDealDamage,   // 데미지를 준 직후
    BeforeAction,
}
