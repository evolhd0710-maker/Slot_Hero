using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 성소 전용 난수. UnityEngine.Random 은 전역 상태를 쓰기 때문에 쓰지 않는다.
    /// 같은 시드로 같은 순서로 뽑으면 어느 기기에서든 같은 진열이 나온다.
    ///
    /// 맵의 MapRandom 과 같은 xorshift32 다.
    /// 성소는 진열을 새로고침할 때마다 난수를 이어서 굴려야 하므로
    /// <see cref="State"/>를 꺼내 저장하고 <see cref="FromState"/>로 되살리는 길을 더 뒀다.
    /// </summary>
    public sealed class SanctumRandom
    {
        private uint _state;

        public SanctumRandom(int seed)
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

        private SanctumRandom(uint state, bool warmUp)
        {
            _state = state == 0u ? 0x9E3779B9u : state;

            if (warmUp)
            {
                for (int i = 0; i < 4; i++)
                {
                    NextUInt();
                }
            }
        }

        /// <summary>런 시드와 방 번호를 섞어 성소 하나의 난수를 만든다.</summary>
        public static SanctumRandom ForSanctum(int runSeed, int sanctumIndex)
        {
            unchecked
            {
                int mixed = (int)((uint)runSeed * 0x85EBCA6Bu + (uint)(sanctumIndex * 0x45D9F3B5u));
                return new SanctumRandom(mixed);
            }
        }

        /// <summary>
        /// 유물이 나올 순서를 뽑는 난수. 성소마다의 난수와 섞이지 않게 따로 둔다.
        /// 같은 런이라도 테이블 번호가 다르면 순서가 다르게 나온다.
        /// </summary>
        public static SanctumRandom ForRelicPool(int runSeed, int tableIndex)
        {
            unchecked
            {
                int mixed = (int)((uint)runSeed * 0xC2B2AE35u
                                  + (uint)(tableIndex * 0x27220A95)
                                  + 0x165667B1u);
                return new SanctumRandom(mixed);
            }
        }

        /// <summary>저장해 둔 상태에서 이어 굴린다. 불러오기와 새로고침에 쓴다.</summary>
        public static SanctumRandom FromState(uint state)
        {
            return new SanctumRandom(state, false);
        }

        /// <summary>지금 난수 상태. 이 값을 저장해 두면 다음에 이어서 굴릴 수 있다.</summary>
        public uint State
        {
            get { return _state; }
        }

        public uint NextUInt()
        {
            // xorshift32
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return _state;
        }

        /// <summary>0 이상 1 미만의 실수.</summary>
        public float NextFloat()
        {
            return (NextUInt() >> 8) / 16777216f;
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

        /// <summary>목록에서 하나를 뽑는다. 비어 있으면 기본값.</summary>
        public T Pick<T>(IReadOnlyList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }

            return list[Range(0, list.Count)];
        }

        /// <summary>목록을 자리에서 섞는다.</summary>
        public void Shuffle<T>(IList<T> list)
        {
            if (list == null)
            {
                return;
            }

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Range(0, i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
