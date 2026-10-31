using System;
using System.Collections.Generic;

namespace SlotHero.Map
{
    /// <summary>
    /// 맵의 방 하나. 격자 위의 한 점에 대응한다.
    /// 화면 위치는 격자 좌표에 지터를 더해 계산하며, 픽셀 값은 UI 쪽에서 구한다.
    /// </summary>
    [Serializable]
    public class MapNode
    {
        /// <summary>맵 안에서 고유한 번호. 0부터 시작한다.</summary>
        public int Id;

        /// <summary>단계. 1부터 시작하며 스테이지 내 단계 수만큼 존재한다.</summary>
        public int Stage;

        /// <summary>줄. 0부터 시작하며 0이 화면 위쪽이다.</summary>
        public int Row;

        /// <summary>배치된 방의 종류.</summary>
        public RoomType RoomType;

        /// <summary>격자 간격을 1로 봤을 때의 가로 지터. -0.25 ~ +0.25 범위.</summary>
        public float JitterX;

        /// <summary>격자 간격을 1로 봤을 때의 세로 지터. -0.20 ~ +0.20 범위.</summary>
        public float JitterY;

        /// <summary>다음 단계로 이어지는 노드 번호 목록.</summary>
        public List<int> NextNodeIds = new List<int>();

        /// <summary>이전 단계에서 이 노드로 들어오는 노드 번호 목록.</summary>
        public List<int> PrevNodeIds = new List<int>();

        /// <summary>이 노드의 간선 수. 방 타입 배분에서 정렬 기준으로 쓴다.</summary>
        public int EdgeCount
        {
            get { return NextNodeIds.Count + PrevNodeIds.Count; }
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}단계 {2}줄 {3}", Id, Stage, Row, RoomType);
        }
    }
}
