using System.Collections.Generic;

namespace SlotHero.Events
{
    /// <summary>
    /// 이벤트 전용 난수.
    ///
    /// `UnityEngine.Random` 을 쓰지 않는다.
    /// 같은 시드로 같은 방에 들어가면 어느 기기에서든 같은 선택지가 나와야 하기 때문이다.
    /// 맵과 성소가 쓰는 것과 같은 xorshift 다.
    /// </summary>
    public sealed class EventRandom
    {
        private uint _state;

        public EventRandom(int seed)
        {
            // 0 은 xorshift 에서 고정점이므로 피한다.
            _state = (uint)seed;
            if (_state == 0u)
            {
                _state = 0x9E3779B9u;
            }
        }

        /// <summary>런 시드와 방 번호를 섞어 이벤트 하나의 난수를 만든다.</summary>
        public static EventRandom ForEvent(int runSeed, int nodeId)
        {
            unchecked
            {
                int mixed = (int)((uint)runSeed * 0x9E3779B1u + (uint)(nodeId * 0x7FEB352Du));
                return new EventRandom(mixed);
            }
        }

        /// <summary>지금 난수 상태. 저장해 두면 이어서 굴릴 수 있다.</summary>
        public uint State
        {
            get { return _state; }
            set { _state = value == 0u ? 0x9E3779B9u : value; }
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

        /// <summary>0 이상 100 미만의 정수. 확률 분기에 쓴다.</summary>
        public int Percent()
        {
            return Range(0, 100);
        }

        /// <summary>
        /// 목록을 섞는다. 앞에서부터 count 개만 쓰면 중복 없이 뽑은 것이 된다.
        /// </summary>
        public void Shuffle<T>(IList<T> list)
        {
            if (list == null)
            {
                return;
            }

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Range(0, i + 1);
                T swap = list[i];
                list[i] = list[j];
                list[j] = swap;
            }
        }

        private uint NextUInt()
        {
            unchecked
            {
                _state ^= _state << 13;
                _state ^= _state >> 17;
                _state ^= _state << 5;
                return _state;
            }
        }
    }
}
