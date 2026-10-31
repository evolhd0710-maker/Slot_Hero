using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotHero.Sanctum.UI
{
    /// <summary>
    /// 행상 화면 표시.
    /// 성소 기획서 v0.2 / 08 화면 구성 의 행상 화면 배치를 맡는다.
    /// 좌측 돗자리에 문양 2 × 2 와 코인 2개, 우측 양탄자에 문양 변경과
    /// 육각형 위 꼭짓점의 유물 새로고침, 육각형 안의 유물 3개, 하단 중앙에 돌아가기 버튼이다.
    ///
    /// 자리 잡기는 프리팹에서 맞추고 여기서는 자리에 무엇이 놓였는지와 가격만 채운다.
    /// </summary>
    public class MerchantScreenView : MonoBehaviour
    {
        [Header("진열")]
        [Tooltip("돗자리 상단의 문양 자리. 2 × 2 로 4개.")]
        [SerializeField] private MerchantSlotView[] _symbolSlots;

        [Tooltip("돗자리 하단의 코인 자리. 가로로 2개.")]
        [SerializeField] private MerchantSlotView[] _coinSlots;

        [Tooltip("육각형 안의 유물 자리. 3개.")]
        [SerializeField] private MerchantSlotView[] _relicSlots;

        [Header("문양 변경")]
        [SerializeField] private Button _symbolChangeButton;
        [SerializeField] private TMP_Text _symbolChangePriceLabel;

        [Header("유물 새로고침")]
        [SerializeField] private Button _relicRefreshButton;
        [SerializeField] private TMP_Text _relicRefreshPriceLabel;

        [Header("돌아가기")]
        [SerializeField] private Button _backButton;

        /// <summary>진열된 물건을 눌렀을 때. 클릭하면 바로 산다.</summary>
        public event Action<MerchantSlotView> SlotClicked;

        /// <summary>물건에 커서가 올라가거나 벗어났을 때. 상세 설명 팝업에 쓴다.</summary>
        public event Action<MerchantSlotView, bool> SlotHoverChanged;

        /// <summary>문양 변경을 눌렀을 때.</summary>
        public event Action SymbolChangeClicked;

        /// <summary>유물 새로고침을 눌렀을 때.</summary>
        public event Action RelicRefreshClicked;

        /// <summary>돌아가기를 눌렀을 때. 03 진입 흐름 의 돌아가기.</summary>
        public event Action BackClicked;

        private void Awake()
        {
            if (_symbolChangeButton != null)
            {
                _symbolChangeButton.onClick.AddListener(HandleSymbolChangeClick);
            }

            if (_relicRefreshButton != null)
            {
                _relicRefreshButton.onClick.AddListener(HandleRelicRefreshClick);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(HandleBackClick);
            }
        }

        private void OnDestroy()
        {
            if (_symbolChangeButton != null)
            {
                _symbolChangeButton.onClick.RemoveListener(HandleSymbolChangeClick);
            }

            if (_relicRefreshButton != null)
            {
                _relicRefreshButton.onClick.RemoveListener(HandleRelicRefreshClick);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(HandleBackClick);
            }
        }

        /// <summary>진열과 가격을 지금 상태로 채운다. 골드가 바뀔 때마다 다시 부른다.</summary>
        public void Bind(
            SanctumState state,
            SanctumPriceConfig prices,
            SanctumVisualConfig visual,
            IMerchantIconSource icons,
            int currentGold)
        {
            if (state == null || state.Stock == null)
            {
                return;
            }

            BindGroup(_symbolSlots, state.Stock.SymbolSlots, visual, icons, currentGold);
            BindGroup(_coinSlots, state.Stock.CoinSlots, visual, icons, currentGold);
            BindGroup(_relicSlots, state.Stock.RelicSlots, visual, icons, currentGold);

            int symbolChangePrice = state.GetSymbolChangePrice(prices);
            BindService(_symbolChangeButton, _symbolChangePriceLabel, symbolChangePrice, visual, currentGold);

            int relicRefreshPrice = state.GetRelicRefreshPrice(prices);
            BindService(_relicRefreshButton, _relicRefreshPriceLabel, relicRefreshPrice, visual, currentGold);
        }

        private void BindGroup(
            MerchantSlotView[] views,
            List<MerchantSlot> slots,
            SanctumVisualConfig visual,
            IMerchantIconSource icons,
            int currentGold)
        {
            if (views == null)
            {
                return;
            }

            for (int i = 0; i < views.Length; i++)
            {
                MerchantSlotView view = views[i];
                if (view == null)
                {
                    continue;
                }

                MerchantSlot slot = slots != null && i < slots.Count ? slots[i] : null;
                view.Bind(slot, visual, icons, currentGold, HandleSlotClick, HandleSlotHover);
            }
        }

        /// <summary>
        /// 문양 변경과 유물 새로고침처럼 쓸 때마다 값이 오르는 자리를 채운다.
        /// 13 가격 · 공통 규칙 에 따라 골드가 모자라면 가격을 붉게 적고 누를 수 없게 한다.
        /// </summary>
        private static void BindService(
            Button button,
            TMP_Text priceLabel,
            int price,
            SanctumVisualConfig visual,
            int currentGold)
        {
            bool affordable = currentGold >= price;

            if (priceLabel != null)
            {
                priceLabel.text = price.ToString();
                priceLabel.color = visual == null
                    ? Color.white
                    : (affordable ? visual.PriceColor : visual.PriceNotAffordableColor);
            }

            if (button != null)
            {
                button.interactable = affordable;
            }
        }

        private void HandleSlotClick(MerchantSlotView view)
        {
            if (SlotClicked != null)
            {
                SlotClicked(view);
            }
        }

        private void HandleSlotHover(MerchantSlotView view, bool entered)
        {
            if (SlotHoverChanged != null)
            {
                SlotHoverChanged(view, entered);
            }
        }

        private void HandleSymbolChangeClick()
        {
            if (SymbolChangeClicked != null)
            {
                SymbolChangeClicked();
            }
        }

        private void HandleRelicRefreshClick()
        {
            if (RelicRefreshClicked != null)
            {
                RelicRefreshClicked();
            }
        }

        private void HandleBackClick()
        {
            if (BackClicked != null)
            {
                BackClicked();
            }
        }
    }
}
