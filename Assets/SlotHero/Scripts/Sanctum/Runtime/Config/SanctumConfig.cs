using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("CampHealAmount")]
        [SerializeField] private int _campHealAmount = 40;
        public int CampHealAmount { get { return _campHealAmount; } set { _campHealAmount = value; } }

        [Tooltip("성소 하나에서 야영을 쓸 수 있는 횟수. 기본 1회.")]
        [Min(0)]
        [FormerlySerializedAs("CampUseLimit")]
        [SerializeField] private int _campUseLimit = 1;
        public int CampUseLimit { get { return _campUseLimit; } set { _campUseLimit = value; } }

        [Tooltip("야영에 드는 골드. 기본 0으로 별도의 재화를 소모하지 않는다.")]
        [Min(0)]
        [FormerlySerializedAs("CampCost")]
        [SerializeField] private int _campCost = 0;
        public int CampCost { get { return _campCost; } set { _campCost = value; } }

        [Header("행상 진열")]
        [Tooltip("진열하는 문양 수. 서로 다른 문양으로 채운다. 기본 4.")]
        [Min(0)]
        [FormerlySerializedAs("SymbolSlotCount")]
        [SerializeField] private int _symbolSlotCount = 4;
        public int SymbolSlotCount { get { return _symbolSlotCount; } set { _symbolSlotCount = value; } }

        [Tooltip("진열하는 코인 수. 보유하지 않은 코인으로 채운다. 기본 2.")]
        [Min(0)]
        [FormerlySerializedAs("CoinSlotCount")]
        [SerializeField] private int _coinSlotCount = 2;
        public int CoinSlotCount { get { return _coinSlotCount; } set { _coinSlotCount = value; } }

        [Tooltip("진열하는 유물 수. 보유하지 않은 유물로 채운다. 기본 3.")]
        [Min(0)]
        [FormerlySerializedAs("RelicSlotCount")]
        [SerializeField] private int _relicSlotCount = 3;
        public int RelicSlotCount { get { return _relicSlotCount; } set { _relicSlotCount = value; } }

        [Header("희귀도 분포")]
        [Tooltip("행상이 코인과 유물을 진열할 때 등급부터 굴리는 비중. 스테이지 범위마다 한 줄이다. " +
                 "비어 있거나 그 스테이지를 덮는 줄이 없으면 등급을 따지지 않고 고르게 뽑는다. 문양은 등급이 없어 쓰지 않는다. " +
                 "기획서에 수치가 없어 비워 둔다(2026년 10월 9일 원재).")]
        [FormerlySerializedAs("RarityByStage")]
        [SerializeField] private List<StageRarityWeights> _rarityByStage = new List<StageRarityWeights>();
        public List<StageRarityWeights> RarityByStage { get { return _rarityByStage; } set { _rarityByStage = value; } }

        /// <summary>그 스테이지의 희귀도 분포. 없으면 null 이고 그때는 고르게 뽑는다.</summary>
        public StageRarityWeights GetRarityWeights(int stage)
        {
            return StageRarityWeights.Find(RarityByStage, stage);
        }

        [Header("유물 새로고침")]
        [Tooltip("새로고침할 때 이미 팔린 자리도 새 유물로 다시 채운다. " +
                 "05 행상 구성 의 '유물 3자리 모두 교체한다'를 글자 그대로 따르는 쪽이다.")]
        [FormerlySerializedAs("RefreshRefillsSoldSlots")]
        [SerializeField] private bool _refreshRefillsSoldSlots = true;
        public bool RefreshRefillsSoldSlots { get { return _refreshRefillsSoldSlots; } set { _refreshRefillsSoldSlots = value; } }

        // 바뀐 문양은 늘 바꾸기 전과 다른 문양이다. 끄는 설정을 두지 않는다(2026년 10월 25일 원재).
        [Header("문양 변경")]
        [Tooltip("바뀐 문양이 이미 가지고 있는 문양이 되지 않게 한다. 후보가 모자라면 이 제한을 푼다.")]
        [FormerlySerializedAs("SymbolChangeExcludesOwned")]
        [SerializeField] private bool _symbolChangeExcludesOwned;
        public bool SymbolChangeExcludesOwned { get { return _symbolChangeExcludesOwned; } set { _symbolChangeExcludesOwned = value; } }

        [Header("재화")]
        [Tooltip("런을 시작할 때의 보유 골드. 06 재화 의 시작 수치 0. " +
                 "골드는 성소 밖에서도 쓰므로 저장소를 받으면 런 시작 설정으로 옮겨도 된다.")]
        [Min(0)]
        [FormerlySerializedAs("GoldStartAmount")]
        [SerializeField] private int _goldStartAmount = 0;
        public int GoldStartAmount { get { return _goldStartAmount; } set { _goldStartAmount = value; } }

        /// <summary>진열 자리 수 합계.</summary>
        public int TotalSlotCount
        {
            get { return SymbolSlotCount + CoinSlotCount + RelicSlotCount; }
        }

        private void OnValidate()
        {
            _campHealAmount = Mathf.Max(0, _campHealAmount);
            _campUseLimit = Mathf.Max(0, _campUseLimit);
            _campCost = Mathf.Max(0, _campCost);
            _symbolSlotCount = Mathf.Max(0, _symbolSlotCount);
            _coinSlotCount = Mathf.Max(0, _coinSlotCount);
            _relicSlotCount = Mathf.Max(0, _relicSlotCount);
            _goldStartAmount = Mathf.Max(0, _goldStartAmount);
        }
    }
}
