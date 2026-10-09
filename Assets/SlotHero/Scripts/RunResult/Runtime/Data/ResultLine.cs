using System;

namespace SlotHero.RunResult
{
    /// <summary>
    /// 칸에 적는 줄 하나.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 의
    /// 최종 구성, 이번 런 기록, 해금 및 도전과제 세 칸이 모두 이 줄을 쌓아 만든다.
    ///
    /// 와이어프레임을 보면 세 칸이 모두 같은 모양이다.
    /// 굵은 소제목과 보통 본문이 줄 간격 하나로 이어지고 문단 사이에 빈 줄이 하나 들어간다.
    /// 그래서 칸마다 다른 구조를 두지 않고 줄 목록 하나로 다룬다.
    /// </summary>
    [Serializable]
    public struct ResultLine
    {
        /// <summary>적는 글. 비어 있으면 빈 줄이 된다.</summary>
        public string Text;

        /// <summary>
        /// 굵게 적는지.
        /// 와이어프레임의 "문양 24장", "태그 정보", "유물", "코인" 같은 소제목이 굵다.
        /// </summary>
        public bool Strong;

        /// <summary>문단을 가르는 빈 줄인지.</summary>
        public bool IsBlank
        {
            get { return string.IsNullOrEmpty(Text); }
        }

        /// <summary>보통 본문 줄.</summary>
        public static ResultLine Body(string text)
        {
            ResultLine line = new ResultLine();
            line.Text = text;
            return line;
        }

        /// <summary>굵은 소제목 줄.</summary>
        public static ResultLine Heading(string text)
        {
            ResultLine line = new ResultLine();
            line.Text = text;
            line.Strong = true;
            return line;
        }

        /// <summary>문단을 가르는 빈 줄.</summary>
        public static ResultLine Blank()
        {
            return new ResultLine();
        }
    }
}
