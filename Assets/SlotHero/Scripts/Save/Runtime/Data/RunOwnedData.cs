using System;
using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 런에서 모은 문양과 유물과 코인.
    /// 저장 시스템 기획서 v0.1 / 05 런 데이터 · 상태와 소유물 의 "소유물"이다.
    ///
    /// 9쪽이 "변화가 있는 내용만 추가/삭제로 갱신한다"로 정했다.
    /// 그래서 넣고 빼는 창구만 두고 통째로 갈아 끼우는 길은 두지 않는다.
    /// </summary>
    [Serializable]
    public class RunOwnedData
    {
        /// <summary>런에서 빌딩한 문양과 각 개수.</summary>
        public List<OwnedSymbol> Symbols = new List<OwnedSymbol>();

        /// <summary>획득한 유물. 획득한 차례대로 쌓인다.</summary>
        public List<OwnedRelic> Relics = new List<OwnedRelic>();

        /// <summary>보유한 코인.</summary>
        public List<string> CoinIds = new List<string>();

        /// <summary>
        /// 코인을 최대 몇 개까지 가질 수 있는지. 전투 UI 기획서 stt 05 의 "코인은 최대 5개까지 소유 가능".
        /// 특정 조건에서 바뀔 수 있어 런마다 저장한다(2026년 10월 9일 원재). 0 까지 내려갈 수 있다.
        /// **음수면 아직 정하지 않은 것이다.** 예전 저장 파일에는 이 칸이 없어 초기값 -1 이 남고,
        /// 런을 열 때 설정의 시작 값으로 채운다(`RunCatalog.Attach`). 0 은 "하나도 가질 수 없음" 이라 정하지 않음에 쓰지 않는다.
        /// </summary>
        public int CoinCapacity = -1;

        /// <summary>유물을 최대 몇 개까지 가질 수 있는지. 전투 UI 기획서 stt 의 "유물 목록(최대 6개)". 음수면 아직 정하지 않은 것이다.</summary>
        public int RelicCapacity = -1;

        /// <summary>문양을 다 세어 몇 장인지.</summary>
        public int TotalSymbolCount
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < Symbols.Count; i++)
                {
                    sum += Symbols[i].Count;
                }

                return sum;
            }
        }

        /// <summary>문양을 넣는다. 이미 있으면 개수만 더한다.</summary>
        public void AddSymbol(string symbolId, int count)
        {
            if (string.IsNullOrEmpty(symbolId) || count <= 0)
            {
                return;
            }

            for (int i = 0; i < Symbols.Count; i++)
            {
                if (Symbols[i].SymbolId == symbolId)
                {
                    OwnedSymbol found = Symbols[i];
                    found.Count += count;
                    Symbols[i] = found;
                    return;
                }
            }

            Symbols.Add(new OwnedSymbol(symbolId, count));
        }

        /// <summary>문양을 뺀다. 0장이 되면 목록에서 지운다. 실제로 뺀 만큼 돌려준다.</summary>
        public int RemoveSymbol(string symbolId, int count)
        {
            if (string.IsNullOrEmpty(symbolId) || count <= 0)
            {
                return 0;
            }

            for (int i = 0; i < Symbols.Count; i++)
            {
                if (Symbols[i].SymbolId != symbolId)
                {
                    continue;
                }

                OwnedSymbol found = Symbols[i];
                int taken = count < found.Count ? count : found.Count;
                found.Count -= taken;

                if (found.Count <= 0)
                {
                    Symbols.RemoveAt(i);
                }
                else
                {
                    Symbols[i] = found;
                }

                return taken;
            }

            return 0;
        }

        /// <summary>그 문양을 몇 장 가졌는지.</summary>
        public int GetSymbolCount(string symbolId)
        {
            for (int i = 0; i < Symbols.Count; i++)
            {
                if (Symbols[i].SymbolId == symbolId)
                {
                    return Symbols[i].Count;
                }
            }

            return 0;
        }

        /// <summary>유물을 넣는다. 이미 가진 유물이면 넣지 않는다.</summary>
        public bool AddRelic(string relicId)
        {
            if (string.IsNullOrEmpty(relicId) || HasRelic(relicId))
            {
                return false;
            }

            Relics.Add(new OwnedRelic(relicId, 0));
            return true;
        }

        /// <summary>유물을 뺀다. 가지고 있지 않으면 아무 일도 하지 않는다.</summary>
        public bool RemoveRelic(string relicId)
        {
            if (string.IsNullOrEmpty(relicId))
            {
                return false;
            }

            for (int i = 0; i < Relics.Count; i++)
            {
                if (Relics[i].RelicId == relicId)
                {
                    Relics.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>그 유물을 가졌는지.</summary>
        public bool HasRelic(string relicId)
        {
            for (int i = 0; i < Relics.Count; i++)
            {
                if (Relics[i].RelicId == relicId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>유물 안에 쌓인 횟수를 바꾼다.</summary>
        public void SetRelicStack(string relicId, int stack)
        {
            for (int i = 0; i < Relics.Count; i++)
            {
                if (Relics[i].RelicId != relicId)
                {
                    continue;
                }

                OwnedRelic found = Relics[i];
                found.Stack = stack < 0 ? 0 : stack;
                Relics[i] = found;
                return;
            }
        }

        /// <summary>코인을 넣는다. 같은 코인을 여러 개 가질 수 있어 겹침을 막지 않는다.</summary>
        public void AddCoin(string coinId)
        {
            if (!string.IsNullOrEmpty(coinId))
            {
                CoinIds.Add(coinId);
            }
        }

        /// <summary>코인 하나를 뺀다.</summary>
        public bool RemoveCoin(string coinId)
        {
            return CoinIds.Remove(coinId);
        }

        /// <summary>코인을 몇 개 더 가질 수 있는지. 한도를 정하지 않았으면 무한이다. 넘쳤으면 0 이다.</summary>
        public int CoinRoom
        {
            get { return Room(CoinCapacity, CoinIds.Count); }
        }

        /// <summary>유물을 몇 개 더 가질 수 있는지. 한도를 정하지 않았으면 무한이다. 넘쳤으면 0 이다.</summary>
        public int RelicRoom
        {
            get { return Room(RelicCapacity, Relics.Count); }
        }

        /// <summary>코인이 한도를 몇 개 넘었는지. 한도가 줄면 생긴다.</summary>
        public int CoinOverflow
        {
            get { return CoinCapacity >= 0 && CoinIds.Count > CoinCapacity ? CoinIds.Count - CoinCapacity : 0; }
        }

        /// <summary>유물이 한도를 몇 개 넘었는지. 한도가 줄면 생긴다.</summary>
        public int RelicOverflow
        {
            get { return RelicCapacity >= 0 && Relics.Count > RelicCapacity ? Relics.Count - RelicCapacity : 0; }
        }

        private static int Room(int capacity, int count)
        {
            if (capacity < 0)
            {
                return int.MaxValue;
            }

            return count >= capacity ? 0 : capacity - count;
        }
    }

    /// <summary>문양 한 종류와 그 장수.</summary>
    [Serializable]
    public struct OwnedSymbol
    {
        public string SymbolId;
        public int Count;

        public OwnedSymbol(string symbolId, int count)
        {
            SymbolId = symbolId;
            Count = count;
        }
    }

    /// <summary>유물 하나와 그 안에 쌓인 횟수.</summary>
    [Serializable]
    public struct OwnedRelic
    {
        public string RelicId;

        /// <summary>유물 내부 스택 카운트. 쌓이지 않는 유물은 0 으로 둔다.</summary>
        public int Stack;

        public OwnedRelic(string relicId, int stack)
        {
            RelicId = relicId;
            Stack = stack;
        }
    }
}
