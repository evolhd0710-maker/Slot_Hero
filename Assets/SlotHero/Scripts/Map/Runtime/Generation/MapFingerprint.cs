using System.Collections.Generic;
using System.Globalization;

namespace SlotHero.Map
{
    /// <summary>
    /// 지도 한 장의 지문. 같은 지도면 어느 기기에서든 같은 글이 나온다.
    ///
    /// 런은 지도를 저장하지 않고 시드로 다시 만든다(맵 생성 규칙 기획서 v0.2 / `MapRandom.ForStage`).
    /// 업데이트로 생성 설정이나 생성 순서가 바뀌면 같은 시드에서 다른 지도가 나와 저장된 방 번호가 어긋난다.
    /// 런을 시작하거나 스테이지를 넘길 때 지문을 적어 두고, 이어 할 때 다시 만든 지도와 견준다.
    /// 다르면 그 런을 지운다(2026년 10월 22일 원재).
    ///
    /// 플레이에 영향을 주는 것만 담는다. 단계와 줄 수, 노드마다 번호 · 단계 · 줄 · 방 타입 · 다음 노드다.
    /// 지터는 그리는 자리만 바꾸고 기기마다 소수 끝자리가 다를 수 있어 넣지 않는다.
    /// `string.GetHashCode` 는 실행마다 달라 쓰지 않고 FNV-1a 64비트를 직접 계산한다.
    /// </summary>
    public static class MapFingerprint
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        /// <summary>그 지도의 지문. 지도가 없으면 빈 글이다.</summary>
        public static string Of(StageMap map)
        {
            if (map == null || map.Nodes == null)
            {
                return string.Empty;
            }

            ulong hash = OffsetBasis;
            hash = Mix(hash, map.StageCount);
            hash = Mix(hash, map.RowCount);
            hash = Mix(hash, map.Nodes.Count);

            List<MapNode> nodes = new List<MapNode>(map.Nodes);
            nodes.Sort((a, b) => a.Id.CompareTo(b.Id));

            List<int> next = new List<int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                MapNode node = nodes[i];
                hash = Mix(hash, node.Id);
                hash = Mix(hash, node.Stage);
                hash = Mix(hash, node.Row);
                hash = Mix(hash, (int)node.RoomType);

                next.Clear();
                if (node.NextNodeIds != null)
                {
                    next.AddRange(node.NextNodeIds);
                }

                next.Sort();
                hash = Mix(hash, next.Count);
                for (int j = 0; j < next.Count; j++)
                {
                    hash = Mix(hash, next[j]);
                }
            }

            return hash.ToString("x16", CultureInfo.InvariantCulture);
        }

        /// <summary>정수 하나를 네 바이트로 나눠 섞는다.</summary>
        private static ulong Mix(ulong hash, int value)
        {
            uint bits = unchecked((uint)value);
            for (int i = 0; i < 4; i++)
            {
                hash ^= (bits >> (i * 8)) & 0xFFu;
                hash = unchecked(hash * Prime);
            }

            return hash;
        }
    }
}
