using System;

namespace SlotHero.Map
{
    /// <summary>
    /// 한 단계의 노드에서 다음 단계의 노드로 이어지는 간선.
    /// 항상 다음 단계로만 이어지므로 방향은 왼쪽에서 오른쪽 한 방향뿐이다.
    /// </summary>
    [Serializable]
    public struct MapEdge : IEquatable<MapEdge>
    {
        public int FromNodeId;
        public int ToNodeId;

        public MapEdge(int fromNodeId, int toNodeId)
        {
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
        }

        public bool Equals(MapEdge other)
        {
            return FromNodeId == other.FromNodeId && ToNodeId == other.ToNodeId;
        }

        public override bool Equals(object obj)
        {
            return obj is MapEdge && Equals((MapEdge)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FromNodeId * 397) ^ ToNodeId;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", FromNodeId, ToNodeId);
        }
    }
}
