using System.Collections.Generic;

namespace SlotHero.Combat
{
    /// <summary>
    /// 전투 전용 난수. `UnityEngine.Random` 은 전역 상태를 쓰므로 쓰지 않는다.
    /// 같은 시드로 같은 차례로 뽑으면 어느 기기에서든 같은 값이 나온다.
    ///
    /// 맵의 `MapRandom` 과 같은 xorshift32 다. 영역끼리 서로를 모르게 하려고 따로 둔다.
    /// </summary>
    public sealed class CombatRandom
    {
        private uint _state;

        public CombatRandom(int seed)
        {
            // 0은 xorshift 에서 고정점이므로 피한다.
            _state = (uint)seed;
            if (_state == 0u)
            {
                _state = 0x9E3779B9u;
            }

            // 초기 상태가 시드에 따라 충분히 흩어지도록 몇 번 돌린다.
            for (int i = 0; i < 4; i++)
            {
                NextUInt();
            }
        }

        /// <summary>런 시드와 스테이지와 방 번호를 섞어 전투 하나의 시드를 만든다.</summary>
        public static int MixSeed(int runSeed, int stageIndex, int nodeId)
        {
            unchecked
            {
                uint mixed = (uint)runSeed * 0x85EBCA6Bu;
                mixed ^= (uint)(stageIndex + 1) * 0x27D4EB2Du;
                mixed ^= (uint)(nodeId + 1) * 0x165667B1u;
                return (int)mixed;
            }
        }

        public uint NextUInt()
        {
            // xorshift32
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }

        /// <summary>minInclusive 이상 maxExclusive 미만의 정수.</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            uint span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % span);
        }

        /// <summary>목록을 제자리에서 섞는다. 피셔–예이츠.</summary>
        public void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Range(0, i + 1);
                T swap = list[i];
                list[i] = list[j];
                list[j] = swap;
            }
        }
    }
}
