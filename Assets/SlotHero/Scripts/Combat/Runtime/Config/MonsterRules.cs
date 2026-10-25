using System;
using UnityEngine;

namespace SlotHero.Combat
{
    /// <summary>
    /// 검투사의 패턴 수치. `slot hero 테스트 몬스터.pptx` 01 검투사 를 옮겼다.
    ///   ① 타격 [공격] 50%, 첫 턴 고정 · 데미지 6
    ///   ② 방어 [방어] 30%, 첫 턴 미발동 · 방어도 8
    ///   ③ 함성 [버프] 20%, 첫 턴 미발동 · 기세 50 (1 당 데미지 1% 증가, 중첩)
    /// </summary>
    [Serializable]
    public class GladiatorRule
    {
        [Min(0)] public int StrikeDamage = 6;
        [Min(0)] public int StrikeWeight = 50;

        [Min(0)] public int GuardBlock = 8;
        [Min(0)] public int GuardWeight = 30;

        [Tooltip("함성 한 번에 쌓는 기세. 1 당 주는 피해 1% 증가.")]
        [Min(0)] public int ShoutMomentum = 50;
        [Min(0)] public int ShoutWeight = 20;
    }

    /// <summary>
    /// 장군의 패턴 수치. 06 장군 을 옮겼다.
    ///   ① 진형 갖추기 [소환] 방패병이 없으면 고정, 1 ~ 2기면 30% · 방패병 1기 (최대 3)
    ///   ② 기마병 돌격 [공격] 1 ~ 2기면 40%, 3기면 4/7, 없으면 미발동 · 데미지 15
    ///   ③ 투창 명령 [버프] 1 ~ 2기면 30%, 3기면 3/7, 없으면 미발동 · 모든 방패병에 '투창 명령'
    /// 3기일 때 4/7 · 3/7 은 진형 갖추기를 뺀 40 : 30 을 다시 나눈 값이라 가중치 하나로 둔다.
    /// </summary>
    [Serializable]
    public class GeneralRule
    {
        [Min(1)] public int MaxShieldmen = 3;

        [Min(0)] public int FormationWeight = 30;

        [Min(0)] public int ChargeDamage = 15;
        [Min(0)] public int ChargeWeight = 40;

        [Min(0)] public int JavelinOrderWeight = 30;
    }

    /// <summary>
    /// 방패병의 수치. 06-1 방패병 을 옮겼다.
    ///   ① 방어 상태 [방어] '투창 명령' 이 없을 때 항상 · 방어도 8
    ///   ② 투창 [공격] '투창 명령' 이 있을 때 · 데미지 7, 버프 소모
    /// </summary>
    [Serializable]
    public class ShieldmanRule
    {
        public string Name = "방패병";
        public string Description = "장군 앞을 막아 영웅의 공격을 대신 받는다. 투창 명령을 받으면 다음 턴에 창을 던진다.";

        [Min(1)] public int MaxHealth = 15;
        [Min(0)] public int GuardBlock = 8;
        [Min(0)] public int JavelinDamage = 7;
    }
}
