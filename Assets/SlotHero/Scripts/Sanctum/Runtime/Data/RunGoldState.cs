using System;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 런 하나가 들고 있는 골드.
    /// 성소 기획서 v0.2 / 06 재화 의 표를 그대로 옮긴 것이다.
    ///
    /// 골드를 얻는 곳은 전투 보상, 오버킬, 이벤트라 성소 밖이지만
    /// 재화의 규칙을 정한 문서가 성소 기획서이므로 여기에 둔다.
    /// 유지 단위가 런이라 저장 시스템 기획서의 런 데이터에 그대로 실린다.
    /// </summary>
    [Serializable]
    public class RunGoldState
    {
        /// <summary>지금 가진 골드. 최소 수치는 0이다.</summary>
        public int Gold;

        /// <summary>골드가 바뀔 때. 앞 값과 뒤 값을 넘긴다. 상단 표시줄 갱신에 쓴다.</summary>
        [NonSerialized] public Action<int, int> Changed;

        /// <summary>런을 시작할 때의 보유 골드로 되돌린다.</summary>
        public void ResetForNewRun(int startAmount)
        {
            SetGold(startAmount < 0 ? 0 : startAmount);
        }

        /// <summary>골드를 얻는다. 전투 보상, 오버킬, 이벤트가 부른다.</summary>
        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetGold(Gold + amount);
        }

        /// <summary>그만큼 치를 수 있는지.</summary>
        public bool CanSpend(int amount)
        {
            return amount >= 0 && Gold >= amount;
        }

        /// <summary>
        /// 값을 치른다. 보유 수치를 넘겨 소모할 수 없으므로 모자라면 아무것도 하지 않고 실패한다.
        /// 행상의 구매, 문양 변경, 유물 새로고침이 모두 이 길을 지난다.
        /// </summary>
        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            SetGold(Gold - amount);
            return true;
        }

        /// <summary>
        /// 보유량과 무관하게 깎는다. 모자라면 0으로 보정한다.
        /// 06 재화 의 "초과 소모되는 경우 최솟값 보정으로 0이 된다"에 해당하는 길로,
        /// 이벤트가 대가를 물릴 때처럼 구매가 아닌 소모에만 쓴다.
        /// </summary>
        public int Lose(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int lost = amount > Gold ? Gold : amount;
            SetGold(Gold - lost);
            return lost;
        }

        private void SetGold(int next)
        {
            if (next < 0)
            {
                next = 0;
            }

            if (next == Gold)
            {
                return;
            }

            int previous = Gold;
            Gold = next;

            if (Changed != null)
            {
                Changed(previous, Gold);
            }
        }
    }
}
