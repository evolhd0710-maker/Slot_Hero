using System;
using System.Collections.Generic;

namespace SlotHero.RunResult
{
    /// <summary>
    /// 런 종료 결과 화면에 한 번에 그릴 것 전부.
    /// 인게임 화면 기획서 v0.2 / 12 런 종료 결과 화면 이 정한 다섯 덩어리를 담는다.
    ///
    /// 무엇을 적을지 고르는 일은 각 시스템이 맡고 이 화면은 받은 것을 그리기만 한다.
    /// 줄을 만드는 데 쓰는 도우미는 <see cref="RunResultText"/> 에 있다.
    /// </summary>
    [Serializable]
    public class RunResultData
    {
        /// <summary>결과. 화면 위쪽에 크게 적는다.</summary>
        public RunOutcome Outcome;

        /// <summary>
        /// 도달 지점. 스테이지와 단계, 마지막으로 상대한 몬스터를 한 줄로 적는다.
        /// 와이어프레임 예시는 "스테이지 1 · 9번째 방 · 엘리트에게 쓰러짐"이다.
        /// </summary>
        public string LocationText = string.Empty;

        /// <summary>최종 구성. 마지막 시점에 가지고 있던 것을 간략하게 적는다.</summary>
        public List<ResultLine> FinalBuild = new List<ResultLine>();

        /// <summary>이번 런 기록. 요약과 주요 기록을 적는다.</summary>
        public List<ResultLine> RunRecord = new List<ResultLine>();

        /// <summary>해금 및 도전과제. 이번 런에서 새로 해금된 항목을 적는다.</summary>
        public List<ResultLine> Unlocks = new List<ResultLine>();

        /// <summary>세 칸 중 가장 긴 칸의 줄 수. 칸 높이를 볼 때 쓴다.</summary>
        public int GetLongestPanelLineCount()
        {
            int longest = FinalBuild.Count;

            if (RunRecord.Count > longest)
            {
                longest = RunRecord.Count;
            }

            if (Unlocks.Count > longest)
            {
                longest = Unlocks.Count;
            }

            return longest;
        }
    }
}
