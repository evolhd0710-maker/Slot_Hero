using System;

namespace SlotHero.Save
{
    /// <summary>
    /// 이번 런에서 쌓인 통계.
    /// 저장 시스템 기획서 v0.1 / 05 런 데이터 의 "런 통계" 한 칸을 풀어 쓴 것이다.
    ///
    /// 담을 항목은 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의
    /// "이번 런 기록" 칸이 무엇을 보여 주는지에서 가져왔다.
    /// 런이 끝나면 이 값이 그대로 결과 화면에 올라가고 메타 데이터에 누적된다.
    /// </summary>
    [Serializable]
    public class RunStatisticsData
    {
        /// <summary>지나온 방 수. 깬 방만 센다.</summary>
        public int RoomsCleared;

        /// <summary>처치한 몬스터 수. 엘리트는 빼고 센다.</summary>
        public int MonstersKilled;

        /// <summary>처치한 엘리트 수.</summary>
        public int ElitesKilled;

        /// <summary>런 동안 얻은 골드를 다 더한 값.</summary>
        public int GoldGained;

        /// <summary>런 동안 쓴 골드를 다 더한 값.</summary>
        public int GoldSpent;

        /// <summary>한 번에 준 가장 큰 피해.</summary>
        public int BiggestHit;

        /// <summary>골드를 얻는다. 통계와 재화를 함께 올린다.</summary>
        public void GainGold(RunStatusData status, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            GoldGained += amount;

            if (status != null)
            {
                status.Gold += amount;
            }
        }

        /// <summary>골드를 쓴다. 모자라면 쓰지 않고 false 를 돌려준다.</summary>
        public bool SpendGold(RunStatusData status, int amount)
        {
            if (status == null || !status.SpendGold(amount))
            {
                return false;
            }

            GoldSpent += amount;
            return true;
        }

        /// <summary>준 피해를 적는다. 지금까지 가장 큰 값만 남는다.</summary>
        public void ReportHit(int damage)
        {
            if (damage > BiggestHit)
            {
                BiggestHit = damage;
            }
        }

        /// <summary>값이 앞뒤가 맞는지. 모두 0 이상이어야 한다.</summary>
        public bool IsConsistent()
        {
            return RoomsCleared >= 0
                && MonstersKilled >= 0
                && ElitesKilled >= 0
                && GoldGained >= 0
                && GoldSpent >= 0
                && BiggestHit >= 0;
        }
    }
}
