using System.Collections.Generic;

namespace SlotHero.Sanctum
{
    /// <summary>
    /// 문양 변경에서 **바꿀 문양**을 고른다.
    ///
    /// **대체 길이다.** 성소 기획서 05 행상 구성 의 문양 변경은
    /// 플레이어가 가진 문양 가운데 하나를 직접 고르는 것이고,
    /// 2026년 10월 5일부터 고르기 화면(`ItemPickerScreenController`)이 그 일을 한다.
    ///
    /// 흐름에 고르기 화면이 붙어 있지 않을 때만 시드 난수로 하나 뽑아 확인 팝업에 띄운다.
    /// 확인용 씬처럼 고르기 화면 없이 성소만 돌릴 때 쓴다.
    ///
    /// `UnityEngine.Random` 을 쓰지 않는다. 같은 런에서 같은 횟수째 문양 변경이면
    /// 어느 기기에서든 같은 문양이 나와야 하기 때문이다.
    /// </summary>
    public static class SymbolChangePicker
    {
        /// <summary>
        /// 바꿀 문양 하나를 고른다. 고를 것이 없으면 -1 을 돌려준다.
        ///
        /// changeCount 는 이 성소에서 문양 변경을 몇 번 썼는지다.
        /// 이것을 섞어야 연달아 눌렀을 때 같은 문양만 계속 나오지 않는다.
        /// </summary>
        public static int Pick(IList<MerchantItem> owned, int runSeed, int sanctumIndex, int changeCount)
        {
            if (owned == null || owned.Count == 0)
            {
                return -1;
            }

            if (owned.Count == 1)
            {
                return 0;
            }

            SanctumRandom random = SanctumRandom.ForSanctum(runSeed, MixIndex(sanctumIndex, changeCount));
            return random.Range(0, owned.Count);
        }

        /// <summary>성소 번호와 사용 횟수를 한 숫자로 섞는다.</summary>
        private static int MixIndex(int sanctumIndex, int changeCount)
        {
            unchecked
            {
                // 성소 번호에 자리를 넉넉히 주어 사용 횟수가 다음 성소와 겹치지 않게 한다.
                return sanctumIndex * 1013 + changeCount;
            }
        }
    }
}
