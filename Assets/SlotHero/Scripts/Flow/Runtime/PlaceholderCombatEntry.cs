using System;
using UnityEngine;
using SlotHero.Map;
using SlotHero.Popup;
using SlotHero.Popup.UI;

namespace SlotHero.Flow
{
    /// <summary>
    /// 전투 대역.
    ///
    /// **게임 자료가 아니다.** 전투 코드가 붙을 때까지 흐름을 한 바퀴 돌려 보려고 둔 것이다.
    /// 전투 방에 들어가면 팝업을 띄워 이겼는지 졌는지를 직접 고르게 한다.
    /// 그래야 맵, 저장, 런 종료 결과까지 실제로 이어지는지 확인할 수 있다.
    ///
    /// 전투가 오면 이 컴포넌트를 떼고 진짜 것을 붙인다.
    /// </summary>
    public class PlaceholderCombatEntry : MonoBehaviour, ICombatEntry
    {
        [Header("붙일 것")]
        [SerializeField] private PopupPresenter _popup;

        [Header("대역 수치")]
        [Tooltip("이겼다고 고르면 주는 골드.")]
        [SerializeField] private int _victoryGold = 40;

        [Tooltip("이겼다고 고르면 적는 가장 큰 피해.")]
        [SerializeField] private int _victoryBiggestHit = 60;

        [Tooltip("이겼다고 고르면 보상 화면에 얹는 오버킬 골드. 0 이면 오버킬 줄을 비운다.")]
        [SerializeField] private int _victoryOverkillGold;

        [Tooltip("졌다고 고르면 깎는 체력. 체력이 0 이 되면 런이 끝난다.")]
        [SerializeField] private int _defeatDamage = 999;

        private Action<CombatResult> _onFinished;
        private RunContext _context;

        /// <inheritdoc />
        public void Enter(RunContext context, MapNode node, CombatOptions options, Action<CombatResult> onFinished)
        {
            _context = context;
            _onFinished = onFinished;

            if (_popup == null)
            {
                // 팝업이 없으면 묻지 않고 이긴 것으로 넘긴다.
                Finish(true);
                return;
            }

            string roomName = node != null ? RoomTypes.GetDisplayName(node.RoomType) : "전투";
            string body = "전투 코드가 아직 붙지 않았다.\n흐름을 확인하려고 결과를 직접 고른다.";

            // 이벤트가 건 조건을 적어 둔다. 실제로 거는 것은 전투 코드의 몫이다.
            if (options.HasPenalty)
            {
                body += "\n\n부정 효과 " + options.PenaltyId + " · " + options.PenaltyText;
            }

            if (options.DoubleReward)
            {
                body += "\n이기면 보상 두 배";
            }

            PopupSpec spec = new PopupSpec(roomName + " 방", body);

            spec.Add(PopupButtonSpec.Danger("defeat", "패배로 처리"));
            spec.Add(PopupButtonSpec.Normal("victory", "승리로 처리"));

            _popup.Show(spec, HandleChosen);
        }

        /// <inheritdoc />
        public void Leave()
        {
            _onFinished = null;
            _context = null;
        }

        private void HandleChosen(string buttonId)
        {
            Finish(buttonId == "victory");
        }

        private void Finish(bool won)
        {
            Action<CombatResult> callback = _onFinished;
            _onFinished = null;

            if (!won && _context != null)
            {
                _context.TakeDamage(_defeatDamage);
            }

            if (callback != null)
            {
                callback(won
                    ? CombatResult.Victory(_victoryBiggestHit, _victoryGold, _victoryOverkillGold)
                    : CombatResult.Defeat());
            }
        }
    }
}
