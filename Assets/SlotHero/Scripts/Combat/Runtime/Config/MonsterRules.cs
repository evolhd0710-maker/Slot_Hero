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

        [Header("행동 설명 · 정보 칸의 다음 행동 아래에 적는다")]
        [Tooltip("{0} 은 예상 피해다.")]
        public string StrikeText = "영웅에게 {0} 피해를 준다.";

        [Tooltip("{0} 은 얻는 방어도다.")]
        public string GuardText = "방어도 {0} 을 얻는다. 방어도는 영웅의 다음 공격을 먼저 막는다.";

        [Tooltip("{0} 은 쌓는 기세다.")]
        public string ShoutText = "기세 {0} 을 쌓는다. 기세 1 당 주는 피해가 1% 늘어난다.";
    }

    /// <summary>
    /// 장군의 패턴 수치. 장군 패턴 재설계(2026년 10월 29일)를 옮겼다.
    ///
    /// 방패병은 따로 서는 적이 아니라 장군이 가진 수다. 공격 대상도 고르지 않는다.
    ///   [턴 시작] → [방패병 소환] → [기술 선택] → [플레이어 행동] → [기술 사용]
    ///
    /// 기술. 위력의 방패병 수는 장군이 기술을 쓰는 순간의 수다. 가중치는 기술을 고를 때의 수로 본다.
    ///   ① 돌격      10 × (방패병 + 1) 피해 · 방패병 1 ~ 2기일 때 50
    ///   ② 진형 형성 방패병 2 소환        · 방패병 1기일 때 20
    ///   ③ 방패 진형 15 × (방패병 + 1) 피해 · 방패병 2 ~ 3기일 때 70
    ///   ④ 총공세    30 × 방패병 피해     · 방패병 3기일 때 130
    ///
    /// 방패병의 피해 감소. 영웅 공격의 피해 계산이 모두 끝난 뒤 50 이상이면 방패병 1기를 써서 10 줄인다.
    /// 공격 한 번에 한 번만 일어난다.
    /// </summary>
    [Serializable]
    public class GeneralRule
    {
        [Tooltip("방패병 최대 수.")]
        [Min(0)] public int MaxShieldmen = 3;

        [Tooltip("매 턴 시작에 소환하는 방패병 수.")]
        [Min(0)] public int SummonPerTurn = 1;

        [Tooltip("이 피해 이상을 받으면 방패병 1기가 피해를 줄인다.")]
        [Min(0)] public int GuardThreshold = 50;

        [Tooltip("방패병 1기가 줄이는 피해.")]
        [Min(0)] public int GuardReduction = 10;

        [Header("① 돌격 · 10 × (방패병 + 1)")]
        [Min(0)] public int ChargeDamage = 10;
        [Min(0)] public int ChargeWeight = 50;

        [Header("② 진형 형성 · 방패병 2 소환")]
        [Min(0)] public int FormationSummon = 2;
        [Min(0)] public int FormationWeight = 20;

        [Header("③ 방패 진형 · 15 × (방패병 + 1)")]
        [Min(0)] public int ShieldWallDamage = 15;
        [Min(0)] public int ShieldWallWeight = 70;

        [Header("④ 총공세 · 30 × 방패병")]
        [Min(0)] public int AllOutDamage = 30;
        [Min(0)] public int AllOutWeight = 130;

        [Header("행동 설명 · 정보 칸의 다음 행동 아래에 적는다")]
        [Tooltip("{0} 예상 피해, {1} 방패병 하나당 피해, {2} 지금 방패병 수.")]
        public string ChargeText = "영웅에게 {0} 피해를 준다.\n{1} × (방패병 {2} + 1)";

        [Tooltip("{0} 소환하는 수, {1} 방패병 최대 수.")]
        public string FormationText = "방패병 {0} 기를 소환한다. 최대 {1} 기다.";

        [Tooltip("{0} 예상 피해, {1} 방패병 하나당 피해, {2} 지금 방패병 수.")]
        public string ShieldWallText = "영웅에게 {0} 피해를 준다.\n{1} × (방패병 {2} + 1)";

        [Tooltip("{0} 예상 피해, {1} 방패병 하나당 피해, {2} 지금 방패병 수.")]
        public string AllOutText = "영웅에게 {0} 피해를 준다.\n{1} × 방패병 {2}";

        [Tooltip("정보 칸에 적는 장군 설명. {0} 턴마다 소환 수, {1} 지금 방패병 수, {2} 피해 문턱, {3} 줄이는 피해.")]
        [TextArea]
        public string DescriptionFormat =
            "매 턴 시작시 방패병을 {0} 소환합니다\n(현재 방패병 수:{1})\n({2}) 이상 데미지를 받을 때마다 데미지를 ({3}) 감소시키고 방패병이 1기 사라집니다";

        /// <summary>지금 방패병 수로 쓴 설명.</summary>
        public string Describe(int shieldmen)
        {
            return string.Format((DescriptionFormat ?? string.Empty).Replace("\\n", "\n"),
                SummonPerTurn, shieldmen, GuardThreshold, GuardReduction);
        }
    }
}
