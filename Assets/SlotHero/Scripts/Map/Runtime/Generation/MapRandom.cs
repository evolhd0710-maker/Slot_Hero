using System.Collections.Generic;

namespace SlotHero.Map
{
    /// <summary>
    /// 맵 생성 전용 난수. UnityEngine.Random 은 전역 상태를 쓰기 때문에 쓰지 않는다.
    /// 같은 시드로 같은 순서로 뽑으면 어느 기기에서든 같은 값이 나온다.
    /// </summary>
    public sealed class MapRandom
    {
        private uint _state;

        public MapRandom(int seed)
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

        /// <summary>런 시드와 스테이지 번호를 섞어 스테이지 전용 난수를 만든다.</summary>
        public static MapRandom ForStage(int runSeed, int stageIndex)
        {
            unchecked
            {
                int mixed = (int)((uint)runSeed * 0x85EBCA6Bu + (uint)(stageIndex * 0x27D4EB2Du));
                return new MapRandom(mixed);
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

        /// <summary>0 이상 1 미만의 실수.</summary>
        public float NextFloat()
        {
            return (NextUInt() >> 8) / 16777216f;
        }

        /// <summary>-1 이상 1 미만의 실수.</summary>
        public float NextSignedFloat()
        {
            return NextFloat() * 2f - 1f;
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
