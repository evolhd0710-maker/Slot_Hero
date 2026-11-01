using System.Collections.Generic;

namespace SlotHero.Save
{
    /// <summary>
    /// 바뀐 문양 식별자. 옛 식별자를 새 식별자로 옮긴다.
    ///
    /// 2026년 10월 19일 원재가 문양 이름을 기획서에 맞추기로 정했다.
    /// 기준은 전투 시스템 기획서 10c 의 04 문양/태그 표와 04 문양/태그 영문명 표다.
    /// `Pheonix`, `Shipwerck` 는 영어 철자로는 틀렸지만 기획서 표기를 그대로 쓴다.
    ///
    /// 런 저장은 문양을 식별자로 담으므로(`OwnedSymbol.SymbolId`) 바꾸기 전에 저장한 런은
    /// 읽은 직후 여기서 새 식별자로 옮긴다(`SaveRepository.LoadRun`). 저장 형식은 그대로라 판 번호는 올리지 않는다.
    /// 물건 목록 에셋도 같은 표로 고친다(`CreateDefaultConfigs`).
    /// 이름이 또 바뀌면 이 표에 한 줄을 더한다.
    /// </summary>
    public static class SymbolIdRenames
    {
        /// 옛 식별자와 새 식별자. 차례대로 한 쌍이다.
        private static readonly string[,] Table =
        {
            { "QuickSilver", "Quicksilver" },
            { "Phoenix", "Pheonix" },
            { "Shipwreck", "Shipwerck" },
        };

        /// <summary>지금 쓰는 식별자. 바뀐 적 없는 식별자는 그대로 돌려준다.</summary>
        public static string Current(string id)
        {
            for (int i = 0; i < Table.GetLength(0); i++)
            {
                if (Table[i, 0] == id)
                {
                    return Table[i, 1];
                }
            }

            return id;
        }

        /// <summary>
        /// 런의 소지 문양을 새 식별자로 옮긴다. 옮긴 종류 수를 돌려준다.
        /// 목록의 차례는 그대로 둔다. 이벤트가 이 차례로 시드 난수를 굴리기 때문이다.
        /// 옮긴 식별자가 이미 목록에 있으면 장수를 그쪽에 합친다.
        /// 런 저장에서 문양 식별자가 들어가는 칸은 소지 문양뿐이다.
        /// 행상 진열과 이벤트 진행은 저장하지 않고, 유물 순서와 만난 이벤트는 다른 식별자를 담는다.
        /// </summary>
        public static int Apply(RunSaveData run)
        {
            if (run == null || run.Owned == null || run.Owned.Symbols == null)
            {
                return 0;
            }

            List<OwnedSymbol> symbols = run.Owned.Symbols;
            int moved = 0;

            for (int i = symbols.Count - 1; i >= 0; i--)
            {
                string current = Current(symbols[i].SymbolId);
                if (current == symbols[i].SymbolId)
                {
                    continue;
                }

                moved++;
                int already = IndexOf(symbols, current);

                if (already < 0)
                {
                    symbols[i] = new OwnedSymbol(current, symbols[i].Count);
                    continue;
                }

                OwnedSymbol merged = symbols[already];
                merged.Count += symbols[i].Count;
                symbols[already] = merged;
                symbols.RemoveAt(i);
            }

            return moved;
        }

        private static int IndexOf(List<OwnedSymbol> symbols, string id)
        {
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i].SymbolId == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
