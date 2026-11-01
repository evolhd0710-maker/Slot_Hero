namespace SlotHero.Audio
{
    /// <summary>
    /// 같은 소리의 파일 여럿 가운데 하나를 고르는 난수. UnityEngine.Random 은 전역 상태를 쓰기 때문에 쓰지 않는다.
    /// 맵의 MapRandom, 성소의 SanctumRandom 과 같은 xorshift32 다.
    ///
    /// 소리는 게임 결과에 닿지 않아 런 시드를 쓰지 않고 고정 시드로 시작한다.
    /// 같은 순서로 소리를 내면 어느 기기에서든 같은 파일이 나온다.
    /// </summary>
    public sealed class SoundRandom
    {
        private uint _state;

        public SoundRandom(int seed)
        {
            // 0은 xorshift 에서 고정점이므로 피한다.
            _state = (uint)seed;
            if (_state == 0u)
            {
                _state = 0x9E3779B9u;
            }

            for (int i = 0; i < 4; i++)
            {
                NextUInt();
            }
        }

        public uint NextUInt()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        /// <summary>
        /// 0 이상 count 미만에서 하나를 고른다. previous 는 바로 앞에 고른 것이고 둘 이상일 때는 그것을 피한다.
        /// 동전 소리처럼 되풀이되는 소리가 같은 파일로 두 번 이어 나지 않게 하려는 것이다.
        /// </summary>
        public int Pick(int count, int previous)
        {
            if (count <= 0)
            {
                return -1;
            }

            if (count == 1)
            {
                return 0;
            }

            if (previous < 0 || previous >= count)
            {
                return (int)(NextUInt() % (uint)count);
            }

            // 앞의 것을 뺀 count - 1 개에서 고르고, 앞의 것 자리부터는 하나씩 민다.
            int index = (int)(NextUInt() % (uint)(count - 1));
            return index >= previous ? index + 1 : index;
        }
    }
}
