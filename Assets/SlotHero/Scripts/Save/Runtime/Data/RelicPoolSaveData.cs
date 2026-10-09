using System;
using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 유물이 나올 순서를 어디까지 꺼냈는지. 런 데이터에 실린다.
    ///
    /// 성소 쪽의 `RunRelicPool` 을 저장하는 꼴이다. 저장은 성소 코드를 모르게 두려고 값만 옮겨 담는다.
    /// 옮기는 일은 흐름이 한다.
    ///
    /// 순서는 시드와 테이블 번호로 다시 만들 수 있지만 `Order` 도 함께 담는다.
    /// 한 진열에 같은 유물이 두 번 오르지 않게 하느라 꺼내는 도중에 순서를 미루는 일이 있어
    /// 시드만으로는 지금 순서가 되살아나지 않을 수 있기 때문이다. 유물 식별자 몇십 개라 크지 않다.
    /// </summary>
    [Serializable]
    public class RelicPoolSaveData
    {
        /// <summary>지금 쓰고 있는 테이블 번호. 1부터 센다. 0 이면 아직 만든 적이 없다.</summary>
        public int TableIndex;

        /// <summary>지금 테이블에서 다음에 꺼낼 자리.</summary>
        public int NextIndex;

        /// <summary>지금 테이블의 순서. 유물 식별자다.</summary>
        public List<string> Order = new List<string>();

        /// <summary>저장된 것이 있는지. 없으면 런 시드로 처음부터 만든다.</summary>
        public bool HasData
        {
            get { return TableIndex > 0 && Order != null && Order.Count > 0; }
        }
    }
}
