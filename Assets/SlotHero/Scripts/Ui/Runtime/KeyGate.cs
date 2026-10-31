using System.Collections.Generic;
using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 한 번 누른 키를 한 곳만 쓰게 한다.
    ///
    /// ESC 하나를 흐름, 설정 화면, 현재 빌드 화면이 저마다 `Update` 에서 읽는다.
    /// `Input.GetKeyDown` 은 그 프레임 내내 참이고 `Update` 차례는 정해져 있지 않아서
    /// 한 번 누른 ESC 를 둘이 함께 받아 버렸다.
    ///
    /// - 설정이 열린 채 ESC → 설정이 닫고, 흐름이 "열린 게 없네" 하고 **다시 연다**
    /// - 맵에서 ESC → 흐름이 설정을 열고, 설정이 같은 프레임에 그 ESC 로 **바로 닫는다**
    /// - 현재 빌드가 열린 채 ESC → 닫히면서 **설정이 대신 열린다**
    ///
    /// 차례에 따라 되기도 하고 안 되기도 해서 눈으로 잡기 어려웠다.
    /// 이제 먼저 쓴 쪽만 쓰고, 같은 프레임의 나머지는 못 쓴다.
    ///
    /// **쓸지 말지를 먼저 따지고 나서 가져간다.** 안 쓸 거면 가져가지 않아야
    /// 다음 차례가 쓸 수 있다. 흐름은 겹쳐 뜬 화면이 있으면 가져가지 않는다.
    /// </summary>
    public static class KeyGate
    {
        private static readonly Dictionary<KeyCode, int> _usedFrame = new Dictionary<KeyCode, int>();

        /// <summary>
        /// 이 프레임에 그 키가 눌렸고 아직 아무도 안 썼으면 가져간다.
        /// 가져가면 참이다. 같은 프레임의 다른 쪽은 거짓을 받는다.
        /// </summary>
        public static bool TryUse(KeyCode key)
        {
            return TryUse(key, Input.GetKeyDown(key), Time.frameCount);
        }

        /// <summary>
        /// 눌림과 프레임을 밖에서 넣어 주는 것. 검사에서 쓴다.
        /// 게임 코드는 위의 것을 쓴다.
        /// </summary>
        public static bool TryUse(KeyCode key, bool pressed, int frame)
        {
            if (!pressed)
            {
                return false;
            }

            int used;
            if (_usedFrame.TryGetValue(key, out used) && used == frame)
            {
                return false;
            }

            _usedFrame[key] = frame;
            return true;
        }

        /// <summary>기록을 비운다. 검사에서만 쓴다.</summary>
        public static void ResetForTests()
        {
            _usedFrame.Clear();
        }
    }
}
