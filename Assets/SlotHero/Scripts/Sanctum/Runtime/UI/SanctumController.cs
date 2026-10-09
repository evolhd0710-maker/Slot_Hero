using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Sanctum.UI
{
    /// <summary>
    /// 성소 방의 진행 처리.
    /// 성소 기획서 v0.2 / 03 진입 흐름 의
    /// 맵 → 성소 진입 → 성소 화면 → (야영 · 행상 · 나가기) → 성소 퇴장 → 맵 을 그대로 따른다.
    ///
    /// 이 스크립트는 성소 안에서 벌어지는 일까지만 맡는다.
    /// 방을 끝낸 뒤 맵으로 돌아가는 처리, 상단 표시줄 갱신, 아이템 상세 팝업은
    /// 각각 맵 · 상단 UI 바 · 인게임 화면 기획서를 따르는 쪽이 이벤트를 받아 처리한다.
    /// </summary>
    public class SanctumController : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private SanctumConfig _config;
        [SerializeField] private SanctumPriceConfig _priceConfig;
        [SerializeField] private SanctumVisualConfig _visualConfig;

        [Header("성소 화면")]
        [SerializeField] private GameObject _sanctumRoot;
        [SerializeField] private SanctumScreenView _sanctumView;

        [Header("행상 화면")]
        [SerializeField] private GameObject _merchantRoot;
        [SerializeField] private MerchantScreenView _merchantView;

        private SanctumState _state = new SanctumState();
        private IMerchantCatalog _catalog;
        private IMerchantIconSource _icons;
        private IPlayerVitals _vitals;
        private RunGoldState _gold;
        private RunRelicPool _relicPool;
        private readonly List<MerchantItem> _ownedSymbolBuffer = new List<MerchantItem>();
        private bool _waitingSymbolChangeTarget;

        /// 소지 한도까지 차 버릴 것을 고르는 동안 기다리는 진열 칸. 없으면 null 이다.
        private MerchantSlot _pendingSlot;

        /// <summary>야영을 썼을 때. 실제로 회복한 체력을 넘긴다. 04 야영 의 사용 피드백에 쓴다.</summary>
        public event Action<int> CampUsed;

        /// <summary>물건을 샀을 때.</summary>
        public event Action<MerchantItem> ItemPurchased;

        /// <summary>문양을 바꿨을 때. 바꾸기 전 문양의 식별자와 새로 받은 문양을 넘긴다.</summary>
        public event Action<string, MerchantItem> SymbolChanged;

        /// <summary>유물 진열을 새로고침했을 때.</summary>
        public event Action RelicsRefreshed;

        /// <summary>값을 치르지 못했을 때. 골드 부족이 대부분이다.</summary>
        public event Action<PurchaseResult> ActionFailed;

        /// <summary>
        /// 코인이나 유물을 사려는데 소지 한도까지 차 있다. 넘기는 것은 사려던 물건이다.
        /// 받은 쪽이 버릴 것을 고르게 한 뒤 `ConfirmPendingPurchase` 나 `CancelPendingPurchase` 를 부른다.
        /// 아무도 받지 않으면 `ActionFailed` 로 `NoRoom` 을 알린다.
        /// </summary>
        public event Action<MerchantItem> RoomNeeded;

        /// <summary>
        /// 물건에 커서가 올라가거나 벗어났을 때.
        /// 상세 설명 팝업은 인게임 화면 기획서 14 아이템 상세 팝업 소관이라 알리기만 한다.
        /// </summary>
        public event Action<MerchantItem, bool> ItemDetailRequested;

        /// <summary>
        /// 문양 변경에서 바꿀 문양을 골라야 할 때. 지금 가진 문양 목록을 넘긴다.
        /// 고르는 화면을 띄운 쪽이 <see cref="ConfirmSymbolChange"/>나 <see cref="CancelSymbolChange"/>를 부른다.
        /// </summary>
        public event Action<IReadOnlyList<MerchantItem>> SymbolChangeTargetRequested;

        /// <summary>나가기를 골라 성소 방이 끝났을 때.</summary>
        public event Action Exited;

        /// <summary>지금 성소의 상태. 저장 시스템이 런 데이터에 실어 보낸다.</summary>
        public SanctumState State
        {
            get { return _state; }
        }

        /// <summary>성소 화면이 열려 있는지.</summary>
        public bool IsOpen
        {
            get { return _sanctumRoot != null && _sanctumRoot.activeInHierarchy; }
        }

        /// <summary>행상 화면이 열려 있는지.</summary>
        public bool IsMerchantOpen
        {
            get { return _merchantRoot != null && _merchantRoot.activeInHierarchy; }
        }

        private void Awake()
        {
            if (_sanctumView != null)
            {
                _sanctumView.CampClicked += UseCamp;
                _sanctumView.MerchantClicked += OpenMerchant;
                _sanctumView.ExitClicked += Exit;
            }

            if (_merchantView != null)
            {
                _merchantView.SlotClicked += HandleSlotClicked;
                _merchantView.SlotHoverChanged += HandleSlotHover;
                _merchantView.SymbolChangeClicked += BeginSymbolChange;
                _merchantView.RelicRefreshClicked += RefreshRelics;
                _merchantView.BackClicked += CloseMerchant;
            }
        }

        private void OnDestroy()
        {
            if (_sanctumView != null)
            {
                _sanctumView.CampClicked -= UseCamp;
                _sanctumView.MerchantClicked -= OpenMerchant;
                _sanctumView.ExitClicked -= Exit;
            }

            if (_merchantView != null)
            {
                _merchantView.SlotClicked -= HandleSlotClicked;
                _merchantView.SlotHoverChanged -= HandleSlotHover;
                _merchantView.SymbolChangeClicked -= BeginSymbolChange;
                _merchantView.RelicRefreshClicked -= RefreshRelics;
                _merchantView.BackClicked -= CloseMerchant;
            }

            DetachGold();
        }

        /// <summary>
        /// 성소에 새로 들어온다. 진열을 만들고 성소 화면을 연다.
        /// sanctumIndex 는 런 안에서 몇 번째 성소인지로, 같은 런에서 성소마다 다른 진열이 나오게 한다.
        /// </summary>
        public void Enter(
            int runSeed,
            int sanctumIndex,
            IMerchantCatalog catalog,
            IPlayerVitals vitals,
            RunGoldState gold,
            RunRelicPool relicPool,
            IMerchantIconSource icons = null)
        {
            Attach(catalog, vitals, gold, relicPool, icons);

            _state = new SanctumState();
            _state.Begin(runSeed, sanctumIndex, _config, _priceConfig, _catalog, _relicPool);

            _waitingSymbolChangeTarget = false;
            _pendingSlot = null;
            Open();
        }

        /// <summary>저장에서 불러온 성소 상태를 그대로 붙인다.</summary>
        public void Load(
            SanctumState state,
            IMerchantCatalog catalog,
            IPlayerVitals vitals,
            RunGoldState gold,
            RunRelicPool relicPool,
            IMerchantIconSource icons = null)
        {
            Attach(catalog, vitals, gold, relicPool, icons);

            _state = state ?? new SanctumState();
            _state.RestoreAfterLoad(_config);

            _waitingSymbolChangeTarget = false;
            _pendingSlot = null;
            Open();
        }

        /// <summary>성소 화면을 연다. 행상 화면은 닫는다.</summary>
        public void Open()
        {
            SetActive(_sanctumRoot, true);
            SetActive(_merchantRoot, false);
            Refresh();
        }

        /// <summary>행상 화면으로 들어간다.</summary>
        public void OpenMerchant()
        {
            SetActive(_sanctumRoot, false);
            SetActive(_merchantRoot, true);
            Refresh();
        }

        /// <summary>행상 화면에서 성소 화면으로 돌아간다.</summary>
        public void CloseMerchant()
        {
            _waitingSymbolChangeTarget = false;
            _pendingSlot = null;
            Open();
        }

        /// <summary>야영을 쓴다. 성소당 1회, 값은 들지 않고 바로 회복한다.</summary>
        public void UseCamp()
        {
            int healed;
            PurchaseResult result = _state.TryUseCamp(_config, _vitals, _gold, out healed);

            if (result != PurchaseResult.Success)
            {
                RaiseFailed(result);
                return;
            }

            Refresh();

            if (CampUsed != null)
            {
                CampUsed(healed);
            }
        }

        /// <summary>
        /// 문양 변경을 시작한다.
        /// 바꿀 문양을 고르는 화면이 필요하므로 여기서는 값만 확인하고 목록을 넘긴다.
        /// </summary>
        public void BeginSymbolChange()
        {
            if (_catalog == null || _gold == null || _priceConfig == null)
            {
                RaiseFailed(PurchaseResult.Unavailable);
                return;
            }

            int price = _state.GetSymbolChangePrice(_priceConfig);
            if (!_gold.CanSpend(price))
            {
                RaiseFailed(PurchaseResult.NotEnoughGold);
                return;
            }

            _ownedSymbolBuffer.Clear();
            _catalog.CollectOwnedSymbols(_ownedSymbolBuffer);
            if (_ownedSymbolBuffer.Count == 0)
            {
                RaiseFailed(PurchaseResult.NoCandidate);
                return;
            }

            _waitingSymbolChangeTarget = true;

            if (SymbolChangeTargetRequested != null)
            {
                SymbolChangeTargetRequested(_ownedSymbolBuffer);
            }
        }

        /// <summary>고른 문양을 다른 문양으로 바꾼다. 고르는 화면이 부른다.</summary>
        public void ConfirmSymbolChange(string ownedSymbolId)
        {
            if (!_waitingSymbolChangeTarget)
            {
                return;
            }

            _waitingSymbolChangeTarget = false;

            MerchantItem newSymbol;
            PurchaseResult result = _state.TryChangeSymbol(
                ownedSymbolId, _config, _priceConfig, _gold, _catalog, out newSymbol);

            if (result != PurchaseResult.Success)
            {
                RaiseFailed(result);
                return;
            }

            Refresh();

            if (SymbolChanged != null)
            {
                SymbolChanged(ownedSymbolId, newSymbol);
            }
        }

        /// <summary>문양 고르기를 그만둔다. 값은 치르지 않는다.</summary>
        public void CancelSymbolChange()
        {
            _waitingSymbolChangeTarget = false;
        }

        /// <summary>유물 진열을 모두 새로 채운다.</summary>
        public void RefreshRelics()
        {
            PurchaseResult result = _state.TryRefreshRelics(_config, _priceConfig, _gold, _catalog, _relicPool);

            if (result != PurchaseResult.Success)
            {
                RaiseFailed(result);
                return;
            }

            Refresh();

            if (RelicsRefreshed != null)
            {
                RelicsRefreshed();
            }
        }

        /// <summary>
        /// 성소를 나간다. 03 진입 흐름 의 성소 퇴장.
        ///
        /// 이미 닫혀 있으면 아무 일도 하지 않는다.
        /// `Exited` 를 듣는 쪽이 화면을 정리하면서 여기를 다시 부를 수 있는데
        /// 그때 또 알리면 서로 끝없이 부른다.
        /// </summary>
        public void Exit()
        {
            if (!IsOpen && !IsMerchantOpen)
            {
                return;
            }

            Close();

            if (Exited != null)
            {
                Exited();
            }
        }

        /// <summary>
        /// 화면만 감춘다. **방을 끝낸 것으로 알리지 않는다.**
        ///
        /// 흐름이 다른 화면으로 넘어가면서 이 화면을 치울 때 쓴다.
        /// 그때 `Exit` 을 부르면 `Exited` 가 울려 방 완료가 저장되고 보상까지 열린다.
        /// 지도 버튼을 성소에서 누르면 방을 깬 적도 없는데 그렇게 됐다.
        ///
        /// 방을 끝내는 것은 플레이어가 나가기를 눌렀을 때뿐이다. 그때는 `Exit` 이다.
        /// </summary>
        public void Close()
        {
            _waitingSymbolChangeTarget = false;
            _pendingSlot = null;
            SetActive(_sanctumRoot, false);
            SetActive(_merchantRoot, false);
        }

        /// <summary>두 화면을 지금 상태로 다시 그린다.</summary>
        public void Refresh()
        {
            if (_sanctumView != null)
            {
                _sanctumView.Refresh(_state, _config, _visualConfig);
            }

            if (_merchantView != null)
            {
                _merchantView.Bind(_state, _priceConfig, _visualConfig, _icons, _gold != null ? _gold.Gold : 0);
            }
        }

        private void Attach(
            IMerchantCatalog catalog,
            IPlayerVitals vitals,
            RunGoldState gold,
            RunRelicPool relicPool,
            IMerchantIconSource icons)
        {
            DetachGold();

            _catalog = catalog;
            _vitals = vitals;
            _gold = gold;
            _relicPool = relicPool;

            // 아이콘을 따로 넘기지 않으면 카탈로그가 함께 맡는 것으로 본다.
            _icons = icons ?? catalog as IMerchantIconSource;

            if (_gold != null)
            {
                _gold.Changed += HandleGoldChanged;
            }
        }

        private void DetachGold()
        {
            if (_gold != null)
            {
                _gold.Changed -= HandleGoldChanged;
            }
        }

        private void HandleGoldChanged(int previous, int current)
        {
            // 골드가 바뀌면 살 수 있는 것과 없는 것이 달라진다.
            Refresh();
        }

        private void HandleSlotClicked(MerchantSlotView view)
        {
            if (view == null)
            {
                return;
            }

            Buy(view.Slot);
        }

        /// <summary>
        /// 버릴 것을 골라 자리가 났다. 기다리던 물건을 마저 산다.
        /// 흐름이 `RoomNeeded` 를 받아 버릴 것을 고르게 한 뒤 부른다.
        /// </summary>
        public void ConfirmPendingPurchase()
        {
            MerchantSlot slot = _pendingSlot;
            _pendingSlot = null;
            Buy(slot);
        }

        /// <summary>버리지 않고 포기했다. 사지 않는다. 골드는 치르지 않았으므로 그대로다.</summary>
        public void CancelPendingPurchase()
        {
            _pendingSlot = null;
        }

        /// <summary>자리가 나기를 기다리는 구매가 있는지.</summary>
        public bool HasPendingPurchase
        {
            get { return _pendingSlot != null; }
        }

        private void Buy(MerchantSlot slot)
        {
            if (slot == null || _state == null)
            {
                return;
            }

            MerchantItem item = slot.Item;
            PurchaseResult result = _state.TryBuy(slot, _gold, _catalog);

            // 소지 한도까지 찼다. 사기 전에 버릴 것을 고르게 한다. 2026년 10월 9일 원재가 정했다.
            if (result == PurchaseResult.NoRoom && RoomNeeded != null)
            {
                _pendingSlot = slot;
                RoomNeeded(item);
                return;
            }

            if (result != PurchaseResult.Success)
            {
                RaiseFailed(result);
                return;
            }

            Refresh();

            if (ItemPurchased != null)
            {
                ItemPurchased(item);
            }
        }

        private void HandleSlotHover(MerchantSlotView view, bool entered)
        {
            if (view == null || view.Slot == null || !view.Slot.HasItem)
            {
                return;
            }

            if (ItemDetailRequested != null)
            {
                ItemDetailRequested(view.Slot.Item, entered);
            }
        }

        private void RaiseFailed(PurchaseResult result)
        {
            if (ActionFailed != null)
            {
                ActionFailed(result);
            }
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null && target.activeSelf != value)
            {
                target.SetActive(value);
            }
        }
    }
}
