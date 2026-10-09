using System;

namespace SlotHero.Save
{
    /// <summary>
    /// 런 동안 변하는 캐릭터 수치와 재화.
    /// 저장 시스템 기획서 v0.1 / 05 런 데이터 · 상태와 소유물 의 "스테이터스"와 "재화"다.
    ///
    /// 저장할 때마다 통째로 다시 쓴다. 9쪽이 그렇게 정했다.
    /// 생존 자원은 체력이다. 칩이 아니다.
    /// </summary>
    [Serializable]
    public class RunStatusData
    {
        /// <summary>지금 체력.</summary>
        public int Health;

        /// <summary>최대 체력.</summary>
        public int MaxHealth;

        /// <summary>보유한 골드.</summary>
        public int Gold;

        /// <summary>런을 시작하고 흐른 시간. 초 단위다.</summary>
        public int ElapsedSeconds;

        /// <summary>체력이 0 이하인지. 런이 끝났는지 가리는 데 쓴다.</summary>
        public bool IsDead
        {
            get { return Health <= 0; }
        }

        /// <summary>값이 앞뒤가 맞는지.</summary>
        public bool IsConsistent()
        {
            if (MaxHealth <= 0)
            {
                return false;
            }

            if (Health < 0 || Health > MaxHealth)
            {
                return false;
            }

            return Gold >= 0 && ElapsedSeconds >= 0;
        }

        /// <summary>체력을 깎는다. 0 아래로는 내려가지 않는다.</summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Health -= amount;
            if (Health < 0)
            {
                Health = 0;
            }
        }

        /// <summary>체력을 채운다. 최대 체력을 넘지 않는다. 실제로 채워진 양을 돌려준다.</summary>
        public int Heal(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int before = Health;
            Health += amount;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }

            return Health - before;
        }

        /// <summary>골드를 쓴다. 모자라면 쓰지 않고 false 를 돌려준다.</summary>
        public bool SpendGold(int amount)
        {
            if (amount < 0 || Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            return true;
        }
    }
}
