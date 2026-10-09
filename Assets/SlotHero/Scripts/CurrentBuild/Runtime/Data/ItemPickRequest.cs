using System.Collections.Generic;

namespace SlotHero.CurrentBuild
{
    /// <summary>고르기 화면에서 고르는 물건의 종류.</summary>
    public enum ItemPickKind
    {
        /// <summary>문양. 정렬 칸과 오른쪽 태그 비중 칸이 함께 나온다.</summary>
        Symbol = 0,

        /// <summary>코인. 코인 칸만 나온다.</summary>
        Coin = 1,

        /// <summary>유물. 유물 칸만 나온다.</summary>
        Relic = 2,
    }

    /// <summary>
    /// 고르기 화면을 열 때 넘기는 것.
    ///
    /// 이벤트와 성소에서 가진 것 중 하나를 직접 고를 때 현재 빌드 화면과 비슷한 화면을 띄운다.
    /// 2026년 10월 5일 원재가 정했다. 유물을 고르면 유물만, 코인을 고르면 코인만,
    /// 문양을 고르면 정렬 칸과 오른쪽 태그 비중까지 나온다.
    ///
    /// 무엇을 후보로 낼지는 부르는 쪽이 정한다. 성소 문양 변경은 가진 문양 전부,
    /// 08 피의 거래 는 게임의 모든 문양이다.
    /// </summary>
    public class ItemPickRequest
    {
        /// <summary>무엇을 고르는지.</summary>
        public ItemPickKind Kind = ItemPickKind.Symbol;

        /// <summary>화면 왼쪽 위에 적는 제목. "문양 선택" 같은 것이다.</summary>
        public string Title = string.Empty;

        /// <summary>고를 수 있는 것. 이 차례대로 칸이 놓인다. 문양은 정렬 칸이 다시 줄 세운다.</summary>
        public List<BuildEntry> Candidates = new List<BuildEntry>();

        /// <summary>
        /// 지금 빌드. 문양을 고를 때 오른쪽 태그 비중 칸이 이것을 센다.
        /// 후보가 가진 것이 아니어도(08 피의 거래) 태그 비중은 지금 가진 문양으로 보여 준다.
        /// </summary>
        public CurrentBuildSnapshot Build;

        /// <summary>칸에 개수 배지를 붙일지. 가진 것을 고를 때만 켠다.</summary>
        public bool ShowCount = true;

        /// <summary>
        /// 고른 뒤 아래 줄에 적는 꼴. {0} 자리에 고른 것의 이름이 들어간다.
        /// "[{0}] 을 다른 문양으로 바꾼다" 같은 것이다.
        /// </summary>
        public string ResultFormat = "[{0}]";

        /// <summary>확인 버튼 글. 비우면 설정의 기본 글을 쓴다.</summary>
        public string ConfirmText = string.Empty;

        /// <summary>취소 버튼 글. 비우면 설정의 기본 글을 쓴다. 버릴 것을 고를 때는 "포기" 다.</summary>
        public string CancelText = string.Empty;

        /// <summary>
        /// 고르지 않고 닫을 수 있는지. 끄면 취소 버튼을 감추고 ESC 도 받지 않는다.
        /// 소지 한도가 줄어 넘친 만큼 반드시 버려야 할 때 끈다. 2026년 10월 9일 원재가 정했다.
        /// </summary>
        public bool Cancellable = true;
    }
}
