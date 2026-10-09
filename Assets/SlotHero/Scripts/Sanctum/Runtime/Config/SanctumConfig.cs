using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 성소 한 곳의 규칙 수치.
    /// 기본값은 성소 기획서 v0.2 의 04 야영, 05 행상 구성, 06 재화 를 그대로 옮긴 것이다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SanctumConfig",
        menuName = "Slot Hero/성소/성소 규칙 설정",
        order = 0)]
    public class SanctumConfig : ScriptableObject
    {
        [Header("야영")]
        [Tooltip("한 번 쓸 때 회복하는 체력. 기본 40. 최대 체력을 넘기지 않는다.")]
        [Min(0)]
        public int CampHealAmount = 40;

        [Tooltip("성소 하나에서 야영을 쓸 수 있는 횟수. 기본 1회.")]
        [Min(0)]
        public int CampUseLimit = 1;

        [Tooltip("야영에 드는 골드. 기본 0으로 별도의 재화를 소모하지 않는다.")]
        [Min(0)]
        public int CampCost = 0;

        [Header("행상 진열")]
        [Tooltip("진열하는 문양 수. 서로 다른 문양으로 채운다. 기본 4.")]
        [Min(0)]
        public int SymbolSlotCount = 4;

        [Tooltip("진열하는 코인 수. 보유하지 않은 코인으로 채운다. 기본 2.")]
        [Min(0)]
        public int CoinSlotCount = 2;

        [Tooltip("진열하는 유물 수. 보유하지 않은 유물로 채운다. 기본 3.")]
        [Min(0)]
        public int RelicSlotCount = 3;

        [Header("희귀도 분포")]
        [Tooltip("행상이 코인과 유물을 진열할 때 등급부터 굴리는 비중. 스테이지 범위마다 한 줄이다. " +
                 "비어 있거나 그 스테이지를 덮는 줄이 없으면 등급을 따지지 않고 고르게 뽑는다. 문양은 등급이 없어 쓰지 않는다. " +
                 "기획서에 수치가 없어 비워 둔다(2026년 10월 9일 원재).")]
        public List<StageRarityWeights> RarityByStage = new List<StageRarityWeights>();

        /// <summary>그 스테이지의 희귀도 분포. 없으면 null 이고 그때는 고르게 뽑는다.</summary>
        public StageRarityWeights GetRarityWeights(int stage)
        {
            return StageRarityWeights.Find(RarityByStage, stage);
        }

        [Header("유물 새로고침")]
        [Tooltip("새로고침할 때 이미 팔린 자리도 새 유물로 다시 채운다. " +
                 "05 행상 구성 의 '유물 3자리 모두 교체한다'를 글자 그대로 따르는 쪽이다.")]
        public bool RefreshRefillsSoldSlots = true;

        [Header("문양 변경")]
        [Tooltip("바뀐 문양이 바꾸기 전과 같은 문양이 되지 않게 한다.")]
        public bool SymbolChangeExcludesSource = true;

        [Tooltip("바뀐 문양이 이미 가지고 있는 문양이 되지 않게 한다. 후보가 모자라면 이 제한을 푼다.")]
        public bool SymbolChangeExcludesOwned;

        [Header("재화")]
        [Tooltip("런을 시작할 때의 보유 골드. 06 재화 의 시작 수치 0. " +
                 "골드는 성소 밖에서도 쓰므로 저장소를 받으면 런 시작 설정으로 옮겨도 된다.")]
        [Min(0)]
        public int GoldStartAmount = 0;

        /// <summary>진열 자리 수 합계.</summary>
        public int TotalSlotCount
        {
            get { return SymbolSlotCount + CoinSlotCount + RelicSlotCount; }
        }

        private void OnValidate()
        {
            CampHealAmount = Mathf.Max(0, CampHealAmount);
            CampUseLimit = Mathf.Max(0, CampUseLimit);
            CampCost = Mathf.Max(0, CampCost);
            SymbolSlotCount = Mathf.Max(0, SymbolSlotCount);
            CoinSlotCount = Mathf.Max(0, CoinSlotCount);
            RelicSlotCount = Mathf.Max(0, RelicSlotCount);
            GoldStartAmount = Mathf.Max(0, GoldStartAmount);
        }
    }
}
